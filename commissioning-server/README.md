# MSH Commissioning Server

Native PC application for Matter device commissioning with BLE support.

## Overview

The MSH Commissioning Server is a **PC-based application** that handles:
- **BLE device discovery** and scanning (via PC Bluetooth adapter)
- **Matter device commissioning** using chip-tool (on PC) ✅ **WORKING**
- **Credential management** and secure storage
- **Transfer to Pi** for device control and management

## ✅ **Recent Achievements**

### **Device Commissioning Success**
- **Device 1 (Office-Socket 1)**: Node ID `4328ED19954E9DC0` - ✅ **WORKING**
- **Device 2 (Office-Socket 2)**: Node ID `4328ED19954E9DC1` - ✅ **WORKING**
- **Independent Control**: Both devices respond to individual toggle commands
- **API Commissioning**: Enhanced with reliable BLE-WiFi method

### **Technical Breakthroughs**
- **Dynamic Node ID Generation**: Unique 64-bit Node IDs preventing conflicts
- **Automatic Discriminator Detection**: BLE scan for actual device discriminators
- **Enhanced Error Handling**: Specific error messages for troubleshooting
- **Persistent Storage**: Device-to-Node-ID mappings saved to file

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    PC Commissioning Server                  │
├─────────────────────────────────────────────────────────────┤
│  Web Interface (Port 8888)                                  │
│  ├── Device Discovery (BLE Scanner) ✅ WORKING              │
│  ├── Commissioning Interface (chip-tool) ✅ WORKING         │ 
│  └── Credential Management                                  │
├─────────────────────────────────────────────────────────────┤
│  Commissioning API (Port 8888) ✅ WORKING                   │
│  ├── BLE Commissioning (PC Bluetooth) ✅ WORKING            │
│  ├── WiFi Commissioning (PC Network) ✅ WORKING             │
│  └── Credential Storage (Local)                            │
├─────────────────────────────────────────────────────────────┤
│  Matter SDK Integration (PC) ✅ WORKING                     │
│  ├── chip-tool (PC Native) ✅ WORKING                       │
│  ├── chip-repl (PC Native)                                 │
│  └── BLE Stack (PC Bluetooth Adapter) ✅ WORKING           │
├─────────────────────────────────────────────────────────────┤
│  System Integration (PC) ✅ WORKING                         │
│  ├── Bluetooth Stack (USB Adapter) ✅ WORKING               │ 
│  ├── Network Stack (WiFi/Ethernet) ✅ WORKING               │
│  └── File System (Local Storage) ✅ WORKING                 │
└─────────────────────────────────────────────────────────────┘
                    ↓ (SSH Transfer)
┌─────────────────────────────────────────────────────────────┐
│                    Raspberry Pi (Target)                   │
├─────────────────────────────────────────────────────────────┤
│  Device Control & Management                                │
│  ├── Device State Monitoring                               │
│  ├── Automation Rules                                       │
│  └── Web Interface (Port 8083)                             │
└─────────────────────────────────────────────────────────────┘
```

## Hardware Requirements

### **PC Requirements (Commissioning Server)**
- **PC**: Ubuntu 20.04+ or Windows 11
- **Bluetooth Adapter**: USB Bluetooth adapter for BLE commissioning ✅ **WORKING**
- **Network**: LAN connection for Pi communication
- **Storage**: 1GB free space for dependencies
- **Matter SDK**: Installed on PC for commissioning tools ✅ **WORKING**

### **Raspberry Pi Requirements (Target)**
- **Pi**: Raspberry Pi 4 (2GB+ RAM)
- **Network**: LAN connection for device control
- **Storage**: 8GB+ SD card for device management
- **Purpose**: Device control and automation (not commissioning)

## Quick Start

### 1. Environment Setup

```bash
# Navigate to commissioning server
cd commissioning-server

# Create and activate environment
./env-switcher.sh create commissioning-server
./env-switcher.sh commissioning-server
```

### 2. Hardware Test

```bash
# Test your hardware setup
python test-hardware.py
```

### 3. Start Server

```bash
# Start the commissioning server
python main.py --debug

