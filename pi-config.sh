#!/bin/bash

# Pi Configuration
# This file centralizes Pi-related settings that can be sourced by other scripts

# Pi IP Address - Update this when the Pi's IP changes
export PI_IP="192.168.0.104"

# Pi SSH settings
export PI_USER="chregg"
export PI_SSH_KEY="~/.ssh/id_ed25519"

# Pi directories
export PI_HOME="/home/chregg"
export PI_MATTER_DIR="$PI_HOME/matter-sdk"

# Transfer settings
export TRANSFER_TIMEOUT=300  # 5 minutes timeout for transfers

# Display current configuration
echo "Pi Configuration:"
echo "  IP Address: $PI_IP"
echo "  User: $PI_USER"
echo "  Matter Directory: $PI_MATTER_DIR" 