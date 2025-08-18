#!/bin/bash

# MSH Database Backup Script
# This script creates comprehensive backups of the MSH database

set -e

# Configuration
PI_IP=${PI_IP:-"192.168.0.107"}
DB_NAME="matter_dev"
BACKUP_DIR="$(dirname "$0")"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

echo "🔄 Starting MSH database backup..."

# Create backup directory if it doesn't exist
mkdir -p "$BACKUP_DIR"

# 1. Full database backup (schema + data)
echo "📦 Creating full database backup..."
ssh chregg@$PI_IP "docker exec msh_db pg_dump -U postgres -d $DB_NAME --no-owner --no-privileges" > "$BACKUP_DIR/${DB_NAME}_full_${TIMESTAMP}.sql"

# 2. Schema-only backup
echo "🏗️  Creating schema-only backup..."
ssh chregg@$PI_IP "docker exec msh_db pg_dump -U postgres -d $DB_NAME --schema-only --no-owner --no-privileges" > "$BACKUP_DIR/${DB_NAME}_schema_${TIMESTAMP}.sql"

# 3. Data-only backup (with warnings about circular constraints)
echo "📊 Creating data-only backup..."
ssh chregg@$PI_IP "docker exec msh_db pg_dump -U postgres -d $DB_NAME --data-only --no-owner --no-privileges" > "$BACKUP_DIR/${DB_NAME}_data_${TIMESTAMP}.sql" 2>/dev/null || echo "⚠️  Data-only backup completed with warnings about circular constraints"

# 4. Create a compressed backup
echo "🗜️  Creating compressed backup..."
gzip "$BACKUP_DIR/${DB_NAME}_full_${TIMESTAMP}.sql"

# 5. Create a summary of the backup
echo "📋 Creating backup summary..."
cat > "$BACKUP_DIR/backup_summary_${TIMESTAMP}.txt" << EOF
MSH Database Backup Summary
==========================
Timestamp: $(date)
Database: $DB_NAME
Pi IP: $PI_IP

Backup Files Created:
- ${DB_NAME}_full_${TIMESTAMP}.sql.gz (Full backup - compressed)
- ${DB_NAME}_schema_${TIMESTAMP}.sql (Schema only)
- ${DB_NAME}_data_${TIMESTAMP}.sql (Data only)

Database Statistics:
$(ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d $DB_NAME -c \"SELECT 'Users: ' || COUNT(*) FROM \\\"ApplicationUsers\\\"; SELECT 'Rooms: ' || COUNT(*) FROM \\\"Rooms\\\"; SELECT 'Devices: ' || COUNT(*) FROM \\\"Devices\\\"; SELECT 'Device Types: ' || COUNT(*) FROM \\\"DeviceTypes\\\"; SELECT 'Device Groups: ' || COUNT(*) FROM \\\"DeviceGroups\\\";\"" 2>/dev/null || echo "Could not retrieve database statistics")

Restore Commands:
- Full restore: gunzip -c ${DB_NAME}_full_${TIMESTAMP}.sql.gz | ssh chregg@$PI_IP "docker exec -i msh_db psql -U postgres -d $DB_NAME"
- Schema only: ssh chregg@$PI_IP "docker exec -i msh_db psql -U postgres -d $DB_NAME" < ${DB_NAME}_schema_${TIMESTAMP}.sql
- Data only: ssh chregg@$PI_IP "docker exec -i msh_db psql -U postgres -d $DB_NAME" < ${DB_NAME}_data_${TIMESTAMP}.sql

Note: Data-only restore may require --disable-triggers due to circular foreign key constraints.
EOF

echo "✅ Backup completed successfully!"
echo "📁 Backup files saved in: $BACKUP_DIR"
echo "📄 Summary file: $BACKUP_DIR/backup_summary_${TIMESTAMP}.txt"

# List the created files
echo ""
echo "📋 Created backup files:"
ls -la "$BACKUP_DIR"/*"${TIMESTAMP}"*
