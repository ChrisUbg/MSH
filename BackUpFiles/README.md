# MSH Database Backup System

This directory contains the backup and restore system for the MSH (Matter Smart Home) database.

## 📁 Files

- `backup_database.sh` - Script to create comprehensive database backups
- `restore_database.sh` - Script to restore database from backups
- `matter_dev_*.sql` - Database backup files (schema, data, full)
- `matter_dev_*.sql.gz` - Compressed full database backups
- `backup_summary_*.txt` - Summary files with backup details and restore instructions

## 🔄 Creating Backups

### Automatic Backup
```bash
./BackUpFiles/backup_database.sh
```

This creates:
- **Full backup** (schema + data) - compressed `.sql.gz` file
- **Schema-only backup** - database structure only
- **Data-only backup** - data only (with warnings about circular constraints)
- **Backup summary** - detailed information about the backup

### Manual Backup
```bash
# Full backup
ssh chregg@192.168.0.107 "docker exec msh_db pg_dump -U postgres -d matter_dev --no-owner --no-privileges" > BackUpFiles/matter_dev_full_$(date +%Y%m%d_%H%M%S).sql

# Schema only
ssh chregg@192.168.0.107 "docker exec msh_db pg_dump -U postgres -d matter_dev --schema-only --no-owner --no-privileges" > BackUpFiles/matter_dev_schema_$(date +%Y%m%d_%H%M%S).sql

# Data only
ssh chregg@192.168.0.107 "docker exec msh_db pg_dump -U postgres -d matter_dev --data-only --no-owner --no-privileges" > BackUpFiles/matter_dev_data_$(date +%Y%m%d_%H%M%S).sql
```

## 🔧 Restoring Backups

### Using the Restore Script
```bash
# Full restore (recommended)
./BackUpFiles/restore_database.sh matter_dev_full_20250817_230354.sql.gz

# Schema only
./BackUpFiles/restore_database.sh matter_dev_schema_20250817_230354.sql schema

# Data only
./BackUpFiles/restore_database.sh matter_dev_data_20250817_230354.sql data
```

### Manual Restore
```bash
# Full restore from compressed backup
gunzip -c BackUpFiles/matter_dev_full_20250817_230354.sql.gz | ssh chregg@192.168.0.107 "docker exec -i msh_db psql -U postgres -d matter_dev"

# Schema only
ssh chregg@192.168.0.107 "docker exec -i msh_db psql -U postgres -d matter_dev" < BackUpFiles/matter_dev_schema_20250817_230354.sql

# Data only (with triggers disabled for circular constraints)
ssh chregg@192.168.0.107 "docker exec -i msh_db psql -U postgres -d matter_dev --disable-triggers" < BackUpFiles/matter_dev_data_20250817_230354.sql
```

## ⚠️ Important Notes

### Before Restoring
1. **Stop the web application** to prevent conflicts
2. **Verify the backup file** is correct
3. **Confirm the restore** - it will overwrite current data

### Circular Foreign Key Constraints
The database has circular foreign key constraints on the `ApplicationUsers` table. This means:
- Data-only restores may require `--disable-triggers`
- Full restores are generally safer
- Schema-only restores are safe

### Backup Frequency
- **Before major changes**: Always backup before schema changes, migrations, or troubleshooting
- **After successful operations**: Backup after confirming everything works
- **Regular backups**: Consider automated daily/weekly backups

## 📊 Current Database Statistics

From the latest backup:
- **Users**: 2 (System + Admin)
- **Rooms**: 4 (Living Room, Kitchen, Bedroom, Bathroom)
- **Devices**: 2 (Office Socket 1 & 2)
- **Device Types**: 12 (including duplicates)
- **Device Groups**: 5

## 🚨 Emergency Recovery

If the database is corrupted or data is lost:

1. **Stop all services**:
   ```bash
   ssh chregg@192.168.0.107 "docker stop msh_web msh_db"
   ```

2. **Restore from latest backup**:
   ```bash
   ./BackUpFiles/restore_database.sh matter_dev_full_LATEST.sql.gz
   ```

3. **Verify restoration**:
   ```bash
   ssh chregg@192.168.0.107 "docker exec msh_db psql -U postgres -d matter_dev -c \"SELECT COUNT(*) FROM \\\"Devices\\\";\""
   ```

4. **Restart services**:
   ```bash
   ssh chregg@192.168.0.107 "docker start msh_db msh_web"
   ```

## 🔍 Troubleshooting

### Backup Issues
- **Permission denied**: Ensure SSH key authentication is set up
- **Connection failed**: Check Pi IP address and SSH connectivity
- **Disk space**: Ensure sufficient space for backup files

### Restore Issues
- **Circular constraints**: Use `--disable-triggers` for data-only restores
- **Permission errors**: Ensure proper PostgreSQL permissions
- **Application conflicts**: Stop web application before restore

## 📝 Best Practices

1. **Test restores** on a development environment first
2. **Keep multiple backups** from different points in time
3. **Document changes** that require backups
4. **Monitor backup file sizes** and disk space
5. **Verify backup integrity** by testing restores periodically
