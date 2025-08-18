#!/bin/bash

# Safely add chip-tool container to existing Pi setup
# This script only adds the chip-tool container without touching existing containers

set -e

echo "=== Adding chip-tool container to existing setup ==="

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

echo "1. Checking existing containers..."
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo ""
echo "2. Pulling ARM64 chip-tool image..."
docker pull --platform linux/arm64 arduino/chip-tool:latest

echo "3. Creating chip-tool config directory..."
mkdir -p chip-tool-config

echo "4. Stopping existing chip-tool container if it exists..."
docker stop chip_tool 2>/dev/null || true
docker rm chip_tool 2>/dev/null || true

echo "5. Starting chip-tool container..."
docker run -d \
  --name chip_tool \
  --platform linux/arm64 \
  --network host \
  -v /dev/bus/usb:/dev/bus/usb \
  -v $(pwd)/chip-tool-config:/chip-tool-config \
  -e CHIP_TOOL_CONFIG_PATH=/chip-tool-config \
  arduino/chip-tool:latest \
  sleep infinity

echo "6. Waiting for chip-tool container to start..."
sleep 5

echo "7. Checking if chip-tool container is running..."
if docker ps | grep -q chip_tool; then
    echo "✓ chip-tool container is running"
else
    echo "✗ chip-tool container failed to start"
    docker logs chip_tool || true
    exit 1
fi

echo "8. Testing chip-tool container..."
if docker exec chip_tool chip-tool --help > /dev/null 2>&1; then
    echo "✓ chip-tool is working in container"
else
    echo "✗ chip-tool test failed"
    docker exec chip_tool ls -la /usr/local/bin/ || true
    exit 1
fi

echo "9. Testing device connectivity..."
echo "Testing with device 4328ED19954E9DC0..."
if docker exec chip_tool chip-tool onoff read on-off 0x4328ED19954E9DC0 1 > /dev/null 2>&1; then
    echo "✓ Device connectivity test passed"
else
    echo "⚠ Device connectivity test failed (this might be normal if device is offline)"
fi

echo ""
echo "=== chip-tool container added successfully ==="
echo ""
echo "Current containers:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo ""
echo "To test chip-tool directly:"
echo "docker exec chip_tool chip-tool onoff toggle 0x4328ED19954E9DC0 1"
echo ""
echo "To view chip-tool logs:"
echo "docker logs chip_tool"
echo ""
echo "Next step: Update the web GUI to use the chip-tool container"
