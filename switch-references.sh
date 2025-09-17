#!/bin/bash

# Configurable Reference Switcher - Switch between local project references and NuGet package references
# Uses reference-config.json for configuration
# Usage: ./switch-references.sh [local|nuget|status] [--config <file>] [--validate]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default configuration file
CONFIG_FILE="reference-config.json"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ORIGINAL_WD="$(pwd)"
CONFIG_DIR=""

log_info() {
    echo -e "${BLUE}[INFO] $1${NC}"
}

log_success() {
    echo -e "${GREEN}[SUCCESS] $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}[WARNING] $1${NC}"
}

log_error() {
    echo -e "${RED}[ERROR] $1${NC}"
}

load_config() {
    # Convert relative config path to absolute if needed (relative to script directory as fallback)
    if [[ ! "$CONFIG_FILE" = /* ]]; then
        if [ ! -f "$CONFIG_FILE" ]; then
            CONFIG_FILE="$SCRIPT_DIR/$CONFIG_FILE"
        fi
    fi
    
    if [ ! -f "$CONFIG_FILE" ]; then
        log_error "Configuration file not found: $CONFIG_FILE"
        exit 1
    fi
    
    if ! command -v jq &> /dev/null; then
        log_error "jq is required but not installed. Please install jq to parse JSON configuration."
        exit 1
    fi
    
    # Validate JSON syntax
    if ! jq empty "$CONFIG_FILE" 2>/dev/null; then
        log_error "Invalid JSON in configuration file: $CONFIG_FILE"
        exit 1
    fi
    
    # Set config directory and change to it for relative path resolution
    CONFIG_DIR="$(dirname "$CONFIG_FILE")"
    log_info "Loaded configuration from: $CONFIG_FILE"
    log_info "Using config directory as working directory: $CONFIG_DIR"
    
    # Change to config directory for relative path resolution
    cd "$CONFIG_DIR" || {
        log_error "Failed to change to config directory: $CONFIG_DIR"
        exit 1
    }
}

get_package_version() {
    local package_name="$1"
    local version_source=$(jq -r ".packages[] | select(.name == \"$package_name\") | .version_source" "$CONFIG_FILE")
    
    if [ "$version_source" == "null" ] || [ -z "$version_source" ]; then
        log_error "No version source configured for package: $package_name"
        return 1
    fi
    
    # Resolve relative path from config directory
    if [[ ! "$version_source" = /* ]]; then
        version_source="$CONFIG_DIR/$version_source"
    fi
    
    if [ ! -f "$version_source" ]; then
        log_error "Version source file not found: $version_source"
        return 1
    fi
    
    local version_pattern=$(jq -r ".settings.default_version_extraction" "$CONFIG_FILE")
    local version=$(grep -oE "$version_pattern" "$version_source" | sed -E 's/<\/?Version>//g' | head -1)
    
    if [ -z "$version" ]; then
        log_error "Could not extract version from: $version_source"
        return 1
    fi
    
    echo "$version"
}

backup_file() {
    local file="$1"
    local backup_enabled=$(jq -r ".settings.backup_files" "$CONFIG_FILE")
    
    if [ "$backup_enabled" == "true" ]; then
        cp "$file" "${file}.bak"
    fi
}

cleanup_backup() {
    local file="$1"
    local backup_file="${file}.bak"
    local auto_restore=$(jq -r ".settings.auto_restore" "$CONFIG_FILE")
    
    if [ -f "$backup_file" ] && [ "$auto_restore" == "true" ]; then
        rm "$backup_file" 2>/dev/null || true
    fi
}

switch_to_local() {
    log_info "Switching to local project references..."
    local changes_made=0
    
    # Process each package from configuration
    while IFS= read -r package; do
        local package_name=$(echo "$package" | jq -r '.name')
        local local_path=$(echo "$package" | jq -r '.local_path')
        local target_projects=$(echo "$package" | jq -r '.target_projects[]')
        
        log_info "Processing package: $package_name"
        
        # Process each target project
        while IFS= read -r project_file; do
            if [ ! -f "$project_file" ]; then
                log_warning "Project file not found: $project_file"
                continue
            fi
            
            # Check for PackageReference with this package name
            if grep -q "PackageReference.*Include=\"$package_name\"" "$project_file"; then
                backup_file "$project_file"
                # Replace PackageReference with ProjectReference (Unix-style path)
                sed -i.tmp "s|<PackageReference Include=\"$package_name\"[^>]*/>|<ProjectReference Include=\"$local_path\" />|g" "$project_file"
                rm "${project_file}.tmp" 2>/dev/null || true
                cleanup_backup "$project_file"
                log_success "Updated $project_file to use local reference"
                ((changes_made++))
            else
                log_info "$project_file already uses local references or doesn't contain $package_name"
            fi
        done <<< "$target_projects"
    done <<< "$(jq -c '.packages[]' "$CONFIG_FILE")"
    
    if [ $changes_made -eq 0 ]; then
        log_warning "No changes were needed - projects already use local references"
    else
        log_success "Made $changes_made changes to use local references"
    fi
}

switch_to_nuget() {
    log_info "Switching to NuGet package references..."
    local changes_made=0
    
    # Process each package from configuration
    while IFS= read -r package; do
        local package_name=$(echo "$package" | jq -r '.name')
        local local_path=$(echo "$package" | jq -r '.local_path')
        local target_projects=$(echo "$package" | jq -r '.target_projects[]')
        
        log_info "Processing package: $package_name"
        
        # Get version for this package
        local version=$(get_package_version "$package_name")
        if [ $? -ne 0 ]; then
            log_error "Failed to get version for $package_name, skipping..."
            continue
        fi
        
        log_info "Using version: $version"
        
        # Process each target project
        while IFS= read -r project_file; do
            if [ ! -f "$project_file" ]; then
                log_warning "Project file not found: $project_file"
                continue
            fi
            
            # Check for ProjectReference with this package (handle both Unix and Windows paths)
            local project_name=$(basename "$local_path" .csproj)
            if grep -q "ProjectReference.*Include=\".*$project_name\.csproj\"" "$project_file"; then
                backup_file "$project_file"
                
                # Replace any ProjectReference that includes this project name with PackageReference
                # This will handle both Unix and Windows style paths
                sed -i.tmp \
                    "s|<ProjectReference Include=\"[^\"]*$project_name\.csproj\" />|<PackageReference Include=\"$package_name\" Version=\"$version\" />|g" \
                    "$project_file"
                    
                rm "${project_file}.tmp" 2>/dev/null || true
                cleanup_backup "$project_file"
                log_success "Updated $project_file to use NuGet reference (v$version)"
                ((changes_made++))
            else
                log_info "$project_file already uses NuGet references or doesn't contain $package_name"
            fi
        done <<< "$target_projects"
    done <<< "$(jq -c '.packages[]' "$CONFIG_FILE")"
    
    if [ $changes_made -eq 0 ]; then
        log_warning "No changes were needed - projects already use NuGet references"
    else
        log_success "Made $changes_made changes to use NuGet references"
    fi
}

validate_configuration() {
    log_info "Validating configuration and project files..."
    local validation_errors=0
    local total_references=0
    
    # Check if all configured packages exist in target projects
    while IFS= read -r package; do
        local package_name=$(echo "$package" | jq -r '.name')
        local local_path=$(echo "$package" | jq -r '.local_path')
        local target_projects=$(echo "$package" | jq -r '.target_projects[]')
        local version_source=$(echo "$package" | jq -r '.version_source')
        
        log_info "Validating package: $package_name"
        
        # Check if version source exists (resolve relative path)
        local resolved_version_source="$version_source"
        if [[ ! "$version_source" = /* ]]; then
            resolved_version_source="$CONFIG_DIR/$version_source"
        fi
        if [ ! -f "$resolved_version_source" ]; then
            log_error "  Version source file not found: $resolved_version_source"
            ((validation_errors++))
        fi
        
        # Check if local project exists (resolve relative path)
        local resolved_local_path="$local_path"
        if [[ ! "$local_path" = /* ]]; then
            resolved_local_path="$CONFIG_DIR/$local_path"
        fi
        if [ ! -f "$resolved_local_path" ]; then
            log_error "  Local project file not found: $resolved_local_path"
            ((validation_errors++))
        fi
        
        # Check each target project
        while IFS= read -r project_file; do
            if [ ! -f "$project_file" ]; then
                log_error "  Target project file not found: $project_file"
                ((validation_errors++))
                continue
            fi
            
            # Count total references for this package
            local package_refs=$(grep -c "PackageReference.*$package_name\|ProjectReference.*$(basename "$local_path" .csproj)" "$project_file" 2>/dev/null || echo "0")
            ((total_references += package_refs))
            
            if [ $package_refs -eq 0 ]; then
                log_warning "  No references to $package_name found in $project_file"
            fi
        done <<< "$target_projects"
    done <<< "$(jq -c '.packages[]' "$CONFIG_FILE")"
    
    # Look for unconfigured references in project files
    log_info "Scanning for unconfigured references..."
    local configured_packages=$(jq -r '.packages[].name' "$CONFIG_FILE")
    
    while IFS= read -r package; do
        local target_projects=$(echo "$package" | jq -r '.target_projects[]')
        
        while IFS= read -r project_file; do
            if [ -f "$project_file" ]; then
                # Find all PackageReference and ProjectReference lines
                local all_refs=$(grep -E "(PackageReference|ProjectReference)" "$project_file" 2>/dev/null || true)
                
                if [ -n "$all_refs" ]; then
                    while IFS= read -r ref_line; do
                        if [[ "$ref_line" =~ PackageReference.*Include=\"([^\"]+)\" ]]; then
                            local ref_package="${BASH_REMATCH[1]}"
                            if ! echo "$configured_packages" | grep -q "^$ref_package$"; then
                                log_warning "  Unconfigured PackageReference: $ref_package in $project_file"
                            fi
                        elif [[ "$ref_line" =~ ProjectReference.*Include=\"([^\"]+)\" ]]; then
                            local ref_project="${BASH_REMATCH[1]}"
                            local configured_locals=$(jq -r '.packages[].local_path' "$CONFIG_FILE")
                            if ! echo "$configured_locals" | grep -q "$ref_project"; then
                                log_warning "  Unconfigured ProjectReference: $ref_project in $project_file"
                            fi
                        fi
                    done <<< "$all_refs"
                fi
            fi
        done <<< "$target_projects"
    done <<< "$(jq -c '.packages[]' "$CONFIG_FILE")"
    
    echo "================================================================================"
    if [ $validation_errors -eq 0 ]; then
        log_success "Configuration validation completed successfully"
        log_info "Found $total_references total configured references"
    else
        log_error "Configuration validation failed with $validation_errors errors"
        return 1
    fi
    echo "================================================================================"
}

show_current_references() {
    log_info "Current reference status:"
    echo "================================================================================"
    
    local total_local=0
    local total_nuget=0
    
    # Process each package from configuration
    while IFS= read -r package; do
        local package_name=$(echo "$package" | jq -r '.name')
        local local_path=$(echo "$package" | jq -r '.local_path')
        local target_projects=$(echo "$package" | jq -r '.target_projects[]')
        
        echo "Package: $package_name"
        
        # Check each target project
        while IFS= read -r project_file; do
            if [ ! -f "$project_file" ]; then
                echo "  ❌ $project_file: FILE NOT FOUND"
                continue
            fi
            
            local project_basename=$(basename "$project_file")
            local project_name=$(basename "$local_path" .csproj)
            
            # Check for ProjectReference (both Unix and Windows paths)
            if grep -q "ProjectReference.*Include=\".*$project_name\.csproj\"" "$project_file"; then
                echo "  🔗 $project_basename: LOCAL project reference"
                ((total_local++))
            elif grep -q "PackageReference.*Include=\"$package_name\"" "$project_file"; then
                local version=$(grep -oE "PackageReference Include=\"$package_name\" Version=\"[^\"]*\"" "$project_file" | grep -oE 'Version="[^"]*"' | sed 's/Version="//g' | sed 's/"//g')
                echo "  📦 $project_basename: NUGET reference (v$version)"
                ((total_nuget++))
            else
                echo "  ❓ $project_basename: NO REFERENCE FOUND"
            fi
        done <<< "$target_projects"
        echo
    done <<< "$(jq -c '.packages[]' "$CONFIG_FILE")"
    
    echo "Summary: $total_local local references, $total_nuget NuGet references"
    echo "================================================================================"
}

show_usage() {
    echo "Configurable Reference Switcher"
    echo "Usage: $0 [mode] [options]"
    echo ""
    echo "Modes:"
    echo "  local      Switch to local project references (for development)"
    echo "  nuget      Switch to NuGet package references (for distribution)"
    echo "  status     Show current reference types"
    echo "  validate   Validate configuration and scan for unconfigured references"
    echo ""
    echo "Options:"
    echo "  --config <file>    Use custom configuration file (default: reference-config.json)"
    echo "  --help, -h         Show this help message"
    echo ""
    echo "Configuration:"
    echo "  The script uses '$CONFIG_FILE' to define packages and their references."
    echo "  This file should contain package definitions with local paths and target projects."
    echo ""
    echo "Examples:"
    echo "  $0 local                           # Switch to local references"
    echo "  $0 nuget                           # Switch to NuGet references"
    echo "  $0 status                          # Show current reference types"
    echo "  $0 validate                        # Validate configuration"
    echo "  $0 local --config my-config.json  # Use custom configuration file"
    echo ""
    echo "Requirements:"
    echo "  - jq (for JSON parsing)"
    echo "  - All project files must exist"
    echo "  - Configuration file must be valid JSON"
}

parse_arguments() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            --config)
                CONFIG_FILE="$2"
                shift 2
                ;;
            --help|-h)
                show_usage
                exit 0
                ;;
            local|nuget|status|validate)
                if [ -z "$MODE" ]; then
                    MODE="$1"
                else
                    log_error "Multiple modes specified. Please specify only one mode."
                    exit 1
                fi
                shift
                ;;
            *)
                log_error "Unknown option: $1"
                show_usage
                exit 1
                ;;
        esac
    done
    
    # Set default mode if none specified
    if [ -z "$MODE" ]; then
        MODE="status"
    fi
    
    # Convert relative config path to absolute based on original working directory
    if [[ ! "$CONFIG_FILE" = /* ]]; then
        CONFIG_FILE="$ORIGINAL_WD/$CONFIG_FILE"
    fi
}

main() {
    local MODE=""
    
    # Parse command line arguments
    parse_arguments "$@"
    
    # Load and validate configuration
    load_config
    
    case $MODE in
        local)
            log_info "Switching to local project references..."
            switch_to_local
            echo ""
            show_current_references
            log_success "Switch to local references completed"
            ;;
        nuget)
            log_info "Switching to NuGet package references..."
            switch_to_nuget
            echo ""
            show_current_references
            log_success "Switch to NuGet references completed"
            ;;
        status)
            show_current_references
            ;;
        validate)
            validate_configuration
            ;;
        *)
            log_error "Unknown mode: $MODE"
            show_usage
            exit 1
            ;;
    esac
    
    # Return to original working directory
    cd "$ORIGINAL_WD" || true
}

# Run main function with all arguments
main "$@"