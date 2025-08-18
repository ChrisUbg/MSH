#!/bin/bash

# MSH Database Restore Script
# This script restores the MSH database from a backup

set -e

# Configuration
PI_IP=${PI_IP:-"192.168.0.107"}
DB_NAME="matter_dev"
BACKUP_DIR="$(dirname "$0")"

echo "🔄 MSH Database Restore Script"
echo "=============================="

# Check if backup file is provided
if [ $# -eq 0 ]; then
    echo "❌ No backup file specified!"
    echo ""
    echo "Usage: $0 <backup_file> [restore_type]"
    echo ""
    echo "Available backup files:"
    ls -la "$BACKUP_DIR"/*.sql* 2>/dev/null | grep -v restore_database.sh || echo "No backup files found"
    echo ""
    echo "Restore types:"
    echo "  full     - Full restore (schema + data) - DEFAULT"
    echo "  schema   - Schema only"
    echo "  data     - Data only (may require --disable-triggers)"
    echo ""
    echo "Examples:"
    echo "  $0 matter_dev_full_20250817_230354.sql.gz"
    echo "  $0 matter_dev_schema_20250817_230354.sql schema"
    echo "  $0 matter_dev_data_20250817_230354.sql data"
    exit 1
fi

BACKUP_FILE="$1"
RESTORE_TYPE="${2:-full}"

# Check if backup file exists
if [ ! -f "$BACKUP_FILE" ]; then
    if [ -f "$BACKUP_DIR/$BACKUP_FILE" ]; then
        BACKUP_FILE="$BACKUP_DIR/$BACKUP_FILE"
    else
        echo "❌ Backup file not found: $BACKUP_FILE"
        exit 1
    fi
fi

echo "📁 Backup file: $BACKUP_FILE"
echo "🔧 Restore type: $RESTORE_TYPE"
echo ""

# Confirm restore
read -p "⚠️  This will overwrite the current database. Are you sure? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "❌ Restore cancelled"
    exit 1
fi

echo "🔄 Starting database restore..."

# Stop the web application to prevent conflicts
echo "⏹️  Stopping MSH web application..."
ssh chregg@$PI_IP "docker stop msh_web" 2>/dev/null || echo "⚠️  Web application was not running"

# Perform restore based on type
case $RESTORE_TYPE in
    "full")
        echo "📦 Performing full restore..."
        if [[ "$BACKUP_FILE" == *.gz ]]; then
            gunzip -c "$BACKUP_FILE" | ssh chregg@$PI_IP "docker exec -i msh_db psql -U postgres -d $DB_NAME"
        else
            ssh chregg@$PI_IP "docker exec -i msh_db psql -U postgres -d $DB_NAME" < "$BACKUP_FILE"
        fi
        ;;
    "schema")
        echo "🏗️  Performing schema-only restore..."
        ssh chregg@$PI_IP "docker exec -i msh_db psql -U postgres -d $DB_NAME" < "$BACKUP_FILE"
        ;;
    "data")
        echo "📊 Performing data-only restore..."
        echo "⚠️  Using --disable-triggers due to circular foreign key constraints..."
        ssh chregg@$PI_IP "docker exec -i msh_db psql -U postgres -d $DB_NAME --disable-triggers" < "$BACKUP_FILE"
        ;;
    *)
        echo "❌ Invalid restore type: $RESTORE_TYPE"
        echo "Valid types: full, schema, data"
        exit 1
        ;;
esac

# Restart the web application
echo "▶️  Starting MSH web application..."
ssh chregg@$PI_IP "docker start msh_web"

# Verify restore
echo "🔍 Verifying restore..."
sleep 5
ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d $DB_NAME -c \"SELECT 'Users: ' || COUNT(*) FROM \\\"ApplicationUsers\\\"; SELECT 'Rooms: ' || COUNT(*) FROM \\\"Rooms\\\"; SELECT 'Devices: ' || COUNT(*) FROM \\\"Devices\\\"; SELECT 'Device Types: ' || COUNT(*) FROM \\\"DeviceTypes\\\"; SELECT 'Device Groups: ' || COUNT(*) FROM \\\"DeviceGroups\\\";\""

echo "✅ Database restore completed successfully!"
echo "🌐 Web application restarted"
echo "📊 Check the application at: http://$PI_IP:8083"
