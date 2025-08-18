#!/bin/bash
source config/environment.sh

# Build and deploy Matter Bridge to Pi
# Using simplified approach with Python Matter libraries instead of full SDK build

echo "=== MSH Matter Bridge Build & Deploy (Simplified) ==="

# Check if PI_IP is set, if not try to auto-detect or prompt user
if [ -z "$PI_IP" ]; then
    echo "⚠️  PI_IP not set. Attempting to auto-detect Pi IP..."
    
    # Try to find Pi using the find-pi.sh script
    if [ -f "./find-pi.sh" ]; then
        DETECTED_IP=$(./find-pi.sh 2>/dev/null | grep -E '^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$' | head -1)
        if [ -n "$DETECTED_IP" ]; then
            PI_IP="$DETECTED_IP"
            echo "✅ Auto-detected Pi IP: $PI_IP"
        fi
    fi
    
    # If still not set, prompt user
    if [ -z "$PI_IP" ]; then
        echo "❌ Could not auto-detect Pi IP"
        echo "Please set the PI_IP environment variable or provide it as an argument:"
        echo "  export PI_IP=192.168.1.100"
        echo "  or"
        echo "  ./build-and-deploy-matter.sh [pi-ip-address]"
        exit 1
    fi
fi

echo "🎯 Using Pi IP: $PI_IP"

# Copy network-config.sh to Matter directory for deployment
echo "Copying network-config.sh to Matter directory..."
cp network-config.sh "${MATTER_DIR}/"

# Build using simplified Dockerfile 
echo "Building Matter Bridge image using simplified approach..."
cd Matter

# Build the image using simplified Dockerfile
docker build --platform linux/amd64 /
  -t msh-matter-bridge:latest /
  -f Dockerfile.simple /
  .

# Check if build was successful
if [ $? -ne 0 ]; then
    echo "Build failed!"
    # Clean up
    rm -f network-config.sh
    exit 1
fi

echo "Build completed successfully!"
echo "Testing the simplified build..."

# Test the image locally
echo "Testing the built image..."
docker run --rm msh-matter-bridge:latest echo "Container runs successfully!"

echo "=== Simplified build test complete! ==="
echo "If this works, we can proceed with cross-compilation for Pi deployment."

# TODO: Add cross-compilation or buildx setup for actual Pi deployment
echo "Note: This image is built for AMD64. For Pi deployment, we need ARM64 cross-compilation."
echo "Consider installing buildx or setting up cross-compilation."

# Deploy to Pi by building directly on Pi with simplified Dockerfile 
echo "Deploying to Pi using simplified Dockerfile..."
echo "Transferring Matter files to Pi..."

echo "Transferring Matter files to Pi..."
scp -r . "${PI_USER}@${PI_IP}:${PROJECT_ROOT}/${MATTER_DIR}/"
scp ../docker-compose.prod-msh.yml "${PI_USER}@${PI_IP}:${PROJECT_ROOT}/"

# Clean up the copied file
rm -f network-config.sh

echo "Stopping existing containers..."
ssh "${PI_USER}@${PI_IP}" "cd ${PROJECT_ROOT} && docker-compose -f docker-compose.prod-msh.yml down"

echo "Building containers on Pi..."
CACHE_BUSTER=$(date +%s)
ssh "${PI_USER}@${PI_IP}" "cd ${PROJECT_ROOT} && docker-compose -f docker-compose.prod-msh.yml build --no-cache --build-arg CACHE_BUSTER=$CACHE_BUSTER"

echo "Starting containers..."
ssh "${PI_USER}@${PI_IP}" "cd ${PROJECT_ROOT} && docker-compose -f docker-compose.prod-msh.yml up -d"

echo "=== Deployment complete! ===" 