#!/bin/bash
source config/environment.sh

# Find Pi IP Address Script
# This script helps discover the Pi's IP address

set -e

echo "🔍 Finding Raspberry Pi..."

# Function to check if a Pi is broadcasting on port 8080
check_broadcast() {
    local ip=$1
    echo -n "  Checking broadcast on $ip:8080... "
    
    # Try to get the broadcast page
    if curl -s --connect-timeout 2 "http://$ip:8080" > /dev/null 2>&1; then
        echo "✅ Found Pi broadcast!"
        echo "🌐 Pi info available at: http://$ip:8080"
        echo "📱 MSH Web UI: http://$ip:8083"
        echo "🔧 Matter Bridge: http://$ip:8085"
        return 0
    else
        echo "❌"
        return 1
    fi
}

# Function to scan network for Pi
scan_network() {
    echo "🔍 Scanning network for Pi..."
    
    # Get local network prefix
    local local_ip=$(hostname -I | awk '{print $1}')
    local network_prefix=$(echo "$local_ip" | cut -d. -f1-3)
    
    echo "  Scanning $network_prefix.0/24..."
    
    # First, try to find broadcast servers
    for i in {100..110}; do
        local test_ip="$network_prefix.$i"
        if check_broadcast "$test_ip"; then
            echo "$test_ip"
            return 0
        fi
    done
    
    # If no broadcast found, try SSH
    echo "  No broadcast found, trying SSH..."
    for i in {100..110}; do
        local test_ip="$network_prefix.$i"
        echo -n "  Testing SSH on $test_ip... "
        
        if ping -c 1 -W 1 "$test_ip" > /dev/null 2>&1; then
            if ssh -o ConnectTimeout=2 -o StrictHostKeyChecking=accept-new "/${PI_USER}@$test_ip" "echo 'SSH test'" > /dev/null 2>&1; then
                echo "✅ Found Pi!"
                echo "$test_ip"
                return 0
            fi
        fi
        echo "❌"
    done
    
    return 1
}

# Main script
PI_IP=$(scan_network)

if [ $? -eq 0 ]; then
    echo ""
    echo "🎯 Found Pi at: $PI_IP"
    echo ""
    echo "📋 Quick commands:"
    echo "  SSH to Pi: ssh /${PI_USER}@$PI_IP"
    echo "  Deploy MSH: ./deploy-to-pi.sh $PI_IP"
    echo "  Auto-deploy: ./deploy-to-pi.sh"
else
    echo "❌ Could not find Pi"
    echo ""
    echo "💡 Troubleshooting:"
    echo "  1. Make sure Pi is powered on and connected to network"
    echo "  2. Run './pi-ip-broadcast.sh' on the Pi to start broadcasting"
    echo "  3. Check if Pi is using a different IP range"
    echo "  4. Try manual IP: ./deploy-to-pi.sh [pi-ip-address]"
fi 