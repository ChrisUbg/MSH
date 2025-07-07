# Instructions for Building Matter SDK for ARM64

## 🎯 **Objective**
Build the Matter SDK (ConnectedHomeIP) for ARM64 architecture using Visual Studio 2022 on Windows 11, then transfer the built tools to Raspberry Pi.

## 📋 **Prerequisites**

### **System Requirements**
- Windows 11 with Visual Studio 2022
- At least 16GB RAM (32GB recommended)
- 50GB+ free disk space
- Git for Windows installed
- WSL2 (Windows Subsystem for Linux) installed and enabled

### **Visual Studio 2022 Components**
- **Workloads**: 
  - "Desktop development with C++"
  - "Linux development with C++"
- **Individual Components**:
  - CMake tools for Visual Studio
  - Git for Windows
  - Python 3 development tools

## 🔧 **Step 1: Environment Setup**

### **1.1 Install WSL2**
```powershell
# Open PowerShell as Administrator
wsl --install
# Restart computer when prompted
```

### **1.2 Install Ubuntu 22.04 in WSL2**
```powershell
wsl --install -d Ubuntu-22.04
```

### **1.3 Update Ubuntu**
```bash
# In WSL2 Ubuntu terminal
sudo apt update && sudo apt upgrade -y
```

## 🏗️ **Step 2: Matter SDK Setup in WSL2**

### **2.1 Clone Repository**
```bash
# In WSL2 Ubuntu terminal
cd ~
git clone https://github.com/project-chip/connectedhomeip.git
cd connectedhomeip
```

### **2.2 Install Dependencies**
```bash
# Install required packages
sudo apt-get install -y /
    g++ /
    gcc /
    git /
    libavahi-client-dev /
    libcairo2-dev /
    libdbus-1-dev /
    libgirepository1.0-dev /
    libglib2.0-dev /
    libssl-dev /
    ninja-build /
    pkg-config /
    protobuf-compiler /
    python3 /
    python3-dev /
    python3-venv /
    unzip

# Install additional ARM64 cross-compilation tools
sudo apt-get install -y /
    gcc-aarch64-linux-gnu /
    g++-aarch64-linux-gnu /
    binutils-aarch64-linux-gnu
```

### **2.3 Bootstrap Environment**
```bash
# Run Matter SDK bootstrap
source scripts/activate.sh
# This will download Pigweed and set up the build environment
```

### **2.4 Update Submodules**
```bash
# Download all required submodules
git submodule update --init --recursive
```

## 🎯 **Step 3: Configure for ARM64 Cross-Compilation**

### **3.1 Create ARM64 Toolchain File**
```bash
# Create toolchain configuration
cat > arm64-toolchain.cmake << 'EOF'
set(CMAKE_SYSTEM_NAME Linux)
set(CMAKE_SYSTEM_PROCESSOR aarch64)

# Specify the cross compiler
set(CMAKE_C_COMPILER aarch64-linux-gnu-gcc)
set(CMAKE_CXX_COMPILER aarch64-linux-gnu-g++)

# Set target environment
set(CMAKE_FIND_ROOT_PATH /usr/aarch64-linux-gnu)

# Search for programs in the build host directories
set(CMAKE_FIND_ROOT_PATH_MODE_PROGRAM NEVER)

# Search for libraries and headers in the target directories
set(CMAKE_FIND_ROOT_PATH_MODE_LIBRARY ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_INCLUDE ONLY)
EOF
```

### **3.2 Configure GN for ARM64**
```bash
# Generate ARM64 build configuration
gn gen out/arm64 --args='target_cpu="arm64" target_os="linux" is_debug=false'
```

## 🔨 **Step 4: Build Matter SDK for ARM64**

### **4.1 Build Python Components**
```bash
# Build Python libraries and tools
ninja -C out/arm64 python_lib
```

### **4.2 Build chip-repl (Commissioning Tool)**
```bash
# Build the commissioning tool
ninja -C out/arm64 chip-repl
```

### **4.3 Build chip-tool (Device Control Tool)**
```bash
# Build the device control tool
ninja -C out/arm64 chip-tool
```

### **4.4 Build Additional Tools**
```bash
# Build other useful tools
ninja -C out/arm64 chip-tool-server
ninja -C out/arm64 chip-cert
```

