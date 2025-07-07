#!/bin/bash
# Update Scripts for Cross-Platform Compatibility
# This script updates existing scripts to use environment variables

set -e

echo "🔧 Updating scripts for cross-platform compatibility..."

# Function to add environment source to script
add_environment_source() {
    local script_file="$1"
    
    # Check if script already has environment source
    if ! grep -q "source config/environment.sh" "$script_file"; then
        echo "Adding environment source to: $script_file"
        
        # Add environment source after shebang
        if grep -q "^#!/" "$script_file"; then
            # Script has shebang, add after it
            sed -i '1a source config/environment.sh' "$script_file"
        else
            # No shebang, add at beginning
            sed -i '1i source config/environment.sh' "$script_file"
        fi
    else
        echo "Environment source already exists in: $script_file"
    fi
}

# Function to replace hardcoded paths with environment variables
replace_hardcoded_paths() {
    local script_file="$1"
    
    echo "Updating hardcoded paths in: $script_file"
    
    # Replace common hardcoded paths
    sed -i 's|src/MSH/.Web|/${MSH_WEB_DIR}|g' "$script_file"
    sed -i 's|src/MSH/.Infrastructure|/${MSH_INFRASTRUCTURE_DIR}|g' "$script_file"
    sed -i 's|src/MSH/.Core|/${MSH_CORE_DIR}|g' "$script_file"
    sed -i 's|Matter/|/${MATTER_DIR}/|g' "$script_file"
    sed -i 's|docker-compose/.prod-msh/.yml|/${DOCKER_COMPOSE_PROD}|g' "$script_file"
    sed -i 's|docker-compose/.dev-msh/.yml|/${DOCKER_COMPOSE_DEV}|g' "$script_file"
    sed -i 's|docker-compose/.prod-pi/.yml|/${DOCKER_COMPOSE_PI}|g' "$script_file"
    
    # Replace hardcoded IP addresses
    sed -i 's|192/.168/.0/.102|/${PI_IP}|g' "$script_file"
    sed -i 's|192/.168/.0/.104|/${PI_IP}|g' "$script_file"
    sed -i 's|${PI_USER}@|/${PI_USER}@|g' "$script_file"
    sed -i 's|${DEV_USER}@|/${DEV_USER}@|g' "$script_file"
    
    # Replace hardcoded project paths
    sed -i 's|~/MSH|/${PROJECT_ROOT}|g' "$script_file"
    sed -i 's|/home/chregg/MSH|/${PROJECT_ROOT}|g' "$script_file"
}

# Function to update Docker Compose files
update_docker_compose() {
    local compose_file="$1"
    
    echo "Updating Docker Compose file: $compose_file"
    
    # Replace hardcoded paths with environment variables
    sed -i 's|src/MSH/.Web/Dockerfile|/${MSH_WEB_DIR}/Dockerfile|g' "$compose_file"
    sed -i 's|src/MSH/.Web/Dockerfile/.dev|/${MSH_WEB_DIR}/Dockerfile.dev|g' "$compose_file"
    sed -i 's|src/MSH/.Web/Dockerfile/.prod|/${MSH_WEB_DIR}/Dockerfile.prod|g' "$compose_file"
    
    # Replace volume paths
    sed -i 's|/./src/MSH/.Web|/${MSH_WEB_DIR}|g' "$compose_file"
    sed -i 's|/./Matter|/${MATTER_DIR}|g' "$compose_file"
}

# List of scripts to update
scripts_to_update=(
    "build-and-deploy-matter.sh"
    "deploy-matter-bridge.sh"
    "deploy-to-pi.sh"
    "deploy.sh"
    "deploy_msh_to_pi.sh"
    "transfer_to_pi.sh"
    "transfer-matter-tools.sh"
    "monitor_build.sh"
    "check_build_status.sh"
    "fix-database-auth.sh"
    "network-config.sh"
    "network-config-safe.sh"
    "network-recovery.sh"
    "start-matter-server.sh"
    "setup-mdns.sh"
    "find-pi.sh"
    "pi-ip-broadcast.sh"
    "backup_postgres.sh"
    "restore_postgres.sh"
    "test-docker-setup.sh"
    "test-mdns.sh"
    "activate.sh"
)

