# API Documentation

## NetworkController Endpoints

### Get Current Network Mode
- **Route:** `GET /api/network/mode`
- **Description:** Returns the current network mode (`normal` or `commissioning`).
- **Response:**
  - `200 OK`: Returns a string with the current mode.
  - `500 Internal Server Error`: Error getting current network mode.
- **Example Request:**
  ```http
  GET /api/network/mode
  ```
- **Example Response:**
  ```json
  "normal"
  ```

---

### Switch Network Mode
- **Route:** `POST /api/network/switch/{mode}`
- **Description:** Switches the network mode to `normal` or `commissioning`.
- **Parameters:**
  - `mode` (string, required): Must be `normal` or `commissioning`.
- **Response:**
  - `200 OK`: Successfully switched mode.
  - `400 Bad Request`: Invalid mode.
  - `500 Internal Server Error`: Error switching network mode.
- **Example Request:**
  ```http
  POST /api/network/switch/commissioning
  ```
- **Example Response:**
  ```json
  "Switched to commissioning mode"
  ```

---

### Test Network Configuration
- **Route:** `GET /api/network/test`
- **Description:** Returns diagnostics about the network configuration script and current mode.
- **Response:**
  - `200 OK`: Returns a JSON object with script existence, path, mode, permissions, and network interfaces.
  - `500 Internal Server Error`: Error testing network configuration.
- **Example Request:**
  ```http
  GET /api/network/test
  ```
- **Example Response:**
  ```json
  {
    "ScriptExists": true,
    "ScriptPath": "/app/network-config.sh",
    "CurrentMode": "normal",
    "ScriptPermissions": "UserRead, UserWrite, ...",
    "NetworkInterfaces": {
      "RawOutput": "...output of ip addr show..."
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