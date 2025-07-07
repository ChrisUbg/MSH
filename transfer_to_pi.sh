#!/bin/bash

# Matter SDK Transfer Script
# Transfers built tools from development machine to Raspberry Pi

PI_USER="chregg"
PI_HOST="192.168.0.106"
PI_PATH="~/connectedhomeip"

echo "=== Matter SDK Transfer to Pi ==="
echo "Transferring from: $(pwd)/out/python_lib/"
echo "Transferring to: ${PI_USER}@${PI_HOST}:${PI_PATH}/out/python_lib/"
echo ""

# Check if build is complete
if [ ! -f "out/python_lib/chip-repl" ]; then
    echo "❌ ERROR: chip-repl not found!"
    echo "Build may not be complete. Please wait for build to finish."
    exit 1
fi

if [ ! -f "out/python_lib/chip-tool" ]; then
    echo "❌ ERROR: chip-tool not found!"
    echo "Build may not be complete. Please wait for build to finish."
    exit 1
fi

echo "✅ Build appears complete. Found:"
echo "   - chip-repl: $(ls -lh out/python_lib/chip-repl | awk '{print $5}')"
echo "   - chip-tool: $(ls -lh out/python_lib/chip-tool | awk '{print $5}')"
echo ""

# Check Pi connectivity
echo "=== Checking Pi Connectivity ==="
if ! ping -c 1 $PI_HOST > /dev/null 2>&1; then
    echo "❌ ERROR: Cannot reach Pi at $PI_HOST"
    echo "Please check network connectivity and Pi status."
    exit 1
fi
echo "✅ Pi is reachable"

# Create remote directory if needed
echo "=== Preparing Pi Directory ==="
ssh ${PI_USER}@${PI_HOST} "mkdir -p ${PI_PATH}/out/python_lib/"

# Transfer files
echo "=== Transferring Files ==="
echo "This may take a few minutes..."

# Method 1: Transfer entire directory (recommended)
echo "Transferring entire python_lib directory..."
rsync -avz --progress out/python_lib/ ${PI_USER}@${PI_HOST}:${PI_PATH}/out/python_lib/

if [ $? -eq 0 ]; then
    echo "✅ Transfer completed successfully!"
else
    echo "❌ Transfer failed. Trying alternative method..."
    
    # Method 2: Transfer key files individually
    echo "Transferring key files individually..."
    scp out/python_lib/chip-repl ${PI_USER}@${PI_HOST}:${PI_PATH}/out/python_lib/
    scp out/python_lib/chip-tool ${PI_USER}@${PI_HOST}:${PI_PATH}/out/python_lib/
    
    if [ $? -eq 0 ]; then
        echo "✅ Individual file transfer completed!"
    else
        echo "❌ Transfer failed completely."
        exit 1
    fi
fi

# Verify transfer
echo "=== Verifying Transfer ==="
ssh ${PI_USER}@${PI_HOST} "cd ${PI_PATH} && echo 'Checking transferred files:' && ls -la out/python_lib/chip-repl out/python_lib/chip-tool 2>/dev/null || echo 'Files not found'"

# Make files executable on Pi
echo "=== Setting Permissions ==="
ssh ${PI_USER}@${PI_HOST} "cd ${PI_PATH} && chmod +x out/python_lib/chip-repl out/python_lib/chip-tool"

# Test tools on Pi
echo "=== Testing Tools on Pi ==="
echo "Testing chip-repl:"
ssh ${PI_USER}@${PI_HOST} "cd ${PI_PATH} && ./out/python_lib/chip-repl --help | head -5"

echo "Testing chip-tool:"
ssh ${PI_USER}@${PI_HOST} "cd ${PI_PATH} && ./out/python_lib/chip-tool --help | head -5"

echo ""
echo "🎉 Transfer Complete! 🎉"
echo ""
echo "Next Steps on Pi:"
echo "1. SSH to Pi: ssh ${PI_USER}@${PI_HOST}"
echo "2. Navigate to: cd ${PI_PATH}"
echo "3. Test commissioning: ./out/python_lib/chip-repl"
echo "4. Test device control: ./out/python_lib/chip-tool"
echo ""
echo "The Pi now has the full Matter SDK tools for BLE commissioning!" 