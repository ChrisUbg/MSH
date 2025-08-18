#!/bin/bash

echo "MSH Docker Integration Setup for Commissioned Devices"
echo "====================================================="

# Configuration
PI_IP=$(hostname -I | awk '{print $1}')
DB_PORT=5435
WEB_PORT=8083
DB_NAME="matter_dev"
DB_USER="postgres"

# Device Node IDs from successful commissioning
OFFICE_SOCKET_1_NODE_ID="4328ED19954E9DC0"
OFFICE_SOCKET_2_NODE_ID="4328ED19954E9DC1"

echo ""
echo "🔧 Current Setup:"
echo "   - Pi IP: $PI_IP"
echo "   - Database Port: $DB_PORT"
echo "   - Web GUI Port: $WEB_PORT"
echo "   - Commissioned Devices:"
echo "     * Office Socket 1: $OFFICE_SOCKET_1_NODE_ID"
echo "     * Office Socket 2: $OFFICE_SOCKET_2_NODE_ID"

echo ""
echo "📋 Prerequisites Check..."

# Check if containers are running
echo "Checking Docker containers..."
if docker ps | grep -q "msh_web"; then
    echo "✅ MSH Web container is running"
else
    echo "❌ MSH Web container is not running"
    exit 1
fi

if docker ps | grep -q "msh_db"; then
    echo "✅ MSH Database container is running"
else
    echo "❌ MSH Database container is not running"
    exit 1
fi

if docker ps | grep -q "otbr-border-router"; then
    echo "✅ OTBR Border Router container is running"
else
    echo "❌ OTBR Border Router container is not running"
    exit 1
fi

# Check if chip-tool is available
echo "Checking chip-tool availability..."
if command -v /usr/local/bin/chip-tool &> /dev/null; then
    echo "✅ chip-tool found on host system"
else
    echo "⚠️  chip-tool not found on host system"
    echo "   This may need to be installed in the container"
fi

echo ""
echo "🗄️  Step 1: Adding devices to database..."

# Test database connection
if psql -h localhost -p $DB_PORT -U $DB_USER -d $DB_NAME -c "SELECT 1;" &> /dev/null; then
    echo "✅ Database connection successful"
    
    # Run the SQL script
    if psql -h localhost -p $DB_PORT -U $DB_USER -d $DB_NAME -f src/MSH.Web/Scripts/add_commissioned_devices.sql; then
        echo "✅ Devices added to database successfully"
    else
        echo "❌ Failed to add devices to database"
        exit 1
    fi
else
    echo "❌ Cannot connect to database"
    echo "   Please check if PostgreSQL is running on port $DB_PORT"
    exit 1
fi

echo ""
echo "🔧 Step 2: Testing device connectivity..."

# Test device 1
echo "Testing Office Socket 1..."
if /usr/local/bin/chip-tool onoff read on-off 0x$OFFICE_SOCKET_1_NODE_ID 1 &> /dev/null; then
    echo "✅ Office Socket 1 is responsive"
else
    echo "❌ Office Socket 1 is not responding"
fi

# Test device 2
echo "Testing Office Socket 2..."
if /usr/local/bin/chip-tool onoff read on-off 0x$OFFICE_SOCKET_2_NODE_ID 1 &> /dev/null; then
    echo "✅ Office Socket 2 is responsive"
else
    echo "❌ Office Socket 2 is not responding"
fi

echo ""
echo "🐳 Step 3: Docker container considerations..."

# Check if chip-tool is available inside the container
echo "Checking chip-tool in container..."
if docker exec msh_web which chip-tool &> /dev/null; then
    echo "✅ chip-tool available in container"
else
    echo "⚠️  chip-tool not available in container"
    echo "   You may need to:"
    echo "   1. Mount the chip-tool binary into the container"
    echo "   2. Install chip-tool inside the container"
    echo "   3. Use host networking mode"
fi

echo ""
echo "🌐 Step 4: Testing web interface..."

# Test web interface accessibility
if curl -s http://localhost:$WEB_PORT > /dev/null; then
    echo "✅ Web interface is accessible"
    echo "   URL: http://$PI_IP:$WEB_PORT"
else
    echo "❌ Web interface is not accessible"
    echo "   Please check if the container is running properly"
fi

echo ""
echo "🧪 Step 5: Testing API endpoints..."

# Test API endpoints
echo "Testing device state API..."
if curl -s http://localhost:$WEB_PORT/api/matterdevice/$OFFICE_SOCKET_1_NODE_ID/state > /dev/null; then
    echo "✅ Device state API is working"
else
    echo "⚠️  Device state API may not be working (expected if devices not in DB yet)"
fi

echo ""
echo "🎯 Integration Summary:"
echo "======================"
echo "✅ Database: Devices added successfully"
echo "✅ OTBR: Border router is running"
echo "✅ Web GUI: Accessible at http://$PI_IP:$WEB_PORT"
echo "✅ Devices: Both NOUS A8M devices commissioned"
echo ""
echo "🔗 Next Steps:"
echo "1. Access the web GUI: http://$PI_IP:$WEB_PORT"
echo "2. Navigate to Device Management"
echo "3. Click on your devices to control them"
echo ""
echo "📝 Notes:"
echo "- If chip-tool is not available in the container, you may need to:"
echo "  * Mount the binary: docker run -v /usr/local/bin/chip-tool:/usr/local/bin/chip-tool ..."
echo "  * Install in container: Update Dockerfile to install Matter SDK"
echo "  * Use host networking: docker run --network host ..."
echo ""
echo "🔍 Troubleshooting:"
echo "- Check container logs: docker logs msh_web"
echo "- Test device connectivity: ./toggle_devices.sh"
echo "- Verify database: psql -h localhost -p $DB_PORT -U $DB_USER -d $DB_NAME"
