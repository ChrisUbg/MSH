#!/bin/bash
source ../config/environment.sh

# Unified MSH Deployment Script
# Builds locally and deploys to Raspberry Pi
# Usage: ./deploy-unified.sh [pi-ip-address]

set -e

# Configuration
PI_USER="chregg"
PI_IP=${1:-""}
PROJECT_DIR="/home/chregg/msh"
IMAGE_NAME="msh-web"
DB_NAME="matter_dev"
DB_PASSWORD="postgres"
WEB_PORT="8083"
DB_PORT="5435"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

# Function to detect Pi IP automatically
detect_pi_ip() {
    echo -e "${YELLOW}🔍 Auto-detecting Pi IP address...${NC}"
    
    # Try common Pi IP ranges
    local possible_ips=(
        "192.168.0.107"  # From setup_fresh_db.sh
        "192.168.0.106"  # From deploy-to-pi.sh
        "192.168.1.100"
        "192.168.1.101"
        "192.168.0.100"
        "192.168.0.101"
        "192.168.0.103"
        "192.168.0.105"
    )
    
    for ip in "${possible_ips[@]}"; do
        echo -e "  Testing $ip..."
        if ping -c 1 -W 1 "$ip" > /dev/null 2>&1; then
            if ssh -o ConnectTimeout=3 -o StrictHostKeyChecking=accept-new "$PI_USER@$ip" "echo 'SSH test'" > /dev/null 2>&1; then
                echo -e "${GREEN}✅ Found Pi at $ip${NC}"
                echo "$ip"
                return 0
            fi
        fi
    done
    
    echo -e "${RED}❌ Could not auto-detect Pi IP${NC}"
    return 1
}

# Function to check prerequisites
check_prerequisites() {
    echo -e "${YELLOW}🔧 Checking prerequisites...${NC}"
    
    # Check Docker
    if ! command -v docker &> /dev/null; then
        echo -e "${RED}❌ Docker is not installed${NC}"
        exit 1
    fi
    
    # Check docker buildx
    if ! docker buildx version &> /dev/null; then
        echo -e "${RED}❌ Docker buildx is not available${NC}"
        exit 1
    fi
    
    # Check rsync
    if ! command -v rsync &> /dev/null; then
        echo -e "${RED}❌ rsync is not installed${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✅ Prerequisites check passed${NC}"
}

# Function to build ARM64 image locally
build_image() {
    echo -e "${YELLOW}🛠️  Building ARM64 Docker image locally...${NC}"
    
    # Clean up old image
    docker rmi $IMAGE_NAME 2>/dev/null || true
    
    # Build for ARM64
    docker buildx build --platform linux/arm64 --load -t $IMAGE_NAME -f Dockerfile . || {
        echo -e "${RED}❌ Build failed${NC}"
        exit 1
    }
    
    echo -e "${GREEN}✅ Image built successfully${NC}"
}

# Function to save and transfer image
transfer_image() {
    echo -e "${YELLOW}📤 Transferring image to Pi...${NC}"
    
    # Save image as TAR
    echo -e "  Saving image as TAR file..."
    docker save -o $IMAGE_NAME.tar $IMAGE_NAME || {
        echo -e "${RED}❌ Failed to save image${NC}"
        exit 1
    }
    
    # Create directory on Pi
    ssh $PI_USER@$PI_IP "mkdir -p $PROJECT_DIR"
    
    # Transfer TAR file
    echo -e "  Transferring to Pi..."
    scp $IMAGE_NAME.tar $PI_USER@$PI_IP:$PROJECT_DIR/ || {
        echo -e "${RED}❌ Failed to transfer image${NC}"
        exit 1
    }
    
    # Clean up local TAR
    rm $IMAGE_NAME.tar
    
    echo -e "${GREEN}✅ Image transferred successfully${NC}"
}

# Function to deploy on Pi
deploy_on_pi() {
    echo -e "${YELLOW}🚀 Deploying on Raspberry Pi...${NC}"
    
    ssh $PI_USER@$PI_IP << EOF
        cd $PROJECT_DIR
        
        # Load image
        echo "🔍 Loading Docker image..."
        docker load -i $IMAGE_NAME.tar || {
            echo "❌ Failed to load image"
            exit 1
        }
        
        # Clean up old containers
        echo "🧹 Cleaning up old containers..."
        docker stop msh_web msh_db 2>/dev/null || true
        docker rm msh_web msh_db 2>/dev/null || true
        
        # Create network
        echo "🌐 Creating Docker network..."
        docker network create msh-network 2>/dev/null || true
        
        # Start database
        echo "🐘 Starting PostgreSQL..."
        docker run -d \\
            --name msh_db \\
            --network msh-network \\
            -e POSTGRES_PASSWORD=$DB_PASSWORD \\
            -e POSTGRES_DB=$DB_NAME \\
            -p $DB_PORT:5432 \\
            -v msh_postgres_data:/var/lib/postgresql/data \\
            postgres:16 || {
            echo "❌ Failed to start database"
            exit 1
        }
        
        # Wait for database
        echo "⏳ Waiting for database to be ready..."
        sleep 15
        
        # Start web application
        echo "⚡ Starting MSH web application..."
        docker run -d \\
            --name msh_web \\
            --network msh-network \\
            -p $WEB_PORT:8082 \\
            -e ASPNETCORE_ENVIRONMENT=Production \\
            -e ConnectionStrings__DefaultConnection="Host=msh_db;Port=5432;Database=$DB_NAME;Username=postgres;Password=$DB_PASSWORD" \\
            -v msh_dataprotection:/root/.aspnet/DataProtection-Keys \\
            -v /var/run/docker.sock:/var/run/docker.sock \\
            $IMAGE_NAME || {
            echo "❌ Failed to start web application"
            exit 1
        }
        
        # Clean up TAR file
        rm $IMAGE_NAME.tar
        
        echo "✅ Deployment completed!"
EOF
}

