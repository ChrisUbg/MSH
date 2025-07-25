#!/bin/bash

# Script to start nRF Connect for Desktop with proper J-Link configuration

echo "🔧 Setting up nRF Connect for Desktop with J-Link V8.18..."

# Kill any existing nRF Connect instances
echo "🔄 Stopping any existing nRF Connect instances..."
pkill -f nrfconnect || true
sleep 2

# Set J-Link environment variables
export LD_LIBRARY_PATH=/opt/JLink_Linux_V818_x86_64:$LD_LIBRARY_PATH
export JLINK_PATH=/opt/JLink_Linux_V818_x86_64
export JLINK_GDB_SERVER_PATH=/opt/JLink_Linux_V818_x86_64/JLinkGDBServerExe

# Verify J-Link installation
echo "📋 Checking J-Link installation..."
if [ -f "/opt/JLink_Linux_V818_x86_64/JLinkExe" ]; then
    echo "✅ J-Link V8.18 found at /opt/JLink_Linux_V818_x86_64/"
    /opt/JLink_Linux_V818_x86_64/JLinkExe --help | head -5
else
    echo "❌ J-Link not found at expected location"
    exit 1
fi

# Check Nordic dongle
echo "🔍 Checking Nordic dongle..."
if lsusb | grep -q "Nordic"; then
    echo "✅ Nordic dongle detected:"
    lsusb | grep Nordic
else
    echo "❌ Nordic dongle not found"
    exit 1
fi

# Create a wrapper script for the AppImage
echo "📝 Creating AppImage wrapper with J-Link environment..."
cat > /tmp/nrfconnect_wrapper.sh << 'EOF'
#!/bin/bash
export LD_LIBRARY_PATH=/opt/JLink_Linux_V818_x86_64:$LD_LIBRARY_PATH
export JLINK_PATH=/opt/JLink_Linux_V818_x86_64
export JLINK_GDB_SERVER_PATH=/opt/JLink_Linux_V818_x86_64/JLinkGDBServerExe
export PATH=/opt/JLink_Linux_V818_x86_64:$PATH

echo "🚀 Starting nRF Connect for Desktop with J-Link environment..."
echo "   - J-Link Path: $JLINK_PATH"
echo "   - Library Path: $LD_LIBRARY_PATH"

exec "$@"
EOF

chmod +x /tmp/nrfconnect_wrapper.sh

echo "🚀 Starting nRF Connect for Desktop..."
echo "   - J-Link Path: $JLINK_PATH"
echo "   - Library Path: $LD_LIBRARY_PATH"

# Start nRF Connect for Desktop with the wrapper
/tmp/nrfconnect_wrapper.sh ~/Downloads/nrfconnect-5.2.0-x86_64.AppImage --no-sandbox 