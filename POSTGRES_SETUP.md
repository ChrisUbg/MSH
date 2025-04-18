PostgreSQL Setup for Matter Smart Home

## Implementation Checklist

### 1. Development Environment Setup
```bash
# Start PostgreSQL container
docker run --name matter-dev-db-15 \
  -e POSTGRES_PASSWORD=securepassword123 \
  -p 5432:5432 \
  -v ~/matter_db/dev_data:/var/lib/postgresql/data \
  -d postgres:15
# matter_dev

# Verify running
docker ps -f name=matter-dev-db
```
# Change IP Address
```bash
psql -h localhost -p 5432 -U postgres -W

### 2. Production Setup (Raspberry Pi)
```bash
# Install PostgreSQL
sudo apt update && sudo apt install -y postgresql-15 postgresql-client-15

# Configure database
sudo -u postgres psql -c "CREATE DATABASE matter_prod;"
sudo -u postgres psql -c "CREATE USER matter_admin WITH PASSWORD 'strongprodpassword';"
sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE matter_prod TO matter_admin;"

# Enable remote connections (optional)
sudo sed -i "s/#listen_addresses = 'localhost'/listen_addresses = '*'/" /etc/postgresql/15/main/postgresql.conf
echo "host matter_prod matter_admin 192.168.1.0/24 md5" | sudo tee -a /etc/postgresql/15/main/pg_hba.conf
sudo systemctl restart postgresql
```

### 3. Backup Configuration
```bash
# Create backup directories
sudo mkdir -p /backups/{daily,wal}
sudo chown -R postgres:postgres /backups
sudo chmod 700 /backups

# Add cron jobs (as postgres user)
    sudo crontab -u postgres -e
```
Add these entries:
```cron
# Daily full backups at 3 AM
0 3 * * * pg_dump -U postgres matter_prod | gzip > /backups/daily/matter_prod_$(date +\%Y-\%m-\%d).sql.gz

# WAL archiving
* * * * * pg_archivecleanup /backups/wal/ $(ls -t /backups/wal/ | tail -n 1 | cut -d'.' -f1)

# Retention policy (keep 7 days)
0 4 * * * find /backups/daily -name "*.sql.gz" -mtime +7 -delete
```

### 4. Monitoring Setup
```bash
# Install monitoring tools
sudo apt install -y postgresql-15-stat-collector

# Configure logging
sudo -u postgres psql -c "ALTER SYSTEM SET log_statement = 'all';"
sudo -u postgres psql -c "ALTER SYSTEM SET log_connections = on;"
sudo systemctl restart postgresql
```

### 5. Migration Path (Dev → Production)
```bash
# From development machine:

echo "localhost:5432:matter_prod:postgres:84jfapa0d8f09qzhf22t" > ~/.pgpass
   chmod 600 ~/.pgpass

pg_dump -h localhost -U postgres postgres | gzip > matter_dev_backup.sql.gz

pg_dump -h localhost -U postgres postgres | gzip > postgres_$(date +\%Y\%m\%d).sql.gz



# To Raspberry Pi:
scp matter_dev_*.sql.gz chregg@192.168.0.104:/tmp/
ssh chregg@192.168.0.104 "gunzip -c /tmp/matter_dev_backup.sql.gz | PGPASSWORD='84jfapa0d8f09qzhf22t' psql -U postgres matter_prod"

ssh chregg@192.168.0.104 "gunzip -c /tmp/postgres_*.sql.gz | PGPASSWORD='84jfapa0d8f09qzhf22t' psql -U postgres matter_prod"

# Verify migration
ssh chregg@192.168.0.104 "sudo -u postgres psql -d matter_prod -c '\dt+'"
```

## Verification Steps
1. [ ] Test local database connection
   ```bash
   psql -h localhost -U postgres -c "\l"
   ```
2. [ ] Verify production DB accessibility
   ```bash
   psql -h raspberrypi -U matter_admin -d matter_prod -c "SELECT version();"
   ```
3. [ ] Test backup restoration
   ```bash
   gunzip -c /backups/daily/matter_prod_*.sql.gz | psql -U postgres matter_test_restore
   ```
4. [ ] Validate monitoring
   ```bash
   tail -n 20 /var/log/postgresql/postgresql-15-main.log
   ```

## Maintenance Commands
```bash

sudo -u postgres psql -c "ALTER USER postgres WITH PASSWORD '84jfapa0d8f09qzhf22t';"

 chregg chregg   326 16. Apr 20:40 matter_dev_backup.sql.gz

# Check database size
sudo -u postgres psql -c "SELECT pg_size_pretty(pg_database_size('matter_prod'));"

# List active connections
sudo -u postgres psql -c "SELECT * FROM pg_stat_activity;"

# Vacuum and analyze
sudo -u postgres psql -c "VACUUM (VERBOSE, ANALYZE);"

# script for update prod environment

scp migrations.sql chregg@192.168.0.104:/tmp/
ssh chregg@192.168.0.104 "docker exec -i matter-dev-db-15 psql -U postgres matter_prod < /tmp/migrations.sql"

```
# Entity Framework Commands

dotnet ef migrations add AddPeopleTable --project src/MSH.Infrastructure --startup-project src/MSH.Web --verbose

dotnet ef database update --project src/MSH.Infrastructure --startup-project src/MSH.Web

This document covers the complete setup from development to production with built-in backup solutions. Would you like me to add any additional sections such as:
- Performance tuning parameters
- SSL/TLS configuration
- Replication setup
- Disaster recovery procedures