# Real Commissioning Testing Guide

## 🎯 Overview
This guide helps you test the new real Matter commissioning implementation with NOUS A8M devices. The system now supports both BLE and WiFi commissioning with automatic network mode switching.

## 🚀 Quick Deployment

### 1. Deploy the Updated Matter Bridge
```bash
# Auto-detect Pi IP and deploy
./deploy-matter-bridge.sh

# Or specify Pi IP manually
./deploy-matter-bridge.sh ${PI_IP}
```

### 2. Verify Deployment
```bash
# Test health endpoint
curl http://${PI_IP}:8085/health

# Test real commissioning diagnostics
curl http://${PI_IP}:8085/dev/real-commissioning-test
```

## 🧪 Testing Phases

### Phase 1: System Diagnostics

#### 1.1 Health Check
```bash
curl http://${PI_IP}:8085/health
```
**Expected Response:**
```json
{
  "status": "healthy",
  "matter_server": "python-matter-server",
  "timestamp": 1705852800.0
}
```

#### 1.2 Real Commissioning Test
```bash
curl http://${PI_IP}:8085/dev/real-commissioning-test
```
**Expected Response:**
```json
{
  "test_type": "real-commissioning",
  "timestamp": 1705852800.0,
  "results": {
    "real_commissioning_available": true,
    "commissioner_initialized": true,
    "commissioned_devices_count": 0,
    "network_mode": "normal",
    "ble_support": true,
    "wifi_support": true,
    "qr_parsing": {
      "success": true,
      "parsed_data": {
        "version": "1",
        "vendor_id": 4660,
        "product_id": 22136,
        "discriminator": 1234,
        "passcode": "20202021"
      }
    },
    "errors": []
  },
  "status": "success"
}
```

### Phase 2: Network Mode Testing

#### 2.1 Test Network Mode Switching
```bash
# Check current network mode
ssh ${PI_USER}@${PI_IP} "test -f /etc/msh/commissioning && echo 'Commissioning Mode' || echo 'Normal Mode'"

# Test switching to commissioning mode
ssh ${PI_USER}@${PI_IP} "cd ~/MSH && sudo ./network-config.sh commissioning"

# Test switching back to normal mode
ssh ${PI_USER}@${PI_IP} "cd ~/MSH && sudo ./network-config.sh normal"
```

#### 2.2 Verify Access Point Configuration
When in commissioning mode, the Pi should:
- Create WiFi network "MSH_Setup" with password "KrenWury34#"
- Assign IP 192.168.4.1 to wlan0
- Provide DHCP range 192.168.4.0/24

### Phase 3: NOUS A8M Device Commissioning

#### 3.1 Prepare NOUS A8M Device
1. **Power on the NOUS A8M socket**
2. **Put device in commissioning mode** (usually by pressing and holding a button)
3. **Note the QR code** or PIN code (default: 20202021)
4. **Ensure device is within BLE range** of the Pi

#### 3.2 Commission via Web UI
1. **Access web UI**: http://${PI_IP}:8083
2. **Navigate to Device Commissioning page**
3. **Enter device details**:
   - Device Name: "Living Room Socket"
   - Device Type: "NOUS A8M Socket"
   - QR Code: `MT:1+0x1234+0x5678+0x0000+1234+20202021` (replace with actual QR)
   - Network SSID: "08-TvM6xr-FQ"
   - Network Password: "Kartoffelernte"
4. **Click "Commission Device"**

#### 3.3 Monitor Commissioning Process
```bash
# Watch Matter bridge logs
ssh ${PI_USER}@${PI_IP} "cd ~/MSH && docker-compose -f docker-compose.prod-msh.yml logs -f matter-bridge"
```

**Expected Log Sequence:**
```
INFO: Starting real Matter device commissioning for Living Room Socket
INFO: Using real Matter commissioning implementation
INFO: Starting commissioning with QR code: MT:1+0x1234+0x5678...
INFO: Attempting BLE commissioning...
INFO: Discovering device via BLE with discriminator: 1234
INFO: Found device via BLE: <device_info>
INFO: Establishing PASE session...
INFO: PASE session established: <session_id>
INFO: Exchanging WiFi credentials...
INFO: Performing Matter commissioning...
INFO: Matter commissioning successful - Node ID: <node_id>
INFO: BLE commissioning successful
INFO: Device commissioned successfully: nous_a8m_<node_id> (method: ble-commissioning, mock: false)
```

### Phase 4: Device Control Testing

#### 4.1 Test Power Control
```bash
# Toggle device power
curl -X POST http://${PI_IP}:8085/device/nous_a8m_<node_id>/power

# Get device state
curl http://${PI_IP}:8085/device/nous_a8m_<node_id>/state

# Get power metrics
curl http://${PI_IP}:8085/device/nous_a8m_<node_id>/power-metrics
```

