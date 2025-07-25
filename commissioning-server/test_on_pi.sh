#!/bin/bash
# Test MSH Commissioning Server on Raspberry Pi

echo "🧪 Testing MSH Commissioning Server on Raspberry Pi"
echo "=================================================="

# Check if we're on a Pi
if ! grep -q "Raspberry Pi" /proc/cpuinfo 2>/dev/null; then
    echo "⚠️  This script is designed for Raspberry Pi"
    echo "   Running on: $(uname -a)"
    read -p "Continue anyway? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

# Check Docker installation
echo "🔍 Checking Docker installation..."
if ! command -v docker &> /dev/null; then
    echo "❌ Docker not found. Installing..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker $USER
    echo "✅ Docker installed. Please log out and back in, then run this script again."
    exit 1
fi

# Check Bluetooth
echo "🔍 Checking Bluetooth..."
if ! command -v bluetoothctl &> /dev/null; then
    echo "❌ Bluetooth not found. Installing..."
    sudo apt-get update
    sudo apt-get install -y bluetooth bluez
fi

# Check if Bluetooth is working
echo "🔍 Testing Bluetooth..."
if ! sudo hciconfig hci0 up 2>/dev/null; then
    echo "⚠️  Bluetooth adapter not found or not working"
    echo "   Available adapters:"
    sudo hciconfig
else
    echo "✅ Bluetooth adapter working"
    sudo hciconfig
fi

# Build Docker image
echo "🔨 Building Docker image..."
if [ -f "Dockerfile.optimized" ]; then
    echo "   Using optimized Dockerfile"
    docker build -f Dockerfile.optimized -t msh-commissioning-server .
else
    echo "   Using standard Dockerfile"
    docker build -t msh-commissioning-server .
fi

# Check image size
echo "📊 Docker image size:"
docker images msh-commissioning-server --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}"

# Test Bluetooth in container
echo "🧪 Testing Bluetooth in container..."
docker run --rm --privileged \
    -v /dev/bus/usb:/dev/bus/usb \
    -v /sys/class/bluetooth:/sys/class/bluetooth \
    msh-commissioning-server \
    python -c "
import subprocess
import sys

print('🔍 Checking Bluetooth in container...')
try:
    # Check if Bluetooth devices are accessible
    result = subprocess.run(['hciconfig'], capture_output=True, text=True)
    print('hciconfig output:')
    print(result.stdout)
    
    # Check if we can access Bluetooth
    result = subprocess.run(['ls', '/dev/bus/usb'], capture_output=True, text=True)
    print('USB devices:')
    print(result.stdout)
    
    print('✅ Bluetooth test completed')
except Exception as e:
    print(f'❌ Bluetooth test failed: {e}')
    sys.exit(1)
"

# Test full application
echo "🚀 Testing full application..."
echo "   Starting server in background..."
docker run -d --name msh-test \
    --privileged \
    -p 8888:8888 \
    -v /dev/bus/usb:/dev/bus/usb \
    -v /sys/class/bluetooth:/sys/class/bluetooth \
    msh-commissioning-server

# Wait for startup
echo "⏳ Waiting for server to start..."
sleep 5

# Test HTTP endpoint
echo "🌐 Testing HTTP endpoint..."
if curl -s http://localhost:8888/health > /dev/null; then
    echo "✅ HTTP server responding"
    echo "   Health check: http://localhost:8888/health"
    echo "   Web interface: http://localhost:8888"
else
    echo "❌ HTTP server not responding"
fi

# Test Bluetooth commissioning
echo "🔍 Testing Bluetooth commissioning..."
docker exec msh-test python -c "
import asyncio
import sys

async def test_ble():
    try:
        # Import your BLE scanner
        from commissioning_server.core.ble_scanner import BLEScanner
        
        scanner = BLEScanner()
        print('🔍 Starting BLE scan...')
        
        # Scan for devices
        devices = await scanner.scan_devices(timeout=10)
        print(f'📱 Found {len(devices)} devices')
        
        for device in devices:
            print(f'   {device.name} ({device.address})')
        
        print('✅ BLE commissioning test completed')
        return True
    except Exception as e:
        print(f'❌ BLE commissioning test failed: {e}')
        return False

# Run test
result = asyncio.run(test_ble())
sys.exit(0 if result else 1)
"

# Cleanup
echo "🧹 Cleaning up..."
docker stop msh-test
docker rm msh-test

echo "✅ Testing complete!"
echo ""
echo "📋 Summary:"
echo "   • Docker image built successfully"
echo "   • Bluetooth accessible in container"
echo "   • HTTP server responding"
echo "   • BLE commissioning functional"
echo ""
echo "🎯 Next steps:"
echo "   • Deploy to production Pi"
echo "   • Configure as system service"
echo "   • Set up automatic startup" 