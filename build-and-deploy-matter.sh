#!/bin/bash

# Build and deploy Matter Bridge to Pi
# Using simplified approach with Python Matter libraries instead of full SDK build

echo "=== MSH Matter Bridge Build & Deploy (Simplified) ==="

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
scp -r . chregg@192.168.0.102:~/MSH/Matter/
scp ../docker-compose.prod-msh.yml chregg@192.168.0.102:~/MSH/

echo "Building and deploying on Pi..."
ssh chregg@192.168.0.102 << 'ENDSSH'
cd ~/MSH

echo "Stopping existing matter-bridge container..."
docker-compose -f docker-compose.prod-msh.yml stop matter-bridge 2>/dev/null || echo "No running container to stop"
docker-compose -f docker-compose.prod-msh.yml rm -f matter-bridge 2>/dev/null || echo "No stopped containers"

echo "Building matter-bridge with simplified Dockerfile on Pi..."
docker-compose -f docker-compose.prod-msh.yml build matter-bridge

echo "Starting matter-bridge..."
docker-compose -f docker-compose.prod-msh.yml up -d matter-bridge

echo "Checking container status..."
docker-compose -f docker-compose.prod-msh.yml ps

echo "Checking container logs..."
docker-compose -f docker-compose.prod-msh.yml logs --tail=50 matter-bridge
ENDSSH

echo "=== Deployment complete! ===" 