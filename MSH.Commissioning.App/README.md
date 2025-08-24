# MSH Commissioning App

A dedicated Blazor Server application for commissioning Matter devices and transferring them to your Raspberry Pi.

## 🎯 Overview

The MSH Commissioning App is a user-friendly desktop application that streamlines the Matter device commissioning process. It replaces the developer-centric command-line approach with an intuitive wizard interface that guides users through:

1. **Hardware Check** - Verifies Bluetooth, chip-tool, and network connectivity
2. **Device Discovery** - Scans for Bluetooth devices and identifies Matter devices
3. **Network Setup** - Configures WiFi credentials for device commissioning
4. **Commissioning** - Executes the commissioning process and transfers devices to Pi

## 🚀 Quick Start

### Prerequisites
- **USB Bluetooth Adapter** - For device discovery and commissioning
- **Matter SDK (chip-tool)** - Must be installed and accessible via command line
- **Network Connection** - To communicate with Raspberry Pi
- **Matter Device** - Ready for commissioning (factory reset if needed)

### Installation & Setup

1. **Clone and Navigate**
   ```bash
   cd MSH.Commissioning.App
   ```

2. **Install Dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the Application**
   ```bash
   dotnet run --urls "http://localhost:5080"
   ```

4. **Access the App**
   - Open browser to: `http://localhost:5080`
   - The commissioning wizard will start automatically

## 🔧 Configuration

### Application Settings
The app uses the following configuration in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Commissioning": {
    "PiHost": "192.168.1.100",
    "PiUser": "chregg",
    "PiPath": "/home/chregg/msh"
  }
}
```

### Environment Variables
- `PI_HOST` - Raspberry Pi IP address (default: auto-detected)
- `PI_USER` - Pi username (default: "chregg")
- `PI_PATH` - Project path on Pi (default: "/home/chregg/msh")

## 📋 Usage Guide

### Step 1: Hardware Check
The app automatically verifies:
- ✅ **Bluetooth Status** - USB adapter availability
- ✅ **chip-tool Status** - Matter SDK installation
- ✅ **Network Connectivity** - Pi communication

**Troubleshooting:**
- If Bluetooth fails: Ensure USB adapter is connected and not in use
- If chip-tool fails: Install Matter SDK or verify PATH
- If network fails: Check Pi connectivity and SSH access

### Step 2: Device Discovery
1. Click **"Start Scanning"** to begin BLE device discovery
2. Wait for devices to appear in the list
3. Look for devices with Matter indicators (specific naming patterns)
4. Select your target device from the list
5. Click **"Continue to Network Setup"**

**Device Identification:**
- Matter devices typically have names like "MatterDevice", "NOUS_A8M", etc.
- RSSI values help identify proximity
- Device addresses are shown for verification

### Step 3: Network Setup
1. Enter your **WiFi SSID** (network name)
2. Enter your **WiFi Password**
3. Verify the credentials are correct
4. Click **"Continue to Commissioning"**

**Network Requirements:**
- 2.4GHz WiFi network (Matter requirement)
- WPA2/WPA3 security
- Network must be accessible from both PC and Pi

### Step 4: Commissioning
1. Review the commissioning parameters
2. Click **"Start Commissioning"** to begin
3. Monitor progress through the status updates
4. Wait for completion confirmation
5. Click **"Transfer to Pi"** to move device to Pi's fabric

**Commissioning Process:**
- Generates unique Node ID for the device
- Executes chip-tool commissioning commands
- Tests device connectivity
- Transfers commissioning data to Pi

## 🔍 Troubleshooting

### Common Issues

#### Bluetooth Problems
```bash
# Check Bluetooth adapter
bluetoothctl show

# Restart Bluetooth service
sudo systemctl restart bluetooth

# Check for conflicting processes
sudo lsof /dev/bus/usb
```

#### chip-tool Issues
```bash
# Verify chip-tool installation
which chip-tool

# Check chip-tool version
chip-tool --version

# Test basic functionality
chip-tool pairing code [device-id]
```

#### Network Connectivity
```bash
# Test SSH connection to Pi
ssh chregg@[PI_IP] "echo 'Connection successful'"

# Check Pi's chip-tool container
ssh chregg@[PI_IP] "docker ps | grep chip_tool"

# Verify commissioning data transfer
ssh chregg@[PI_IP] "ls -la /home/chregg/msh/chip-tool-config/"
```

### Error Messages

| Error | Cause | Solution |
|-------|-------|----------|
| "Bluetooth not available" | Adapter not connected/working | Check USB connection, restart Bluetooth service |
| "chip-tool not found" | Matter SDK not installed | Install chip-tool or add to PATH |
| "Network connection failed" | Pi unreachable | Check IP address, SSH keys, network |
| "Commissioning failed" | Device not ready | Factory reset device, check WiFi credentials |
| "Transfer failed" | Pi container issues | Restart chip-tool container, check disk space |

## 🔧 Advanced Configuration

### Custom chip-tool Path
If chip-tool is not in PATH, specify custom location:

```csharp
// In CommissioningService.cs
private const string CHIP_TOOL_PATH = "/custom/path/to/chip-tool";
```

### SSH Key Configuration
For passwordless SSH to Pi:

```bash
# Generate SSH key (if not exists)
ssh-keygen -t rsa -b 4096

