#!/bin/bash

# Build and deploy Matter Bridge to Pi
# Using simplified approach with Python Matter libraries instead of full SDK build

echo "=== MSH Matter Bridge Build & Deploy (Simplified) ==="

# Copy network-config.sh to Matter directory for deployment
echo "Copying network-config.sh to Matter directory..."
cp network-config.sh Matter/

# Build using simplified Dockerfile 
echo "Building Matter Bridge image using simplified approach..."
cd Matter

# Build the image using simplified Dockerfile
docker build --platform linux/amd64 \
  -t msh-matter-bridge:latest \
  -f Dockerfile.simple \
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
scp -r . chregg@192.168.0.104:~/MSH/Matter/
scp ../docker-compose.prod-msh.yml chregg@192.168.0.104:~/MSH/

# Clean up the copied file
rm -f network-config.sh

echo "Stopping existing containers..."
ssh chregg@192.168.0.104 "cd ~/MSH && docker-compose -f docker-compose.prod-msh.yml down"

echo "Building containers on Pi..."
CACHE_BUSTER=$(date +%s)
ssh chregg@192.168.0.104 "cd ~/MSH && docker-compose -f docker-compose.prod-msh.yml build --no-cache --build-arg CACHE_BUSTER=$CACHE_BUSTER"

echo "Starting containers..."
ssh chregg@192.168.0.104 "cd ~/MSH && docker-compose -f docker-compose.prod-msh.yml up -d"

echo "=== Deployment complete! ===" 