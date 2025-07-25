# PC Commissioning Server Design

## Overview
Native PC application for Matter device commissioning that can run on both Ubuntu and Windows 11, with easy deployment and installation.

## Architecture

### Core Components
```
┌─────────────────────────────────────────────────────────────┐
│                    PC Commissioning Server                  │
├─────────────────────────────────────────────────────────────┤
│  Web Interface (Port 8080)                                  │
│  ├── Device Discovery                                       │
│  ├── Commissioning Interface                                │ 
│  └── Credential Management                                  │                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
├─────────────────────────────────────────────────────────────┤                                                                             
│  Commissioning API (Port 8081)                              │
│  ├── BLE Commissioning                                      │
│  ├── WiFi Commissioning                                     │
│  └── Credential Storage                                     │
├─────────────────────────────────────────────────────────────┤
│  Matter SDK Integration                                     │
│  ├── chip-tool (Native)                                     │
│  ├── chip-repl (Native)                                     │
│  └── BLE Stack (Direct)                                     │
├─────────────────────────────────────────────────────────────┤
│  System Integration                                         │
│  ├── Bluetooth Stack                                        │ 
│  ├── Network Stack                                          │
│  └── File System                                            │
└─────────────────────────────────────────────────────────────┘
```

## Technology Stack

### Backend (Python/FastAPI)
- **FastAPI**: Web framework for API
- **uvicorn**: ASGI server
- **pydantic**: Data validation
- **sqlite**: Local credential storage
- **asyncio**: Async operations

### Matter Integration
- **Native Matter SDK**: Direct system integration
- **chip-tool**: Command-line commissioning
- **chip-repl**: Interactive commissioning
- **BLE Stack**: Direct Bluetooth access

### Frontend (Web Interface)
- **HTML/CSS/JavaScript**: Simple web interface
- **WebSocket**: Real-time updates
- **QR Code Scanner**: Device QR codes

## Installation Methods

### Method 1: Python Package (Recommended)
```bash
# Install via pip
pip install msh-commissioning-server

# Run server
msh-commissioning-server --port 8080
```

### Method 2: Standalone Executable
```bash
# Download executable
wget https://github.com/your-repo/releases/latest/download/msh-commissioning-server

# Make executable
chmod +x msh-commissioning-server

# Run
./msh-commissioning-server --port 8080
```

### Method 3: System Service
```bash
# Install as systemd service (Ubuntu)
sudo systemctl enable msh-commissioning-server
sudo systemctl start msh-commissioning-server

# Install as Windows Service (Windows 11)
msh-commissioning-server --install-service
```

## API Endpoints

### Device Discovery
```http
GET /api/devices/discover
POST /api/devices/scan-ble
POST /api/devices/scan-wifi
```

### Commissioning
```http
POST /api/devices/commission
{
  "device_id": "string",
  "commissioning_type": "ble|wifi",
  "qr_code": "string",
  "manual_code": "string"
}
```

### Credential Management
```http
GET /api/devices/credentials
POST /api/devices/transfer-credentials
DELETE /api/devices/{device_id}/credentials
```

## Configuration

### config.yaml
```yaml
server:
  host: "0.0.0.0"
  port: 8080
  debug: false

matter:
  sdk_path: "/usr/local/matter-sdk"
  chip_tool_path: "/usr/local/bin/chip-tool"
  chip_repl_path: "/usr/local/bin/chip-repl"

bluetooth:
  adapter: "hci0"
  timeout: 30

storage:
  type: "sqlite"
  path: "./credentials.db"

security:
  api_key_required: true
  allowed_hosts: ["192.168.0.0/24"]
```

## Cross-Platform Support

### Ubuntu Dependencies
```bash
# System packages
sudo apt-get update
sudo apt-get install -y \
  python3.10 \
  python3-pip \
  bluetooth \
  bluez \
  libbluetooth-dev \
  libudev-dev \
  build-essential \
  cmake \
  ninja-build \
  clang

# Python packages
pip install fastapi uvicorn pydantic sqlite3 websockets
```

### Windows 11 Dependencies
```powershell
# Install Python 3.10+
winget install Python.Python.3.10

# Install Bluetooth drivers
# (Windows handles this automatically)

# Python packages
pip install fastapi uvicorn pydantic sqlite3 websockets
```

## Security Considerations

### Local Network Only
- Server binds to local network interfaces only
- No external internet access required
- Firewall rules for local subnet

### Credential Security
- Encrypted credential storage
- Secure transfer to Pi
- API key authentication

### Bluetooth Security
- Local Bluetooth pairing only
- No external Bluetooth access
- Secure device authentication

## Integration with Raspberry Pi

### Credential Transfer
```http
POST /api/transfer-to-pi
{
  "pi_ip": "192.168.0.104",
  "pi_user": "chregg",
  "device_credentials": {...}
}
```

### Real-time Sync
- WebSocket connection between PC and Pi
- Real-time device state updates
- Live commissioning status

## Development Workflow

### 1. Local Development
```bash
# Clone repository
git clone https://github.com/your-repo/msh-commissioning-server

# Install dependencies
pip install -r requirements.txt

# Run development server
python -m uvicorn main:app --reload --port 8080
```

### 2. Testing
```bash
# Run tests
pytest tests/

# Test with real device
python -m pytest tests/test_commissioning.py -v
```

### 3. Building
```bash
# Build executable
pyinstaller --onefile msh-commissioning-server.py

# Build Docker image (for non-Bluetooth operations)
docker build -t msh-commissioning-server .
```

## Deployment Options

### Option A: Dedicated Commissioning PC
- Single-purpose machine for commissioning
- Always-on server
- Network accessible to Pi

### Option B: Development Machine
- Run commissioning server when needed
- Temporary commissioning sessions
- Manual startup/shutdown

### Option C: Virtual Machine
- Isolated commissioning environment
- Snapshot-based deployment
- Easy backup/restore

## Benefits of This Approach

✅ **No Docker Bluetooth limitations**  
✅ **Direct system access**  
✅ **Cross-platform compatibility**  
✅ **Easy installation**  
✅ **Secure credential management**  
✅ **Real-time integration with Pi**  
✅ **Scalable architecture**  

## Next Steps

1. **Design the API specification**
2. **Create the Python application structure**
3. **Implement Matter SDK integration**
4. **Build the web interface**
5. **Create installation packages**
6. **Test with real Matter devices** 