#### 4.2 Verify Device Response
- **Power ON**: Socket should turn on, LED indicator should light up
- **Power OFF**: Socket should turn off, LED indicator should go out
- **State Reading**: Should return current power state accurately

### Phase 5: Error Handling Testing

#### 5.1 Test BLE Failure Fallback
1. **Disable Bluetooth** on Pi temporarily
2. **Attempt commissioning** - should fall back to WiFi commissioning
3. **Re-enable Bluetooth** and test again

#### 5.2 Test Network Mode Failure
1. **Remove network-config.sh** temporarily
2. **Attempt commissioning** - should fall back to python-matter-server
3. **Restore network-config.sh** and test again

#### 5.3 Test Invalid QR Code
```bash
# Test with invalid QR code
curl -X POST http://${PI_IP}:8085/commission /
  -H "Content-Type: application/json" /
  -d '{
    "device_name": "Test Device",
    "device_type": "Test Type",
    "qr_code": "INVALID_QR_CODE",
    "network_ssid": "test",
    "network_password": "test"
  }'
```

## 🔍 Troubleshooting

### Common Issues

#### 1. Commissioner Initialization Failed
**Symptoms:** `"commissioner_initialized": false`
**Solutions:**
- Check if python-matter-server is running
- Verify chip stack dependencies are installed
- Check Docker container logs for errors

#### 2. BLE Discovery Failed
**Symptoms:** "Device not found via BLE discovery"
**Solutions:**
- Ensure Bluetooth is enabled on Pi
- Check device is in commissioning mode
- Verify device is within range
- Check discriminator matches QR code

#### 3. Network Mode Switching Failed
**Symptoms:** "Failed to switch to commissioning mode"
**Solutions:**
- Ensure network-config.sh is executable
- Check sudo permissions
- Verify hostapd and dnsmasq are installed
- Check wlan0 interface is available

#### 4. PASE Session Failed
**Symptoms:** "Failed to establish PASE session"
**Solutions:**
- Verify passcode matches device (default: 20202021)
- Check device is still in commissioning mode
- Ensure device supports Matter protocol
- Try WiFi commissioning as fallback

### Debug Commands

#### Check System Status
```bash
# Check Bluetooth status
ssh ${PI_USER}@${PI_IP} "bluetoothctl show"

# Check WiFi interfaces
ssh ${PI_USER}@${PI_IP} "iwconfig"

# Check network services
ssh ${PI_USER}@${PI_IP} "systemctl status hostapd dnsmasq"
```

#### Check Matter Bridge Logs
```bash
# View real-time logs
ssh ${PI_USER}@${PI_IP} "cd ~/MSH && docker-compose -f docker-compose.prod-msh.yml logs -f matter-bridge"

# View recent logs
ssh ${PI_USER}@${PI_IP} "cd ~/MSH && docker-compose -f docker-compose.prod-msh.yml logs --tail=100 matter-bridge"
```

#### Test Individual Components
```bash
# Test QR code parsing
curl http://${PI_IP}:8085/dev/real-commissioning-test | jq '.results.qr_parsing'

# Test network mode detection
ssh ${PI_USER}@${PI_IP} "test -f /etc/msh/commissioning && echo 'Commissioning' || echo 'Normal'"

# Test BLE availability
ssh ${PI_USER}@${PI_IP} "bluetoothctl --version"
```

## ✅ Success Criteria

### Commissioning Success
- [ ] Device appears in device list
- [ ] Device shows as "Online" status
- [ ] Commissioning method shows "ble-commissioning" or "wifi-commissioning"
- [ ] Device has valid node_id assigned
- [ ] Device appears in main database

### Control Success
- [ ] Power toggle commands work
- [ ] Device responds to on/off commands
- [ ] State reading returns accurate information
- [ ] Power metrics are available
- [ ] Real-time state updates work

### Error Handling Success
- [ ] BLE failure falls back to WiFi
- [ ] WiFi failure falls back to python-matter-server
- [ ] Network mode switching recovers from errors
- [ ] Invalid inputs are handled gracefully
- [ ] Comprehensive error logging

## 🎉 Next Steps After Successful Testing

1. **Commission multiple NOUS A8M devices** to test scalability
2. **Test different device types** if available
3. **Implement real-time state updates** using WebSocket/SignalR
4. **Add advanced device features** (scheduling, automation)
5. **Optimize performance** based on testing results

## 📞 Support

If you encounter issues during testing:
1. Check the troubleshooting section above
2. Review Matter bridge logs for detailed error information
3. Verify all prerequisites are met
4. Test with known working devices first
5. Document any new issues for future reference 