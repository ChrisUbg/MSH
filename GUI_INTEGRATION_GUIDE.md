# MSH Web GUI Integration with Commissioned Devices

## 🎯 Overview

This guide explains how to integrate your successfully commissioned NOUS A8M devices with the MSH web GUI. The devices communicate through the OTBR border router and can be controlled directly via chip-tool commands.

## ✅ Commissioned Devices

- **Office Socket 1**: Node ID `4328ED19954E9DC0`
- **Office Socket 2**: Node ID `4328ED19954E9DC1`

## 🔧 Architecture

```
Web GUI (ASP.NET Core Docker) 
    ↓ (Port 8083 external)
MatterDeviceControlService
    ↓
chip-tool commands
    ↓
OTBR Border Router (Docker)
    ↓
Commissioned Devices (NOUS A8M)
```

## 📋 Prerequisites

1. **OTBR Border Router**: Running in Docker (`otbr-border-router`)
2. **Commissioned Devices**: Both NOUS A8M devices successfully commissioned
3. **chip-tool**: Available on the Pi host system
4. **Database**: PostgreSQL running in Docker on port 5435
5. **Web GUI**: MSH Web running in Docker on port 8083

## 🚀 Setup Steps

### 1. Add Devices to Database

Run the SQL script to add your commissioned devices:

```bash
# Connect to the PostgreSQL container
psql -h localhost -p 5435 -U postgres -d matter_dev -f src/MSH.Web/Scripts/add_commissioned_devices.sql
```

### 2. Rebuild and Restart Web Container

```bash
# Navigate to the web project
cd src/MSH.Web

# Build the new image with Matter device control
docker build -t msh-web:latest .

# Restart the container to pick up changes
docker restart msh_web
```

### 3. Access Web GUI

Navigate to: `http://your-pi-ip:8083`

## 🎮 Device Control Features

### Web Interface
- **Device Management**: View all devices in the system
- **Device Details**: Individual device control page
- **Power Control**: Toggle ON/OFF for socket devices
- **Real-time Status**: Device online/offline status

### API Endpoints
- `POST /api/matterdevice/{nodeId}/toggle` - Toggle device
- `POST /api/matterdevice/{nodeId}/on` - Turn device ON
- `POST /api/matterdevice/{nodeId}/off` - Turn device OFF
- `GET /api/matterdevice/{nodeId}/state` - Get device state
- `GET /api/matterdevice/{nodeId}/online` - Check if device is online
- `GET /api/matterdevice/commissioned` - List all commissioned devices

## 🧪 Testing

### 1. Test Device Connectivity

```bash
./test_gui_integration.sh
```

### 2. Test API Endpoints

```bash
# Test device state
curl http://your-pi-ip:8083/api/matterdevice/4328ED19954E9DC0/state

# Test device toggle
curl -X POST http://your-pi-ip:8083/api/matterdevice/4328ED19954E9DC0/toggle

# Test device ON
curl -X POST http://your-pi-ip:8083/api/matterdevice/4328ED19954E9DC0/on

# Test device OFF
curl -X POST http://your-pi-ip:8083/api/matterdevice/4328ED19954E9DC0/off
```

### 3. Test Web Interface

1. Navigate to Device Management at `http://your-pi-ip:8083/device-management`
2. Click on "Office Socket 1" or "Office Socket 2"
3. Use the power toggle switch
4. Verify device responds

## 🔍 Troubleshooting

### Device Not Responding
1. Check if OTBR is running: `docker ps | grep otbr`
2. Verify chip-tool path: `which chip-tool`
3. Test direct command: `/usr/local/bin/chip-tool onoff read on-off 0x4328ED19954E9DC0 1`

### Web GUI Issues
1. Check container logs: `docker logs msh_web`
2. Verify database connection in container
3. Check device exists in database

### API Issues
1. Check controller logs in container output
2. Verify service registration in `Program.cs`
3. Test with curl commands above

### Docker-Specific Issues
1. **chip-tool access**: Ensure chip-tool is available inside the container
2. **Network access**: Container needs access to OTBR network
3. **Volume mounts**: May need to mount chip-tool binary

## 📊 Monitoring

### Logs
- Container logs: `docker logs msh_web`
- Application logs show all device control attempts
- chip-tool output is logged for debugging
- API requests are logged with Node IDs

### Status Indicators
- Device online/offline status
- Last seen timestamp
- Control success/failure messages

## 🔄 Integration with Existing Systems

### Device Management
- Devices appear in Device Management page
- Can be assigned to rooms
- Support device grouping

### Automation
- API endpoints can be used for external automation
- WebSocket support for real-time updates
- Event logging for device state changes

## 🎯 Next Steps

1. **Add More Devices**: Commission additional Matter devices
2. **Enhanced Control**: Add dimming, color control for compatible devices
3. **Automation Rules**: Create rules based on device states
4. **Mobile App**: Develop mobile interface for device control
5. **Voice Control**: Integrate with voice assistants

## 📝 Configuration

### chip-tool Path
Default: `/usr/local/bin/chip-tool`
May need to be mounted into container or installed in container

### Database Connection
- Host: `msh_db` (internal Docker network)
- Port: `5432` (internal)
- External access: `localhost:5435`

### Web Server Port
- Internal: `8082`
- External: `8083`
- Configured in Docker port mapping

## 🔐 Security

- Device control requires authentication
- API endpoints are protected
- All commands are logged for audit
- Node IDs are validated before execution

## 🐳 Docker Considerations

### Container Access to chip-tool
The container needs access to chip-tool. Options:
1. **Mount the binary**: Add volume mount in docker-compose
2. **Install in container**: Add chip-tool installation to Dockerfile
3. **Host networking**: Use host network mode

### Recommended Dockerfile Update
```dockerfile
# Add chip-tool installation
RUN apt-get update && apt-get install -y \
    build-essential \
    git \
    cmake \
    ninja-build \
    && rm -rf /var/lib/apt/lists/*

# Install Matter SDK and chip-tool
# (Add your Matter SDK installation steps here)
```

---

*This integration provides a complete solution for controlling your commissioned Matter devices through a modern web interface while maintaining direct communication with the OTBR border router in your Docker environment.*