# Or with custom settings
python main.py --host 0.0.0.0 --port 8888 --debug
```

### 4. Access Web Interface

- **Main Interface**: http://localhost:8888
- **API Documentation**: http://localhost:8888/docs
- **Health Check**: http://localhost:8888/

### 5. Test Commissioning

```bash
# Test API commissioning for both devices
./commission_via_api.sh both

# Test individual devices
./commission_via_api.sh device1
./commission_via_api.sh device2

# Python test script
python3 test_api_commissioning.py
```

## Implementation Status

### **✅ Fully Functional Components**
- **BLE Device Discovery**: Successfully finding MATTER devices
- **Device Commissioning**: Both NOUS A8M devices commissioned successfully
- **API Endpoints**: All commissioning and control endpoints working
- **Device Control**: Both devices independently controllable
- **Error Handling**: Enhanced with specific error messages
- **Testing Tools**: Comprehensive testing scripts created

### **🔧 Technical Features**
- **Dynamic Node ID Generation**: Unique 64-bit Node IDs for each device
- **Automatic Discriminator Detection**: BLE scan for actual device discriminators
- **Persistent Storage**: Device-to-Node-ID mappings saved to `device_node_mappings.json`
- **Enhanced API Responses**: Detailed commissioning results and error messages
- **Device Control Integration**: Automatic control testing after commissioning

### **📊 Commissioning Success**
```json
{
  "device_1": {
    "name": "Office-Socket 1",
    "qr_code": "0150-175-1910",
    "passcode": "85064361",
    "discriminator": "97",
    "node_id": "4328ED19954E9DC0",
    "status": "✅ WORKING"
  },
  "device_2": {
    "name": "Office-Socket 2", 
    "qr_code": "3096-783-6060",
    "passcode": "59090382",
    "discriminator": "3078",
    "node_id": "4328ED19954E9DC1",
    "status": "✅ WORKING"
  }
}
```

## API Endpoints

### **Commissioning**
- `POST /commission` - Commission a Matter device using BLE-WiFi method
- `GET /api/status` - Server status and health check
- `POST /api/devices/scan-ble` - Scan for BLE devices

### **Device Control**
- `POST /api/devices/control` - Control commissioned devices
- `GET /api/devices/{device_id}/credentials` - Get device credentials
- `POST /api/devices/transfer-credentials` - Transfer credentials to Pi

### **Testing**
- `GET /api/test-qr` - Test QR code parsing
- `WebSocket /ws` - Real-time commissioning updates

## Key Success Factors

1. **Correct Factory Reset**: 6-second button hold (not 10-15 seconds)
2. **BLE-WiFi Method**: Must use `chip-tool pairing ble-wifi` (not code-based)
3. **Discriminator Handling**: Use actual discriminator from BLE scan, not QR code
4. **Attestation Bypass**: Always use `--bypass-attestation-verifier true` for NOUS devices
5. **Unique Node IDs**: Each device must have a unique 64-bit Node ID

## Troubleshooting

### **Common Issues**
- **LED keeps blinking**: Commissioning not completed - check discriminator and passcode
- **"Failed to verify peer's MAC"**: Wrong discriminator - use BLE scan result
- **"Invalid argument destination-id"**: Node ID format issue - use 64-bit hex with 0x prefix
- **Device not responding**: Wrong Node ID - clear storage and re-commission

### **Solutions**
- **Factory Reset**: Use 6-second button hold, not 10-15 seconds
- **Discriminator**: Always use BLE scan result, not QR code discriminator
- **Node IDs**: Ensure unique 64-bit Node IDs for each device
- **Network**: Verify SSID and password are correct

## Documentation

- **[NOUS A8M Commissioning Guide](NOUS_A8M_COMMISSIONING_GUIDE.md)** - Complete working guide
- **[API Improvements](API_IMPROVEMENTS.md)** - Enhanced API documentation
- **[Deployment Guide](DEPLOYMENT.md)** - Docker and deployment instructions
- **[Hardware Setup](hardware-setup.md)** - Hardware configuration guide

## Next Steps

1. **Test API Commissioning**: Use `./commission_via_api.sh both`
2. **Integrate with Web Application**: Add commissioned devices to web UI
3. **Database Integration**: Store commissioning data in PostgreSQL
4. **Automation**: Develop automated commissioning workflows
5. **Device Management**: Implement ongoing device control and monitoring 