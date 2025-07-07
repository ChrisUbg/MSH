#!/bin/bash

# SAFE Network Configuration Script for MSH
# This script NEVER changes network mode - only sets commissioning flags
# The Pi ALWAYS stays on the main network and remains reachable

# Configuration variables
NORMAL_SSID="08-TvM6xr-FQ" 
NORMAL_PSK="Kartoffelernte"
AP_SSID="MSH_Setup"
AP_PSK="KrenWury34#" 
AP_CHANNEL=1
AP_IP="192.168.4.1"
AP_NETWORK="192.168.4.0/24"

# Function to check if we're in commissioning mode
is_commissioning_mode() {
    if [ -f "/etc/msh/commissioning" ]; then
        return 0
    else
        return 1
    fi
}

# Function to check if we're in client commissioning mode
is_client_commissioning_mode() {
    if [ -f "/etc/msh/client_commissioning" ]; then
        return 0
    else
        return 1
    fi
}

# Function to check if we're in auto commissioning mode
is_auto_commissioning_mode() {
    if [ -f "/etc/msh/auto_commissioning" ]; then
        return 0
    else
        return 1
    fi
}

# Function to switch to normal mode (SAFE - only removes flags)
switch_to_normal_mode() {
    echo "Switching to normal mode (SAFE - no network changes)..."
    
    # Remove all commissioning flags
    rm -f /etc/msh/commissioning
    rm -f /etc/msh/client_commissioning
    rm -f /etc/msh/auto_commissioning
    
    echo "✅ Switched to normal mode"
    echo "✅ Pi remains on main network (192.168.0.104)"
    echo "✅ GUI accessible at msh.local:8083"
}

# Function to switch to client commissioning mode (SAFE - only sets flag)
switch_to_client_commissioning_mode() {
    echo "Switching to client commissioning mode (SAFE - no network changes)..."
    
    # Create client commissioning flag
    mkdir -p /etc/msh
    touch /etc/msh/client_commissioning
    
    # Remove other commissioning flags
    rm -f /etc/msh/commissioning
    rm -f /etc/msh/auto_commissioning
    
    echo "✅ Switched to client commissioning mode"
    echo "✅ Pi remains on main network (192.168.0.104)"
    echo "✅ GUI accessible at msh.local:8083"
    echo "✅ Safe for BLE commissioning"
}

# Function to switch to auto commissioning mode (SAFE - only sets flag)
switch_to_auto_commissioning_mode() {
    echo "Switching to auto commissioning mode (SAFE - no network changes)..."
    
    # Create auto commissioning flag
    mkdir -p /etc/msh
    touch /etc/msh/auto_commissioning
    
    # Remove other commissioning flags
    rm -f /etc/msh/commissioning
    rm -f /etc/msh/client_commissioning
    
    echo "✅ Switched to auto commissioning mode"
    echo "✅ Pi remains on main network (192.168.0.104)"
    echo "✅ GUI accessible at msh.local:8083"
    echo "✅ BLE commissioning will work in client mode"
    echo "✅ After successful commissioning, mode will auto-switch back to client"
}

# Function to complete commissioning (SAFE - only removes flags)
complete_commissioning() {
    echo "Completing commissioning (SAFE - no network changes)..."
    
    # Switch back to normal mode
    switch_to_normal_mode
    
    echo "✅ Commissioning completed successfully"
    echo "✅ Pi is back in normal client mode"
    echo "✅ GUI accessible at msh.local:8083"
}

# Function to get current mode status
get_mode_status() {
    echo "=== SAFE Network Mode Status ==="
    if is_commissioning_mode; then
        echo "Current Mode: AP Commissioning Mode (FLAG ONLY)"
        echo "Network: Main network (192.168.0.104) - NO CHANGE"
        echo "GUI Access: ✅ Available at msh.local:8083"
        echo "Note: This is a flag-only mode - no network switching"
    elif is_client_commissioning_mode; then
        echo "Current Mode: Client Commissioning Mode (FLAG ONLY)"
        echo "Network: Main network (192.168.0.104) - NO CHANGE"
        echo "GUI Access: ✅ Available at msh.local:8083"
        echo "Status: Safe for BLE commissioning"
    elif is_auto_commissioning_mode; then
        echo "Current Mode: Auto Commissioning Mode (FLAG ONLY)"
        echo "Network: Main network (192.168.0.104) - NO CHANGE"
        echo "GUI Access: ✅ Available at msh.local:8083"
        echo "Status: GUI-driven commissioning workflow"
    else
        echo "Current Mode: Normal Client Mode"
        echo "Network: Main network (192.168.0.104)"
        echo "GUI Access: ✅ Available at msh.local:8083"
    fi
    
    echo ""
    echo "🔒 SAFE MODE: No network configuration changes"
    echo "✅ Pi always remains reachable at msh.local:8083"
    echo ""
    echo "Available Commands:"
    echo "  $0 normal              - Switch to normal mode (SAFE)"
    echo "  $0 auto-commissioning  - Start GUI-driven commissioning (SAFE)"
    echo "  $0 commissioning-client - Switch to client commissioning (SAFE)"
    echo "  $0 complete            - Complete commissioning (SAFE)"
    echo "  $0 status              - Show current mode status"
}

# Main script
case "$1" in
    "auto-commissioning")
        switch_to_auto_commissioning_mode
        ;;
    "commissioning-client")
        switch_to_client_commissioning_mode
        ;;
    "commissioning-ap"|"commissioning")
        echo "⚠️  WARNING: AP mode switching is DISABLED for safety"
        echo "✅ Use 'auto-commissioning' or 'commissioning-client' instead"
        echo "✅ These modes keep the Pi reachable at all times"
        echo ""
        echo "If you really need AP mode, use the original network-config.sh"
        echo "But be aware it may cause connectivity loss!"
        ;;
    "complete")
        complete_commissioning
        ;;
    "normal")
        switch_to_normal_mode
        ;;
    "status")
        get_mode_status
        ;;
    *)
        echo "Usage: $0 [normal|auto-commissioning|commissioning-client|complete|status]"
        echo ""
        echo "🔒 SAFE NETWORK CONFIGURATION"
        echo "✅ Pi ALWAYS remains reachable at msh.local:8083"
        echo "✅ No network mode switching - only flag changes"
        echo "✅ Perfect for GUI-driven commissioning workflow"
        echo ""
        echo "Recommended Workflow:"
        echo "1. $0 auto-commissioning  # Start commissioning (SAFE)"
        echo "2. Use web GUI (msh.local:8083) for commissioning"
        echo "3. BLE commissioning works in client mode"
        echo "4. After success: $0 complete  # Finish commissioning (SAFE)"
        exit 1
        ;;
esac 