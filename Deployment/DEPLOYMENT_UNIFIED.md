# MSH Unified Deployment Guide

## Overview

This guide provides a **unified deployment strategy** for the MSH (Matter Smart Home) application, addressing the inconsistencies found across multiple deployment scripts. The recommended approach is **local building** to avoid Raspberry Pi build issues.

## 🎯 Recommended Deployment Strategy

### **Primary Method: Local Build + Transfer**
- ✅ **Build ARM64 image locally** on development machine
- ✅ **Transfer as TAR file** to Raspberry Pi
- ✅ **Deploy using Docker containers** on Pi
- ✅ **Auto-detect Pi IP** for convenience
- ✅ **Comprehensive verification** and health checks

### **Why Local Building?**
- 🚫 **Avoids Pi build issues** (memory, performance, dependencies)
- ⚡ **Faster builds** on development machine
- 🔧 **Better debugging** capabilities
- 📦 **Consistent artifacts** across deployments

## 📋 Prerequisites

### Development Machine
```bash
# Required tools
sudo apt update
sudo apt install docker.io docker-compose rsync curl

# Enable Docker buildx for multi-architecture builds
docker buildx create --use
```

### Raspberry Pi
```bash
# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER

# Install Docker Compose
sudo apt install docker-compose

# Reboot to apply changes
sudo reboot
```

## 🚀 Quick Deployment

### 1. Use the Unified Script
```bash
# Auto-detect Pi IP
./deploy-unified.sh

# Or specify IP manually
./deploy-unified.sh 192.168.0.107
```

### 2. Setup Database (First Time Only)
```bash
# After deployment, setup fresh database
./setup_fresh_db.sh
```

## 📊 Deployment Scripts Analysis

### **Current Scripts Status**

| Script | Status | Approach | Issues |
|--------|--------|----------|---------|
| `deploy-unified.sh` | ✅ **RECOMMENDED** | Local build + TAR transfer | None |
| `deploy-to-pi.sh` | ⚠️ **PARTIAL** | Remote build + Docker Compose | Build issues on Pi |
| `deploy.sh` | ❌ **BROKEN** | References missing files | `docker-compose.prod.yml` not found |
| `deploy_msh_to_pi.sh` | ✅ **WORKING** | Local build + TAR transfer | German comments, hardcoded values |

### **Key Differences**

#### Database Configuration
- **Unified Script**: `matter_dev` (consistent with setup_fresh_db.sh)
- **deploy-to-pi.sh**: `msh` (inconsistent)
- **deploy_msh_to_pi.sh**: `matter_dev` (consistent)

#### Port Configuration
- **Web Port**: 8083 (all scripts agree)
- **Database Port**: 5435 (all scripts agree)
- **Internal Port**: 8082 (web), 5432 (database)

#### Build Strategy
- **Local Build**: `deploy-unified.sh`, `deploy_msh_to_pi.sh`
- **Remote Build**: `deploy-to-pi.sh`
- **Mixed**: `deploy.sh` (broken)

## 🔧 Configuration Details

### Docker Compose Files
- **Primary**: `docker-compose.prod-msh.yml` ✅ (exists)
- **Missing**: `docker-compose.prod.yml` ❌ (referenced by deploy.sh)

### Environment Variables
```bash
# From config/environment.sh
PI_USER="chregg"
PI_IP="${PI_IP}"  # Auto-detected or provided
PROJECT_DIR="/home/chregg/msh"
```

### Container Configuration
```yaml
# Database Container
- Name: msh_db
- Image: postgres:16
- Port: 5435:5432
- Database: matter_dev
- Volume: msh_postgres_data

# Web Container  
- Name: msh_web
- Image: msh-web (ARM64)
- Port: 8083:8082
- Environment: Production
- Volume: msh_dataprotection
```

## 🗄️ Database Setup

### Automatic Migration
The application includes automatic Entity Framework migrations on startup.

### Manual Database Setup
```bash
# Run after deployment
./setup_fresh_db.sh
```

This script:
1. ✅ Waits for database to be ready
2. ✅ Runs Entity Framework migrations
3. ✅ Seeds database with master data
4. ✅ Verifies seeded data

### Seeded Data
- **System User**: Required for entity relationships
- **Admin User**: For application administration
- **Default Rooms**: Living Room, Kitchen, Bedroom, Bathroom
- **Device Groups**: Smart Plugs, Smart Lighting, Smart Sensors
- **Device Types**: Smart Plug, Smart Switch, Smart Bulb

## 🌐 Network Configuration

### mDNS Setup
The deployment automatically configures mDNS for easy access:
- **Hostname**: `msh.local`
- **Port**: 8083
- **Service**: MSH Smart Home Web App

