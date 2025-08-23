# Phase 3: Data Migration Guide

## Overview

Phase 3 migrates all existing data from the 'public' schema to the new 'db' schema, handling schema differences and maintaining data integrity.

## What Phase 3 Does

### 🔄 **Data Migration Process**
- Migrates all 36 tables from 'public' to 'db' schema
- Handles schema differences (column types, constraints)
- Preserves all relationships and foreign keys
- Excludes shadow properties from the old schema
- Maintains data integrity throughout the process

### 🛡️ **Safety Features**
- **Automatic backup** before migration
- **Transaction-based** migration (rollback capability)
- **Conflict handling** with `ON CONFLICT DO NOTHING`
- **Data verification** after migration
- **Schema difference handling** (text → varchar length limits)

### 📊 **Migration Order**
1. **Independent Entities** (AspNetUsers, AspNetRoles, etc.)
2. **Level 1 Dependencies** (Rooms, DeviceTypes, DeviceGroups)
3. **Level 2 Dependencies** (Devices, Groups)
4. **Level 3 Dependencies** (DeviceStates, DeviceEvents, GroupMembers)
5. **Level 4 Dependencies** (DeviceHistory)
6. **Level 5 Dependencies** (Rules, RuleConditions, etc.)
7. **Junction Tables** (DeviceDeviceGroup)
8. **Settings/Permissions** (UserSettings, Notifications, etc.)

## Schema Differences Handled

### Column Type Changes
- `text` → `character varying(50/100/150/500)` for length constraints
- Preserves all data while respecting new schema limits

### Foreign Key Handling
- Removes corrupted shadow properties (`DeviceId1`, `GroupId1`, etc.)
- Maintains clean relationships in 'db' schema
- Preserves all valid foreign key relationships

### Missing Constraints
- 'db' schema doesn't have user tracking foreign keys (by design)
- Migration preserves user IDs without constraint violations

## Execution

### Prerequisites
- ✅ Pi database is running and accessible
- ✅ Application is stopped (recommended)
- ✅ Backup of public schema data (automatic)

### Running the Migration

```bash
# Execute Phase 3 migration
./Deployment/execute_phase3_migration.sh

# Or specify custom Pi IP
./Deployment/execute_phase3_migration.sh 192.168.0.107
```

### What to Expect

1. **Prerequisites Check** - Verifies Pi connectivity and database access
2. **Backup Creation** - Creates timestamped backup before migration
3. **Data Count Check** - Shows current record counts in both schemas
4. **Confirmation Prompt** - Asks for user confirmation before proceeding
5. **Migration Execution** - Runs the SQL migration script
6. **Verification** - Compares record counts between schemas
7. **Application Test** - Checks if application can connect to new schema

## Verification

### Automatic Verification
The script automatically verifies:
- Record counts match between 'public' and 'db' schemas
- Key tables (Devices, Users, Rooms, Groups) migrated correctly
- Application health check passes

### Manual Verification
```bash
# Check specific table counts
ssh chregg@192.168.0.107 "docker exec msh_db psql -U postgres -d matter_dev -c 'SELECT \"Public\" as schema, COUNT(*) as count FROM public.\"Devices\" UNION ALL SELECT \"DB\" as schema, COUNT(*) as count FROM db.\"Devices\";'"

# List all tables in db schema
ssh chregg@192.168.0.107 "docker exec msh_db psql -U postgres -d matter_dev -c 'SELECT table_name FROM information_schema.tables WHERE table_schema = '\''db'\'' ORDER BY table_name;'"
```

## Post-Migration Steps

### 1. Restart Application
```bash
docker-compose -f docker-compose.dev-local.yml restart web
```

### 2. Verify Functionality
- Check web interface: http://localhost:8083
- Test device operations
- Verify all features work correctly

### 3. Clean Up (Optional)
Once everything is verified working:
```bash
# Drop old public schema (BE CAREFUL!)
ssh chregg@192.168.0.107 "docker exec msh_db psql -U postgres -d matter_dev -c 'DROP SCHEMA public CASCADE;'"
```

**Note**: Keep the backup file until you're 100% sure everything works correctly!

## Troubleshooting

### Migration Fails
- Check Pi connectivity: `ping 192.168.0.107`
- Verify database is running: `ssh chregg@192.168.0.107 'docker ps | grep msh_db'`
- Check logs: `ssh chregg@192.168.0.107 'docker logs msh_db'`

### Data Mismatch
- Restore from backup: `ssh chregg@192.168.0.107 "docker exec -i msh_db psql -U postgres -d matter_dev < /tmp/backup_*.sql"`
- Re-run migration after fixing issues

### Application Issues
- Check connection string includes `SearchPath=db`
- Verify migration completed successfully
- Restart application: `docker-compose -f docker-compose.dev-local.yml restart web`

## Benefits After Phase 3

### ✅ **Clean Schema**
- No shadow properties
- Proper foreign key relationships
- Consistent naming conventions

### ✅ **EF Core Compatibility**
- Migration system works perfectly
- No more corrupted table names
- Clean entity relationships

### ✅ **Future Development**
- Easy to add new entities
- Reliable migration process
- Maintainable codebase

## Files Created

- `Deployment/migrate_public_to_db_schema.sql` - Main migration script
- `Deployment/execute_phase3_migration.sh` - Execution script
- `Deployment/PHASE3_MIGRATION_GUIDE.md` - This guide

## Success Criteria

Phase 3 is successful when:
- ✅ All data migrated from 'public' to 'db' schema
- ✅ Record counts match between schemas
- ✅ Application starts and functions correctly
- ✅ No data loss or corruption
- ✅ EF Core migrations work properly

---

**Ready to proceed with Phase 3?** Run `./Deployment/execute_phase3_migration.sh` when you're ready!
