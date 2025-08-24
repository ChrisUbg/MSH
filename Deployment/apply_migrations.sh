#!/bin/bash

# Apply Pending Migrations Script
# This script applies only pending migrations to an existing MSH database

set -e

echo "🔄 Applying pending migrations to MSH database..."

# Configuration
PI_IP=${PI_IP:-"192.168.0.107"}
DB_NAME="matter_dev"
DB_USER="postgres"
DB_PASSWORD="postgres"
DB_PORT="5435"

# Connection string
CONNECTION_STRING="Host=$PI_IP;Port=$DB_PORT;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASSWORD;TrustServerCertificate=true;Include Error Detail=true;"

echo "📊 Target Database: $DB_NAME on $PI_IP:$DB_PORT"

# 1. Wait for database to be ready
echo "⏳ Waiting for database to be ready..."
sleep 5

# 2. Test database connection
echo "📊 Testing database connection..."
ssh chregg@$PI_IP "docker exec msh_db psql -U $DB_USER -d $DB_NAME -c 'SELECT 1;'"

if [ $? -eq 0 ]; then
    echo "✅ Database connection successful"
else
    echo "❌ Database connection failed"
    exit 1
fi

# 3. Check current migration status
echo "🔍 Checking current migration status..."
ssh chregg@$PI_IP "docker exec msh_db psql -U $DB_USER -d $DB_NAME -c \"SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\";\""

# 4. Run Entity Framework migration (apply all pending migrations)
echo "🔧 Applying pending migrations..."
cd src
dotnet ef database update --project MSH.Infrastructure --startup-project MSH.Web --connection "$CONNECTION_STRING"

if [ $? -eq 0 ]; then
    echo "✅ Database migration completed successfully"
else
    echo "❌ Database migration failed"
    exit 1
fi

cd ..

# 5. Verify the new migration was applied
echo "🔍 Verifying migration application..."
ssh chregg@$PI_IP "docker exec msh_db psql -U $DB_USER -d $DB_NAME -c \"SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\";\""

# 6. Test the new table exists
echo "🔍 Verifying new tables..."
ssh chregg@$PI_IP "docker exec msh_db psql -U $DB_USER -d $DB_NAME -c \"SELECT table_name FROM information_schema.tables WHERE table_name = 'DeviceActionDelays';\""

echo "🎉 Migration application completed successfully!"
echo "📋 Summary:"
echo "   - Database connection verified"
echo "   - Pending migrations applied"
echo "   - New tables verified"
echo ""
echo "🚀 The MSH application should now work with the new features!"
