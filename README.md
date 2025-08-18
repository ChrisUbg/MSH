# MSH (Matter Smart Home) 🏠

A comprehensive Matter device management system with web-based control interface, designed to run on Raspberry Pi with local device control capabilities.

## 🎯 Project Overview

MSH provides a complete solution for managing Matter devices through a web interface, with the following key features:

- **Web-based Device Management**: Control Matter devices through a modern web interface
- **Local Device Control**: Direct communication with Matter devices via chip-tool
- **Docker-based Deployment**: Easy deployment using Docker containers
- **Database Management**: PostgreSQL database with Entity Framework Core
- **mDNS Support**: Network discovery and local access

## 🚀 Quick Start

### Prerequisites
- Raspberry Pi (ARM64) with Docker installed
- Matter devices for testing
- Network connectivity

### Deployment
```bash
# Clone the repository
git clone <repository-url>
cd MSH

# Deploy to Raspberry Pi
./Deployment/deploy-unified.sh [pi-ip-address]

# Setup database (first time only)
./Deployment/setup_fresh_db.sh
```

### Access
- **Web Interface**: `http://[PI_IP]:8083` or `http://msh.local:8083`
- **Health Check**: `http://[PI_IP]:8083/health`

## 📁 Project Structure

```
MSH/
├── src/                    # Main application source code
│   ├── MSH.Web/           # ASP.NET Core web application
│   ├── MSH.Infrastructure/# Database and services
│   ├── MSH.Core/          # Core business logic
│   └── MSH.Matter/        # Matter-specific functionality
├── Deployment/            # Deployment scripts and configuration
├── config/               # Environment configuration
├── BackUpFiles/          # Database backups
├── docker/               # Docker configurations
├── docs/                 # Documentation
└── History/              # Deprecated files and backups
```

## 🔧 Key Components

### Core Application
- **MSH.Web**: ASP.NET Core Blazor web application
- **MSH.Infrastructure**: Entity Framework Core with PostgreSQL
- **MSH.Core**: Business logic and domain models
- **MSH.Matter**: Matter device control services

### Deployment
- **deploy-unified.sh**: Main deployment script
- **setup_fresh_db.sh**: Database initialization
- **Dockerfile**: Multi-stage container build
- **docker-compose*.yml**: Container orchestration

### Device Control
- **MatterDeviceControlService**: Interface with chip-tool
- **Device Management**: CRUD operations for devices
- **Room Management**: Organize devices by rooms
- **Real-time Control**: Toggle devices on/off

## 🗄️ Database Schema

The application uses PostgreSQL with the following key entities:
- **Devices**: Matter device information and status
- **Rooms**: Device organization
- **DeviceTypes**: Device categorization
- **DeviceGroups**: Group management
- **Users**: User management and permissions

## 🔌 Matter Integration

### Device Communication
- Uses `chip-tool` Docker container for device control
- Supports on/off, toggle, and status reading
- Automatic Node ID formatting and conversion
- Secure communication via Matter fabric

### Commissioning
- Devices can be commissioned on PC and transferred to Pi
- Fabric consistency maintained across deployments
- KVS (Key-Value Store) persistence

## 🐳 Docker Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   msh_web       │    │   msh_db        │    │   chip_tool     │
│   (Port 8083)   │    │   (Port 5435)   │    │   (Internal)    │
│                 │    │                 │    │                 │
│  ASP.NET Core   │    │  PostgreSQL     │    │  Matter Control │
│  Web Interface  │    │  Database       │    │  Commands       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 📚 Documentation

### Core Documentation
- **Deployment Guide**: `Deployment/README.md`
- **API Documentation**: `api.md`
- **Basic Rules**: `basic-rules.md`
- **Changelog**: `changelog.md`

### 🔧 Technical Documentation
- **[Database Schema Issues](DATABASE_SCHEMA_ISSUES.md)** - **CRITICAL**: EF Core schema mismatch fixes and manual database corrections
- **[Commissioning Process](COMMISSIONING_PROCESS_DOCUMENTATION.md)** - **IMPORTANT**: Non-standard device commissioning for NOUS A8M devices
- **[Migration Guide](migration.md)** - Database migration commands and procedures

## 🔍 Troubleshooting

### Common Issues
1. **Device Control Fails**: Check chip-tool container status
2. **Database Connection**: Verify PostgreSQL container is running
3. **Network Issues**: Check mDNS configuration

### Debug Commands
```bash
# Check container status
docker ps

# View application logs
docker logs msh_web -f

# Test device control
docker exec chip_tool /usr/sbin/chip-tool onoff read on-off [NODE_ID] 1
```

## 🤝 Contributing

1. Follow the project structure and coding standards
2. Test changes locally before deployment
3. Update documentation for new features
4. Create comprehensive commit messages

## 📄 License

[Add your license information here]

## 🏷️ Version

Current Version: 1.0.0
Last Updated: August 2025

---

**Note**: This project has been cleaned up and streamlined for production use. Old files and deprecated scripts have been moved to the `History/` folder for reference.
