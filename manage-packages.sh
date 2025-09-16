#!/bin/bash

# NuGet Package Management Script for affolterNET.Web libraries
# Usage: ./manage-packages.sh [action]
# Actions: build, pack, test, publish-local

set -e

BUILD_CONFIG="Release"
PACKAGES_OUTPUT="./packages"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

log_error() {
    echo -e "${RED}❌ $1${NC}"
}

check_prerequisites() {
    log_info "Checking prerequisites..."
    
    if ! command -v dotnet &> /dev/null; then
        log_error ".NET SDK not found. Please install .NET 9.0 SDK"
        exit 1
    fi
    
    local dotnet_version=$(dotnet --version)
    log_success ".NET SDK version: $dotnet_version"
    
    for project in "affolterNET.Web.Core" "affolterNET.Web.Api" "affolterNET.Web.Bff"; do
        if [ ! -d "$project" ]; then
            log_error "Project directory not found: $project"
            exit 1
        fi
    done
}

update_versions() {
    local version=$1
    log_info "Updating package versions to: $version"
    
    for project in "affolterNET.Web.Core" "affolterNET.Web.Api" "affolterNET.Web.Bff"; do
        local project_file="$project/$project.csproj"
        if [ -f "$project_file" ]; then
            sed -i.bak "s/<Version>.*<\/Version>/<Version>$version<\/Version>/g" "$project_file"
            rm "${project_file}.bak" 2>/dev/null || true
            log_success "Updated $project to version $version"
        else
            log_warning "Project file not found: $project_file"
        fi
    done
}

restore_dependencies() {
    log_info "Restoring dependencies..."
    dotnet restore
    log_success "Dependencies restored"
}

build_projects() {
    log_info "Building projects..."
    
    for project in "affolterNET.Web.Core" "affolterNET.Web.Api" "affolterNET.Web.Bff"; do
        local project_file="$project/$project.csproj"
        if [ -f "$project_file" ]; then
            log_info "Building $project..."
            dotnet build "$project_file" --configuration "$BUILD_CONFIG" --no-restore
            log_success "Built $project"
        fi
    done
}

run_tests() {
    log_info "Running tests..."
    
    local test_projects=$(find . -name "*Test*.csproj" -o -name "*.Tests.csproj")
    if [ -n "$test_projects" ]; then
        dotnet test --configuration "$BUILD_CONFIG" --no-build --verbosity normal
        log_success "Tests completed"
    else
        log_warning "No test projects found"
    fi
}

pack_packages() {
    log_info "Packing NuGet packages..."
    
    mkdir -p "$PACKAGES_OUTPUT"
    rm -f "$PACKAGES_OUTPUT"/*.nupkg "$PACKAGES_OUTPUT"/*.snupkg
    
    # Pack in dependency order
    for project in "affolterNET.Web.Core" "affolterNET.Web.Api" "affolterNET.Web.Bff"; do
        local project_file="$project/$project.csproj"
        if [ -f "$project_file" ]; then
            log_info "Packing $project..."
            dotnet pack "$project_file" \
                --configuration "$BUILD_CONFIG" \
                --no-restore \
                --output "$PACKAGES_OUTPUT" \
                --include-symbols \
                --include-source \
                -p:SymbolPackageFormat=snupkg
            log_success "Packed $project"
        fi
    done
    
    log_info "Created packages:"
    ls -la "$PACKAGES_OUTPUT"/*.nupkg
}

publish_local() {
    log_info "Publishing packages to local NuGet source..."
    
    # Create local NuGet source if it doesn't exist
    local local_source="$HOME/.nuget/local-packages"
    mkdir -p "$local_source"
    
    for package in "$PACKAGES_OUTPUT"/*.nupkg; do
        if [ -f "$package" ]; then
            cp "$package" "$local_source/"
            log_success "Published $(basename "$package") to local source"
        fi
    done
    
    log_info "To use these packages locally, add this source to your project:"
    echo "dotnet nuget add source $local_source --name local"
}

show_usage() {
    echo "Usage: $0 [action]"
    echo ""
    echo "Arguments:"
    echo "  action     Action to perform (optional)"
    echo ""
    echo "Actions:"
    echo "  build        Build projects only"
    echo "  test         Run tests only"
    echo "  pack         Pack NuGet packages only"
    echo "  publish-local Publish to local NuGet source"
    echo "  all          Build, test, and pack (default)"
    echo ""
    echo "Examples:"
    echo "  $0              # Build, test, and pack (auto-increment patch version)"
    echo "  $0 build        # Only build (auto-increment patch version)"
    echo "  $0 pack         # Only pack (auto-increment patch version)"
}

# Get current version from the first project file
get_current_version() {
    local project_file="affolterNET.Web.Core/affolterNET.Web.Core.csproj"
    local version=$(grep -oE '<Version>[0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9.]+)?</Version>' "$project_file" | sed -E 's/<\/?Version>//g')
    echo "$version"
}

# Increment patch version
increment_patch_version() {
    local version=$1
    local main_version=$(echo "$version" | cut -d'-' -f1)
    local prerelease=$(echo "$version" | grep -oE '-[a-zA-Z0-9.]+')
    local major=$(echo "$main_version" | cut -d'.' -f1)
    local minor=$(echo "$main_version" | cut -d'.' -f2)
    local patch=$(echo "$main_version" | cut -d'.' -f3)
    patch=$((patch + 1))
    local new_version="$major.$minor.$patch"
    if [ -n "$prerelease" ]; then
        new_version="$new_version$prerelease"
    fi
    echo "$new_version"
}

main() {
    local action=${1:-all}

    check_prerequisites

    local current_version=$(get_current_version)
    local new_version=$(increment_patch_version "$current_version")
    log_info "Current version: $current_version"
    log_info "New version: $new_version"

    case $action in
        build)
            update_versions "$new_version"
            restore_dependencies
            build_projects
            ;;
        test)
            run_tests
            ;;
        pack)
            update_versions "$new_version"
            restore_dependencies
            build_projects
            pack_packages
            ;;
        publish-local)
            publish_local
            ;;
        all)
            update_versions "$new_version"
            restore_dependencies
            build_projects
            run_tests
            pack_packages
            ;;
        *)
            log_error "Unknown action: $action"
            show_usage
            exit 1
            ;;
    esac

    log_success "Operation completed successfully!"
}

main "$@"