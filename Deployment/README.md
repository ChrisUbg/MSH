# MSH Deployment

This folder contains all deployment-related scripts and documentation for the MSH (Matter Smart Home) application.

## 🚀 Quick Start

### Primary Deployment Script
```bash
# From the MSH root directory
./Deployment/deploy-unified.sh
```

### Database Setup (First Time Only)
```bash
# After deployment, setup fresh database
./Deployment/setup_fresh_db.sh
```

## 📁 Files Overview

### Core Deployment Scripts

| File | Purpose | Status |
|------|---------|--------|
| `deploy-unified.sh` | **Primary deployment script** - Builds locally and deploys to Pi | ✅ **RECOMMENDED** |
| `setup_fresh_db.sh` | Database setup and seeding | ✅ **WORKING** |
| `setup-mdns.sh` | mDNS configuration for network discovery | ✅ **WORKING** |

### Matter & IoT Deployment Scripts

| File | Purpose | Status |
|------|---------|--------|
| `deploy-otbr-to-pi.sh` | OpenThread Border Router deployment | ✅ **WORKING** |
| `deploy-matter-bridge.sh` | Matter Bridge deployment | ✅ **WORKING** |
| `build-and-deploy-matter.sh` | Matter SDK build and deployment | ✅ **WORKING** |
| `deploy_chip_tool_docker.sh` | CHIP Tool Docker deployment | ✅ **WORKING** |
| `deploy_chip_tool_only.sh` | CHIP Tool only deployment | ✅ **WORKING** |

### Commissioning Server Scripts

| File | Purpose | Status |
|------|---------|--------|
| `commissioning-server-deploy-arm64.sh` | Commissioning server ARM64 deployment | ✅ **WORKING** |
| `commissioning-server-deploy-to-pi.sh` | Commissioning server Pi deployment | ✅ **WORKING** |
| `commissioning-server-build-for-pi.sh` | Commissioning server build for Pi | ✅ **WORKING** |

### Setup & Configuration Scripts

| File | Purpose | Status |
|------|---------|--------|
| `setup_docker_integration.sh` | Docker integration setup | ✅ **WORKING** |
| `simple_device_setup.sh` | Simple device setup | ✅ **WORKING** |
| `test-docker-setup.sh` | Docker setup testing | ✅ **WORKING** |

### Local Development Scripts

| File | Purpose | Status |
|------|---------|--------|
| `build-local-dev.sh` | **Local development build** - Builds locally, connects to Pi DB | ✅ **RECOMMENDED** |
| `ensure-pi-database.sh` | Ensure Pi database is running | ✅ **WORKING** |

### Documentation

| File | Purpose |
|------|---------|
| `DEPLOYMENT_UNIFIED.md` | **Main deployment guide** - Complete instructions |
| `LOCAL_DEVELOPMENT.md` | **Local development guide** - Build locally, connect to Pi DB |
| `DEPLOYMENT_GUIDE.md` | Legacy deployment guide (for reference) |
| `DEPRECATED_SCRIPTS.md` | List of deprecated scripts and why |
| `commissioning-server-DEPLOYMENT.md` | Commissioning server deployment guide |
| `debian-setup.md` | Debian system setup guide |
| `mdns-setup.md` | mDNS configuration guide |
| `postgres-setup.md` | PostgreSQL setup guide |

## 🎯 Recommended Workflow

### 1. Initial Deployment
```bash
# Deploy the main application
./Deployment/deploy-unified.sh

# Setup database (first time only)
./Deployment/setup_fresh_db.sh
```

### 2. Local Development
```bash
# Ensure Pi database is running
./Deployment/ensure-pi-database.sh

# Build and run local development
./Deployment/build-local-dev.sh
```

### 3. Additional Services
```bash
# Deploy OpenThread Border Router
./Deployment/deploy-otbr-to-pi.sh

# Deploy Matter Bridge
./Deployment/deploy-matter-bridge.sh

# Deploy Commissioning Server
./Deployment/commissioning-server-deploy-arm64.sh
```

### 4. Updates
```bash
# Deploy application updates
./Deployment/deploy-unified.sh
```

## 🔧 Configuration

### Environment Variables
The scripts use `config/environment.sh` for configuration:
- `PI_USER`: Raspberry Pi username (default: "chregg")
- `PI_IP`: Raspberry Pi IP address (auto-detected if not set)
- `PROJECT_DIR`: Project directory on Pi (default: "/home/chregg/msh")

### Port Configuration
- **Web Application**: 8083 (external) → 8082 (internal)
- **Database**: 5435 (external) → 5432 (internal)
- **mDNS**: `msh.local:8083`
- **Commissioning Server**: 8888 (external)

## 🌐 Access URLs

After deployment, access your application at:
- **Direct IP**: `http://[PI_IP]:8083`
- **mDNS**: `http://msh.local:8083`
- **Health Check**: `http://[PI_IP]:8083/health`
- **Commissioning Server**: `http://[PI_IP]:8888`

## 🔍 Troubleshooting

### Common Issues
1. **"Cannot reach Pi"** - Check network connectivity and IP address
2. **"Build failed"** - Ensure Docker buildx is available
3. **"Database connection failed"** - Wait for database to be ready

### Debug Commands
```bash
# Check container status
ssh chregg@[PI_IP] "docker ps"

# View application logs
ssh chregg@[PI_IP] "docker logs msh_web -f"

# View database logs
ssh chregg@[PI_IP] "docker logs msh_db -f"

# Test database connection
ssh chregg@[PI_IP] "docker exec msh_db psql -U postgres -d matter_dev -c 'SELECT 1;'"
```

## 📚 Documentation

- **Main Guide**: `DEPLOYMENT_UNIFIED.md` - Complete deployment instructions
- **Legacy Guide**: `DEPLOYMENT_GUIDE.md` - Previous deployment approach
- **Deprecated Scripts**: `DEPRECATED_SCRIPTS.md` - What not to use
- **Commissioning Server**: `commissioning-server-DEPLOYMENT.md` - Commissioning server guide
- **System Setup**: `debian-setup.md`, `mdns-setup.md`, `postgres-setup.md`

## 🔄 Maintenance

### Update Application
```bash
./Deployment/deploy-unified.sh
```

### Backup Database
```bash
ssh chregg@[PI_IP] "docker exec msh_db pg_dump -U postgres matter_dev" > backup.sql
```

### Restore Database
```bash
ssh chregg@[PI_IP] "docker exec -i msh_db psql -U postgres -d matter_dev" < backup.sql
```

## 🧪 Testing

### Test Docker Setup
```bash
./Deployment/test-docker-setup.sh
```

### Test Device Setup
```bash
./Deployment/simple_device_setup.sh
```

---

**Note**: All scripts should be run from the MSH root directory, not from within the Deployment folder.
