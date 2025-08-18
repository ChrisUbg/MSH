# Matter Device Commissioning Process Documentation

## 🎯 **Overview**

This document details the commissioning process for Matter devices in the MSH system, including both standard and non-standard approaches that were required for our specific hardware.

---

## 📋 **Current Commissioned Devices**

### **✅ Successfully Commissioned Devices**

#### **Device 1: Office Socket 1**
- **Manufacturer**: NOUS A8M
- **Type**: Smart Socket
- **QR Code**: `0150-175-1910`
- **Passcode**: `85064361`
- **Discriminator**: `97`
- **Node ID**: `4328ED19954E9DC0` (hex)
- **Status**: ✅ **WORKING**

#### **Device 2: Office Socket 2**
- **Manufacturer**: NOUS A8M
- **Type**: Smart Socket
- **QR Code**: `3096-783-6060`
- **Passcode**: `59090382`
- **Discriminator**: `3078`
- **Node ID**: `4328ED19954E9DC1` (hex)
- **Status**: ✅ **WORKING**

---

## 🚨 **Non-Standard Commissioning Process**

### **⚠️ Important: These devices required non-standard commissioning**

The NOUS A8M devices did **NOT** follow the standard Matter commissioning process. This is critical information for future device additions.

### **Standard vs Non-Standard Process**

#### **Standard Matter Commissioning (Expected)**
1. Scan QR code
2. Extract passcode and discriminator
3. Use `chip-tool pairing code` command
4. Device joins Matter fabric automatically

#### **Non-Standard Process (What Actually Worked)**
1. **Factory Reset**: 6-second button hold (not 10-15 seconds)
2. **BLE-WiFi Method**: Must use `chip-tool pairing ble-wifi` (not code-based)
3. **Discriminator Handling**: Use actual discriminator from BLE scan, not QR code
4. **Attestation Bypass**: Always use `--bypass-attestation-verifier true`
5. **Unique Node IDs**: Each device must have a unique 64-bit Node ID

---

## 🔧 **Commissioning Commands Used**

### **Device 1 Commissioning**
```bash
# Factory reset (6 seconds, not 10-15)
# Then scan for device
sudo bluetoothctl scan on

# Commission using BLE-WiFi method
chip-tool pairing ble-wifi 97 85064361 20202021 3840 \
  --bypass-attestation-verifier true \
  --node-id 0x4328ED19954E9DC0
```

### **Device 2 Commissioning**
```bash
# Factory reset (6 seconds, not 10-15)
# Then scan for device
sudo bluetoothctl scan on

# Commission using BLE-WiFi method
chip-tool pairing ble-wifi 3078 59090382 20202021 3840 \
  --bypass-attestation-verifier true \
  --node-id 0x4328ED19954E9DC1
```

---

## 📊 **Key Differences from Standard**

### **1. Factory Reset Timing**
- **Standard**: 10-15 seconds
- **NOUS A8M**: 6 seconds exactly
- **Impact**: Wrong timing prevents proper reset

### **2. Commissioning Method**
- **Standard**: `chip-tool pairing code <passcode> <discriminator>`
- **NOUS A8M**: `chip-tool pairing ble-wifi <discriminator> <passcode> <ssid> <password>`
- **Impact**: Code-based method fails completely

### **3. Discriminator Source**
- **Standard**: Extract from QR code
- **NOUS A8M**: Use actual discriminator from BLE scan
- **Impact**: QR code discriminator may be incorrect

### **4. Attestation Verification**
- **Standard**: Automatic attestation verification
- **NOUS A8M**: Must bypass attestation verification
- **Impact**: Standard process fails with attestation errors

### **5. Node ID Assignment**
- **Standard**: Automatic Node ID assignment
- **NOUS A8M**: Manual unique Node ID assignment required
- **Impact**: Prevents Node ID conflicts

---

## 🔍 **Troubleshooting Commissioning Issues**

### **Common Problems and Solutions**

#### **Problem 1: Device Not Found in BLE Scan**
```bash
# Solution: Check Bluetooth adapter
sudo hciconfig hci0 up
sudo bluetoothctl scan on

# Look for devices with MATTER prefix
# Example: MATTER-0097, MATTER-3078
```

#### **Problem 2: Attestation Verification Failed**
```bash
# Solution: Always use bypass flag
--bypass-attestation-verifier true
```

