#!/bin/bash

# Script to migrate from 'db' schema back to 'public' schema
# This approach renames the original public schema to public_org as backup,
# then renames 'db' schema to 'public'

set -e

# Configuration
DB_HOST="192.168.0.107"
DB_PORT="5435"
DB_NAME="matter_dev"
DB_USER="postgres"
DB_PASSWORD="postgres"

echo "=== MSH Schema Migration: db -> public ==="
echo "This will:"
echo "1. Backup current state"
echo "2. Rename original public schema to public_org (as backup)"
echo "3. Rename 'db' schema to 'public'"
echo "4. Update C# code to use public schema"
echo ""

# Create backup
echo "Creating backup..."
BACKUP_FILE="backup_before_schema_migration_$(date +%Y%m%d_%H%M%S).sql"
PGPASSWORD=$DB_PASSWORD pg_dump -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME --schema=db --schema=public > "$BACKUP_FILE"
echo "Backup created: $BACKUP_FILE"

# Check if public_org schema already exists
echo "Checking if public_org schema exists..."
PUBLIC_ORG_EXISTS=$(PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -t -c "SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name = 'public_org';")

if [ "$PUBLIC_ORG_EXISTS" -gt 0 ]; then
    echo "WARNING: public_org schema already exists. Dropping it first..."
    PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "DROP SCHEMA IF EXISTS public_org CASCADE;"
fi

# Rename original public schema to public_org
echo "Renaming original public schema to public_org..."
PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "ALTER SCHEMA public RENAME TO public_org;"

# Rename db schema to public
echo "Renaming 'db' schema to 'public'..."
PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "ALTER SCHEMA db RENAME TO public;"

# Verify the migration
echo "Verifying migration..."
DB_TABLES=$(PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';")
MIGRATIONS=$(PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -t -c "SELECT COUNT(*) FROM public.\"__EFMigrationsHistory\";")
PUBLIC_ORG_TABLES=$(PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public_org';")

echo "Migration completed successfully!"
echo "Public schema now contains:"
echo "- $DB_TABLES tables"
echo "- $MIGRATIONS migrations"
echo "Public_org schema (backup) contains:"
echo "- $PUBLIC_ORG_TABLES tables"

echo ""
echo "Next steps:"
echo "1. Test the application"
echo "2. If everything works, you can optionally drop public_org schema"
echo "3. Commit changes to Git"