### Access URLs
- **Direct IP**: `http://192.168.0.107:8083`
- **mDNS**: `http://msh.local:8083`
- **Health Check**: `http://192.168.0.107:8083/health`

## 🔍 Troubleshooting

### Common Issues

#### 1. "exec format error"
**Cause**: Architecture mismatch
**Solution**: Use `docker buildx build --platform linux/arm64`

#### 2. "Cannot reach Pi"
**Cause**: Network connectivity or wrong IP
**Solution**: 
```bash
# Test connectivity
ping 192.168.0.107

# Test SSH
ssh chregg@192.168.0.107 "echo 'SSH working'"
```

#### 3. "Database connection failed"
**Cause**: Database not ready or wrong credentials
**Solution**:
```bash
# Check database container
ssh chregg@192.168.0.107 "docker logs msh_db"

# Test connection
ssh chregg@192.168.0.107 "docker exec msh_db psql -U postgres -d matter_dev -c 'SELECT 1;'"
```

#### 4. "Web application not responding"
**Cause**: Application startup issues
**Solution**:
```bash
# Check application logs
ssh chregg@192.168.0.107 "docker logs msh_web"

# Check container status
ssh chregg@192.168.0.107 "docker ps"
```

### Debug Commands

```bash
# View all container logs
ssh chregg@192.168.0.107 "docker logs msh_web -f"
ssh chregg@192.168.0.107 "docker logs msh_db -f"

# Check container resources
ssh chregg@192.168.0.107 "docker stats"

# Test database connection
ssh chregg@192.168.0.107 "docker exec msh_db psql -U postgres -d matter_dev -c 'SELECT version();'"

# Check mDNS
ssh chregg@192.168.0.107 "avahi-browse -at | grep MSH"
```

## 🔄 Maintenance

### Update Application
```bash
# Deploy new version
./deploy-unified.sh

# Database migrations run automatically
```

### Backup Database
```bash
# Create backup
ssh chregg@192.168.0.107 "docker exec msh_db pg_dump -U postgres matter_dev" > backup.sql

# Restore backup
ssh chregg@192.168.0.107 "docker exec -i msh_db psql -U postgres matter_dev" < backup.sql
```

### Stop/Start Services
```bash
# Stop all services
ssh chregg@192.168.0.107 "docker stop msh_web msh_db"

# Start all services
ssh chregg@192.168.0.107 "docker start msh_db msh_web"

# Restart all services
ssh chregg@192.168.0.107 "docker restart msh_web msh_db"
```

## 📈 Performance Notes

### Container Resources
- **Database**: ~50MB RAM, ~1GB disk
- **Web Application**: ~200MB RAM, ~500MB disk
- **Startup Time**: ~30 seconds total

### Optimization Tips
1. **Use SSD storage** on Pi for better database performance
2. **Increase swap space** if needed: `sudo dphys-swapfile swapoff && sudo dphys-swapfile set 2048 && sudo dphys-swapfile swapon`
3. **Monitor resource usage**: `htop` or `docker stats`

## 🔒 Security Considerations

### Network Security
- ✅ **Local network only** - No external internet access required
- ✅ **Docker network isolation** - Containers communicate via internal network
- ✅ **Port mapping** - Only necessary ports exposed

### Container Security
- ⚠️ **Database password** - Change from default `postgres` for production
- ⚠️ **Root user** - Application runs as root for hardware access
- ✅ **Volume persistence** - Data protected across container restarts

### Recommendations for Production
1. **Change database password**
2. **Use HTTPS** with reverse proxy
3. **Implement authentication**
4. **Regular security updates**

## 📚 Additional Resources

### Documentation
- [API Documentation](http://192.168.0.107:8083/docs) (when running)
- [Matter SDK Guide](https://github.com/project-chip/connectedhomeip)
- [Docker Documentation](https://docs.docker.com/)

### Scripts Reference
- **Primary**: `deploy-unified.sh` - Recommended deployment script
- **Database**: `setup_fresh_db.sh` - Database setup and seeding
- **mDNS**: `setup-mdns.sh` - Network discovery configuration
- **Environment**: `config/environment.sh` - Cross-platform configuration

---

## 🎉 Summary

The **unified deployment approach** solves the inconsistencies across multiple scripts by:

1. ✅ **Standardizing on local building** to avoid Pi build issues
2. ✅ **Using consistent database configuration** (`matter_dev`)
3. ✅ **Providing auto-detection** for Pi IP addresses
4. ✅ **Including comprehensive verification** and health checks
5. ✅ **Supporting mDNS** for easy network discovery

**Use `./deploy-unified.sh` for all future deployments!**