#### **Problem 3: Node ID Conflict**
```bash
# Solution: Use unique 64-bit Node IDs
--node-id 0x4328ED19954E9DC0  # Device 1
--node-id 0x4328ED19954E9DC1  # Device 2
```

#### **Problem 4: Factory Reset Not Working**
```bash
# Solution: Use 6-second hold, not 10-15 seconds
# Hold button for exactly 6 seconds, then release
```

---

## 📝 **Commissioning Checklist**

### **Pre-Commissioning**
- [ ] Device powered on and in factory reset mode
- [ ] Bluetooth adapter working (`sudo hciconfig hci0 up`)
- [ ] WiFi network credentials ready
- [ ] Unique Node ID prepared
- [ ] Device discriminator identified via BLE scan

### **Commissioning Process**
- [ ] Factory reset (6-second button hold)
- [ ] BLE scan for device (`sudo bluetoothctl scan on`)
- [ ] Note actual discriminator from scan
- [ ] Execute `chip-tool pairing ble-wifi` command
- [ ] Verify device joins Matter fabric
- [ ] Test device control commands

### **Post-Commissioning**
- [ ] Verify device responds to control commands
- [ ] Check LED status (solid = commissioned)
- [ ] Add device to database
- [ ] Test web interface control
- [ ] Document commissioning parameters

---

## 🧪 **Testing Commissioned Devices**

### **Device Control Commands**
```bash
# Read device status
chip-tool onoff read on-off 0x4328ED19954E9DC0 1

# Turn device ON
chip-tool onoff on 0x4328ED19954E9DC0 1

# Turn device OFF
chip-tool onoff off 0x4328ED19954E9DC0 1

# Toggle device
chip-tool onoff toggle 0x4328ED19954E9DC0 1
```

### **Expected Responses**
```bash
# Successful read
CHIP:TOO:   OnOff: TRUE/FALSE

# Successful control
CHIP:DMG:   status = 0x00 (SUCCESS)
```

---

## ⚠️ **Important Warnings**

### **1. Non-Standard Process**
- **These devices do NOT follow standard Matter commissioning**
- **Standard commissioning tools will fail**
- **Always use BLE-WiFi method for NOUS A8M devices**

### **2. Node ID Management**
- **Each device must have a unique Node ID**
- **Never reuse Node IDs**
- **Document all Node IDs used**

### **3. Attestation Bypass**
- **Always use `--bypass-attestation-verifier true`**
- **This is required for NOUS A8M devices**
- **May not be required for other manufacturers**

### **4. Factory Reset Timing**
- **Use 6-second hold, not 10-15 seconds**
- **Wrong timing prevents proper reset**
- **LED behavior indicates reset success**

---

## 🔮 **Future Device Commissioning**

### **Unknown Device Types**
When commissioning new device types:

1. **Try Standard Process First**
   ```bash
   chip-tool pairing code <passcode> <discriminator>
   ```

2. **If Standard Fails, Try BLE-WiFi**
   ```bash
   chip-tool pairing ble-wifi <discriminator> <passcode> <ssid> <password>
   ```

3. **Add Attestation Bypass if Needed**
   ```bash
   --bypass-attestation-verifier true
   ```

4. **Document the Process**
   - Record what worked
   - Note any deviations from standard
   - Update this documentation

### **Standard Device Commissioning (Untested)**
We have **NOT** tested standard Matter commissioning with other device types. When adding new devices:

1. **Document the manufacturer and model**
2. **Try standard commissioning first**
3. **If it fails, try the NOUS A8M process**
4. **Update this documentation with results**

---

## 📚 **References**

### **Matter Specification**
- [Matter Specification](https://csa-iot.org/developer-resources/specifications-download/)
- [Matter Commissioning Guide](https://developers.home.google.com/matter/guides/commissioning)

### **NOUS A8M Documentation**
- [NOUS A8M Commissioning Guide](commissioning-server/NOUS_A8M_COMMISSIONING_GUIDE.md)
- [API Improvements](commissioning-server/API_IMPROVEMENTS.md)

### **Related Documentation**
- [Project Overview](PROJECT_OVERVIEW.md)
- [Changelog](changelog.md)
- [Database Schema Issues](DATABASE_SCHEMA_ISSUES.md)

---

*Last Updated: August 18, 2025*
*Status: ✅ DOCUMENTED - Both standard and non-standard processes documented*
