#!/bin/bash
# Build multi-architecture Docker image for MSH Commissioning Server

echo "🔨 Building Multi-Architecture Docker Image"
echo "=========================================="

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

# Build for both architectures
echo "🔨 Building for linux/amd64 and linux/arm64..."
docker buildx build \
    --platform linux/amd64,linux/arm64 \
    -f Dockerfile.optimized \
    -t msh-commissioning-server:latest \
    --push \
    .

# Also build locally for current architecture
echo "🔨 Building local image for current architecture..."
docker buildx build \
    --platform linux/amd64 \
    -f Dockerfile.optimized \
    -t msh-commissioning-server:latest \
    --load \
    .

echo "✅ Multi-architecture build complete!"
echo ""
echo "📊 Image information:"
docker images msh-commissioning-server --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}"

echo ""
echo "🎯 Next steps:"
echo "   • Test locally: docker run -p 8888:8888 --privileged msh-commissioning-server:latest"
echo "   • Deploy to Pi: ./deploy_to_pi.sh"
echo "   • Access web interface: http://localhost:8888" 