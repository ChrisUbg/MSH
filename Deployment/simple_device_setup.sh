#!/bin/bash

echo "Simple Device Setup for Web GUI"
echo "==============================="

# Configuration for Pi Docker setup
DB_HOST="localhost"  # Docker exposes PostgreSQL on localhost of Pi
DB_PORT=5435         # External port mapped from Docker
DB_NAME="matter_dev"
DB_USER="postgres"
DB_PASSWORD="postgres"  # Password from Docker setup

# Device Node IDs from successful commissioning
OFFICE_SOCKET_1_NODE_ID="4328ED19954E9DC0"
OFFICE_SOCKET_2_NODE_ID="4328ED19954E9DC1"

echo ""
echo "✅ Commissioned Devices:"
echo "   - Office Socket 1: Node ID $OFFICE_SOCKET_1_NODE_ID"
echo "   - Office Socket 2: Node ID $OFFICE_SOCKET_2_NODE_ID"

echo ""
echo "🐳 Pi Docker Database Setup:"
echo "   - Host: $DB_HOST (Pi localhost)"
echo "   - Port: $DB_PORT (Docker external mapping)"
echo "   - Database: $DB_NAME"
echo "   - User: $DB_USER"

echo ""
echo "📋 Checking Docker containers on Pi..."

# Check if we're on the Pi and containers are running
if ! docker ps | grep -q "msh_db"; then
    echo "❌ MSH database container not found"
    echo "   This script should be run on the Pi where the Docker containers are running"
    echo "   Current containers:"
    docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    exit 1
fi

echo "✅ MSH database container is running"

echo ""
echo "🗄️  Adding devices to database..."

# Test database connection to Docker container on Pi
if PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "SELECT 1;" &> /dev/null; then
    echo "✅ Database connection successful"
    
    # Run the SQL script
    if PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f src/MSH.Web/Scripts/add_commissioned_devices.sql; then
        echo "✅ Devices added to database successfully"
    else
        echo "❌ Failed to add devices to database"
        exit 1
    fi
else
    echo "❌ Cannot connect to database"
    echo "   Please check:"
    echo "   1. You're running this on the Pi (not your local machine)"
    echo "   2. Docker containers are running: docker ps"
    echo "   3. PostgreSQL container is up: docker ps | grep msh_db"
    echo "   4. Port 5435 is accessible: netstat -tlnp | grep 5435"
    exit 1
fi

echo ""
echo "🎯 Setup Complete!"
echo "=================="
echo "✅ Devices are now in the database"
echo "✅ They will appear in the web GUI at http://your-pi-ip:8083"
echo "✅ Navigate to Device Management to see them"
echo ""
echo "📝 Notes:"
echo "- Device control is handled by your existing toggle_devices.sh script"
echo "- Web GUI shows device status and information"
echo "- Use the web GUI for device management and monitoring"
echo ""
echo "🔗 Next Steps:"
echo "1. Access web GUI: http://your-pi-ip:8083"
echo "2. Go to Device Management"
echo "3. See your commissioned devices listed"
echo "4. Use existing scripts for actual device control"
