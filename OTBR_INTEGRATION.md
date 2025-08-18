# OpenThread Border Router Integration with MSH

## Overview

This document describes the integration of OpenThread Border Router (OTBR) with your MSH (Matter Smart Home) system using Docker with hardware passthrough.

## Architecture

```
MSH System (Pi - 192.168.0.107)
├── MSH Commissioning Server (Port 8080)
│   ├── BLE Commissioning (Nordic dongle)
│   ├── WiFi Devices (NOUS A8M sockets)
│   └── Device Management
├── OpenThread Border Router (Port 9999)
│   ├── Thread Network (Nordic dongle)
│   ├── Thread Devices (arre temperature sensor)
│   └── Thread-WiFi Bridge
└── Unified Control Interface
```

## Port Configuration

- **MSH Commissioning Server**: `http://192.168.0.107:8080`
- **OpenThread Border Router Web UI**: `http://192.168.0.107:9999`
- **OpenThread Border Router REST API**: `http://192.168.0.107:8081`

## Hardware Requirements

### Nordic nRF52840 Dongle
- **Current Use**: BLE commissioning for MSH
- **New Use**: Thread border router for OTBR
- **Configuration**: Can be used for both BLE and Thread (with firmware switching)

## Deployment

### 1. Deploy to Pi
```bash
# On the Raspberry Pi
cd /home/chris/RiderProjects/MSH
./deploy-otbr-to-pi.sh
```

### 2. Test Deployment
```bash
# Test the setup
./test-otbr.sh
```

### 3. Access Web Interface
- Open browser to: `http://192.168.0.107:9999`
- Configure Thread network settings
- Commission Thread devices

## Docker Configuration

### File: `docker/docker-compose-otbr.yml`
```yaml
version: '3.8'
services:
  otbr:
    image: openthread/otbr:latest
    container_name: otbr-border-router
    restart: unless-stopped
    privileged: true
    network_mode: host
    devices:
      - /dev/bus/usb:/dev/bus/usb
    volumes:
      - otbr_data:/var/lib/thread
    ports:
      - "9999:8080"  # Web interface
      - "8081:8081"  # REST API
    environment:
      - OTBR_INFRA_IF_NAME=wlan0
      - OTBR_DBUS=ON
      - OTBR_WEB=ON
      - OTBR_REST=ON
      - OTBR_FIREWALL=ON
      - OTBR_BACKBONE_ROUTER=ON
      - OTBR_NAT64=ON
```

## Management Commands

### Start OTBR
```bash
docker-compose -f docker/docker-compose-otbr.yml up -d
```

### Stop OTBR
```bash
docker-compose -f docker/docker-compose-otbr.yml down
```

### View Logs
```bash
docker-compose -f docker/docker-compose-otbr.yml logs -f otbr
```

### Restart OTBR
```bash
docker-compose -f docker/docker-compose-otbr.yml restart
```

## Thread Device Commissioning

### 1. Access Web Interface
- Navigate to `http://192.168.0.107:9999`
- Click "Join Thread Network"

### 2. Commission Thread Device (arre sensor)
- Put device in commissioning mode
- Scan QR code or enter manual code
- Device joins Thread network

### 3. Verify Commissioning
```bash
# Check Thread network status
docker-compose -f docker/docker-compose-otbr.yml exec otbr ot-ctl state
```

## Integration with MSH

### Current MSH Capabilities
- ✅ BLE device commissioning (NOUS A8M)
- ✅ WiFi device management
- ✅ Device control via API
- ✅ Database storage

### New OTBR Capabilities
- ✅ Thread device commissioning (arre sensor)
- ✅ Thread-WiFi bridging
- ✅ Thread network management
- ✅ Thread device control

### Combined Architecture
```
MSH Commissioning Server
├── BLE Commissioning → NOUS A8M sockets
├── WiFi Management → WiFi devices
└── Thread Integration → arre temperature sensor

OpenThread Border Router
├── Thread Network → Thread devices
├── Thread-WiFi Bridge → Cross-protocol communication
└── Thread Management → Network configuration
```

## Testing Thread Devices

### 1. Commission arre Temperature Sensor
```bash
# Access OTBR web interface
http://192.168.0.107:9999

# Follow commissioning process
# 1. Put arre sensor in commissioning mode
# 2. Scan QR code or enter manual code
# 3. Device joins Thread network
```

### 2. Test Thread Device Control
```bash
# Via REST API
curl -X GET http://192.168.0.107:8081/api/thread/network

# Via Web Interface
http://192.168.0.107:9999
```

### 3. Verify Cross-Protocol Communication
- Thread sensor → WiFi device control
- WiFi device → Thread sensor reading
- Unified automation across protocols

## Troubleshooting

### Common Issues

#### 1. Nordic Dongle Not Detected
```bash
# Check USB device
lsusb | grep Nordic

# Check device permissions
ls -la /dev/bus/usb/

# Fix permissions if needed
sudo chmod 666 /dev/bus/usb/001/002
```

#### 2. Port Conflicts
```bash
# Check what's using port 9999
sudo netstat -tlnp | grep 9999

# Stop conflicting service
sudo systemctl stop conflicting-service
```

#### 3. Container Won't Start
```bash
# Check container logs
docker-compose -f docker/docker-compose-otbr.yml logs otbr

# Check system resources
docker system df
docker system prune
```

#### 4. Thread Network Issues
```bash
# Check Thread network status
docker-compose -f docker/docker-compose-otbr.yml exec otbr ot-ctl state

# Reset Thread network if needed
docker-compose -f docker/docker-compose-otbr.yml exec otbr ot-ctl factoryreset
```

## Next Steps

### 1. Commission Thread Devices
- Deploy OTBR to Pi
- Commission arre temperature sensor
- Test Thread device functionality

### 2. Integrate with MSH
- Connect OTBR REST API to MSH
- Create unified device management
- Implement cross-protocol automation

### 3. Expand Thread Network
- Add more Thread devices
- Test mesh network capabilities
- Optimize network performance

### 4. Production Deployment
- Configure persistent storage
- Set up monitoring and logging
- Implement backup and recovery

## Benefits

### For Your MSH System
- ✅ **Extended Device Support**: Thread devices (arre sensor)
- ✅ **Mesh Network**: Self-healing Thread network
- ✅ **Low Power**: Battery-efficient Thread devices
- ✅ **Unified Control**: Single interface for all protocols

### For Thread Devices
- ✅ **Mesh Networking**: Extended range via device relaying
- ✅ **Low Power**: Long battery life for sensors
- ✅ **Reliable**: Self-healing network topology
- ✅ **Standard**: Open Thread protocol

## Resources

- [OpenThread Border Router Documentation](https://openthread.io/guides/border-router)
- [Thread Protocol Specification](https://threadgroup.org/)
- [Nordic nRF52840 Documentation](https://www.nordicsemi.com/Products/nRF52840)
- [arre Temperature Sensor Manual](https://arre.com/thread-sensor) 