#!/bin/bash
source ../config/environment.sh

# Local Development Build Script
# Builds the application locally and connects to Pi's PostgreSQL database

set -e

# Configuration
PI_IP=${1:-"192.168.0.107"}
DB_PORT="5435"
WEB_PORT="8083"
DB_NAME="matter_dev"
DB_USER="postgres"
DB_PASSWORD="postgres"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${GREEN}🚀 Starting local development build...${NC}"

# Function to check prerequisites
check_prerequisites() {
    echo -e "${YELLOW}🔧 Checking prerequisites...${NC}"
    
    # Check Docker
    if ! command -v docker &> /dev/null; then
        echo -e "${RED}❌ Docker is not installed${NC}"
        exit 1
    fi
    
    # Check Docker Compose
    if ! command -v docker-compose &> /dev/null; then
        echo -e "${RED}❌ Docker Compose is not installed${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✅ Prerequisites check passed${NC}"
}

# Function to check Pi database connectivity
check_pi_database() {
    echo -e "${YELLOW}🔍 Checking Pi database connectivity...${NC}"
    
    # Test if Pi is reachable
    if ! ping -c 1 $PI_IP > /dev/null 2>&1; then
        echo -e "${RED}❌ Cannot reach Pi at $PI_IP${NC}"
        echo -e "${YELLOW}💡 Make sure the Pi is running and accessible${NC}"
        exit 1
    fi
    
    # Test database connection
    if ! ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d $DB_NAME -c 'SELECT 1;'" > /dev/null 2>&1; then
        echo -e "${RED}❌ Cannot connect to database on Pi${NC}"
        echo -e "${YELLOW}💡 Make sure the database is running: ssh chregg@$PI_IP 'docker ps | grep msh_db'${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✅ Pi database is accessible${NC}"
}

# Function to build and run locally
build_and_run_local() {
    echo -e "${YELLOW}🛠️  Building and running local development environment...${NC}"
    
    # Stop any existing containers
    echo -e "🧹 Stopping existing containers..."
    docker-compose -f docker-compose.dev-local.yml down 2>/dev/null || true
    
    # Build the application
    echo -e "🔨 Building application..."
    docker-compose -f docker-compose.dev-local.yml build --no-cache || {
        echo -e "${RED}❌ Build failed${NC}"
        exit 1
    }
    
    # Start the application
    echo -e "🚀 Starting application..."
    docker-compose -f docker-compose.dev-local.yml up -d || {
        echo -e "${RED}❌ Failed to start application${NC}"
        exit 1
    }
    
    echo -e "${GREEN}✅ Application started successfully${NC}"
}

# Function to verify deployment
verify_deployment() {
    echo -e "${YELLOW}🔍 Verifying deployment...${NC}"
    
    # Wait for application to be ready
    echo -e "⏳ Waiting for application to be ready..."
    sleep 10
    
    # Check container status
    echo -e "📊 Container status:"
    docker-compose -f docker-compose.dev-local.yml ps
    
    # Test web application
    echo -e "🌐 Testing web application..."
    if curl -f -s http://localhost:$WEB_PORT/health > /dev/null 2>&1; then
        echo -e "${GREEN}✅ Web application is responding${NC}"
    else
        echo -e "${YELLOW}⚠️  Web application not responding yet (checking logs)...${NC}"
        docker-compose -f docker-compose.dev-local.yml logs web --tail 20
    fi
    
    # Test database connection from container
    echo -e "🗄️  Testing database connection from container..."
    if docker-compose -f docker-compose.dev-local.yml exec web curl -f -s http://localhost:8082/health > /dev/null 2>&1; then
        echo -e "${GREEN}✅ Database connection from container is working${NC}"
    else
        echo -e "${YELLOW}⚠️  Database connection test failed (checking logs)...${NC}"
        docker-compose -f docker-compose.dev-local.yml logs web --tail 10
    fi
}

# Function to show useful commands
show_commands() {
    echo -e "${GREEN}🎉 Local development environment is ready!${NC}"
    echo ""
    echo -e "${YELLOW}📱 Access your application:${NC}"
    echo -e "   Web UI: http://localhost:$WEB_PORT"
    echo -e "   Health Check: http://localhost:$WEB_PORT/health"
    echo ""
    echo -e "${YELLOW}🔧 Useful commands:${NC}"
    echo -e "   View logs: docker-compose -f docker-compose.dev-local.yml logs web -f"
    echo -e "   Stop services: docker-compose -f docker-compose.dev-local.yml down"
    echo -e "   Restart services: docker-compose -f docker-compose.dev-local.yml restart"
    echo -e "   Rebuild: docker-compose -f docker-compose.dev-local.yml build --no-cache"
    echo -e "   Shell access: docker-compose -f docker-compose.dev-local.yml exec web bash"
    echo ""
    echo -e "${YELLOW}🗄️  Database info:${NC}"
    echo -e "   Host: $PI_IP"
    echo -e "   Port: $DB_PORT"
    echo -e "   Database: $DB_NAME"
    echo -e "   User: $DB_USER"
    echo ""
    echo -e "${YELLOW}💡 Development tips:${NC}"
    echo -e "   - Changes to source code require rebuilding the container"
    echo -e "   - Database changes are persisted on the Pi"
    echo -e "   - Use 'docker-compose -f docker-compose.dev-local.yml logs web -f' to monitor logs"
}

# Main execution
main() {
    echo -e "${GREEN}🔧 Local Development Build Script${NC}"
    echo -e "${YELLOW}Connecting to Pi database at $PI_IP${NC}"
    echo ""
    
    # Check prerequisites
    check_prerequisites
    
    # Check Pi database connectivity
    check_pi_database
    
    # Build and run locally
    build_and_run_local
    
    # Verify deployment
    verify_deployment
    
    # Show useful commands
    show_commands
}

# Run main function
main "$@"
