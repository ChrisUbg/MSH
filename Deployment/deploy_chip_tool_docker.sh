#!/bin/bash

# Deploy chip-tool Docker integration on Raspberry Pi
# This script sets up the ARM64 chip-tool container and updates the web GUI

set -e

echo "=== Deploying chip-tool Docker integration on Pi ==="

# Check if we're on the Pi (should have ARM64 architecture)
if [[ $(uname -m) != "aarch64" ]]; then
    echo "ERROR: This script should be run on the Raspberry Pi (ARM64)"
    echo "Current architecture: $(uname -m)"
    exit 1
fi

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "ERROR: Docker is not running"
    exit 1
fi

echo "1. Stopping existing containers..."
docker-compose -f docker-compose-matter-integration.yml down || true

echo "2. Pulling ARM64 chip-tool image..."
docker pull --platform linux/arm64 arduino/chip-tool:latest

echo "3. Creating chip-tool config directory..."
mkdir -p chip-tool-config

echo "4. Starting the updated Docker services..."
docker-compose -f docker-compose-matter-integration.yml up -d

echo "5. Waiting for services to start..."
sleep 10

echo "6. Checking if chip-tool container is running..."
if docker ps | grep -q chip_tool; then
    echo "✓ chip-tool container is running"
else
    echo "✗ chip-tool container failed to start"
    docker logs chip_tool || true
    exit 1
fi

echo "7. Testing chip-tool container..."
if docker exec chip_tool chip-tool --help > /dev/null 2>&1; then
    echo "✓ chip-tool is working in container"
else
    echo "✗ chip-tool test failed"
    docker exec chip_tool ls -la /usr/local/bin/ || true
    exit 1
fi

echo "8. Testing device connectivity..."
echo "Testing with device 4328ED19954E9DC0..."
if docker exec chip_tool chip-tool onoff read on-off 0x4328ED19954E9DC0 1 > /dev/null 2>&1; then
    echo "✓ Device connectivity test passed"
else
    echo "⚠ Device connectivity test failed (this might be normal if device is offline)"
fi

echo "9. Checking web GUI..."
if curl -s http://localhost:8083 > /dev/null; then
    echo "✓ Web GUI is accessible"
else
    echo "✗ Web GUI is not accessible"
    docker logs msh_web || true
fi

echo ""
echo "=== Deployment Complete ==="
echo ""
echo "Services running:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo ""
echo "To test device control from web GUI:"
echo "1. Open http://localhost:8083 in your browser"
echo "2. Go to Device Management"
echo "3. Click on a device to see details and control options"
echo ""
echo "To test chip-tool directly:"
echo "docker exec chip_tool chip-tool onoff toggle 0x4328ED19954E9DC0 1"
echo ""
echo "To view logs:"
echo "docker logs msh_web"
echo "docker logs chip_tool"
