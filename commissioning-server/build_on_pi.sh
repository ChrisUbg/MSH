#!/bin/bash
# Build MSH Commissioning Server directly on Pi

PI_HOST=${1:-"chregg@192.168.0.107"}

echo "🔨 Building Docker image directly on Pi"
echo "======================================"
echo "Target: $PI_HOST"

# Copy source files to Pi
echo "📤 Copying source files to Pi..."
rsync -av --exclude='venv' --exclude='__pycache__' --exclude='*.pyc' --exclude='.git' . $PI_HOST:~/msh-commissioning-server/

# Build on Pi
echo "🔧 Building on Pi..."
ssh $PI_HOST << 'EOF'
cd ~/msh-commissioning-server

echo "🧪 Building Docker image on Pi..."

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker not found. Installing..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker $USER
    echo "✅ Docker installed. Please log out and back in, then run this script again."
    exit 1
fi

# Build the image for ARM64
echo "🔨 Building optimized Docker image..."
docker build -f Dockerfile.optimized -t msh-commissioning-server .

# Check image size
echo "📊 Docker image size:"
docker images msh-commissioning-server --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}"

# Test the image
echo "🧪 Testing Docker image..."
docker run --rm --privileged -p 8888:8888 msh-commissioning-server &
CONTAINER_PID=$!

# Wait for startup
sleep 10

# Test HTTP endpoint
echo "🌐 Testing HTTP endpoint..."
if curl -s http://localhost:8888/docs > /dev/null; then
    echo "✅ Server responding"
    echo "   Web interface: http://$(hostname -I | awk '{print $1}'):8888"
else
    echo "❌ Server not responding"
fi

# Stop test container
kill $CONTAINER_PID
docker stop $(docker ps -q --filter ancestor=msh-commissioning-server) 2>/dev/null || true

echo "✅ Build and test complete!"
EOF

echo "✅ Build script completed!"
echo ""
echo "🎯 Next steps:"
echo "   • Deploy to production: ./deploy_to_pi.sh"
echo "   • Test BLE functionality on Pi"
echo "   • Access web interface: http://192.168.0.107:8888" 