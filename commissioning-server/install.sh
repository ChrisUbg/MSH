#!/bin/bash
# MSH Commissioning Server Installation Script for Ubuntu

set -e

echo "🚀 Installing MSH Commissioning Server..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if running as root
if [[ $EUID -eq 0 ]]; then
   print_error "This script should not be run as root"
   exit 1
fi

# Update package list
print_status "Updating package list..."
sudo apt-get update

# Install system dependencies
print_status "Installing system dependencies..."
sudo apt-get install -y \
    python3.10 \
    python3-pip \
    python3-venv \
    bluetooth \
    bluez \
    libbluetooth-dev \
    libudev-dev \
    build-essential \
    cmake \
    ninja-build \
    clang \
    git \
    curl \
    wget \
    jq

# Install Matter SDK dependencies
print_status "Installing Matter SDK dependencies..."
sudo apt-get install -y \
    pkg-config \
    libssl-dev \
    libdbus-1-dev \
    libglib2.0-dev \
    libavahi-client-dev \
    libreadline-dev \
    libncurses-dev \
    libudev-dev \
    libasound2-dev \
    libavcodec-dev \
    libavformat-dev \
    libswscale-dev \
    libavutil-dev \
    libavfilter-dev \
    libavdevice-dev \
    libpostproc-dev \
    libswresample-dev \
    libavresample-dev

# Create virtual environment
print_status "Creating Python virtual environment..."
python3 -m venv venv
source venv/bin/activate

# Upgrade pip
print_status "Upgrading pip..."
pip install --upgrade pip

# Install Python dependencies
print_status "Installing Python dependencies..."
pip install -r requirements.txt

# Install the commissioning server
print_status "Installing MSH Commissioning Server..."
pip install -e .

# Create configuration directory
print_status "Creating configuration directory..."
mkdir -p ~/.msh

# Create default configuration
print_status "Creating default configuration..."
cat > ~/.msh/config.yaml << EOF
server:
  host: "0.0.0.0"
  port: 8080
  debug: false

matter:
  sdk_path: "/usr/local/matter-sdk"
  chip_tool_path: "/usr/local/bin/chip-tool"
  chip_repl_path: "/usr/local/bin/chip-repl"
  fabric_id: "1"
  node_id: "112233"

bluetooth:
  adapter: "hci0"
  timeout: 30
  scan_duration: 10

storage:
  type: "sqlite"
  path: "~/.msh/credentials.db"

security:
  api_key_required: false
  allowed_hosts: ["192.168.0.0/24"]
  encrypt_credentials: true

pi:
  default_ip: "192.168.0.104"
  default_user: "chregg"
  ssh_key_path: "~/.ssh/id_ed25519"
EOF

# Set up Bluetooth permissions
print_status "Setting up Bluetooth permissions..."
sudo usermod -a -G bluetooth $USER

# Create systemd service (optional)
read -p "Do you want to install as a systemd service? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    print_status "Creating systemd service..."
    sudo tee /etc/systemd/system/msh-commissioning-server.service > /dev/null << EOF
[Unit]
Description=MSH Commissioning Server
After=network.target bluetooth.service

[Service]
Type=simple
User=$USER
WorkingDirectory=$(pwd)
Environment=PATH=$(pwd)/venv/bin
ExecStart=$(pwd)/venv/bin/msh-commissioning-server --host 0.0.0.0 --port 8080
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
EOF

    # Enable and start service
    sudo systemctl daemon-reload
    sudo systemctl enable msh-commissioning-server
    print_status "Service installed and enabled"
fi

# Create desktop shortcut (optional)
read -p "Do you want to create a desktop shortcut? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    print_status "Creating desktop shortcut..."
    cat > ~/Desktop/msh-commissioning-server.desktop << EOF
[Desktop Entry]
Version=1.0
Type=Application
Name=MSH Commissioning Server
Comment=Native PC application for Matter device commissioning
Exec=$(pwd)/venv/bin/msh-commissioning-server --host 0.0.0.0 --port 8080
Icon=network-server
Terminal=true
Categories=Network;System;
EOF

    chmod +x ~/Desktop/msh-commissioning-server.desktop
    print_status "Desktop shortcut created"
fi

# Print installation summary
echo
print_status "Installation completed successfully!"
echo
echo "📋 Installation Summary:"
echo "  • Python virtual environment: $(pwd)/venv"
echo "  • Configuration: ~/.msh/config.yaml"
echo "  • Database: ~/.msh/credentials.db"
echo "  • Web interface: http://localhost:8080"
echo "  • API documentation: http://localhost:8080/docs"
echo
echo "🚀 To start the server:"
echo "  • Manual: source venv/bin/activate && msh-commissioning-server"
echo "  • Service: sudo systemctl start msh-commissioning-server"
echo "  • Desktop: Double-click the desktop shortcut"
echo
echo "📖 Next steps:"
echo "  1. Configure your Raspberry Pi IP in ~/.msh/config.yaml"
echo "  2. Set up SSH keys for Pi communication"
echo "  3. Install Matter SDK tools (chip-tool, chip-repl)"
echo "  4. Test with a Matter device"
echo
print_warning "Note: You may need to log out and back in for Bluetooth group changes to take effect" 