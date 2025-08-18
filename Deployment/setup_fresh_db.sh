#!/bin/bash
source ../config/environment.sh

# Setup Fresh Database Script
# This script sets up a fresh MSH database with proper migration and seeding

set -e

echo "🔄 Setting up fresh MSH database..."

# 1. Wait for database to be ready
echo "⏳ Waiting for database to be ready..."
sleep 10

# 2. Test database connection
echo "📊 Testing database connection..."
# Use PI_IP from environment or default to common Pi IP
PI_IP=${PI_IP:-"192.168.0.107"}
ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d matter_dev -c 'SELECT 1;'"

if [ $? -eq 0 ]; then
    echo "✅ Database connection successful"
else
    echo "❌ Database connection failed"
    exit 1
fi

# 3. Run Entity Framework migration
echo "🔧 Running Entity Framework migration..."
cd src
dotnet ef database update --project MSH.Infrastructure --startup-project MSH.Web

if [ $? -eq 0 ]; then
    echo "✅ Database migration completed"
else
    echo "❌ Database migration failed"
    exit 1
fi

cd ..

# 4. Run the seeding script
echo "🌱 Seeding database with master data..."
ssh chregg@$PI_IP "docker exec -i msh_db psql -U postgres -d matter_dev" < seed_database.sql

if [ $? -eq 0 ]; then
    echo "✅ Database seeded successfully"
else
    echo "❌ Database seeding failed"
    exit 1
fi

# 5. Verify the seeding
echo "🔍 Verifying seeded data..."
ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d matter_dev -c 'SELECT COUNT(*) as user_count FROM \"ApplicationUsers\";'"
ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d matter_dev -c 'SELECT COUNT(*) as room_count FROM \"Rooms\";'"
ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d matter_dev -c 'SELECT COUNT(*) as device_group_count FROM \"DeviceGroups\";'"
ssh chregg@$PI_IP "docker exec msh_db psql -U postgres -d matter_dev -c 'SELECT COUNT(*) as device_type_count FROM \"DeviceTypes\";'"

echo "🎉 Fresh database setup completed successfully!"
echo "📋 Summary of seeded data:"
echo "   - System and Admin users"
echo "   - Default rooms (Living Room, Kitchen, Bedroom, Bathroom)"
echo "   - Device groups (Smart Plugs, Smart Lighting, Smart Sensors, etc.)"
echo "   - Device types (Smart Plug, Smart Switch, Smart Bulb, etc.)"
echo "   - ASP.NET Core Identity roles and admin user"
echo ""
echo "🚀 Ready to deploy the MSH web application!" 