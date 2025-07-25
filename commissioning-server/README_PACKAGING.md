# MSH Commissioning Server - Packaging Guide

## Overview
This guide shows how to package the MSH Commissioning Server into portable, easily deployable applications.

## Packaging Options

### Option 1: PyInstaller (Recommended)
Creates a single executable file that includes Python runtime and all dependencies.

**Advantages:**
- ✅ Single file deployment
- ✅ No Python installation required on target machine
- ✅ Cross-platform support
- ✅ Easy to distribute

**Usage:**
```bash
# Install PyInstaller
pip install pyinstaller

# Build portable executable
python build_portable_app.py

# Or build manually
pyinstaller --onefile --windowed main.py
```

### Option 2: Docker Container
Package as a Docker image for consistent deployment.

**Advantages:**
- ✅ Consistent environment
- ✅ Easy version management
- ✅ Isolated dependencies
- ✅ Works on any Docker host

**Usage:**
```bash
# Build Docker image
docker build -t msh-commissioning-server .

# Run container
docker run -p 8888:8888 --privileged msh-commissioning-server
```

### Option 3: Python Package (pip)
Install via pip for Python environments.

**Advantages:**
- ✅ Standard Python packaging
- ✅ Easy updates
- ✅ Dependency management
- ✅ Professional distribution

**Usage:**
```bash
# Install from PyPI
pip install msh-commissioning-server

# Run
msh-commissioning-server --port 8888
```

## Build Scripts

### 1. PyInstaller Build
```bash
# Run the build script
python build_portable_app.py

# Output: dist/msh-commissioning-server/
# ├── msh-commissioning-server (executable)
# ├── config.yaml
# ├── start-server.sh (Linux/Mac)
# ├── start-server.bat (Windows)
# └── README.md
```

### 2. Docker Build
```bash
# Build Docker image
docker build -t msh-commissioning-server .

# Create portable container
docker save msh-commissioning-server > msh-commissioning-server.tar

# Load on target machine
docker load < msh-commissioning-server.tar
```

### 3. Python Package Build
```bash
# Build wheel
python setup.py bdist_wheel

# Install locally
pip install dist/msh_commissioning_server-1.0.0-py3-none-any.whl
```

## Deployment Methods

### Method 1: Portable Executable
```bash
# Copy to target machine
scp -r dist/msh-commissioning-server/ user@target-pc:/opt/

# Run on target
cd /opt/msh-commissioning-server
./start-server.sh  # Linux/Mac
# or
start-server.bat   # Windows
```

### Method 2: Docker Deployment
```bash
# Copy Docker image
scp msh-commissioning-server.tar user@target-pc:~/

# Load and run on target
docker load < msh-commissioning-server.tar
docker run -d -p 8888:8888 --name msh-server msh-commissioning-server
```

### Method 3: System Service
```bash
# Install as system service
sudo ./install.sh

# Manage service
sudo systemctl start msh-commissioning-server
sudo systemctl stop msh-commissioning-server
sudo systemctl status msh-commissioning-server
```

## Platform-Specific Considerations

### Linux
- **Bluetooth permissions**: May need `--privileged` or specific device access
- **System dependencies**: Ensure Bluetooth stack is installed
- **Service management**: Use systemd for auto-start

### Windows
- **Admin privileges**: May need UAC elevation for Bluetooth access
- **Firewall**: Configure Windows Firewall for port 8888
- **Service installation**: Use Windows Service Manager

### macOS
- **Bluetooth permissions**: Grant permissions in System Preferences
- **Code signing**: May need code signing for distribution
- **Gatekeeper**: Handle macOS security restrictions

## Configuration Management

### Environment Variables
```bash
export MSH_CONFIG_PATH="./config.yaml"
export MSH_DATA_PATH="./data"
export MSH_LOG_LEVEL="INFO"
```

### Configuration File
```yaml
# config.yaml
server:
  host: "0.0.0.0"
  port: 8888
  debug: false

bluetooth:
  adapter: "hci0"
  timeout: 30

storage:
  type: "sqlite"
  path: "./data/credentials.db"
```

## Distribution Options

### 1. GitHub Releases
```bash
# Create release package
tar -czf msh-commissioning-server-linux.tar.gz dist/msh-commissioning-server/
zip -r msh-commissioning-server-windows.zip dist/msh-commissioning-server/
```

### 2. Docker Hub
```bash
# Push to Docker Hub
docker tag msh-commissioning-server username/msh-commissioning-server
docker push username/msh-commissioning-server
```

### 3. PyPI
```bash
# Upload to PyPI
python -m twine upload dist/*
```

## Troubleshooting

### Common Issues

1. **Bluetooth not working in container**
   - Use `--privileged` flag
   - Mount Bluetooth devices: `-v /dev/bus/usb:/dev/bus/usb`

2. **Port already in use**
   - Change port in config: `port: 8081`
   - Kill existing process: `sudo lsof -ti:8888 | xargs kill`

3. **Permission denied**
   - Run with sudo (Linux)
   - Run as Administrator (Windows)
   - Grant Bluetooth permissions (macOS)

4. **Executable not found**
   - Check file permissions: `chmod +x msh-commissioning-server`
   - Verify architecture compatibility

### Debug Mode
```bash
# Enable debug logging
export MSH_LOG_LEVEL="DEBUG"
./msh-commissioning-server --debug

# Check logs
tail -f logs/msh-server.log
```

## Best Practices

### 1. Version Management
- Include version in executable name
- Use semantic versioning
- Maintain changelog

### 2. Security
- Don't include sensitive data in packages
- Use environment variables for secrets
- Validate configuration on startup

### 3. Performance
- Optimize startup time
- Minimize package size
- Use compression for distribution

### 4. Testing
- Test on target platforms
- Verify Bluetooth functionality
- Check network connectivity

## Example Deployment Script

```bash
#!/bin/bash
# deploy-msh-server.sh

echo "🚀 Deploying MSH Commissioning Server..."

# Download latest release
wget https://github.com/your-repo/releases/latest/download/msh-commissioning-server-linux.tar.gz

# Extract
tar -xzf msh-commissioning-server-linux.tar.gz

# Install dependencies
sudo apt-get update
sudo apt-get install -y bluetooth bluez

# Configure Bluetooth
sudo usermod -a -G bluetooth $USER

# Start server
cd msh-commissioning-server
./start-server.sh

echo "✅ Deployment complete!"
echo "🌐 Server running at: http://localhost:8888"
```

This packaging approach gives you maximum flexibility for deployment while maintaining the simplicity of the original design. 