## 📦 **Step 5: Package Build Output**

### **5.1 Create Distribution Package**
```bash
# Create directory for ARM64 build
mkdir -p ~/matter-arm64-build

# Copy built executables
cp out/arm64/chip-repl ~/matter-arm64-build/
cp out/arm64/chip-tool ~/matter-arm64-build/
cp out/arm64/chip-tool-server ~/matter-arm64-build/
cp out/arm64/chip-cert ~/matter-arm64-build/

# Copy Python libraries
cp -r out/arm64/python_lib/python/gen/src/chip ~/matter-arm64-build/

# Copy required libraries
ldd out/arm64/chip-repl | grep "=> /" | awk '{print $3}' | xargs -I {} cp {} ~/matter-arm64-build/

# Create package
cd ~
tar -czf matter-arm64-build.tar.gz matter-arm64-build/
```

## 🚀 **Step 6: Transfer to Raspberry Pi**

### **6.1 Copy from WSL2 to Windows (Windows-specific)**
```powershell
# In Windows PowerShell
# Get WSL2 username (replace with your actual username)
$wslUsername = "chris"  # Change this to your WSL2 username

# Copy from WSL2 to Windows
cp "//wsl$/Ubuntu-22.04/home/$wslUsername/matter-arm64-build.tar.gz" "C:/temp/"

# Alternative method using wsl command
wsl cp ~/matter-arm64-build.tar.gz /mnt/c/temp/
```

### **6.2 Transfer to Raspberry Pi (Cross-platform)**
```bash
# On Raspberry Pi
# Create directory
mkdir -p ~/MSH/matter-arm64-tools

# Transfer file (use your preferred method)
# Option 1: SCP (from Windows or Linux)
scp ${DEV_USER}@your-dev-machine:/path/to/matter-arm64-build.tar.gz ~/MSH/

# Option 2: USB drive
# Copy to USB drive on Windows, then to Pi

# Option 3: Network share
# Copy to shared folder accessible by Pi

# Option 4: Direct WSL2 to Pi (if on same network)
# From WSL2 terminal:
scp ~/matter-arm64-build.tar.gz ${DEV_USER}@${PI_IP}:~/MSH/
```

### **6.3 Install on Raspberry Pi**
```bash
# On Raspberry Pi
cd ~/MSH
tar -xzf matter-arm64-build.tar.gz

# Make executables executable
chmod +x matter-arm64-build/*

# Test the tools
./matter-arm64-build/chip-repl --help
./matter-arm64-build/chip-tool --help
```

## 🧪 **Step 7: Test ARM64 Build**

### **7.1 Verify ARM64 Architecture**
```bash
# On Raspberry Pi
file ~/MSH/matter-arm64-build/chip-repl
# Should show: ELF 64-bit LSB executable, ARM aarch64

# Check if it runs
./matter-arm64-build/chip-repl --help
```

### **7.2 Test Commissioning**
```bash
# Test with NOUS A8M device
./matter-arm64-build/chip-repl
# Follow Matter commissioning process
```

## 🔧 **Troubleshooting**

### **Common Issues**

#### **Memory Issues During Build**
```bash
# Increase swap space in WSL2
# Create swap file
sudo fallocate -l 8G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile

# Add to /etc/fstab for persistence
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
```

#### **Cross-Compilation Errors**
```bash
# Ensure all dependencies are installed
sudo apt-get install -y build-essential

# Check toolchain
aarch64-linux-gnu-gcc --version
```

#### **Python Module Issues**
```bash
# Install Python dependencies in WSL2
pip3 install -r requirements.txt
```

#### **GN Build Errors**
```bash
# Check GN configuration
gn gen out/arm64 --args='target_cpu="arm64" target_os="linux" is_debug=false' --list

# Verify build targets
ninja -C out/arm64 -t targets | grep chip
```

#### **WSL2 Path Issues**
```bash
# Check WSL2 username
whoami

# Verify WSL2 distribution name
wsl -l -v

# Access WSL2 from Windows (replace with your username)
# In Windows PowerShell:
# //wsl$/Ubuntu-22.04/home/YOUR_USERNAME/
```

