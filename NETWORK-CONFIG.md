# Network Configuration

This document describes the network configuration system for the Smart Home project, including available modes, the configuration script, switching methods, permissions, and troubleshooting tips.

## Network Modes

- **Normal Mode:**
  - Device operates as a WiFi client on the main network (e.g., "A1 network").
  - Used for standard operation and communication with other devices/services.

- **Commissioning Mode:**
  - Device acts as a WiFi Access Point for onboarding or special configuration.
  - Used for initial setup, device discovery, or troubleshooting.

## Network Configuration Script

- **Location:** `/app/network-config.sh`
- **Purpose:** Handles switching between normal and commissioning modes.
- **Permissions:** Must be executable by the application user. Typical permissions: `-rwxrwxrwx` (for testing; restrict as needed for production).

### Example Script Usage

```sh
# Switch to normal mode
/app/network-config.sh normal

# Switch to commissioning mode
/app/network-config.sh commissioning
```

## Switching Modes

You can switch network modes via:

- **Web UI:** Use the Network Settings page to select and switch modes interactively.
- **API:** Use the endpoint `POST /api/network/switch/{mode}` (see [API.md](API.md)).
- **Direct Script Call:** Run the script manually on the device.

## Diagnostics & Status

- Use the Network Test page in the UI to check:
  - Script existence and permissions
  - Current mode
  - Network interfaces
- Use the API endpoint `GET /api/network/test` for programmatic diagnostics.

## Troubleshooting

- **Script Not Found:** Ensure `/app/network-config.sh` exists and is mounted or copied into the container/device.
- **Permission Denied:** Run `chmod +x /app/network-config.sh` to make the script executable.
- **Mode Not Switching:**
  - Check script logs/output for errors.
  - Ensure the application has permission to modify network interfaces.
  - Verify the device supports both client and AP modes.
- **Network Interfaces Not Listed:** Ensure the `ip` command is available and the app has permission to execute it.

## Example Output

```
Script Exists: True
Script Path: /app/network-config.sh
Current Mode: normal
Script Permissions: UserRead, UserWrite, ...
Network Interfaces: (output of `ip addr show`)
```

## See Also
- [API.md](API.md) — API documentation for network endpoints
- [README.md](README.md) — Project overview and entry point 