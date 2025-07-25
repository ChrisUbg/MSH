#!/bin/bash

# Extract and run nRF Connect for Desktop with J-Link support

echo "🔧 Extracting nRF Connect for Desktop AppImage..."

# Create extraction directory
mkdir -p ~/nrfconnect-extracted

# Extract the AppImage
cd ~/Downloads
./nrfconnect-5.2.0-x86_64.AppImage --appimage-extract-and-run --no-sandbox &
sleep 5

# Kill the extracted instance
pkill -f nrfconnect

# Now run the extracted version with proper environment
echo "🚀 Running extracted nRF Connect with J-Link environment..."

export LD_LIBRARY_PATH=/opt/JLink_Linux_V818_x86_64:/usr/lib:$LD_LIBRARY_PATH
export JLINK_PATH=/opt/JLink_Linux_V818_x86_64
export JLINK_GDB_SERVER_PATH=/opt/JLink_Linux_V818_x86_64/JLinkGDBServerExe
export PATH=/opt/JLink_Linux_V818_x86_64:$PATH

# Run the extracted AppImage
cd ~/Downloads
./nrfconnect-5.2.0-x86_64.AppImage --no-sandbox 