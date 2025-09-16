#!/bin/bash

# Script to switch between local project references and NuGet package references
# Usage: ./switch-references.sh [local|nuget] [version]

set -e

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

get_current_version() {
    local project_file="affolterNET.Web.Core/affolterNET.Web.Core.csproj"
    local version=$(grep -oE '<Version>[0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9.]+)?</Version>' "$project_file" | sed -E 's/<\/?Version>//g')
    echo "$version"
}

switch_to_local() {
    log_info "Switching to local project references..."
    
    # Update affolterNET.Web.Api
    local api_file="affolterNET.Web.Api/affolterNET.Web.Api.csproj"
    if grep -q "PackageReference.*affolterNET.Web.Core" "$api_file"; then
        sed -i.bak 's|<PackageReference Include="affolterNET.Web.Core"[^>]*/>|<ProjectReference Include="../affolterNET.Web.Core/affolterNET.Web.Core.csproj" />|g' "$api_file"
        rm "${api_file}.bak" 2>/dev/null || true
        log_success "Updated $api_file to use local reference"
    else
        log_warning "$api_file already uses local references"
    fi
    
    # Update affolterNET.Web.Bff
    local bff_file="affolterNET.Web.Bff/affolterNET.Web.Bff.csproj"
    if grep -q "PackageReference.*affolterNET.Web.Core" "$bff_file"; then
        sed -i.bak 's|<PackageReference Include="affolterNET.Web.Core"[^>]*/>|<ProjectReference Include="../affolterNET.Web.Core/affolterNET.Web.Core.csproj" />|g' "$bff_file"
        rm "${bff_file}.bak" 2>/dev/null || true
        log_success "Updated $bff_file to use local reference"
    else
        log_warning "$bff_file already uses local references"
    fi
}

switch_to_nuget() {
    local version=$(get_current_version)
    log_info "Switching to NuGet package references (version: $version)..."
    
    # Update affolterNET.Web.Api
    local api_file="affolterNET.Web.Api/affolterNET.Web.Api.csproj"
    if grep -q "ProjectReference.*affolterNET.Web.Core" "$api_file"; then
        sed -i.bak "s|<ProjectReference Include=\"../affolterNET.Web.Core/affolterNET.Web.Core.csproj\" />|<PackageReference Include=\"affolterNET.Web.Core\" Version=\"$version\" />|g" "$api_file"
        rm "${api_file}.bak" 2>/dev/null || true
        log_success "Updated $api_file to use NuGet reference (v$version)"
    else
        log_warning "$api_file already uses NuGet references"
    fi
    
    # Update affolterNET.Web.Bff
    local bff_file="affolterNET.Web.Bff/affolterNET.Web.Bff.csproj"
    if grep -q "ProjectReference.*affolterNET.Web.Core" "$bff_file"; then
        sed -i.bak "s|<ProjectReference Include=\"../affolterNET.Web.Core/affolterNET.Web.Core.csproj\" />|<PackageReference Include=\"affolterNET.Web.Core\" Version=\"$version\" />|g" "$bff_file"
        rm "${bff_file}.bak" 2>/dev/null || true
        log_success "Updated $bff_file to use NuGet reference (v$version)"
    else
        log_warning "$bff_file already uses NuGet references"
    fi
}

show_current_references() {
    log_info "Current reference types:"
    
    local api_file="affolterNET.Web.Api/affolterNET.Web.Api.csproj"
    local bff_file="affolterNET.Web.Bff/affolterNET.Web.Bff.csproj"
    
    if grep -q "ProjectReference.*affolterNET.Web.Core" "$api_file"; then
        echo "  - affolterNET.Web.Api: LOCAL project reference"
    elif grep -q "PackageReference.*affolterNET.Web.Core" "$api_file"; then
        local version=$(grep -oE 'PackageReference Include="affolterNET.Web.Core" Version="[^"]*"' "$api_file" | grep -oE 'Version="[^"]*"' | sed 's/Version="//g' | sed 's/"//g')
        echo "  - affolterNET.Web.Api: NUGET reference (v$version)"
    fi
    
    if grep -q "ProjectReference.*affolterNET.Web.Core" "$bff_file"; then
        echo "  - affolterNET.Web.Bff: LOCAL project reference"
    elif grep -q "PackageReference.*affolterNET.Web.Core" "$bff_file"; then
        local version=$(grep -oE 'PackageReference Include="affolterNET.Web.Core" Version="[^"]*"' "$bff_file" | grep -oE 'Version="[^"]*"' | sed 's/Version="//g' | sed 's/"//g')
        echo "  - affolterNET.Web.Bff: NUGET reference (v$version)"
    fi
}

show_usage() {
    echo "Usage: $0 [mode]"
    echo ""
    echo "Modes:"
    echo "  local      Switch to local project references (for development)"
    echo "  nuget      Switch to NuGet package references (for distribution)"
    echo "  status     Show current reference types"
    echo ""
    echo "Examples:"
    echo "  $0 local                    # Switch to local references"
    echo "  $0 nuget                    # Switch to NuGet with current version from project files"
    echo "  $0 status                   # Show current reference types"
}

main() {
    local mode=${1:-status}
    
    case $mode in
        local)
            switch_to_local
            log_success "Switched to local project references"
            ;;
        nuget)
            switch_to_nuget
            log_success "Switched to NuGet package references"
            ;;
        status)
            show_current_references
            ;;
        *)
            log_error "Unknown mode: $mode"
            show_usage
            exit 1
            ;;
    esac
    
    if [ "$mode" != "status" ]; then
        echo ""
        show_current_references
    fi
}

main "$@"