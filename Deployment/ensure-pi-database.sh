#!/bin/bash
source ../config/environment.sh

# Ensure Pi Database is Running
# This script ensures the PostgreSQL database on the Pi is running and accessible

set -e

# Configuration
PI_IP=${1:-"192.168.0.107"}
DB_PORT="5435"
DB_NAME="matter_dev"
DB_USER="postgres"
DB_PASSWORD="postgres"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${GREEN}🗄️  Ensuring Pi database is running...${NC}"

# Function to check Pi connectivity
check_pi_connectivity() {
    echo -e "${YELLOW}🔍 Checking Pi connectivity...${NC}"
    
    if ! ping -c 1 $PI_IP > /dev/null 2>&1; then
        echo -e "${RED}❌ Cannot reach Pi at $PI_IP${NC}"
        echo -e "${YELLOW}💡 Make sure the Pi is running and accessible${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✅ Pi is reachable${NC}"
}

# Function to check if database container is running
check_database_container() {
    echo -e "${YELLOW}🔍 Checking database container status...${NC}"
    
    # Check if msh_db container exists and is running
    if ! ssh chregg@$PI_IP "docker ps --filter 'name=msh_db' --format 'table {{.Names}}\t{{.Status}}'" | grep -q "msh_db"; then
        echo -e "${YELLOW}⚠️  Database container not running, starting it...${NC}"
        start_database_container
    else
        echo -e "${GREEN}✅ Database container is running${NC}"
    fi
}

# Function to start database container
start_database_container() {
    echo -e "${YELLOW}🚀 Starting database container...${NC}"
    
    ssh chregg@$PI_IP << EOF
        # Create network if it doesn't exist
        docker network create msh-network 2>/dev/null || true
        
        # Start database container
        docker run -d \\
            --name msh_db \\
            --network msh-network \\
            -e POSTGRES_PASSWORD=$DB_PASSWORD \\
            -e POSTGRES_DB=$DB_NAME \\
            -p $DB_PORT:5432 \\
            -v msh_postgres_data:/var/lib/postgresql/data \\
            postgres:16 || {
            echo "❌ Failed to start database container"
            exit 1
        }
        
        echo "✅ Database container started"
EOF
    
    # Wait for database to be ready
    echo -e "${YELLOW}⏳ Waiting for database to be ready...${NC}"
    sleep 15
}

# Function to test database connection
test_database_connection() {
    echo -e "${YELLOW}🔍 Testing database connection...${NC}"
    
    # Test connection
    if ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d $DB_NAME -c 'SELECT 1;'" > /dev/null 2>&1; then
        echo -e "${GREEN}✅ Database connection successful${NC}"
    else
        echo -e "${RED}❌ Database connection failed${NC}"
        echo -e "${YELLOW}💡 Checking database logs...${NC}"
        ssh chregg@$PI_IP "docker logs msh_db --tail 10"
        exit 1
    fi
}

# Function to check database schema
check_database_schema() {
    echo -e "${YELLOW}🔍 Checking database schema...${NC}"
    
    # Check if tables exist
    TABLE_COUNT=$(ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d $DB_NAME -t -c \"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';\"")
    
    if [ "$TABLE_COUNT" -gt 0 ]; then
        echo -e "${GREEN}✅ Database has $TABLE_COUNT tables${NC}"
    else
        echo -e "${YELLOW}⚠️  Database appears to be empty${NC}"
        echo -e "${YELLOW}💡 You may need to run: ./Deployment/setup_fresh_db.sh${NC}"
    fi
}

# Function to show database info
show_database_info() {
    echo -e "${GREEN}🎉 Pi database is ready for local development!${NC}"
    echo ""
    echo -e "${YELLOW}🗄️  Database information:${NC}"
    echo -e "   Host: $PI_IP"
    echo -e "   Port: $DB_PORT"
    echo -e "   Database: $DB_NAME"
    echo -e "   User: $DB_USER"
    echo -e "   Password: $DB_PASSWORD"
    echo ""
    echo -e "${YELLOW}🔧 Useful commands:${NC}"
    echo -e "   Check container: ssh chregg@$PI_IP 'docker ps | grep msh_db'"
    echo -e "   View logs: ssh chregg@$PI_IP 'docker logs msh_db -f'"
    echo -e "   Connect to DB: ssh chregg@$PI_IP 'docker exec -it msh_db psql -U postgres -d $DB_NAME'"
    echo -e "   Stop DB: ssh chregg@$PI_IP 'docker stop msh_db'"
    echo -e "   Start DB: ssh chregg@$PI_IP 'docker start msh_db'"
    echo ""
    echo -e "${YELLOW}💡 Next steps:${NC}"
    echo -e "   Run local development: ./Deployment/build-local-dev.sh"
}

# Main execution
main() {
    echo -e "${GREEN}🔧 Pi Database Setup Script${NC}"
    echo -e "${YELLOW}Target Pi: $PI_IP${NC}"
    echo ""
    
    # Check Pi connectivity
    check_pi_connectivity
    
    # Check database container
    check_database_container
    
    # Test database connection
    test_database_connection
    
    # Check database schema
    check_database_schema
    
    # Show database info
    show_database_info
}

# Run main function
main "$@"