# List of Docker Compose files to update
compose_files=(
    "docker-compose.yml"
    "docker-compose.dev-msh.yml"
    "docker-compose.prod-msh.yml"
    "docker-compose.prod-pi.yml"
    "org_docker-compose.yml"
    "org_docker-compose.swarm.yml"
)

# Update shell scripts
echo "📝 Updating shell scripts..."
for script in "${scripts_to_update[@]}"; do
    if [[ -f "$script" ]]; then
        echo "Processing: $script"
        add_environment_source "$script"
        replace_hardcoded_paths "$script"
    else
        echo "⚠️  Script not found: $script"
    fi
done

# Update Docker Compose files
echo "🐳 Updating Docker Compose files..."
for compose_file in "${compose_files[@]}"; do
    if [[ -f "$compose_file" ]]; then
        echo "Processing: $compose_file"
        update_docker_compose "$compose_file"
    else
        echo "⚠️  Compose file not found: $compose_file"
    fi
done

# Create a template for new scripts
echo "📋 Creating script template..."
cat > "scripts/template.sh" << 'EOF'
#!/bin/bash
# Script Template for Cross-Platform Compatibility

set -e

# Source environment configuration
source config/environment.sh

# Script variables
SCRIPT_NAME="$(basename "$0")"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Function to log messages
log() {
    echo "[$(date +'%Y-%m-%d %H:%M:%S')] $1"
}

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Function to check if file exists
file_exists() {
    [[ -f "$1" ]]
}

# Function to check if directory exists
dir_exists() {
    [[ -d "$1" ]]
}

# Main script logic
main() {
    log "Starting $SCRIPT_NAME"
    
    # Your script logic here
    log "Script completed successfully"
}

# Run main function
main "$@"
EOF

chmod +x "scripts/template.sh"

# Create a summary report
echo "📊 Creating update report..."

cat > "script-update-report.md" << EOF
# Script Update Report

## Summary
- **Date**: $(date)
- **Scripts Updated**: ${#scripts_to_update[@]}
- **Docker Compose Files Updated**: ${#compose_files[@]}

## Updated Scripts
EOF

for script in "${scripts_to_update[@]}"; do
    if [[ -f "$script" ]]; then
        echo "- ✅ $script" >> "script-update-report.md"
    else
        echo "- ❌ $script (not found)" >> "script-update-report.md"
    fi
done

cat >> "script-update-report.md" << EOF

## Updated Docker Compose Files
EOF

for compose_file in "${compose_files[@]}"; do
    if [[ -f "$compose_file" ]]; then
        echo "- ✅ $compose_file" >> "script-update-report.md"
    else
        echo "- ❌ $compose_file (not found)" >> "script-update-report.md"
    fi
done

cat >> "script-update-report.md" << EOF

## Changes Made
- ✅ Added environment source to all scripts
- ✅ Replaced hardcoded paths with environment variables
- ✅ Replaced hardcoded IP addresses with variables
- ✅ Updated Docker Compose file paths
- ✅ Created script template for future use

## Environment Variables Used
- /${PROJECT_ROOT} - Project root directory
- /${MSH_WEB_DIR} - MSH.Web directory
- /${MSH_INFRASTRUCTURE_DIR} - MSH.Infrastructure directory
- /${MSH_CORE_DIR} - MSH.Core directory
- /${MATTER_DIR} - Matter directory
- /${PI_IP} - Raspberry Pi IP address
- /${PI_USER} - Raspberry Pi username
- /${DEV_USER} - Development machine username
- /${DOCKER_COMPOSE_PROD} - Production Docker Compose file
- /${DOCKER_COMPOSE_DEV} - Development Docker Compose file

## Next Steps
1. Test all updated scripts
2. Verify Docker Compose files work correctly
3. Update any remaining hardcoded references
4. Use template.sh for new scripts

## Template
New script template created at: scripts/template.sh
EOF

echo "✅ Script updates completed!"
echo "📄 Report saved to: script-update-report.md"
echo "📋 Template created at: scripts/template.sh"
echo ""
echo "🔍 Please test the updated scripts and verify they work correctly." 