# Nordic nRF52840 Dongle Integration Guide

## Overview

This guide explains how to integrate the Nordic Semiconductor nRF52840 USB Dongle with your MSH (Matter Smart Home) commissioning system for enhanced BLE device discovery and commissioning.

## Hardware Requirements

### Nordic nRF52840 Dongle
- **Model**: Nordic Semiconductor nRF52 Connectivity
- **USB ID**: 1915:520f (Nordic Semiconductor ASA nRF52 Connectivity)
- **Firmware**: Nordic Connectivity Firmware (recommended)
- **Capabilities**: 
  - High-power BLE transmission
  - Extended advertising support
  - Advanced scanning capabilities
  - Matter protocol compatibility

## Installation and Setup

### 1. Hardware Connection
```bash
# Check if dongle is detected
lsusb | grep Nordic
# Expected: Bus XXX Device XXX: ID 1915:520f Nordic Semiconductor ASA nRF52 Connectivity

# Check Bluetooth adapter
hciconfig -a
# Look for hci0 or similar adapter
```

### 2. Driver Installation

#### Ubuntu/Debian:
```bash
# Nordic nRF52 typically works with standard Linux Bluetooth stack
sudo apt-get update
sudo apt-get install -y bluetooth bluez libbluetooth-dev

# Verify driver loading
dmesg | grep -i nordic
dmesg | grep -i bluetooth
```

#### Windows 11:
- Windows should auto-detect and install drivers
- If issues, download from Nordic website: https://www.nordicsemi.com/Products/Development-tools/nRF-Connect-for-Desktop

### 3. Firmware Verification
```bash
# Check if Nordic Connectivity firmware is loaded
bluetoothctl list
# Should show Nordic adapter with proper capabilities

# Test basic functionality
bluetoothctl
> scan on
> devices
> scan off
```

## Configuration

### 1. Update config.yaml
The configuration has been enhanced with Nordic-specific optimizations:

```yaml
bluetooth:
  adapter: hci0
  scan_duration: 10
  timeout: 30
  # Nordic nRF52840 specific optimizations
  nordic_optimizations:
    enable_high_power: true
    enable_extended_advertising: true
    scan_interval: 100  # ms - faster scanning
    scan_window: 50     # ms - longer scan window
    advertising_interval: 100  # ms - faster advertising
    connection_interval: 15   # ms - faster connection updates
  # Advanced scanning options
  advanced_scanning:
    enable_duplicate_filtering: true
    enable_whitelist: false
    scan_type: "active"  # active vs passive
    filter_rssi: -80     # dBm - filter weak signals
```

### 2. Enhanced BLE Scanner
A new `NordicBLEScanner` class provides enhanced capabilities:

```python
from commissioning_server.core.nordic_ble_scanner import NordicBLEScanner

# Initialize enhanced scanner
nordic_scanner = NordicBLEScanner(config)

# Check availability
if await nordic_scanner.is_available():
    # Apply optimizations
    await nordic_scanner.optimize_adapter()
    
    # Enhanced device scanning
    devices = await nordic_scanner.scan_devices_enhanced(timeout=15)
```

## Testing Integration

### 1. Run the Test Script
```bash
cd commissioning-server
python test_nordic_dongle.py
```

This will test:
- ✅ Nordic adapter detection
- ✅ Hardware optimization
- ✅ Enhanced BLE scanning
- ✅ Matter device detection
- ✅ Performance metrics

### 2. Manual Testing
```bash
# Test basic Bluetooth functionality
bluetoothctl
> list
> scan on
> devices
> info <device_address>

# Test Nordic-specific commands
hciconfig hci0 up
hcitool dev
hcitool lescan
```

## Enhanced Features

### 1. High-Power Mode
- **Benefit**: Extended BLE range and better signal penetration
- **Configuration**: Automatically enabled in Nordic optimizations
- **Use Case**: Better discovery of devices in different rooms

### 2. Extended Advertising
- **Benefit**: Faster device discovery and more reliable connections
- **Configuration**: Enabled by default for Nordic dongle
- **Use Case**: Improved Matter device commissioning success rate

### 3. Advanced Scanning
- **Benefit**: Better filtering and detection of Matter devices
- **Features**:
  - RSSI-based filtering (-80 dBm threshold)
  - Duplicate filtering
  - Active scanning mode
  - Optimized scan intervals

### 4. Enhanced Device Detection
- **Matter Device Recognition**: Improved detection of NOUS A8M, Philips Hue, IKEA TRÅDFRI
- **Signal Quality**: Better handling of weak signals
- **Connection Stability**: More reliable BLE connections

