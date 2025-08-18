#!/bin/bash
# Build ARM64 Docker image for Raspberry Pi

echo "🔨 Building ARM64 Docker Image for Pi"
echo "====================================="

# Check if buildx is available
if ! docker buildx version &> /dev/null; then
    echo "❌ Docker buildx not available. Please install Docker with buildx support."
    exit 1
fi

# Create multi-arch builder if it doesn't exist
if ! docker buildx inspect multiarch &> /dev/null; then
    echo "🔧 Creating multi-architecture builder..."
    docker buildx create --name multiarch --driver docker-container --use
    docker buildx inspect --bootstrap
fi

# Build for ARM64 (Pi architecture)
echo "🔨 Building for linux/arm64 (Raspberry Pi)..."
docker buildx build \
    --platform linux/arm64 \
    -f Dockerfile.simple \
    -t msh-commissioning-server:arm64 \
    --load \
    .

# Save the ARM64 image
echo "📦 Saving ARM64 image..."
docker save msh-commissioning-server:arm64 > msh-commissioning-server-arm64.tar

# Get image size
IMAGE_SIZE=$(du -h msh-commissioning-server-arm64.tar | cut -f1)
echo "   Image size: $IMAGE_SIZE"

echo "✅ ARM64 build complete!"
echo ""
echo "📊 Image information:"
docker images msh-commissioning-server:arm64 --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}"

echo ""
echo "🎯 Next steps:"
echo "   • Deploy to Pi: ./deploy_arm64_to_pi.sh"
echo "   • Test on Pi: ssh chregg@192.168.0.107 'docker load < msh-commissioning-server-arm64.tar'" 