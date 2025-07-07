# API Documentation

## NetworkController Endpoints

### Get Current Network Mode
- **Route:** `GET /api/network/mode`
- **Description:** Returns the current network mode.
- **Response:**
  - `200 OK`: Returns a string with the current mode.
  - `500 Internal Server Error`: Error getting current network mode.
- **Example Request:**
  ```http
  GET /api/network/mode
  ```
- **Example Response:**
  ```json
  "auto-commissioning"
  ```

---

### Switch Network Mode
- **Route:** `POST /api/network/switch/{mode}`
- **Description:** Switches the network mode.
- **Parameters:**
  - `mode` (string, required): Must be one of: `normal`, `auto-commissioning`, `commissioning-client`, `commissioning-ap`, `commissioning`, `complete`.
- **Response:**
  - `200 OK`: Successfully switched mode.
  - `400 Bad Request`: Invalid mode.
  - `500 Internal Server Error`: Error switching network mode.
- **Example Request:**
  ```http
  POST /api/network/switch/auto-commissioning
  ```
- **Example Response:**
  ```json
  "Switched to auto-commissioning mode"
  ```

---

### Get Detailed Network Status
- **Route:** `GET /api/network/status`
- **Description:** Returns detailed network status including GUI accessibility and available modes.
- **Response:**
  - `200 OK`: Returns a JSON object with current mode, GUI accessibility, network info, and available modes.
  - `500 Internal Server Error`: Error getting network status.
- **Example Request:**
  ```http
  GET /api/network/status
  ```
- **Example Response:**
  ```json
  {
    "currentMode": "auto-commissioning",
    "guiAccessible": true,
    "networkInfo": {
      "network": "Main network",
      "ip": "192.168.0.104",
      "access": "GUI accessible, BLE commissioning active"
    },
    "availableModes": [
      {
        "mode": "normal",
        "description": "Normal client mode",
        "guiAccessible": true
      },
      {
        "mode": "auto-commissioning",
        "description": "GUI-driven commissioning workflow",
        "guiAccessible": true
      }
    ]
  }
  ```

---

### Test Network Configuration
- **Route:** `GET /api/network/test`
- **Description:** Returns diagnostics about the network configuration script and current mode.
- **Response:**
  - `200 OK`: Returns a JSON object with script existence, path, mode, permissions, network interfaces, and GUI accessibility.
  - `500 Internal Server Error`: Error testing network configuration.
- **Example Request:**
  ```http
  GET /api/network/test
  ```
- **Example Response:**
  ```json
  {
    "scriptExists": true,
    "scriptPath": "/app/network-config.sh",
    "currentMode": "auto-commissioning",
    "scriptPermissions": "UserRead, UserWrite, ...",
    "guiAccessible": true,
    "networkInterfaces": {
      "rawOutput": "...output of ip addr show..."
    }
  }
  ```

---

### Ping Endpoint
- **Route:** `GET /api/network/ping`
- **Description:** Simple health check endpoint. Returns `pong` if the controller is reachable.
- **Response:**
  - `200 OK`: Returns `pong`.
- **Example Request:**
  ```http
  GET /api/network/ping
  ```
- **Example Response:**
  ```json
  "pong"
  ```

---

## Network Modes

### Normal Mode
- **Description:** Standard operation on main network
- **GUI Access:** ✅ Available
- **Use Case:** Regular operation

### Auto Commissioning Mode
- **Description:** GUI-driven commissioning workflow (Recommended)
- **GUI Access:** ✅ Available
- **Use Case:** Device commissioning with GUI access

### Client Commissioning Mode
- **Description:** Safe BLE commissioning (maintains connectivity)
- **GUI Access:** ✅ Available
- **Use Case:** Safe device commissioning

### AP Commissioning Mode
- **Description:** AP mode for device control
- **GUI Access:** ❌ Temporarily unavailable
- **Use Case:** Device control after commissioning

### Complete Mode
- **Description:** Complete commissioning and return to client mode
- **GUI Access:** ✅ Available
- **Use Case:** Finish commissioning workflow

---

## Matter Bridge Endpoints (python-matter-server)

### Health Check
- **Route:** `GET /health`
- **Description:** Returns the health status of the Matter Bridge and python-matter-server integration.
- **Response:**
  - `200 OK`: Returns health information.
- **Example Request:**
  ```http
  GET http://pi:8085/health
  ```
