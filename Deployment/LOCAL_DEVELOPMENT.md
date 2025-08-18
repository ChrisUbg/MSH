# Local Development Guide

## Overview

This guide explains how to set up a **local development environment** that builds the MSH application locally using Docker while connecting to the PostgreSQL database running on the Raspberry Pi.

## 🎯 Benefits of Local Development

- ✅ **Fast builds** - No Pi build issues
- ✅ **Full debugging** - Access to all development tools
- ✅ **Shared database** - Same data as production
- ✅ **Hot reloading** - Quick development cycles
- ✅ **Resource efficient** - Uses local machine resources

## 📋 Prerequisites

### Development Machine
```bash
# Install Docker and Docker Compose
sudo apt update
sudo apt install docker.io docker-compose curl

# Add user to docker group
sudo usermod -aG docker $USER

# Logout and login again, or run:
newgrp docker
```

### Raspberry Pi
- ✅ Pi must be running and accessible
- ✅ PostgreSQL database must be running on Pi
- ✅ SSH access to Pi configured

## 🚀 Quick Start

### 1. Ensure Pi Database is Running
```bash
# Check and start Pi database if needed
./Deployment/ensure-pi-database.sh

# Or specify Pi IP manually
./Deployment/ensure-pi-database.sh 192.168.0.107
```

### 2. Build and Run Local Development
```bash
# Build and start local development environment
./Deployment/build-local-dev.sh

# Or specify Pi IP manually
./Deployment/build-local-dev.sh 192.168.0.107
```

### 3. Access Your Application
- **Web UI**: http://localhost:8083
- **Health Check**: http://localhost:8083/health

## 📁 Configuration Files

### `docker-compose.dev-local.yml`
Local development Docker Compose configuration:
- ✅ Builds application locally
- ✅ Connects to Pi's PostgreSQL database
- ✅ Development environment settings
- ✅ Source code mounting for development

### Key Configuration
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - ConnectionStrings__DefaultConnection=Host=192.168.0.107;Port=5435;Database=matter_dev;Username=postgres;Password=postgres;TrustServerCertificate=true;
```

## 🔧 Development Workflow

### Initial Setup
```bash
# 1. Ensure Pi database is running
./Deployment/ensure-pi-database.sh

# 2. Build and start local development
./Deployment/build-local-dev.sh

# 3. Setup database (first time only)
./Deployment/setup_fresh_db.sh
```

### Daily Development
```bash
# Start development environment
./Deployment/build-local-dev.sh

# Make code changes...

# Rebuild after changes
docker-compose -f docker-compose.dev-local.yml build --no-cache
docker-compose -f docker-compose.dev-local.yml up -d

# Or restart for quick changes
docker-compose -f docker-compose.dev-local.yml restart
```

### Stopping Development
```bash
# Stop local development environment
docker-compose -f docker-compose.dev-local.yml down
```

## 🗄️ Database Management

### Database Connection
- **Host**: Raspberry Pi IP (192.168.0.107)
- **Port**: 5435 (external) → 5432 (internal)
- **Database**: matter_dev
- **User**: postgres
- **Password**: postgres

### Database Operations
```bash
# Check database status on Pi
ssh chregg@192.168.0.107 "docker ps | grep msh_db"

# View database logs
ssh chregg@192.168.0.107 "docker logs msh_db -f"

# Connect to database
ssh chregg@192.168.0.107 "docker exec -it msh_db psql -U postgres -d matter_dev"

# Backup database
ssh chregg@192.168.0.107 "docker exec msh_db pg_dump -U postgres matter_dev" > backup.sql

# Restore database
ssh chregg@192.168.0.107 "docker exec -i msh_db psql -U postgres -d matter_dev" < backup.sql
```

## 🔍 Troubleshooting

### Common Issues

#### 1. "Cannot reach Pi"
```bash
# Test connectivity
ping 192.168.0.107

