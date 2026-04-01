#!/bin/bash
# test-script.sh - List directory contents with detailed information
# Usage: ./test-script.sh [directory]

set -e

# Default to current directory if no argument provided
TARGET_DIR="${1:-.}"

# Check if directory exists
if [[ ! -d "$TARGET_DIR" ]]; then
    echo "Error: Directory '$TARGET_DIR' does not exist."
    exit 1
fi

echo "📂 Directory listing for: $TARGET_DIR"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# Format: permissions, size, owner, name
ls -lah "$TARGET_DIR" | tail -n +2 | awk '{
    permissions = $1
    size = $5
    owner = $3
    group = $4
    name = $9
    
    # Convert size to human-readable
    if (size < 1024) {
        size_str = size "B"
    } else if (size < 1024*1024) {
        size_str = int(size/1024) "KB"
    } else {
        size_str = int(size/(1024*1024)) "MB"
    }
    
    printf "%s  %8s  %-10s %-10s  %s\n", permissions, size_str, owner, group, name
}'

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "Total: $(ls -la "$TARGET_DIR" | wc -l) items"
