#!/bin/bash

# Configuration
PI_USER="chregg"
PI_HOST="192.168.0.104"
PI_DEPLOY_DIR="/home/chregg/msh"
PI_PASSWORD=""  # Leave empty if using SSH key authentication

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

# Function to check if SSH key authentication is set up
check_ssh_key() {
    if ! ssh -o BatchMode=yes -o ConnectTimeout=5 $PI_USER@$PI_HOST "echo 'SSH key authentication successful'" &>/dev/null; then
        echo -e "${YELLOW}SSH key authentication not set up.${NC}"
        echo -e "${YELLOW}To set up SSH key authentication:${NC}"
        echo -e "1. Generate SSH key if you don't have one: ssh-keygen -t rsa"
        echo -e "2. Copy your public key to the Pi: ssh-copy-id $PI_USER@$PI_HOST"
        echo -e "3. Try running this script again"
        echo -e "${YELLOW}Alternatively, set PI_PASSWORD in this script to use password authentication.${NC}"
        exit 1
    fi
}

# Function to execute SSH command with proper authentication
ssh_cmd() {
    if [ -z "$PI_PASSWORD" ]; then
        ssh $PI_USER@$PI_HOST "$1"
    else
        sshpass -p "$PI_PASSWORD" ssh $PI_USER@$PI_HOST "$1"
    fi
}

# Function to execute rsync command with proper authentication
rsync_cmd() {
    if [ -z "$PI_PASSWORD" ]; then
        rsync -avz --exclude '.git' --exclude 'bin' --exclude 'obj' "$1" $PI_USER@$PI_HOST:"$2"
    else
        sshpass -p "$PI_PASSWORD" rsync -avz --exclude '.git' --exclude 'bin' --exclude 'obj' "$1" $PI_USER@$PI_HOST:"$2"
    fi
}

echo -e "${YELLOW}Starting deployment process...${NC}"

# Check SSH authentication
if [ -z "$PI_PASSWORD" ]; then
    check_ssh_key
else
    # Check if sshpass is installed
    if ! command -v sshpass &> /dev/null; then
        echo -e "${RED}sshpass is not installed. Please install it with:${NC}"
        echo -e "sudo apt-get install sshpass"
        exit 1
    fi
fi

# Check if rsync is installed
if ! command -v rsync &> /dev/null; then
    echo -e "${RED}rsync is not installed. Please install it with:${NC}"
    echo -e "sudo apt-get install rsync"
    exit 1
fi

# Local build process
echo -e "${YELLOW}Building Docker images locally...${NC}"
docker-compose -f docker-compose.prod.yml down --remove-orphans
docker rmi msh-web
docker-compose -f docker-compose.prod.yml build --no-cache

# Setup deployment directory on Pi
echo -e "${YELLOW}Setting up deployment directory on Pi...${NC}"
ssh_cmd "mkdir -p $PI_DEPLOY_DIR"

# Copy files to Pi
echo -e "${YELLOW}Copying files to Pi...${NC}"
rsync_cmd "./" "$PI_DEPLOY_DIR/"

# Production deployment on Pi
echo -e "${YELLOW}Deploying to production...${NC}"
ssh_cmd "cd $PI_DEPLOY_DIR && \
    docker-compose -f docker-compose.prod.yml down && \
    docker-compose -f docker-compose.prod.yml up -d --build"

# Wait for containers to be ready
echo -e "${YELLOW}Waiting for containers to be ready...${NC}"
sleep 20

# Get container names
WEB_CONTAINER=$(ssh_cmd "cd $PI_DEPLOY_DIR && docker-compose -f docker-compose.prod.yml ps -q web")

# Verify static files
echo -e "${YELLOW}Verifying static files...${NC}"
ssh_cmd "docker exec $WEB_CONTAINER ls -la /app/wwwroot"

# Apply database migrations
echo -e "${YELLOW}Applying database migrations...${NC}"
ssh_cmd "cd $PI_DEPLOY_DIR && \
    docker exec $WEB_CONTAINER dotnet ef database update \
    --project src/MSH.Infrastructure/MSH.Infrastructure.csproj \
    --startup-project src/MSH.Web/MSH.Web.csproj \
    --context ApplicationDbContext"

# Verify deployment
echo -e "${YELLOW}Verifying deployment...${NC}"
ssh_cmd "cd $PI_DEPLOY_DIR && \
    docker-compose -f docker-compose.prod.yml ps"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Deployment completed successfully!${NC}"
    echo -e "${YELLOW}The application should now be running at http://$PI_HOST:8082${NC}"
else
    echo -e "${RED}Deployment failed! Check the logs above for details.${NC}"
fi 