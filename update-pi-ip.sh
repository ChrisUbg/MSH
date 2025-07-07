#!/bin/bash

# Update Pi IP Address
# This script helps update the Pi's IP address in the centralized configuration

if [[ $# -eq 0 ]]; then
    echo "Usage: $0 <new_ip_address>"
    echo "Example: $0 192.168.0.104"
    exit 1
fi

NEW_IP="$1"

# Validate IP format (basic check)
if [[ ! $NEW_IP =~ ^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
    echo "Error: Invalid IP address format: $NEW_IP"
    exit 1
fi

# Update the configuration file
sed -i "s/export PI_IP=\"[^\"]*\"/export PI_IP=\"$NEW_IP\"/" pi-config.sh

echo "✅ Pi IP address updated to: $NEW_IP"
echo "Current configuration:"
./pi-config.sh 