# Pi Configuration System

This directory uses a centralized configuration approach for Pi-related settings.

## Files

- **`pi-config.sh`** - Central configuration file containing all Pi-related settings
- **`update-pi-ip.sh`** - Helper script to update the Pi's IP address
- **`transfer-matter-tools.sh`** - Transfer script that uses the centralized config

## Configuration Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `PI_IP` | Pi's IP address | `${PI_IP}` |
| `PI_USER` | SSH username | `pi` |
| `PI_SSH_KEY` | SSH key path | `~/.ssh/id_rsa` |
| `PI_HOME` | Pi's home directory | `/home/pi` |
| `PI_MATTER_DIR` | Matter SDK directory on Pi | `/home/pi/matter-sdk` |
| `TRANSFER_TIMEOUT` | Transfer timeout in seconds | `300` |

## Usage

### View Current Configuration
```bash
./pi-config.sh
```

### Update Pi IP Address
```bash
./update-pi-ip.sh 192.168.0.105
```

### Transfer Matter Tools
```bash
./transfer-matter-tools.sh
```

## Benefits

1. **Single Source of Truth**: All Pi-related settings are in one place
2. **Easy Updates**: Change IP address once, affects all scripts
3. **Consistency**: All scripts use the same configuration
4. **Maintainability**: Easy to modify settings without hunting through multiple files

## Adding New Scripts

To add a new script that uses Pi configuration:

```bash
#!/bin/bash

# Source configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [[ -f "$SCRIPT_DIR/pi-config.sh" ]]; then
    source "$SCRIPT_DIR/pi-config.sh"
else
    echo "Error: pi-config.sh not found in $SCRIPT_DIR"
    exit 1
fi

# Use configuration variables
echo "Connecting to $PI_USER@$PI_IP..."
ssh -i "$PI_SSH_KEY" "$PI_USER@$PI_IP" "your_command_here"
``` 