# Function to verify deployment
verify_deployment() {
    echo -e "${YELLOW}🔍 Verifying deployment...${NC}"
    
    # Wait for containers to be ready
    sleep 10
    
    # Check container status
    echo -e "📊 Container status:"
    ssh $PI_USER@$PI_IP "docker ps --filter 'name=msh_web|msh_db'"
    
    # Test web application
    echo -e "🌐 Testing web application..."
    if curl -f -s http://$PI_IP:$WEB_PORT/health > /dev/null 2>&1; then
        echo -e "${GREEN}✅ Web application is responding${NC}"
    else
        echo -e "${YELLOW}⚠️  Web application not responding yet (checking logs)...${NC}"
        ssh $PI_USER@$PI_IP "docker logs msh_web --tail 20"
    fi
    
    # Test database
    echo -e "🗄️  Testing database..."
    if ssh $PI_USER@$PI_IP "docker exec msh_db psql -U postgres -d $DB_NAME -c 'SELECT 1;'" > /dev/null 2>&1; then
        echo -e "${GREEN}✅ Database is accessible${NC}"
    else
        echo -e "${RED}❌ Database connection failed${NC}"
    fi
}

# Function to setup mDNS
setup_mdns() {
    echo -e "${YELLOW}🌐 Setting up mDNS configuration...${NC}"
    ssh $PI_USER@$PI_IP "cd $PROJECT_DIR && sudo ./setup-mdns.sh" || {
        echo -e "${YELLOW}⚠️  mDNS setup failed (continuing anyway)${NC}"
    }
}

# Main deployment process
main() {
    echo -e "${GREEN}🚀 Starting unified MSH deployment...${NC}"
    
    # Check prerequisites
    check_prerequisites
    
    # Auto-detect IP if not provided
    if [ -z "$PI_IP" ]; then
        echo -e "${YELLOW}📡 No IP provided, auto-detecting...${NC}"
        DETECTED_IP=$(detect_pi_ip)
        
        if [ $? -eq 0 ]; then
            PI_IP="$DETECTED_IP"
        else
            echo -e "${RED}❌ Could not find Pi automatically${NC}"
            echo -e "Please provide the IP address manually:"
            echo -e "  ./deploy-unified.sh [pi-ip-address]"
            exit 1
        fi
    fi
    
    echo -e "${GREEN}🎯 Using Pi IP: $PI_IP${NC}"
    
    # Check connection to Pi
    echo -e "${YELLOW}📡 Checking connection to Pi...${NC}"
    if ! ping -c 1 $PI_IP > /dev/null 2>&1; then
        echo -e "${RED}❌ Cannot reach Pi at $PI_IP${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✅ Pi is reachable${NC}"
    
    # Build image locally
    build_image
    
    # Transfer image to Pi
    transfer_image
    
    # Deploy on Pi
    deploy_on_pi
    
    # Setup mDNS
    setup_mdns
    
    # Verify deployment
    verify_deployment
    
    # Success message
    echo -e "${GREEN}🎉 Deployment completed successfully!${NC}"
    echo ""
    echo -e "${YELLOW}📱 Access your MSH application:${NC}"
    echo -e "   Web UI: http://$PI_IP:$WEB_PORT"
    echo -e "   Web UI (mDNS): http://msh.local:$WEB_PORT"
    echo ""
    echo -e "${YELLOW}📋 Container ports:${NC}"
    echo -e "   - Web: $WEB_PORT"
    echo -e "   - Database: $DB_PORT"
    echo ""
    echo -e "${YELLOW}🔧 Useful commands:${NC}"
    echo -e "   View logs: ssh $PI_USER@$PI_IP 'docker logs msh_web -f'"
    echo -e "   Stop services: ssh $PI_USER@$PI_IP 'docker stop msh_web msh_db'"
    echo -e "   Restart services: ssh $PI_USER@$PI_IP 'docker restart msh_web msh_db'"
    echo -e "   Setup database: ./setup_fresh_db.sh"
    echo ""
    echo -e "${YELLOW}💡 Tip: Use http://msh.local:$WEB_PORT for consistent access!${NC}"
}

# Run main function
main "$@"