# Test SSH
ssh chregg@192.168.0.107 "echo 'SSH working'"
```

#### 2. "Database connection failed"
```bash
# Check if database is running
ssh chregg@192.168.0.107 "docker ps | grep msh_db"

# Check database logs
ssh chregg@192.168.0.107 "docker logs msh_db --tail 20"

# Test database connection
ssh chregg@192.168.0.107 "docker exec msh_db psql -U postgres -d matter_dev -c 'SELECT 1;'"
```

#### 3. "Build failed"
```bash
# Check Docker buildx
docker buildx version

# Clean Docker cache
docker system prune -a

# Rebuild without cache
docker-compose -f docker-compose.dev-local.yml build --no-cache
```

#### 4. "Application not responding"
```bash
# Check container status
docker-compose -f docker-compose.dev-local.yml ps

# View application logs
docker-compose -f docker-compose.dev-local.yml logs web -f

# Check health endpoint
curl http://localhost:8083/health
```

### Debug Commands

```bash
# View all logs
docker-compose -f docker-compose.dev-local.yml logs -f

# Shell access to container
docker-compose -f docker-compose.dev-local.yml exec web bash

# Check container resources
docker stats

# Test database connection from container
docker-compose -f docker-compose.dev-local.yml exec web curl http://localhost:8082/health
```

## 🔄 Development Tips

### Code Changes
1. **Small changes**: Restart container
   ```bash
   docker-compose -f docker-compose.dev-local.yml restart
   ```

2. **Major changes**: Rebuild container
   ```bash
   docker-compose -f docker-compose.dev-local.yml build --no-cache
   docker-compose -f docker-compose.dev-local.yml up -d
   ```

### Database Changes
- Database changes are persisted on the Pi
- Use Entity Framework migrations for schema changes
- Backup database before major changes

### Performance Optimization
- Use `docker-compose.dev-local.yml` for development
- Use `docker-compose.prod-msh.yml` for production testing
- Monitor resource usage with `docker stats`

## 📊 Monitoring

### Application Monitoring
```bash
# View real-time logs
docker-compose -f docker-compose.dev-local.yml logs web -f

# Check application health
curl http://localhost:8083/health

# Monitor resource usage
docker stats
```

### Database Monitoring
```bash
# Check database status
ssh chregg@192.168.0.107 "docker ps | grep msh_db"

# View database logs
ssh chregg@192.168.0.107 "docker logs msh_db -f"

# Check database size
ssh chregg@192.168.0.107 "docker exec msh_db psql -U postgres -d matter_dev -c 'SELECT pg_size_pretty(pg_database_size(\"matter_dev\"));'"
```

## 🚀 Deployment Workflow

### Development → Production
1. **Develop locally** using this guide
2. **Test thoroughly** with local environment
3. **Deploy to Pi** using `./Deployment/deploy-unified.sh`
4. **Verify production** deployment

### Production → Development
1. **Backup production database**
2. **Restore to development** if needed
3. **Continue local development**

## 📚 Additional Resources

### Scripts
- `ensure-pi-database.sh` - Ensure Pi database is running
- `build-local-dev.sh` - Build and run local development
- `setup_fresh_db.sh` - Setup database schema and data

### Documentation
- `DEPLOYMENT_UNIFIED.md` - Production deployment guide
- `DEPRECATED_SCRIPTS.md` - What not to use

### Docker Commands
```bash
# Useful Docker commands for development
docker-compose -f docker-compose.dev-local.yml ps
docker-compose -f docker-compose.dev-local.yml logs web -f
docker-compose -f docker-compose.dev-local.yml exec web bash
docker-compose -f docker-compose.dev-local.yml down
docker-compose -f docker-compose.dev-local.yml up -d
```

---

## 🎉 Summary

This local development setup provides:
- ✅ **Fast development cycles** with local building
- ✅ **Shared database** with production Pi
- ✅ **Full debugging capabilities**
- ✅ **Easy deployment workflow**

**Start developing with: `./Deployment/build-local-dev.sh`**
