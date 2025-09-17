#!/bin/bash

# Configurable Reference Switcher - Switch between local project references and NuGet package references
# Uses reference-config.json for configuration
# Usage: ./switch-references.sh [local|nuget|status] [--config <file>] [--validate]

set -e  # Enable error exit (but not execution tracing)

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
MAGENTA='\033[0;35m'
NC='\033[0m' # No Color

# Debug logging functions
log_debug() {
    echo -e "${MAGENTA}[DEBUG] $1${NC}" >&2
}

log_command() {
    echo -e "${MAGENTA}[CMD] Running: $1${NC}" >&2
}

log_exit_code() {
    local cmd="$1"
    local exit_code="$2"
    echo -e "${MAGENTA}[EXIT] Command '$cmd' exited with code: $exit_code${NC}" >&2
}

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

log_debug() {
    echo -e "${YELLOW}[DEBUG] $1${NC}"
}

debug_command() {
    local cmd="$1"
    local description="$2"
    log_debug "Executing: $description"
    log_debug "Command: $cmd"
    eval "$cmd" || { 
        local exit_code=$?
        log_error "FAILED: $description (exit code: $exit_code)"
        return $exit_code
    }
    log_debug "SUCCESS: $description"
}

load_config() {
    # Use default config file if path is relative
    if [[ ! "$CONFIG_FILE" = /* ]] && [ ! -f "$CONFIG_FILE" ]; then
        CONFIG_FILE="$SCRIPT_DIR/$CONFIG_FILE"
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
    cd "$CONFIG_DIR" || {
        log_error "Failed to change to config directory: $CONFIG_DIR"
        exit 1
    }
}

get_package_version() {
    local package_name="$1"
    local local_path=$(jq -r ".packages[] | select(.name == \"$package_name\") | .local_path" "$CONFIG_FILE")
    
    if [ "$local_path" == "null" ] || [ -z "$local_path" ]; then
        log_error "No local_path configured for package: $package_name"
        return 1
    fi
    
    # Resolve relative path from config directory
    if [[ ! "$local_path" = /* ]]; then
        local_path="$CONFIG_DIR/$local_path"
    fi
    
    if [ ! -f "$local_path" ]; then
        log_error "Local project file not found: $local_path"
        return 1
    fi
    
    local version_pattern=$(jq -r ".settings.default_version_extraction" "$CONFIG_FILE")
    local version=$(grep -oE "$version_pattern" "$local_path" | sed -E 's/<\/?Version>//g' | head -1)
    
    if [ -z "$version" ]; then
        log_error "Could not extract version from: $local_path"
        return 1
    fi
    
    echo "$version"
}

# Helper functions using dotnet commands for reliable project analysis
has_project_reference() {
    local project_file="$1"
    local project_name="$2"
    
    log_debug "Checking project reference: $project_file for $project_name"
    
    # First check the project file directly for ProjectReference (more reliable in CI)
    log_debug "Searching for ProjectReference in $project_file"
    if grep -q "ProjectReference.*Include=\".*$project_name\.csproj\"" "$project_file"; then
        log_debug "Found ProjectReference via grep"
        return 0
    fi
    
    # Fallback: Use dotnet list reference with timeout to check for project references
    # Parse output to look for the specific project file (basename matching)
    log_debug "Using dotnet list reference as fallback for $project_file"
    local references
    references=$(timeout 10 dotnet list "$project_file" reference 2>/dev/null) || {
        local exit_code=$?
        log_debug "dotnet list reference failed with exit code: $exit_code"
        return 1
    }
    
    if [ -n "$references" ]; then
        log_debug "dotnet list reference output: $references"
        # Skip the header lines and check for project name in the paths
        if echo "$references" | tail -n +3 | grep -q "$project_name\.csproj"; then
            log_debug "Found ProjectReference via dotnet list"
            return 0
        fi
    fi
    
    log_debug "No ProjectReference found for $project_name in $project_file"
    return 1
}

has_package_reference() {
    local project_file="$1"
    local package_name="$2"
    
    log_debug "Checking package reference: $project_file for $package_name"
    
    # First check the project file directly for PackageReference
    log_debug "Searching for PackageReference in $project_file"
    if grep -q "PackageReference.*Include=\"$package_name\"" "$project_file"; then
        log_debug "Found PackageReference via grep"
        return 0
    fi
    
    # Fallback: Use dotnet list package with timeout to check for package references
    # This only works for packages that can be resolved
    log_debug "Using dotnet list package as fallback for $project_file"
    local packages
    packages=$(timeout 10 dotnet list "$project_file" package 2>/dev/null) || {
        local exit_code=$?
        log_debug "dotnet list package failed with exit code: $exit_code"
        return 1
    }
    
    if [ -n "$packages" ]; then
        log_debug "dotnet list package output: $packages"
        if echo "$packages" | grep -q "^   > $package_name "; then
            log_debug "Found PackageReference via dotnet list"
            return 0
        fi
    fi
    
    log_debug "No PackageReference found for $package_name in $project_file"
    return 1
}

get_package_version_from_project() {
    local project_file="$1"
    local package_name="$2"
    
    # First try to get version from project file directly
    local version=$(grep "PackageReference.*Include=\"$package_name\"" "$project_file" | grep -oE 'Version="[^"]*"' | sed 's/Version="//g' | sed 's/"//g' | head -1)
    if [ -n "$version" ]; then
        echo "$version"
        return 0
    fi
    
    # Fallback: Use dotnet list package with timeout to get version information
    # This only works for packages that can be resolved
    local version=$(timeout 10 dotnet list "$project_file" package 2>/dev/null | awk '/Top-level Package/,/^$/' | grep "^   > $package_name " | awk '{print $3}' | head -1)
    echo "${version:-unknown}"
}

switch_to_local() {
    log_info "Switching to local project references..."
    local changes_made=0
    local errors=0
    
    # Process each package from configuration
    while IFS= read -r package; do
        local package_name=$(echo "$package" | jq -r '.name')
        local local_path=$(echo "$package" | jq -r '.local_path')
        local target_projects=$(echo "$package" | jq -r '.target_projects[]')
        
        # Process each target project
        while IFS= read -r project_file; do
            if [ ! -f "$project_file" ]; then
                log_warning "Project file not found: $project_file"
                continue
            fi
            
            # Check for PackageReference with this package name
            if grep -q "PackageReference.*Include=\"$package_name\"" "$project_file"; then
                # Calculate relative path from project directory to local_path
                local project_dir=$(dirname "$project_file")
                local relative_local_path="../$local_path"
                
                # Replace PackageReference with ProjectReference (Unix-style path)
                if sed -i.bak "s|<PackageReference Include=\"$package_name\"[^>]*/>|<ProjectReference Include=\"$relative_local_path\" />|g" "$project_file"; then
                    rm -f "${project_file}.bak"
                    log_success "Updated $project_file to use local reference"
                    changes_made=$((changes_made + 1))
                else
                    log_error "Failed to update $project_file"
                    errors=$((errors + 1))
                fi
            fi
        done <<< "$target_projects"
    done <<< "$(jq -c '.packages[]' "$CONFIG_FILE")"
    
    if [ $errors -gt 0 ]; then
        log_error "Completed with $errors error(s)"
        return 1
    elif [ $changes_made -eq 0 ]; then
        log_warning "No changes were needed - projects already use local references"
    else
        log_success "Made $changes_made changes to use local references"
    fi
    
    return 0
}

switch_to_nuget() {
    log_info "Switching to NuGet package references..."
    local changes_made=0
    local errors=0
    
    # Process each package from configuration
    while IFS= read -r package; do
        local package_name=$(echo "$package" | jq -r '.name')
        local local_path=$(echo "$package" | jq -r '.local_path')
        local target_projects=$(echo "$package" | jq -r '.target_projects[]')
        
        # Get version for this package
        local version=$(get_package_version "$package_name")
        if [ $? -ne 0 ]; then
            log_error "Failed to get version for $package_name, skipping..."
            errors=$((errors + 1))
            continue
        fi
        
        # Process each target project
        while IFS= read -r project_file; do
            if [ ! -f "$project_file" ]; then
                log_warning "Project file not found: $project_file"
                continue
            fi
            
            # Check for ProjectReference with this package (handle both Unix and Windows paths)
            local project_name=$(basename "$local_path" .csproj)
            if grep -q "ProjectReference.*Include=\".*$project_name\.csproj\"" "$project_file"; then
                # Replace any ProjectReference that includes this project name with PackageReference
                # This will handle both Unix and Windows style paths
                if sed -i.bak "s|<ProjectReference Include=\"[^\"]*$project_name\.csproj\" />|<PackageReference Include=\"$package_name\" Version=\"$version\" />|g" "$project_file"; then
                    rm -f "${project_file}.bak"
                    log_success "Updated $project_file to use NuGet reference (v$version)"
                    changes_made=$((changes_made + 1))
                else
                    log_error "Failed to update $project_file"
                    errors=$((errors + 1))
                fi
            fi
        done <<< "$target_projects"
    done <<< "$(jq -c '.packages[]' "$CONFIG_FILE")"
    
    if [ $errors -gt 0 ]; then
        log_error "Completed with $errors error(s)"
        return 1
    elif [ $changes_made -eq 0 ]; then
        log_warning "No changes were needed - projects already use NuGet references"
    else
        log_success "Made $changes_made changes to use NuGet references"
    fi
    
    return 0
}

validate_configuration() {
    log_info "Validating configuration and project files..."
    local validation_errors=0
    
    # Check if all configured packages and projects exist
    while IFS= read -r package; do
        local package_name=$(echo "$package" | jq -r '.name')
        local local_path=$(echo "$package" | jq -r '.local_path')
        local target_projects=$(echo "$package" | jq -r '.target_projects[]')
        
        # Check if local project exists
        local resolved_local_path="$local_path"
        if [[ ! "$local_path" = /* ]]; then
            resolved_local_path="$CONFIG_DIR/$local_path"
        fi
        if [ ! -f "$resolved_local_path" ]; then
            log_error "Local project file not found: $resolved_local_path"
            validation_errors=$((validation_errors + 1))
        fi
        
        # Check each target project exists
        while IFS= read -r project_file; do
            if [ ! -f "$project_file" ]; then
                log_error "Target project file not found: $project_file"
                validation_errors=$((validation_errors + 1))
            fi
        done <<< "$target_projects"
    done <<< "$(jq -c '.packages[]' "$CONFIG_FILE")"
    
    echo "================================================================================"
    if [ $validation_errors -eq 0 ]; then
        log_success "Configuration validation completed successfully"
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
    
    log_debug "Starting to process packages from configuration"
    
    # Process each package from configuration
    while IFS= read -r package; do
        local package_name=$(echo "$package" | jq -r '.name')
        local local_path=$(echo "$package" | jq -r '.local_path')
        local target_projects=$(echo "$package" | jq -r '.target_projects[]')
        
        log_debug "Processing package: $package_name"
        echo "Package: $package_name"
        
        # Check each target project
        while IFS= read -r project_file; do
            log_debug "Processing project file: $project_file"
            
            if [ ! -f "$project_file" ]; then
                log_debug "Project file not found: $project_file"
                echo "  ❌ $project_file: FILE NOT FOUND"
                continue
            fi
            
            local project_basename=$(basename "$project_file")
            local project_name=$(basename "$local_path" .csproj)
            
            log_debug "Checking references for $project_basename (looking for $project_name)"
            
            # Check for ProjectReference using dotnet helper
            if has_project_reference "$project_file" "$project_name"; then
                log_debug "Found LOCAL project reference in $project_basename"
                echo "  🔗 $project_basename: LOCAL project reference"
                total_local=$((total_local + 1))
            elif has_package_reference "$project_file" "$package_name"; then
                log_debug "Found NUGET package reference in $project_basename"
                # Extract version using dotnet helper
                local version=$(get_package_version_from_project "$project_file" "$package_name")
                echo "  📦 $project_basename: NUGET reference (v$version)"
                total_nuget=$((total_nuget + 1))
            else
                log_debug "No reference found in $project_basename"
                echo "  ❓ $project_basename: NO REFERENCE FOUND"
            fi
            
            log_debug "Completed processing $project_basename"
        done <<< "$target_projects"
        echo
        log_debug "Completed processing package: $package_name"
    done <<< "$(jq -c '.packages[]' "$CONFIG_FILE")"
    
    log_debug "Finished processing all packages"
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
            switch_to_local
            echo ""
            show_current_references
            ;;
        nuget)
            switch_to_nuget
            echo ""
            show_current_references
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