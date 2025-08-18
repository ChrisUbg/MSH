# NOUS A8M Matter Socket Commissioning Guide

## Device Information
- **Device Type**: NOUS A8M Matter Socket
- **Commissioning Method**: BLE-WiFi (required)
- **Factory Reset**: 6-second button hold (not 10-15 seconds)
- **QR Code Format**: `0150-XXX-XXXX` or `3096-XXX-XXXX`

## Successful Commissioning Parameters

### Device 1 (Office-Socket 1)
- **QR Code**: `0150-175-1910`
- **Passcode**: `85064361`
- **Discriminator**: `97` (from BLE scan)
- **Network SSID**: `08-TvM6xr-FQ`
- **Network Password**: `Kartoffelernte`
- **Node ID**: `4328ED19954E9DC0`
- **Status**: ✅ Successfully commissioned and controllable

### Device 2 (Office-Socket 2)
- **QR Code**: `3096-783-6060`
- **Passcode**: `59090382`
- **Discriminator**: `3078` (from BLE scan)
- **Network SSID**: `08-TvM6xr-FQ`
- **Network Password**: `Kartoffelernte`
- **Node ID**: `4328ED19954E9DC1`
- **Status**: ✅ Successfully commissioned and controllable

## Key Success Factors

1. **Correct Factory Reset**: Hold button for exactly 6 seconds (not 10-15 seconds)
2. **BLE-WiFi Method**: Must use `chip-tool pairing ble-wifi` (not code-based)
3. **Discriminator Handling**: Use actual discriminator from BLE scan, not QR code
4. **Attestation Bypass**: Always use `--bypass-attestation-verifier true` for NOUS devices
5. **Unique Node IDs**: Each device must have a unique 64-bit Node ID

## Step-by-Step Commissioning Process

### 1. Factory Reset Device
```bash
# Hold the power button for exactly 6 seconds
# LED should flash rapidly, indicating pairing mode
```

### 2. Direct chip-tool Commissioning (Recommended)
```bash
# Device 1
/usr/local/bin/chip-tool pairing ble-wifi 0x4328ED19954E9DC0 08-TvM6xr-FQ Kartoffelernte 85064361 97 --bypass-attestation-verifier true

# Device 2  
/usr/local/bin/chip-tool pairing ble-wifi 0x4328ED19954E9DC1 08-TvM6xr-FQ Kartoffelernte 59090382 3078 --bypass-attestation-verifier true
```

### 3. Test Device Control
```bash
# Toggle Device 1
/usr/local/bin/chip-tool onoff toggle 0x4328ED19954E9DC0 1

# Toggle Device 2
/usr/local/bin/chip-tool onoff toggle 0x4328ED19954E9DC1 1
```

## API Commissioning Process (Improved)

The API now uses the same reliable BLE-WiFi method with enhanced error handling:

### API Endpoint
```
POST http://localhost:8888/commission
```

### Request Format
```json
{
    "device_name": "office-socket-1",
    "device_type": "NOUS A8M Socket",
    "qr_code": "0150-175-1910",
    "network_ssid": "08-TvM6xr-FQ",
    "network_password": "Kartoffelernte",
    "pi_ip": "192.168.0.107"
}
```

### API Features
- **Automatic Discriminator Detection**: Scans BLE for actual discriminator
- **Dynamic Node ID Generation**: Creates unique 64-bit Node IDs
- **Enhanced Error Handling**: Provides specific error messages
- **Device Control Testing**: Automatically tests control after commissioning
- **Persistent Storage**: Saves device-to-Node-ID mappings

### Testing API Commissioning
```bash
# Test both devices via API
./commission_via_api.sh both

# Test individual devices
./commission_via_api.sh device1
./commission_via_api.sh device2

# Python test script
python3 test_api_commissioning.py
```

## Troubleshooting

### Common Issues
1. **LED keeps blinking**: Commissioning not completed - check discriminator and passcode
2. **"Failed to verify peer's MAC"**: Wrong discriminator - use BLE scan result
3. **"Invalid argument destination-id"**: Node ID format issue - use 64-bit hex with 0x prefix
4. **Device not responding**: Wrong Node ID - clear storage and re-commission

### Solutions
1. **Factory Reset**: Use 6-second button hold, not 10-15 seconds
2. **Discriminator**: Always use BLE scan result, not QR code discriminator
3. **Node IDs**: Ensure unique 64-bit Node IDs for each device
4. **Network**: Verify SSID and password are correct

## Device-Specific Notes

- **NOUS A8M**: Requires BLE-WiFi method, cannot use code-based commissioning
- **Attestation**: Always bypass attestation verification
- **Discriminator**: QR code discriminator is incorrect, use BLE scan result
- **Factory Reset**: 6-second hold, not 10-15 seconds as commonly documented

## Network Requirements

- **SSID**: `08-TvM6xr-FQ`
- **Password**: `Kartoffelernte`
- **Security**: WPA2/WPA3
- **Band**: 2.4GHz (required for Matter)

## Data Capture Requirements

For successful commissioning, capture and store:
- Device name and type
- QR code
- Actual discriminator (from BLE scan)
- Passcode (from QR code parsing)
- Network credentials
- Generated Node ID
- Commissioning method used

## Implementation Status

- ✅ **Direct chip-tool commissioning**: Working reliably
- ✅ **API commissioning**: Improved with BLE-WiFi method
- ✅ **Device control**: Both devices independently controllable
- ✅ **Unique Node IDs**: Dynamic generation and persistence
- ✅ **Error handling**: Enhanced with specific error messages
- ✅ **Documentation**: Complete and up-to-date 