#!/bin/bash

echo "Commissioning Device 2 (Office-Socket 2) using BLE method..."

# First, let's generate a new unique Node ID for Device 2
NODE_ID=$(python3 -c "
import time
import uuid

# Generate unique 64-bit Node ID
timestamp = int(time.time() * 1000) % 0xFFFFFFFF
device_hash = hash('office-socket-2') % 0xFFFFFFFF
unique_id = (timestamp << 32) | device_hash
node_id = f'{unique_id:016X}'
print(node_id)
")

echo "Generated Node ID for Device 2: $NODE_ID"

# Commission using BLE method with the new Node ID
echo "Starting BLE commissioning for Device 2..."
/usr/local/bin/chip-tool pairing ble-wifi 0x$NODE_ID 08-TvM6xr-FQ Kartoffelernte 85064361 97 --bypass-attestation-verifier true

if [ $? -eq 0 ]; then
    echo "✅ Device 2 commissioned successfully with Node ID: 0x$NODE_ID"
    
    # Test the device
    echo "Testing Device 2 control..."
    /usr/local/bin/chip-tool onoff toggle 0x$NODE_ID 1
    
    # Save the Node ID mapping
    echo "Saving Node ID mapping..."
    python3 -c "
import json
import os

# Load existing mappings or create new
mappings_file = 'device_node_mappings.json'
if os.path.exists(mappings_file):
    with open(mappings_file, 'r') as f:
        mappings = json.load(f)
else:
    mappings = {}

# Add new mapping
mappings['office-socket-2'] = '$NODE_ID'

# Save mappings
with open(mappings_file, 'w') as f:
    json.dump(mappings, f, indent=2)

print(f'✅ Saved mapping: office-socket-2 -> $NODE_ID')
"
else
    echo "❌ Device 2 commissioning failed"
    exit 1
fi 