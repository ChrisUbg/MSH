#!/bin/bash

# Simple launcher for nRF Connect for Desktop with J-Link support

echo "🚀 Launching nRF Connect for Desktop with J-Link V8.18..."

# Set environment variables
export LD_LIBRARY_PATH=/opt/JLink_Linux_V818_x86_64:$LD_LIBRARY_PATH
export JLINK_PATH=/opt/JLink_Linux_V818_x86_64
export JLINK_GDB_SERVER_PATH=/opt/JLink_Linux_V818_x86_64/JLinkGDBServerExe
export PATH=/opt/JLink_Linux_V818_x86_64:$PATH

# Kill any existing instances
pkill -f nrfconnect 2>/dev/null || true

# Start nRF Connect for Desktop
cd ~/Downloads
./nrfconnect-5.2.0-x86_64.AppImage --no-sandbox 