- **Example Response:**
  ```json
  {
    "status": "healthy",
    "version": "matter-integration-lightweight",
    "matter_server": "python-matter-server",
    "data_path": "/app/matter_data"
  }
  ```

---

### Commission Device
- **Route:** `POST /commission`
- **Description:** Commission a new Matter device using python-matter-server.
- **Request Body:**
  ```json
  {
    "device_name": "string",
    "device_type": "string",
    "pin": "20202021"
  }
  ```
- **Response:**
  - `200 OK`: Device commissioned successfully.
  - `400 Bad Request`: Commissioning failed.
  - `500 Internal Server Error`: Server error.
- **Example Request:**
  ```http
  POST http://pi:8085/commission
  Content-Type: application/json
  
  {
    "device_name": "Living Room Socket",
    "device_type": "nous_a8m_socket",
    "pin": "20202021"
  }
  ```
- **Example Response:**
  ```json
  {
    "status": "success",
    "message": "NOUS A8M device commissioned successfully",
    "device_id": "nous_a8m_1001",
    "node_id": 1001,
    "method": "python-matter-server"
  }
  ```

---

### Toggle Device Power
- **Route:** `POST /device/{device_id}/power`
- **Description:** Toggle the power state of a Matter device.
- **Parameters:**
  - `device_id` (string, required): The device identifier.
- **Response:**
  - `200 OK`: Power state changed successfully.
  - `404 Not Found`: Device not found.
  - `500 Internal Server Error`: Control failed.
- **Example Request:**
  ```http
  POST http://pi:8085/device/nous_a8m_1001/power
  ```
- **Example Response:**
  ```json
  {
    "success": true,
    "device_id": "nous_a8m_1001",
    "power_state": "on",
    "power_consumption": 15.5
  }
  ```

---

### Get Power Metrics
- **Route:** `GET /device/{device_id}/power-metrics`
- **Description:** Get power consumption metrics for a device.
- **Parameters:**
  - `device_id` (string, required): The device identifier.
- **Response:**
  - `200 OK`: Returns power metrics.
  - `404 Not Found`: Device not found.
- **Example Request:**
  ```http
  GET http://pi:8085/device/nous_a8m_1001/power-metrics
  ```
- **Example Response:**
  ```json
  {
    "device_id": "nous_a8m_1001",
    "device_name": "Living Room Socket",
    "device_type": "nous_a8m_socket",
    "power_state": "on",
    "power_consumption": 15.2,
    "voltage": 230.0,
    "current": 0.066,
    "energy_today": 7.6,
    "timestamp": 2409425.509012635,
    "online": true
  }
  ```

---

### Get Device State
- **Route:** `GET /device/{device_id}/state`
- **Description:** Get the current state of a device.
- **Parameters:**
  - `device_id` (string, required): The device identifier.
- **Response:**
  - `200 OK`: Returns device state.
  - `404 Not Found`: Device not found.
- **Example Request:**
  ```http
  GET http://pi:8085/device/nous_a8m_1001/state
  ```
- **Example Response:**
  ```json
  {
    "device_id": "nous_a8m_1001",
    "name": "Living Room Socket",
    "type": "nous_a8m_socket",
    "node_id": 1001,
    "state": {
      "power": true,
      "power_consumption": 15.5,
      "energy": 0.0
    },
    "commissioned": true,
    "online": true,
    "last_seen": 2409425.509012635,
    "mock": false
  }
  ```

---

### List All Devices
- **Route:** `GET /devices`
- **Description:** List all commissioned Matter devices.
- **Response:**
  - `200 OK`: Returns list of devices.
- **Example Request:**
  ```http
  GET http://pi:8085/devices
  ```
- **Example Response:**
  ```json
  {
    "devices": [
      {
        "device_id": "nous_a8m_1001",
        "name": "Living Room Socket",
        "type": "nous_a8m_socket",
        "node_id": 1001,
        "online": true,
        "power_state": true,
        "mock": false
      }
    ],
    "count": 1,
    "matter_server": "python-matter-server"
  }
  ```

---

### Test Matter Server
- **Route:** `GET /dev/matter-server-test`
- **Description:** Test connectivity to python-matter-server.
- **Response:**
  - `200 OK`: Returns connection status.
- **Example Request:**
  ```http
  GET http://pi:8085/dev/matter-server-test
  ```
- **Example Response:**
  ```json
  {
    "status": "connected",
    "matter_server": "python-matter-server",
    "url": "ws://localhost:8084/ws",
    "nodes": 1,
    "test_time": 2409425.509012635
  }
  ``` 