# Copy to Pi
ssh-copy-id chregg@[PI_IP]

# Test connection
ssh chregg@[PI_IP] "echo 'SSH working'"
```

### Logging Configuration
Enable detailed logging in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "MSH.Commissioning.App.Services": "Trace"
    }
  }
}
```

## 🏗️ Architecture

### Project Structure
```
MSH.Commissioning.App/
├── Components/
│   ├── Pages/
│   │   └── CommissioningWizard.razor    # Main wizard interface
│   ├── Layout/
│   │   └── NavMenu.razor                # Navigation menu
│   └── App.razor                        # Root component
├── Models/
│   └── BLEDevice.cs                     # Bluetooth device model
├── Services/
│   ├── IBLEScannerService.cs            # Bluetooth scanning interface
│   ├── BLEScannerService.cs             # Bluetooth implementation
│   ├── ICommissioningService.cs         # Commissioning interface
│   └── CommissioningService.cs          # Commissioning implementation
├── Program.cs                           # Application startup
└── appsettings.json                     # Configuration
```

### Key Components

#### BLEScannerService
- Uses `bluetoothctl` subprocess for device discovery
- Parses BLE device information
- Manages scanning lifecycle

#### CommissioningService
- Orchestrates the commissioning process
- Executes chip-tool commands
- Handles device transfer to Pi
- Provides progress updates via events

#### CommissioningWizard
- Multi-step wizard interface
- Real-time status updates
- Error handling and user feedback

## 🔄 Integration with MSH

### Data Flow
1. **PC Commissioning** → Device commissioned on PC
2. **Data Transfer** → Commissioning data copied to Pi
3. **Pi Control** → Device controlled via Pi's web GUI

### File Transfer Process
```bash
# Commissioning data transferred to Pi
scp -r ~/.matter/ chregg@[PI_IP]:/home/chregg/msh/chip-tool-config/

# Pi container uses transferred data
docker run -v /home/chregg/msh/chip-tool-config:/tmp chip-tool
```

### Web GUI Integration
After commissioning, devices appear in the Pi's web GUI at:
- **URL**: `http://[PI_IP]:8083`
- **Devices Page**: Lists all commissioned devices
- **Control**: Direct device control via web interface

## 🧪 Testing

### Manual Testing
```bash
# Test Bluetooth scanning
bluetoothctl scan on

# Test chip-tool availability
chip-tool --help

# Test Pi connectivity
ssh chregg@[PI_IP] "docker ps"
```

### Automated Testing
```bash
# Run unit tests
dotnet test

# Run integration tests
dotnet test --filter "Category=Integration"
```

## 📚 API Reference

### BLEScannerService Methods
- `IsBluetoothAvailableAsync()` - Check Bluetooth status
- `StartScanningAsync()` - Begin device discovery
- `StopScanningAsync()` - Stop device discovery
- `GetDevicesAsync()` - Get discovered devices
- `ConnectToDeviceAsync(address)` - Connect to specific device

### CommissioningService Methods
- `StartCommissioningAsync(device, network)` - Start commissioning process
- `TestDeviceConnectionAsync(nodeId)` - Test device connectivity
- `TransferToPiAsync(nodeId)` - Transfer device to Pi
- `ProgressUpdated` - Event for progress updates

## 🔒 Security Considerations

### Bluetooth Security
- BLE connections are temporary and local
- No persistent Bluetooth pairing required
- Device addresses are logged for debugging

### Network Security
- WiFi credentials stored temporarily in memory
- SSH connections use key-based authentication
- No sensitive data persisted to disk

### Commissioning Security
- Node IDs generated randomly
- Commissioning data encrypted during transfer
- Pi access requires SSH key authentication

## 🚀 Deployment

### Production Deployment
```bash
# Build for production
dotnet publish -c Release -o ./publish

# Deploy to server
scp -r ./publish/* user@server:/var/www/commissioning/

# Configure reverse proxy (nginx)
# See deployment documentation
```

### Docker Deployment
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY ./publish /app
WORKDIR /app
EXPOSE 5080
ENTRYPOINT ["dotnet", "MSH.Commissioning.App.dll"]
```

## 📞 Support

### Getting Help
1. Check the troubleshooting section above
2. Review the MSH main documentation
3. Check application logs for detailed error information
4. Verify hardware and network connectivity

### Log Locations
- **Application Logs**: Console output during development
- **System Logs**: `/var/log/syslog` (Linux)
- **Bluetooth Logs**: `journalctl -u bluetooth`

### Known Issues
- **Bluetooth adapter conflicts**: Ensure only one adapter is active
- **chip-tool version compatibility**: Use compatible Matter SDK version
- **Network timeout**: Increase timeout values for slow networks

---

**Note**: This application is designed to work with the MSH (Matter Smart Home) ecosystem. Ensure your Raspberry Pi is properly configured with the MSH web GUI and chip-tool container before commissioning devices.