#### **Windows File System Performance**
```bash
# For better performance, clone to WSL2 filesystem instead of Windows filesystem
# In WSL2 terminal:
cd /home/$USER  # Use WSL2 filesystem, not /mnt/c/
git clone https://github.com/project-chip/connectedhomeip.git
```

## 🎯 **Success Criteria**

✅ **ARM64 executables built successfully**
✅ **chip-repl and chip-tool functional on Pi**
✅ **BLE commissioning works with real devices**
✅ **No x86_64 architecture dependencies**
✅ **All tools run natively on ARM64**

## 🚀 **Next Steps After Successful Build**

1. **Integrate with FastAPI Bridge**: Use chip-repl for commissioning
2. **Update Web UI**: Add commissioning interface
3. **Test with NOUS A8M**: Real device commissioning
4. **Deploy to production**: Replace python-matter-server with native tools

## 📝 **Notes**

- **Build Time**: Expect 2-4 hours for full build
- **Memory Usage**: Monitor memory usage during build
- **Network**: Ensure stable internet connection for submodule downloads
- **Backup**: Keep the built package for future use
- **Updates**: Rebuild when Matter SDK updates are needed
- **WSL2 Performance**: Use WSL2 filesystem for better build performance
- **Cross-platform**: Instructions work on both Windows/WSL2 and native Linux

## 🔧 **Cross-Platform Configuration**

### **IP Address Configuration**
The project uses various IP addresses that may need to be updated for your environment:

#### **Common IP Addresses in Project**
- **Raspberry Pi IPs**: `${PI_IP}`, `${PI_IP}`, `192.168.0.106`
- **Username**: `chregg` (Pi user), `chris` (development machine)

#### **How to Update IP Addresses**
```bash
# Find your Pi's IP address
ping msh.local  # If mDNS is working
# OR
nmap -sn 192.168.0.0/24 | grep -B2 -A1 "Raspberry Pi"

# Update configuration files
# Replace all instances of old IP with new IP
find . -name "*.sh" -o -name "*.md" -o -name "*.yml" | xargs sed -i 's/${PI_IP}/YOUR_NEW_IP/g'
```

#### **Environment-Specific Configuration**
```bash
# Create environment-specific config
export PI_IP="${PI_IP}"  # Your Pi's IP
export PI_USER="chregg"        # Your Pi username
export DEV_USER="chris"        # Your dev machine username

# Use in scripts
scp file.tar.gz $DEV_USER@$PI_IP:~/MSH/
```

### **Windows vs Linux Path Handling**

#### **Windows PowerShell**
```powershell
# Windows-style paths
$env:DOCKERFILE_PATH="C:/Users/Dev/source/repos/MSH/Docker/Dockerfile.prod"
cp "//wsl$/Ubuntu-22.04/home/$wslUsername/file.tar.gz" "C:/temp/"
```

#### **Linux/WSL2**
```bash
# Unix-style paths
export DOCKERFILE_PATH="$(pwd)/Docker/Dockerfile.prod"
cp ~/file.tar.gz /mnt/c/temp/
```

#### **Cross-Platform Scripts**
```bash
# Detect OS and use appropriate paths
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" ]]; then
    # Windows
    DOCKERFILE_PATH="C:/Users/Dev/source/repos/MSH/Docker/Dockerfile.prod"
else
    # Linux/WSL2
    DOCKERFILE_PATH="$(pwd)/Docker/Dockerfile.prod"
fi
```

### **WSL2 Integration Best Practices**

#### **File System Performance**
```bash
# Use WSL2 filesystem for builds (faster)
cd /home/$USER/connectedhomeip  # Not /mnt/c/Users/...

# Access Windows files when needed
cp /mnt/c/Users/Dev/source/repos/MSH/config.json ~/MSH/
```

#### **Network Access**
```bash
# WSL2 can access Windows network
# Windows can access WSL2 via //wsl$/Ubuntu-22.04/

# Direct network access from WSL2
scp file.tar.gz ${DEV_USER}@${PI_IP}:~/MSH/
```

---

**Created**: 2024-01-28  
**Purpose**: Build Matter SDK for ARM64 architecture using Windows/WSL2  
**Target**: Raspberry Pi ARM64 deployment 