#!/bin/bash

# ============================================================================
# test-script.sh
# ============================================================================
# Description: List files in a directory with detailed information including
#              file size and permissions.
# Usage:       ./test-script.sh [directory_path]
# Author:      Claude Code
# Date:        2026-04-01
# ============================================================================

set -euo pipefail  # Exit on error, undefined variables, and pipe failures

# Color codes for better readability
readonly RED='\033[0;31m'
readonly GREEN='\033[0;32m'
readonly YELLOW='\033[1;33m'
readonly BLUE='\033[0;34m'
readonly NC='\033[0m' # No Color

# ----------------------------------------------------------------------------
# Function: print_usage
# Description: Print usage information
# ----------------------------------------------------------------------------
print_usage() {
    echo "Usage: $0 [directory_path]"
    echo ""
    echo "Arguments:"
    echo "  directory_path    Path to the directory to list (default: current directory)"
    echo ""
    echo "Example:"
    echo "  $0 /tmp"
    echo "  $0 ."
}

# ----------------------------------------------------------------------------
# Function: print_header
# Description: Print formatted header
# ----------------------------------------------------------------------------
print_header() {
    local dir="$1"
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}Directory: ${GREEN}${dir}${NC}"
    echo -e "${BLUE}========================================${NC}"
    echo ""
}

# ----------------------------------------------------------------------------
# Function: list_files
# Description: List files with detailed information
# Arguments:   $1 - directory path
# ----------------------------------------------------------------------------
list_files() {
    local target_dir="$1"

    # Check if directory exists
    if [[ ! -d "$target_dir" ]]; then
        echo -e "${RED}Error: Directory '$target_dir' does not exist.${NC}" >&2
        return 1
    fi

    # Check if directory is readable
    if [[ ! -r "$target_dir" ]]; then
        echo -e "${RED}Error: Directory '$target_dir' is not readable.${NC}" >&2
        return 1
    fi

    # Print header
    print_header "$(cd "$target_dir" && pwd)"

    # List files with details using ls -la
    # -l: long format (permissions, owner, size, date)
    # -a: include hidden files (starting with .)
    # -h: human-readable sizes (KB, MB, GB)
    echo -e "${YELLOW}Permissions${NC}  ${YELLOW}Links${NC}  ${YELLOW}Owner${NC}  ${YELLOW}Group${NC}  ${YELLOW}Size${NC}    ${YELLOW}Last Modified${NC}       ${YELLOW}Name${NC}"
    echo -e "${BLUE}--------------------------------------------------------------------------${NC}"

    # Use ls to list files, excluding . and .. from the listing
    ls -lah "$target_dir" | tail -n +4 | while read -r line; do
        # Parse permissions for color coding
        local perms
        perms=$(echo "$line" | awk '{print $1}')

        local color="$NC"
        if [[ "$perms" =~ ^d ]]; then
            # Directory - use blue
            color="$BLUE"
        elif [[ "$perms" =~ ^-.*x ]]; then
            # Executable file - use green
            color="$GREEN"
        elif [[ "$perms" =~ ^l ]]; then
            # Symbolic link - use yellow
            color="$YELLOW"
        fi

        # Format and print the line with color
        local name
        name=$(echo "$line" | awk '{print $9}')
        local prefix
        prefix=$(echo "$line" | awk '{print $1" "$2" "$3" "$4" "$5" "$6" "$7" "$8}')

        # Output with colored filename
        echo -e "${prefix} ${color}${name}${NC}"
    done

    echo ""
    echo -e "${BLUE}========================================${NC}"

    # Print summary statistics
    local file_count dir_count link_count
    file_count=$(find "$target_dir" -maxdepth 1 -type f | wc -l | tr -d ' ')
    dir_count=$(find "$target_dir" -maxdepth 1 -type d | wc -l | tr -d ' ')
    link_count=$(find "$target_dir" -maxdepth 1 -type l | wc -l | tr -d ' ')

    # Adjust dir_count to exclude the directory itself
    ((dir_count--))

    echo -e "${GREEN}Summary:${NC}"
    echo -e "  Files:        $file_count"
    echo -e "  Directories:  $dir_count"
    echo -e "  Symlinks:     $link_count"
    echo -e "${BLUE}========================================${NC}"
}

# ----------------------------------------------------------------------------
# Main Script
# ----------------------------------------------------------------------------

# Get target directory from argument or use current directory
TARGET_DIR="${1:-.}"

# Check for help flag
if [[ "$TARGET_DIR" == "-h" || "$TARGET_DIR" == "--help" ]]; then
    print_usage
    exit 0
fi

# Expand tilde to home directory if present
TARGET_DIR="${TARGET_DIR/#\~/$HOME}"

# List files in the target directory
list_files "$TARGET_DIR"
