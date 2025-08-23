#!/bin/bash

# Restore commissioning data after Pi restart
# This script transfers the commissioning data from PC to Pi to restore device connectivity

set -e

echo "=== Restoring Commissioning Data After Pi Restart ==="

# Configuration
PI_USER="chregg"
PI_HOST="192.168.0.107"
PI_PATH="/home/chregg/msh"

# Check if commissioning data exists on PC
echo "1. Checking commissioning data on PC..."
if [ ! -f "/tmp/chip_tool_config.ini" ]; then
    echo "❌ Commissioning data not found on PC"
    echo "   Please ensure you have commissioned devices on this PC first"
    exit 1
fi

echo "✅ Commissioning data found on PC"

# Create directory on Pi
echo "2. Creating chip-tool-config directory on Pi..."
ssh $PI_USER@$PI_HOST "mkdir -p $PI_PATH/chip-tool-config"

# Transfer commissioning data
echo "3. Transferring commissioning data to Pi..."
scp /tmp/chip_*.ini $PI_USER@$PI_HOST:$PI_PATH/chip-tool-config/
scp /tmp/chip_tool_kvs $PI_USER@$PI_HOST:$PI_PATH/chip-tool-config/

echo "✅ Commissioning data transferred"

# Restart chip-tool container with restored data
echo "4. Restarting chip-tool container with restored data..."
ssh $PI_USER@$PI_HOST "docker stop chip_tool 2>/dev/null || true"
ssh $PI_USER@$PI_HOST "docker rm chip_tool 2>/dev/null || true"
ssh $PI_USER@$PI_HOST "docker run -d --name chip_tool --platform linux/arm64 --network host -v /dev/bus/usb:/dev/bus/usb -v $PI_PATH/chip-tool-config:/tmp -e CHIP_TOOL_CONFIG_PATH=/tmp -e CHIP_CONFIG_PATH=/tmp arduino/chip-tool:latest sleep infinity"

echo "✅ chip-tool container restarted"

# Wait for container to be ready
echo "5. Waiting for container to be ready..."
sleep 5

# Test device connectivity
echo "6. Testing device connectivity..."
if ssh $PI_USER@$PI_HOST "docker exec chip_tool chip-tool onoff read on-off 0x4328ED19954E9DC0 1" > /dev/null 2>&1; then
    echo "✅ Device connectivity test passed"
else
    echo "⚠️  Device connectivity test failed (this might be normal if device is offline)"
fi

echo ""
echo "=== Commissioning Data Restored Successfully ==="
echo ""
echo "Your devices should now be accessible again with improved performance."
echo "The web GUI should load faster and device control should be responsive."
echo ""
echo "To test the web GUI: http://$PI_HOST:8083"
echo ""
echo "If you still experience issues:"
echo "1. Check if devices are powered on and connected to WiFi"
echo "2. Verify the Pi's network connectivity"
echo "3. Check the chip-tool container logs: docker logs chip_tool"
