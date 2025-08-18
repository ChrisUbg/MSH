#!/bin/bash

# OpenThread Border Router Deployment Script for Raspberry Pi
# Deploys OTBR using Docker with hardware passthrough

set -e

echo "🚀 Deploying OpenThread Border Router to Pi..."

# Configuration
PI_IP="192.168.0.107"  # Your Pi's IP
OTBR_COMPOSE_FILE="docker/docker-compose-otbr.yml"

# Check if we're on the Pi (allow other hostnames)
if [[ "$(hostname)" != "raspberrypi" ]]; then
    echo "⚠️  This script is designed for Raspberry Pi"
    echo "   Current hostname: $(hostname)"
    echo "   Continuing with deployment..."
fi

echo "📋 Checking prerequisites..."

# Check Docker
if ! command -v docker &> /dev/null; then
    echo "❌ Docker not found. Installing..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker $USER
    echo "✅ Docker installed. Please log out and back in, then run this script again."
    exit 1
fi

# Check Docker Compose
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose not found. Installing..."
    sudo apt-get update
    sudo apt-get install -y docker-compose-plugin
    echo "✅ Docker Compose installed."
fi

# Check Nordic dongle
echo "🔍 Checking for Nordic nRF52840 dongle..."
if lsusb | grep -q "Nordic"; then
    echo "✅ Nordic dongle detected"
    lsusb | grep Nordic
else
    echo "⚠️  Nordic dongle not detected. Please plug in the dongle and run again."
    exit 1
fi

# Create necessary directories
echo "📁 Creating directories..."
sudo mkdir -p /var/lib/thread
sudo chown $USER:$USER /var/lib/thread

# Stop existing services that might conflict
echo "🛑 Stopping conflicting services..."
sudo systemctl stop otbr-agent otbr-web 2>/dev/null || true
sudo systemctl disable otbr-agent otbr-web 2>/dev/null || true

# Pull OTBR image
echo "📥 Pulling OpenThread Border Router image..."
docker pull openthread/otbr:latest

# Deploy OTBR
echo "🚀 Starting OpenThread Border Router..."
cd /home/chris/RiderProjects/MSH
docker-compose -f $OTBR_COMPOSE_FILE up -d

# Wait for service to start
echo "⏳ Waiting for OTBR to start..."
sleep 10

# Check service status
echo "📊 Checking service status..."
docker-compose -f $OTBR_COMPOSE_FILE ps

# Test web interface
echo "🌐 Testing web interface..."
if curl -s http://localhost:9999 > /dev/null; then
    echo "✅ Web interface accessible at http://$PI_IP:9999"
else
    echo "⚠️  Web interface not accessible yet. Check logs:"
    docker-compose -f $OTBR_COMPOSE_FILE logs otbr
fi

# Test REST API
echo "🔌 Testing REST API..."
if curl -s http://localhost:8081 > /dev/null; then
    echo "✅ REST API accessible at http://$PI_IP:8081"
else
    echo "⚠️  REST API not accessible yet."
fi

echo ""
echo "🎉 OpenThread Border Router deployment complete!"
echo ""
echo "📋 Access Points:"
echo "   Web Interface: http://$PI_IP:9999"
echo "   REST API: http://$PI_IP:8081"
echo ""
echo "📋 Management Commands:"
echo "   View logs: docker-compose -f $OTBR_COMPOSE_FILE logs -f otbr"
echo "   Stop service: docker-compose -f $OTBR_COMPOSE_FILE down"
echo "   Restart service: docker-compose -f $OTBR_COMPOSE_FILE restart"
echo ""
echo "🔧 Next Steps:"
echo "   1. Access web interface to configure Thread network"
echo "   2. Commission Thread devices (like arre temperature sensor)"
echo "   3. Integrate with your MSH commissioning server" 