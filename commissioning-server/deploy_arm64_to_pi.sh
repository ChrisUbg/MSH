#!/bin/bash
# Deploy ARM64 MSH Commissioning Server to Raspberry Pi

PI_HOST=${1:-"chregg@192.168.0.107"}
PI_PATH="/opt/msh-commissioning-server"

echo "🚀 Deploying ARM64 MSH Commissioning Server to Pi"
echo "================================================="
echo "Target: $PI_HOST"
echo "Path: $PI_PATH"

# Check if ARM64 image exists
if [ ! -f "msh-commissioning-server-arm64.tar" ]; then
    echo "❌ ARM64 image not found. Building it first..."
    ./build_for_pi.sh
fi

# Get image size
IMAGE_SIZE=$(du -h msh-commissioning-server-arm64.tar | cut -f1)
echo "📦 ARM64 image size: $IMAGE_SIZE"

# Copy to Pi
echo "📤 Copying ARM64 image to Pi..."
scp msh-commissioning-server-arm64.tar $PI_HOST:~/

# Deploy on Pi
echo "🔧 Deploying on Pi..."
ssh $PI_HOST << 'EOF'
echo "🧪 Setting up ARM64 MSH Commissioning Server on Pi..."

# Create directory
sudo mkdir -p /opt/msh-commissioning-server
cd /opt/msh-commissioning-server

# Load ARM64 Docker image
echo "📦 Loading ARM64 Docker image..."
docker load < ~/msh-commissioning-server-arm64.tar

# Create docker-compose.yml with proper permissions
sudo tee docker-compose.yml << 'DOCKER_COMPOSE'
version: '3.8'
services:
  msh-commissioning-server:
    image: msh-commissioning-server:arm64
    container_name: msh-commissioning-server
    restart: unless-stopped
    network_mode: host
    privileged: true
    user: root
    volumes:
      - /dev/bus/usb:/dev/bus/usb
      - /sys/class/bluetooth:/sys/class/bluetooth
      - /sys/devices:/sys/devices
      - /var/run/dbus:/var/run/dbus
      - /home/chregg/.ssh:/root/.ssh:ro
      - ./data:/app/data
    environment:
      - MSH_CONFIG_PATH=/app/config.yaml
      - MSH_DATA_PATH=/app/data
      - DBUS_SESSION_BUS_ADDRESS=unix:path=/var/run/dbus/system_bus_socket
    healthcheck:
      test: ["CMD", "wget", "--no-verbose", "--tries=1", "--spider", "http://localhost:8888/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
DOCKER_COMPOSE

# Create systemd service
sudo tee /etc/systemd/system/msh-commissioning-server.service > /dev/null << 'SYSTEMD'
[Unit]
Description=MSH Commissioning Server
After=docker.service
Requires=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=/opt/msh-commissioning-server
ExecStart=/usr/bin/docker-compose up -d
ExecStop=/usr/bin/docker-compose down
TimeoutStartSec=0

[Install]
WantedBy=multi-user.target
SYSTEMD

# Create data directory with proper permissions
sudo mkdir -p data
sudo chown -R $USER:$USER data

# Test Bluetooth
echo "🔍 Testing Bluetooth..."
if ! sudo hciconfig hci0 up 2>/dev/null; then
    echo "⚠️  Bluetooth adapter not working"
    sudo hciconfig
else
    echo "✅ Bluetooth adapter working"
fi

# Start service
echo "🚀 Starting service..."
sudo systemctl daemon-reload
sudo systemctl enable msh-commissioning-server
sudo systemctl start msh-commissioning-server

# Wait a bit for service to start
sleep 5

# Wait for startup
echo "⏳ Waiting for startup..."
sleep 10

# Check status
echo "📊 Service status:"
sudo systemctl status msh-commissioning-server --no-pager

# Test HTTP endpoint
echo "🌐 Testing HTTP endpoint..."
if curl -s http://localhost:8888/docs > /dev/null; then
    echo "✅ Server responding"
    echo "   Web interface: http://$(hostname -I | awk '{print $1}'):8888"
else
    echo "❌ Server not responding"
fi

# Show logs
echo "📋 Recent logs:"
docker logs msh-commissioning-server --tail 20

echo "✅ Deployment complete!"
echo ""
echo "📋 Management commands:"
echo "   Start:   sudo systemctl start msh-commissioning-server"
echo "   Stop:    sudo systemctl stop msh-commissioning-server"
echo "   Status:  sudo systemctl status msh-commissioning-server"
echo "   Logs:    docker logs msh-commissioning-server"
echo "   Restart: sudo systemctl restart msh-commissioning-server"
EOF

# Cleanup local files
echo "🧹 Cleaning up local files..."
rm msh-commissioning-server-arm64.tar

echo "✅ Deployment complete!"
echo ""
echo "🌐 Access your server at:"
echo "   http://192.168.0.107:8888"
echo ""
echo "📋 To check status:"
echo "   ssh $PI_HOST 'sudo systemctl status msh-commissioning-server'" 