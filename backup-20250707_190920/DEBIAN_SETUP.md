# Debian System Configuration for Matter Smart Home

### Home Directory Setup and Permissions
```bash
# Create necessary files if they don't exist
sudo touch /home/chregg/.bash_profile
sudo touch /home/chregg/.bashrc

# Fix home directory ownership and permissions
sudo chown -R chregg:chregg /home/chregg
sudo chmod 755 /home/chregg

# Fix shell configuration file permissions
sudo chmod 644 /home/chregg/.bash_profile
sudo chmod 644 /home/chregg/.bashrc

# Verify permissions
ls -la /home/chregg
ls -la /home/chregg/.bash*
```

### Basic Shell Configuration
```bash
# Add to .bash_profile
echo 'export DOTNET_ROOT=/usr/share/dotnet' | sudo tee -a /home/chregg/.bash_profile
echo 'export PATH=$PATH:/usr/share/dotnet' | sudo tee -a /home/chregg/.bash_profile

# Source the profile
source /home/chregg/.bash_profile

## System Information
- **Distribution**: Debian GNU/Linux 12 (bookworm)
- **Architecture**: ARM64 (Raspberry Pi 4)
- **Username**: chregg
- **IP Address**: ${PI_IP}
- **SSH Port**: 22

## Required Software

### 1. .NET Development

#### Install .NET SDK (ARM64)
```bash
# Install required dependencies
sudo apt-get update
sudo apt-get install -y wget curl

# Create a directory for the installation
sudo mkdir -p /usr/share/dotnet
cd /usr/share/dotnet

# Download and prepare the installation script
sudo wget https://dot.net/v1/dotnet-install.sh
sudo chmod +x ./dotnet-install.sh

# Install .NET SDK 8.0
sudo ./dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet

# Add .NET to system-wide PATH (create a new file)
sudo bash -c 'cat > /etc/profile.d/dotnet.sh << EOL
export DOTNET_ROOT=/usr/share/dotnet
export PATH=/$PATH:/usr/share/dotnet
EOL'
sudo chmod +x /etc/profile.d/dotnet.sh

# Apply PATH changes immediately
export DOTNET_ROOT=/usr/share/dotnet
export PATH=$PATH:/usr/share/dotnet

# Verify installation
dotnet --version
dotnet --list-sdks
```

#### Post-Installation Steps
```bash
# Fix home directory permissions
sudo chown -R chregg:chregg /home/chregg
sudo chmod 755 /home/chregg

# Create and configure .dotnet directory
sudo mkdir -p /home/chregg/.dotnet
sudo chown -R chregg:chregg /home/chregg/.dotnet

# Create test application
cd /home/chregg
sudo mkdir -p dotnet-test
sudo chown -R chregg:chregg dotnet-test
cd dotnet-test

# Create and run test application using full path
sudo /usr/share/dotnet/dotnet new console
sudo /usr/share/dotnet/dotnet build
sudo /usr/share/dotnet/dotnet run

# Alternative: Add dotnet to sudo PATH
sudo bash -c 'echo "export PATH=/$PATH:/usr/share/dotnet" >> /etc/sudoers.d/dotnet'
sudo chmod 440 /etc/sudoers.d/dotnet
```

### 2. PostgreSQL Database
```bash
# Install PostgreSQL
sudo apt-get install -y postgresql postgresql-contrib

# Start PostgreSQL service
sudo systemctl start postgresql
sudo systemctl enable postgresql

# Configure PostgreSQL (to be added)
```

### 3. Matter Protocol Dependencies
```bash
# Install basic development tools
sudo apt-get install -y git gcc g++ python3 python3-pip

# Install Matter SDK dependencies (to be added after Matter SDK setup)
```

### 4. Bluetooth Configuration
```bash
# Install Bluetooth packages
sudo apt-get install -y bluetooth bluez

# Enable Bluetooth service
sudo systemctl enable bluetooth
sudo systemctl start bluetooth

# Verify Bluetooth status
sudo systemctl status bluetooth
```

### 5. Network Configuration
```bash
# Install network tools
sudo apt-get install -y net-tools wireless-tools

# Verify WLAN status
iwconfig
```

## Security Configuration

### SSH Configuration
```bash
# Secure SSH configuration (recommended settings)
sudo nano /etc/ssh/sshd_config

