#!/usr/bin/env python3
"""
Build script for creating portable PC Commissioning Server
"""

import os
import sys
import subprocess
import shutil
from pathlib import Path

def create_portable_package():
    """Create a portable package with PyInstaller"""
    
    print("🔧 Building Portable PC Commissioning Server...")
    
    # Create build directory
    build_dir = Path("dist/msh-commissioning-server")
    build_dir.mkdir(parents=True, exist_ok=True)
    
    # PyInstaller command
    cmd = [
        "pyinstaller",
        "--onefile",                    # Single executable
        "--windowed",                   # No console window (optional)
        "--name=msh-commissioning-server",
        "--add-data=config.yaml;.",     # Include config
        "--add-data=templates;templates", # Include web templates
        "--add-data=static;static",     # Include static files
        "--hidden-import=uvicorn.logging",
        "--hidden-import=uvicorn.loops",
        "--hidden-import=uvicorn.loops.auto",
        "--hidden-import=uvicorn.protocols",
        "--hidden-import=uvicorn.protocols.http",
        "--hidden-import=uvicorn.protocols.http.auto",
        "--hidden-import=uvicorn.protocols.websockets",
        "--hidden-import=uvicorn.protocols.websockets.auto",
        "--hidden-import=uvicorn.lifespan.on",
        "--hidden-import=uvicorn.lifespan.off",
        "--hidden-import=uvicorn.lifespan.auto",
        "main.py"
    ]
    
    print("📦 Running PyInstaller...")
    subprocess.run(cmd, check=True)
    
    # Copy additional files
    print("📋 Copying additional files...")
    files_to_copy = [
        "config.yaml",
        "README.md",
        "LICENSE"
    ]
    
    for file in files_to_copy:
        if os.path.exists(file):
            shutil.copy2(file, build_dir)
    
    # Create launcher script
    create_launcher_script(build_dir)
    
    print("✅ Portable package created in dist/msh-commissioning-server/")

def create_launcher_script(build_dir):
    """Create a launcher script for easy startup"""
    
    launcher_content = """#!/bin/bash
# MSH Commissioning Server Launcher
# This script starts the commissioning server with proper configuration

echo "🚀 Starting MSH Commissioning Server..."

# Check if executable exists
if [ ! -f "./msh-commissioning-server" ]; then
    echo "❌ Error: msh-commissioning-server executable not found"
    exit 1
fi

# Set environment variables
export MSH_CONFIG_PATH="./config.yaml"
export MSH_DATA_PATH="./data"

# Create data directory if it doesn't exist
mkdir -p ./data

# Start the server
./msh-commissioning-server --port 8888 --host 0.0.0.0

echo "✅ Server stopped"
"""
    
    launcher_path = build_dir / "start-server.sh"
    with open(launcher_path, 'w') as f:
        f.write(launcher_content)
    
    # Make executable
    os.chmod(launcher_path, 0o755)
    
    # Create Windows batch file
    batch_content = """@echo off
REM MSH Commissioning Server Launcher
REM This script starts the commissioning server with proper configuration

echo 🚀 Starting MSH Commissioning Server...

REM Check if executable exists
if not exist "msh-commissioning-server.exe" (
    echo ❌ Error: msh-commissioning-server.exe not found
    pause
    exit /b 1
)

REM Set environment variables
set MSH_CONFIG_PATH=config.yaml
set MSH_DATA_PATH=data

REM Create data directory if it doesn't exist
if not exist "data" mkdir data

REM Start the server
msh-commissioning-server.exe --port 8888 --host 0.0.0.0

echo ✅ Server stopped
pause
"""
    
    batch_path = build_dir / "start-server.bat"
    with open(batch_path, 'w') as f:
        f.write(batch_content)

def create_installer():
    """Create a simple installer script"""
    
    installer_content = """#!/bin/bash
# MSH Commissioning Server Installer

echo "🔧 Installing MSH Commissioning Server..."

# Create installation directory
INSTALL_DIR="/opt/msh-commissioning-server"
sudo mkdir -p $INSTALL_DIR

# Copy files
sudo cp -r * $INSTALL_DIR/

# Create systemd service
sudo tee /etc/systemd/system/msh-commissioning-server.service > /dev/null <<EOF
[Unit]
Description=MSH Commissioning Server
After=network.target

[Service]
Type=simple
User=root
WorkingDirectory=$INSTALL_DIR
ExecStart=$INSTALL_DIR/msh-commissioning-server --port 8888 --host 0.0.0.0
Restart=always

[Install]
WantedBy=multi-user.target
EOF

# Enable and start service
sudo systemctl daemon-reload
sudo systemctl enable msh-commissioning-server
sudo systemctl start msh-commissioning-server

echo "✅ Installation complete!"
echo "🌐 Server running at: http://localhost:8888"
echo "📋 Service status: sudo systemctl status msh-commissioning-server"
"""
    
    with open("install.sh", 'w') as f:
        f.write(installer_content)
    
    os.chmod("install.sh", 0o755)

if __name__ == "__main__":
    create_portable_package()
    create_installer()
    print("\n🎉 Build complete!")
    print("📦 Portable package: dist/msh-commissioning-server/")
    print("🔧 Installer: install.sh") 