## Integration with Existing System

### 1. Commissioning Server Integration
The Nordic scanner can be integrated into your existing commissioning server:

```python
# In your commissioning server
from commissioning_server.core.nordic_ble_scanner import NordicBLEScanner

class EnhancedCommissioningServer:
    def __init__(self):
        self.nordic_scanner = NordicBLEScanner(config)
    
    async def discover_devices(self):
        # Use enhanced scanning
        devices = await self.nordic_scanner.scan_devices_enhanced()
        return devices
    
    async def commission_device(self, device_address):
        # Enhanced connection with retries
        success = await self.nordic_scanner.connect_device_enhanced(device_address)
        return success
```

### 2. API Endpoints
Enhanced endpoints for Nordic dongle:

```http
# Enhanced BLE scanning
POST /api/devices/scan-ble-enhanced
{
  "timeout": 15,
  "high_power": true,
  "extended_advertising": true
}

# Nordic-specific device info
GET /api/devices/{device_address}/info-enhanced
```

## Performance Optimization

### 1. USB Power Management
```bash
# Disable USB power management for Nordic device
echo 'ACTION=="add", SUBSYSTEM=="usb", ATTR{idVendor}=="1915", ATTR{idProduct}=="520f", ATTR{power/autosuspend}="-1"' | sudo tee /etc/udev/rules.d/99-nordic-bluetooth.rules

# Reload rules
sudo udevadm control --reload-rules
```

### 2. Bluetooth Stack Optimization
```bash
# Optimize Bluetooth settings
sudo tee /etc/bluetooth/main.conf << EOF
[General]
Name = MSH-Commissioning
Class = 0x000000
DiscoverableTimeout = 0
PairableTimeout = 0

[Policy]
AutoEnable=true
EOF

# Restart Bluetooth service
sudo systemctl restart bluetooth
```

## Troubleshooting

### Common Issues

#### 1. Device Not Detected
```bash
# Check USB connection
lsusb -v | grep -A 10 -B 10 Nordic

# Check kernel messages
dmesg | grep -i usb
dmesg | grep -i bluetooth

# Reset USB port
sudo usb-reset 1915:520f
```

#### 2. Permission Issues
```bash
# Add user to bluetooth group
sudo usermod -a -G bluetooth $USER

# Log out and back in, or run:
newgrp bluetooth
```

#### 3. Connection Drops
```bash
# Check USB power management
cat /sys/bus/usb/devices/*/power/autosuspend

# Disable power management for Nordic device
echo -1 | sudo tee /sys/bus/usb/devices/*/power/autosuspend
```

#### 4. Poor Performance
```bash
# Check Nordic adapter status
hciconfig hci0

# Reset adapter
sudo hciconfig hci0 down
sudo hciconfig hci0 up

# Check scan parameters
hcitool lescan --duplicates
```

## Matter Device Compatibility

### Supported Devices
The Nordic dongle is optimized for:
- ✅ **NOUS A8M** (your target device)
- ✅ **Philips Hue** devices
- ✅ **IKEA TRÅDFRI** devices
- ✅ **Google Nest** devices
- ✅ **Amazon Echo** devices
- ✅ **Other Matter-compatible devices**

### Expected BLE Characteristics
```
Service UUID: 0000FE0F-0000-1000-8000-00805F9B34FB (Matter)
Characteristic: 0000FE0F-0000-1000-8000-00805F9B34FB
```

## Next Steps

### 1. Test with Real Devices
```bash
# Run the test script
python test_nordic_dongle.py

# Start commissioning server with Nordic enhancements
python main.py --nordic-enhanced
```

### 2. Monitor Performance
- Track commissioning success rates
- Monitor BLE connection stability
- Measure device discovery speed
- Compare with previous Bluetooth adapter

### 3. Fine-tune Configuration
- Adjust scan intervals based on your environment
- Optimize RSSI filtering for your use case
- Configure retry attempts for better reliability

## Benefits Summary

### Enhanced Performance
- **Extended Range**: High-power mode for better coverage
- **Faster Discovery**: Optimized scan parameters
- **Better Reliability**: Enhanced connection handling
- **Improved Compatibility**: Better Matter device support

### Integration Benefits
- **Seamless Integration**: Works with existing MSH system
- **Backward Compatibility**: Falls back to standard BLE if needed
- **Enhanced APIs**: New endpoints for advanced features
- **Better Diagnostics**: Improved error handling and logging

The Nordic nRF52840 dongle provides significant improvements for Matter device commissioning, especially for your NOUS A8M devices and other Matter-compatible smart home devices. 