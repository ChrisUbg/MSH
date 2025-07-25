#!/bin/bash

echo "Starting device toggle test - 3 cycles with 5-second intervals"

# Device-specific Node IDs (from successful commissioning)
OFFICE_SOCKET_1_NODE_ID="4328ED19954E9DC0"
OFFICE_SOCKET_2_NODE_ID="4328ED19954E9DC1"

# Function to toggle a device
toggle_device() {
    local device_name=$1
    local node_id=$2
    echo "Toggling $device_name (Node ID: $node_id)..."
    /usr/local/bin/chip-tool onoff toggle 0x$node_id 1
    if [ $? -eq 0 ]; then
        echo "✅ $device_name toggle successful"
    else
        echo "❌ $device_name toggle failed"
    fi
}

# Perform 3 cycles
for cycle in {1..5}; do
    echo ""
    echo "=== CYCLE $cycle ==="
    echo "Time: $(date)"
    
    # Toggle both devices with their specific Node IDs
    toggle_device "Office-Socket 1" $OFFICE_SOCKET_1_NODE_ID
    sleep 1
    toggle_device "Office-Socket 2" $OFFICE_SOCKET_2_NODE_ID
    
    echo "Waiting 1 seconds before next cycle..."
    sleep 0.5
done

echo ""
echo "✅ Device toggle test completed!"
echo "Both devices should have been toggled 3 times each"
