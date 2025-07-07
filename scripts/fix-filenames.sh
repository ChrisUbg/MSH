#!/bin/bash
# Fix Filenames for Cross-Platform Compatibility
# This script renames files with spaces and special characters

set -e

# Source environment configuration
source config/environment.sh

echo "🔧 Fixing filenames for cross-platform compatibility..."

# Function to safely rename files
safe_rename() {
    local old_name="$1"
    local new_name="$2"
    
    if [[ -f "$old_name" ]]; then
        echo "Renaming: $old_name → $new_name"
        mv "$old_name" "$new_name"
    else
        echo "⚠️  File not found: $old_name"
    fi
}

# Function to update references in files
update_references() {
    local old_name="$1"
    local new_name="$2"
    
    echo "Updating references: $old_name → $new_name"
    
    # Update references in markdown files
    find . -name "*.md" -type f -exec sed -i "s/$old_name/$new_name/g" {} /;
    
    # Update references in shell scripts
    find . -name "*.sh" -type f -exec sed -i "s/$old_name/$new_name/g" {} /;
    
    # Update references in YAML files
    find . -name "*.yml" -o -name "*.yaml" -type f -exec sed -i "s/$old_name/$new_name/g" {} /;
    
    # Update references in JSON files
    find . -name "*.json" -type f -exec sed -i "s/$old_name/$new_name/g" {} /;
}

# List of files to rename (old_name → new_name)
declare -A file_renames=(
    ["Matter-SDK-ARM64-Build-Instructions.md"]="Matter-SDK-ARM64-Build-Instructions.md"
    ["docker-handling.md"]="docker-handling.md"
    ["PI-CONFIG-readme.md"]="pi-config-readme.md"
    ["real-commissioning-test-guide.md"]="real-commissioning-test-guide.md"
    ["project-roadmap.md"]="project-roadmap.md"
    ["project-definition.md"]="project-definition.md"
    ["debian-setup.md"]="debian-setup.md"
    ["postgres-setup.md"]="postgres-setup.md"
    ["network-config.md"]="network-config.md"
    ["dev-process.md"]="dev-process.md"
    ["hybrid-approach.md"]="hybrid-approach.md"
    ["migration.md"]="migration.md"
    ["tasmoda.md"]="tasmoda.md"
    ["mdns-setup.md"]="mdns-setup.md"
    ["CURSOR_AGENT.md"]="cursor-agent.md"
    ["basic-rules.md"]="basic-rules.md"
    ["changelog.md"]="changelog.md"
    ["todo.md"]="todo.md"
    ["api.md"]="api.md"
    ["readme.md"]="readme.md"
)

# Create backup directory
BACKUP_DIR="backup-$(date +%Y%m%d_%H%M%S)"
mkdir -p "$BACKUP_DIR"

echo "📁 Creating backup in: $BACKUP_DIR"

# Rename files and update references
for old_name in "${!file_renames[@]}"; do
    new_name="${file_renames[$old_name]}"
    
    if [[ -f "$old_name" ]]; then
        # Create backup
        cp "$old_name" "$BACKUP_DIR/"
        
        # Rename file
        safe_rename "$old_name" "$new_name"
        
        # Update references (escape special characters for sed)
        old_name_escaped=$(echo "$old_name" | sed 's/[[/.*^$()+?{|]///&/g')
        new_name_escaped=$(echo "$new_name" | sed 's/[[/.*^$()+?{|]///&/g')
        
        update_references "$old_name_escaped" "$new_name_escaped"
    fi
done

# Fix path separators in files
echo "🔧 Fixing path separators..."

# Replace backslashes with forward slashes in all files
find . -name "*.md" -o -name "*.sh" -o -name "*.yml" -o -name "*.yaml" -o -name "*.json" | while read -r file; do
    if [[ "$OS" == "linux" ]]; then
        # On Linux, ensure forward slashes
        sed -i 's|//|/|g' "$file"
    else
        # On Windows, keep forward slashes (they work on Windows too)
        sed -i 's|//|/|g' "$file"
    fi
done

# Update hardcoded paths
echo "🔧 Updating hardcoded paths..."

# Replace hardcoded Windows paths with environment variables
find . -name "*.md" -o -name "*.sh" -o -name "*.yml" -o -name "*.yaml" | while read -r file; do
    # Replace C:/Users/Dev/source/repos/MSH with PROJECT_ROOT
    sed -i 's|C://Users//Dev//source//repos//MSH|/${PROJECT_ROOT}|g' "$file"
    
    # Replace C:/Users/Dev with USER_HOME
    sed -i 's|C://Users//Dev|/${USER_HOME}|g' "$file"
    
    # Replace hardcoded IP addresses with variables
    sed -i 's|192/.168/.0/.102|/${PI_IP}|g' "$file"
    sed -i 's|192/.168/.0/.104|/${PI_IP}|g' "$file"
    sed -i 's|${PI_USER}@|/${PI_USER}@|g' "$file"
    sed -i 's|${DEV_USER}@|/${DEV_USER}@|g' "$file"
done

# Create a summary report
echo "📊 Creating summary report..."

cat > "filename-fix-report.md" << EOF
# Filename Fix Report

## Summary
- **Date**: $(date)
- **Backup Location**: $BACKUP_DIR
- **Files Renamed**: ${#file_renames[@]}

## Renamed Files
EOF

for old_name in "${!file_renames[@]}"; do
    new_name="${file_renames[$old_name]}"
    if [[ -f "$new_name" ]]; then
        echo "- ✅ $old_name → $new_name" >> "filename-fix-report.md"
    else
        echo "- ❌ $old_name → $new_name (not found)" >> "filename-fix-report.md"
    fi
done

cat >> "filename-fix-report.md" << EOF

## Path Separator Fixes
- ✅ All backslashes replaced with forward slashes
- ✅ Hardcoded Windows paths replaced with environment variables
- ✅ IP addresses replaced with environment variables

## Next Steps
1. Review the renamed files
2. Test scripts and documentation
3. Update any remaining hardcoded references
4. Commit changes to version control

## Backup
All original files are backed up in: $BACKUP_DIR
EOF

echo "✅ Filename fixes completed!"
echo "📄 Report saved to: filename-fix-report.md"
echo "📁 Backup saved to: $BACKUP_DIR"
echo ""
echo "🔍 Please review the changes and test the scripts." 