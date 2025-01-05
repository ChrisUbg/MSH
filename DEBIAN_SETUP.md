# Debian System Configuration for Matter Smart Home

## System Information
- **Distribution**: Debian GNU/Linux 12 (bookworm)
- **Architecture**: ARM64 (Raspberry Pi 4)
- **Username**: chregg
- **IP Address**: 192.168.0.104
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
export PATH=\$PATH:/usr/share/dotnet
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
sudo bash -c 'echo "export PATH=\$PATH:/usr/share/dotnet" >> /etc/sudoers.d/dotnet'
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