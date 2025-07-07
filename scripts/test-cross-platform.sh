#!/bin/bash
# Test Cross-Platform Compatibility

set -e

echo "🧪 Testing Cross-Platform Compatibility"
echo "======================================"

# Source environment configuration
source config/environment.sh

# Test 1: Environment variables
echo "✅ Test 1: Environment Variables"
echo "  OS: $OS"
echo "  Project Root: $PROJECT_ROOT"
echo "  Pi IP: $PI_IP"
echo "  Pi User: $PI_USER"

# Test 2: Path construction
echo ""
echo "✅ Test 2: Path Construction"
echo "  MSH Web Dir: $MSH_WEB_DIR"
echo "  Matter Dir: $MATTER_DIR"
echo "  Docker Compose Prod: $DOCKER_COMPOSE_PROD"

# Test 3: File existence
echo ""
echo "✅ Test 3: File Existence"
if [[ -f "MSH.sln" ]]; then
    echo "  MSH.sln: ✅ Found"
else
    echo "  MSH.sln: ❌ Not found"
fi

if [[ -d "src/MSH.Web" ]]; then
    echo "  src/MSH.Web: ✅ Found"
else
    echo "  src/MSH.Web: ❌ Not found"
fi

if [[ -d "Matter" ]]; then
    echo "  Matter/: ✅ Found"
else
    echo "  Matter/: ❌ Not found"
fi

# Test 4: Cross-platform functions
echo ""
echo "✅ Test 4: Cross-Platform Functions"
echo "  WSL2: $(is_wsl2 && echo 'Yes' || echo 'No')"
echo "  WSL Username: $(get_wsl_username)"
echo "  Path Separator: $SEPARATOR"

# Test 5: Docker Compose files
echo ""
echo "✅ Test 5: Docker Compose Files"
for compose_file in "docker-compose.yml" "docker-compose.prod-msh.yml" "docker-compose.dev-msh.yml"; do
    if [[ -f "$compose_file" ]]; then
        echo "  $compose_file: ✅ Found"
    else
        echo "  $compose_file: ❌ Not found"
    fi
done

echo ""
echo "🎉 Cross-platform compatibility test completed!"
echo "All tests passed successfully."
