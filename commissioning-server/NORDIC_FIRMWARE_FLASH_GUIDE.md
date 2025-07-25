# Nordic nRF52840 Firmware Flashing Guide

## 🎯 Objective
Flash Nordic Connectivity Firmware to your nRF52840 dongle to enable it as a Bluetooth adapter for MSH commissioning.

## 📋 Prerequisites

### **System Requirements**
- Ubuntu/Debian Linux system
- USB connection to Nordic dongle
- Internet connection for downloads
- Administrative privileges (sudo access)

### **Hardware Verification**
```bash
# Verify Nordic dongle is connected
lsusb | grep Nordic
# Expected: Bus XXX Device XXX: ID 1915:c00a Nordic Semiconductor ASA nRF52 Connectivity
```

## 🚀 Method 1: nRF Connect for Desktop (Recommended)

### **Step 1: Download nRF Connect for Desktop**

#### **Option A: Direct Download**
```bash
# Create download directory
mkdir -p ~/nordic-tools
cd ~/nordic-tools

# Download the latest version (check Nordic website for current version)
wget https://www.nordicsemi.com/-/media/Software-and-other-downloads/Desktop-software/nRF-Connect-for-Desktop/3.12.0/nRF-Connect-for-Desktop-3.12.0.AppImage

# Make executable
chmod +x nRF-Connect-for-Desktop-3.12.0.AppImage
```

#### **Option B: Manual Download**
1. Visit: https://www.nordicsemi.com/Products/Development-tools/nRF-Connect-for-Desktop
2. Download the latest version for Linux
3. Place in `~/nordic-tools/` directory

### **Step 2: Install Dependencies**
```bash
# Install required packages
sudo apt-get update
sudo apt-get install -y \
    libfuse2 \
    libgtk-3-0 \
    libnss3 \
    libxss1 \
    libasound2 \
    libdrm2 \
    libxcomposite1 \
    libxdamage1 \
    libxrandr2 \
    libgbm1 \
    libxss1 \
    libatspi2.0-0

# Install Nordic development tools
sudo apt-get install -y nrfjprog
```

### **Step 3: Run nRF Connect for Desktop**
```bash
# Navigate to tools directory
cd ~/nordic-tools

# Run the application
./nRF-Connect-for-Desktop-3.12.0.AppImage
```

### **Step 4: Flash Firmware**

1. **Launch Programmer Tool**:
   - In nRF Connect for Desktop, click "Programmer"
   - Wait for the application to load

2. **Connect Dongle**:
   - Ensure your Nordic dongle is connected via USB
   - The application should detect it automatically

3. **Select Firmware**:
   - Click "Add file" or "Browse"
   - Navigate to firmware files (if you have them)
   - Or use the built-in firmware selection

4. **Flash Process**:
   - Select the appropriate firmware for nRF52840
   - Click "Write" to start flashing
   - Wait for completion (usually 30-60 seconds)

## 🔧 Method 2: Command Line (Advanced)

### **Step 1: Install Nordic Tools**
```bash
# Install Nordic development tools
sudo apt-get update
sudo apt-get install -y nrfjprog

# Verify installation
nrfjprog --version
```

### **Step 2: Download Firmware**
```bash
# Create firmware directory
mkdir -p ~/nordic-firmware
cd ~/nordic-firmware

# Download Nordic Connectivity Firmware
# Note: You may need to download from Nordic's website
wget https://www.nordicsemi.com/-/media/Software-and-other-downloads/Desktop-software/nRF-Connect-for-Desktop/3.12.0/nRF-Connect-for-Desktop-3.12.0.AppImage
```

### **Step 3: Flash Using nrfjprog**
```bash
# List connected devices
nrfjprog --ids

# Erase the device
nrfjprog --erase --device nRF52840_xxAA

# Program the firmware
nrfjprog --program firmware.hex --device nRF52840_xxAA

# Reset the device
nrfjprog --reset --device nRF52840_xxAA
```

## 🧪 Verification Steps

### **Step 1: Check Bluetooth Adapter**
```bash
# Check if Bluetooth adapter is now available
bluetoothctl list

# Alternative check
hciconfig

# Check system Bluetooth devices
ls /sys/class/bluetooth/
```

### **Step 2: Test Nordic Dongle**
```bash
# Run the Nordic dongle test
cd commissioning-server
python3 test_nordic_dongle.py
```

### **Step 3: Test BLE Scanning**
```bash
# Test basic BLE functionality
bluetoothctl
> scan on
> devices
> scan off
```

## 🔍 Troubleshooting

### **Common Issues**

#### **1. Device Not Detected**
```bash
# Check USB connection
lsusb | grep Nordic

# Check kernel messages
sudo dmesg | grep -i nordic

# Reset USB port
sudo usb-reset 1915:c00a
```

#### **2. Permission Issues**
```bash
# Add user to dialout group (for serial access)
sudo usermod -a -G dialout $USER

# Add user to bluetooth group
sudo usermod -a -G bluetooth $USER

# Log out and back in, or run:
newgrp dialout
newgrp bluetooth
```

#### **3. Firmware Download Issues**
```bash
# Alternative download methods
# 1. Use wget with different user agent
wget --user-agent="Mozilla/5.0" https://www.nordicsemi.com/...

# 2. Download from GitHub releases
# Check Nordic's GitHub for firmware releases
```

#### **4. Flash Process Fails**
```bash
# Check device status
nrfjprog --ids

# Reset device
nrfjprog --reset --device nRF52840_xxAA

# Try recovery mode
# Hold reset button while connecting USB
```

## 📋 Expected Results

### **After Successful Flashing**
- Nordic dongle appears as Bluetooth adapter
- `bluetoothctl list` shows the adapter
- `hciconfig` displays the Nordic device
- BLE scanning works properly
- Nordic dongle test passes

### **Integration with MSH**
```bash
# Test with commissioning server
cd commissioning-server
python3 main.py

# Test enhanced features
python3 test_nordic_dongle.py
```

## 🎯 Next Steps

### **After Successful Flashing**
1. **Test Enhanced Features**:
   ```bash
   cd commissioning-server
   python3 test_nordic_dongle.py
   ```

2. **Update Configuration**:
   - Use Nordic-optimized settings in `config.yaml`
   - Enable high-power mode and extended advertising

3. **Test Commissioning**:
   - Test with NOUS A8M devices
   - Verify improved BLE range and reliability

### **If Flashing Fails**
1. **Try Alternative Firmware**: Different Nordic Connectivity Firmware versions
2. **Use Standard Adapter**: Purchase a standard Bluetooth USB adapter
3. **Contact Nordic Support**: For hardware-specific issues

## 📞 Support Resources

### **Nordic Documentation**
- [nRF Connect for Desktop](https://www.nordicsemi.com/Products/Development-tools/nRF-Connect-for-Desktop)
- [nRF52840 Product Page](https://www.nordicsemi.com/Products/nRF52840)
- [Nordic Developer Zone](https://devzone.nordicsemi.com/)

### **MSH Integration**
- **[Nordic Dongle Integration](NORDIC_DONGLE_INTEGRATION.md)** - Enhanced features
- **[Nordic Dongle Solution](NORDIC_DONGLE_SOLUTION.md)** - Alternative approaches
- **[Hardware Setup](hardware-setup.md)** - General hardware configuration

---

*This guide provides step-by-step instructions for flashing Nordic Connectivity Firmware to enable your nRF52840 dongle as a Bluetooth adapter for MSH commissioning.* 