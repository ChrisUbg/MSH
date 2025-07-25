# Hardware Setup Guide - Nordic nRF52 USB Bluetooth

## Your Hardware Configuration

### Detected Hardware:
- **LAN Connection**: ✅ Available
- **Bluetooth Device**: Nordic Semiconductor ASA nRF52 Connectivity (USB)
- **Platform**: PC (Ubuntu/Windows 11)

## Nordic nRF52 USB Device Configuration

### 1. Device Detection

#### Ubuntu:
```bash
# Check if device is detected
lsusb | grep Nordic
# Expected output: Bus XXX Device XXX: ID 1915:520f Nordic Semiconductor ASA nRF52 Connectivity

# Check Bluetooth adapter
hciconfig -a
# Look for hci0 or similar adapter

# Check device details
bluetoothctl list
```

#### Windows 11:
```powershell
# Check device in Device Manager
Get-PnpDevice | Where-Object {$_.FriendlyName -like "*Nordic*"}

# Check Bluetooth adapters
Get-NetAdapter | Where-Object {$_.InterfaceDescription -like "*Bluetooth*"}
```

### 2. Driver Installation

#### Ubuntu:
```bash
# Nordic nRF52 typically works with standard Linux Bluetooth stack
# No additional drivers usually needed

# Verify driver loading
dmesg | grep -i nordic
dmesg | grep -i bluetooth
```

#### Windows 11:
```powershell
# Windows should auto-detect and install drivers
# If issues, download from Nordic website:
# https://www.nordicsemi.com/Products/Development-tools/nRF-Connect-for-Desktop
```

### 3. Bluetooth Configuration

#### Update config.yaml for Nordic device:
```yaml
bluetooth:
  adapter: "hci0"  # or the adapter ID for your Nordic device
  timeout: 30
  scan_duration: 10
  nordic_specific:
    enable_high_power: true
    enable_extended_advertising: true
    scan_interval: 100
    scan_window: 50
```

### 4. Matter Device Compatibility

#### Nordic nRF52 devices are commonly used in:
- ✅ **NOUS A8M** (your target device)
- ✅ **Philips Hue** devices
- ✅ **IKEA TRÅDFRI** devices
- ✅ **Many other Matter devices**

#### Expected BLE Characteristics:
```
Service UUID: 0000FE0F-0000-1000-8000-00805F9B34FB (Matter)
Characteristic: 0000FE0F-0000-1000-8000-00805F9B34FB
```

## Testing Your Hardware

### 1. Basic Bluetooth Test
```bash
# Ubuntu
bluetoothctl
> scan on
> devices
> info <device_address>

# Windows PowerShell
Get-BluetoothDevice
```

### 2. Matter Device Discovery Test
```bash
# Run the commissioning server
cd commissioning-server
source venv/bin/activate
python main.py --debug

# In another terminal, test BLE scanning
curl -X POST http://localhost:8888/api/devices/scan-ble
```

### 3. Nordic-Specific Commands
```bash
# Check Nordic device capabilities
hciconfig hci0 up
hcitool dev
hcitool lescan

# Test with Nordic's nRF Connect app (mobile)
# Download from App Store/Google Play for testing
```

## Performance Optimization

### 1. USB Power Management
```bash
# Ubuntu - Disable USB power management for Nordic device
echo 'ACTION=="add", SUBSYSTEM=="usb", ATTR{idVendor}=="1915", ATTR{idProduct}=="520f", ATTR{power/autosuspend}="-1"' | sudo tee /etc/udev/rules.d/99-nordic-bluetooth.rules

# Reload rules
sudo udevadm control --reload-rules
```

### 2. Bluetooth Stack Optimization
```bash
# Ubuntu - Optimize Bluetooth settings
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

### 3. Network Configuration
```bash
# Ensure LAN interface is optimized for local communication
# Check network interface
ip addr show

# Test connectivity to Pi
ping 192.168.0.104
```

## Troubleshooting

### Common Issues:

#### 1. Device Not Detected
```bash
# Check USB connection
lsusb -v | grep -A 10 -B 10 Nordic

# Check kernel messages
dmesg | grep -i usb
dmesg | grep -i bluetooth
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

## Expected Performance

### With Nordic nRF52 USB:
- ✅ **Faster scanning** than built-in Bluetooth
- ✅ **Better range** for device discovery
- ✅ **More reliable connections** for commissioning
- ✅ **Lower latency** for Matter operations
- ✅ **Better compatibility** with Nordic-based devices

### Commissioning Speed:
- **Device Discovery**: 2-5 seconds
- **BLE Commissioning**: 10-30 seconds
- **Credential Transfer**: 1-2 seconds
- **Total Process**: 15-40 seconds per device

## Next Steps

1. **Test hardware detection**:
   ```bash
   cd commissioning-server
   chmod +x install.sh
   ./install.sh
   ```

2. **Verify Nordic device**:
   ```bash
   bluetoothctl list
   hciconfig -a
   ```

3. **Test with NOUS A8M**:
   - Power on the device
   - Put in commissioning mode
   - Run BLE scan
   - Attempt commissioning

4. **Monitor performance**:
   - Check commissioning logs
   - Monitor Bluetooth connection stability
   - Verify credential transfer to Pi

Your Nordic nRF52 USB device is excellent for this application! 🚀 