# Key settings to modify:
# PermitRootLogin no
# PasswordAuthentication yes  # Change to 'no' if using key-based auth
# X11Forwarding no
```

### Firewall Setup
```bash
# Install firewall
sudo apt-get install -y ufw

# Configure basic rules
sudo ufw allow ssh
sudo ufw allow 5000/tcp  # For Blazor Server
sudo ufw enable
```

## Development Environment

### Git Configuration
```bash
# Install Git
sudo apt-get install -y git

# Configure Git
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

## Monitoring and Maintenance

### System Monitoring
```bash
# Install monitoring tools
sudo apt-get install -y htop iotop
```

### Logging
```bash
# View system logs
journalctl -xe

# View application logs (when running)
journalctl -u matter-smart-home.service
```

## Status Tracking
- [x] .NET SDK 8.0.404 installed and verified (2024-01-05)
  - Console application test successful
- [ ] PostgreSQL configured
- [ ] Matter SDK dependencies installed
- [ ] Bluetooth configured
- [ ] Network configured
- [ ] Security settings applied
- [ ] Development tools installed

## Notes
- Keep this document updated with any new configurations
- Document any issues and their solutions
- Add specific Matter protocol requirements as they are identified 

# Bluetooth Setup on Raspberry Pi 4

## Overview
This section outlines the steps to set up Bluetooth on the Raspberry Pi 4 using the GeeekPi nRF52840 USB dongle.

## Steps to Set Up Bluetooth

1. **Update Your System**:
   Ensure your Raspberry Pi OS is up to date:
   ```bash
   sudo apt update
   sudo apt upgrade
   ```

2. **Install Bluetooth Utilities**:
   Install the necessary Bluetooth packages:
   ```bash
   sudo apt install bluetooth bluez blueman
   ```

3. **Plug in the USB Dongle**:
   Insert the GeeekPi nRF52840 USB dongle into one of the USB ports on your Raspberry Pi.

4. **Check if the Dongle is Recognized**:
   Verify that the system recognizes the dongle:
   ```bash
   lsusb
   ```
   Look for an entry corresponding to the nRF52840 dongle.

5. **Start the Bluetooth Service**:
   Ensure that the Bluetooth service is running:
   ```bash
   sudo systemctl start bluetooth
   sudo systemctl enable bluetooth
   ```

6. **Use Bluetoothctl to Manage Bluetooth Devices**:
   - Start the Bluetooth control tool:
     ```bash
     bluetoothctl
     ```
   - Inside the `bluetoothctl` prompt, run the following commands:
     ```bash
     power on          # Turn on the Bluetooth adapter
     agent on          # Enable the agent for pairing
     scan on           # Start scanning for devices
     ```

7. **Pair with a Device**:
   To pair with a device, use the following command (replace `XX:XX:XX:XX:XX:XX` with the actual MAC address):
   ```bash
   pair E8:78:29:C2:F9:D5  # Example for Fairphone 4 5G
   ```

8. **Connect to the Device**:
   After pairing, connect to the device:
   ```bash
   connect E8:78:29:C2:F9:D5  # Example for Fairphone 4 5G
   ```

9. **Trust the Device**:
   To ensure the device automatically connects in the future, trust it:
   ```bash
   trust E8:78:29:C2:F9:D5  # Example for Fairphone 4 5G
   ```

10. **Exit Bluetoothctl**:
    Once done, exit the Bluetooth control tool:
    ```bash
    exit
    ```

## Verification
To verify that the Bluetooth device is connected, you can use: 

## Troubleshooting
- If you encounter issues, check the status of the Bluetooth service:
  ```bash
  sudo systemctl status bluetooth
  ```

This documentation will help in setting up and troubleshooting Bluetooth on the Raspberry Pi 4 using the GeeekPi nRF52840 USB dongle. 

## Copying Files to Raspberry Pi
-PS C:/Users/Dev/source/repos/MSH> 
scp -r ./publish/* ${PI_USER}@${PI_IP}:/home/msh 

## Running the Application   
- dotnet MSH.Web.dll --urls http://0.0.0.0:5001

## Stopping the Application
- dotnet stop MSH.Web.dll

## Restarting the Application
- dotnet restart MSH.Web.dll

## /connectedhomeip/scripts/activate.sh
- source /connectedhomeip/scripts/activate.sh

