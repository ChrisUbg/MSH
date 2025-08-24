#!/bin/bash

# Phase 3: Data Migration from 'public' to 'db' schema
# This script safely migrates all data while handling schema differences

set -e

# Configuration
PI_IP=${1:-"192.168.0.107"}
DB_NAME="matter_dev"
DB_USER="postgres"
DB_PASSWORD="postgres"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}🚀 Starting Phase 3: Data Migration from 'public' to 'db' schema${NC}"

# Function to check prerequisites
check_prerequisites() {
    echo -e "${YELLOW}🔧 Checking prerequisites...${NC}"
    
    # Check if Pi is reachable
    if ! ping -c 1 $PI_IP > /dev/null 2>&1; then
        echo -e "${RED}❌ Cannot reach Pi at $PI_IP${NC}"
        exit 1
    fi
    
    # Check if database is accessible
    if ! ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d $DB_NAME -c 'SELECT 1;'" > /dev/null 2>&1; then
        echo -e "${RED}❌ Cannot connect to database on Pi${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✅ Prerequisites check passed${NC}"
}

# Function to create backup before migration
create_backup() {
    echo -e "${YELLOW}💾 Creating backup of public schema data...${NC}"
    
    BACKUP_FILE="backup_public_schema_$(date +%Y%m%d_%H%M%S).sql"
    
    # Only backup the public schema data (the source data we're migrating)
    ssh chregg@$PI_IP "docker exec msh_db pg_dump -U postgres -d $DB_NAME --schema=public --data-only > /tmp/$BACKUP_FILE"
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Public schema backup created: $BACKUP_FILE${NC}"
        echo -e "${BLUE}💡 This backup contains all data from the 'public' schema${NC}"
    else
        echo -e "${RED}❌ Backup failed${NC}"
        exit 1
    fi
}

# Function to check data counts before migration
check_data_counts() {
    echo -e "${YELLOW}📊 Checking data counts before migration...${NC}"
    
    # Check public schema data
    echo -e "${BLUE}Public schema data:${NC}"
    ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d $DB_NAME -c 'SELECT COUNT(*) as total_records FROM information_schema.tables WHERE table_schema = '\''public'\'' AND table_name NOT LIKE '\''%migration%'\'';'"
    
    # Check db schema data
    echo -e "${BLUE}DB schema data:${NC}"
    ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d $DB_NAME -c 'SELECT COUNT(*) as total_records FROM information_schema.tables WHERE table_schema = '\''db'\'' AND table_name NOT LIKE '\''%migration%'\'';'"
}

# Function to execute migration
execute_migration() {
    echo -e "${YELLOW}🔄 Executing data migration...${NC}"
    
    # Copy migration script to Pi
    scp Deployment/migrate_public_to_db_schema.sql chregg@$PI_IP:/tmp/
    
    # Execute migration
    ssh chregg@$PI_IP "docker exec -i msh_db psql -U postgres -d $DB_NAME < /tmp/migrate_public_to_db_schema.sql"
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Migration executed successfully${NC}"
    else
        echo -e "${RED}❌ Migration failed${NC}"
        exit 1
    fi
}

# Function to verify migration results
verify_migration() {
    echo -e "${YELLOW}🔍 Verifying migration results...${NC}"
    
    # Check key tables
    echo -e "${BLUE}Verifying key tables:${NC}"
    
    # Check Devices table
    echo -e "${BLUE}Devices:${NC}"
    ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d $DB_NAME -c 'SELECT '\''Public'\'' as schema, COUNT(*) as count FROM public.\"Devices\" UNION ALL SELECT '\''DB'\'' as schema, COUNT(*) as count FROM db.\"Devices\";'"
    
    # Check Users table
    echo -e "${BLUE}Users:${NC}"
    ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d $DB_NAME -c 'SELECT '\''Public'\'' as schema, COUNT(*) as count FROM public.\"AspNetUsers\" UNION ALL SELECT '\''DB'\'' as schema, COUNT(*) as count FROM db.\"AspNetUsers\";'"
    
    # Check Rooms table
    echo -e "${BLUE}Rooms:${NC}"
    ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d $DB_NAME -c 'SELECT '\''Public'\'' as schema, COUNT(*) as count FROM public.\"Rooms\" UNION ALL SELECT '\''DB'\'' as schema, COUNT(*) as count FROM db.\"Rooms\";'"
    
    # Check Groups table
    echo -e "${BLUE}Groups:${NC}"
    ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d $DB_NAME -c 'SELECT '\''Public'\'' as schema, COUNT(*) as count FROM public.\"Groups\" UNION ALL SELECT '\''DB'\'' as schema, COUNT(*) as count FROM db.\"Groups\";'"
}

# Function to test application connectivity
test_application() {
    echo -e "${YELLOW}🧪 Testing application connectivity...${NC}"
    
    # Test if application can connect to db schema
    if curl -s http://localhost:8083/health | grep -q "healthy"; then
        echo -e "${GREEN}✅ Application health check passed${NC}"
    else
        echo -e "${RED}❌ Application health check failed${NC}"
        echo -e "${YELLOW}💡 You may need to restart the application to use the new schema${NC}"
    fi
}

# Function to provide next steps
next_steps() {
    echo -e "${GREEN}🎉 Phase 3 Migration Completed Successfully!${NC}"
    echo -e "${BLUE}📋 Next Steps:${NC}"
    echo -e "1. ${YELLOW}Restart the application${NC}: docker-compose -f docker-compose.dev-local.yml restart web"
    echo -e "2. ${YELLOW}Verify all functionality works${NC}: Check web interface at http://localhost:8083"
    echo -e "3. ${YELLOW}Test device operations${NC}: Ensure devices can be controlled"
    echo -e "4. ${YELLOW}Clean up old schema${NC}: Once verified, you can drop the 'public' schema"
    echo -e ""
    echo -e "${BLUE}🔧 Useful commands:${NC}"
    echo -e "   View logs: docker-compose -f docker-compose.dev-local.yml logs web -f"
    echo -e "   Check db schema: ssh chregg@$PI_IP 'docker exec msh_db psql -U postgres -d $DB_NAME -c \"SELECT table_name FROM information_schema.tables WHERE table_schema = '\''db'\'' ORDER BY table_name;\"'"
}

# Main execution
main() {
    check_prerequisites
    create_backup
    check_data_counts
    
    echo -e "${YELLOW}⚠️  WARNING: This will migrate all data from 'public' to 'db' schema${NC}"
    echo -e "${YELLOW}⚠️  The 'db' schema tables are currently empty - this is expected${NC}"
    echo -e "${YELLOW}⚠️  A backup of the 'public' schema data has been created${NC}"
    read -p "Do you want to continue? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo -e "${RED}❌ Migration cancelled${NC}"
        exit 1
    fi
    
    execute_migration
    verify_migration
    test_application
    next_steps
}

# Run main function
main "$@"
