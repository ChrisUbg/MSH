#!/bin/bash
source config/environment.sh

# Network configuration script for MSH
# This script handles both normal and commissioning modes

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

# Function to switch to normal mode
switch_to_normal_mode() {
    echo "Switching to normal mode..."
    
    # Stop the access point
    systemctl stop hostapd
    systemctl stop dnsmasq
    
    # Configure wlan0 for client mode
    cat > /etc/wpa_supplicant/wpa_supplicant-wlan0.conf << EOF
ctrl_interface=DIR=/var/run/wpa_supplicant GROUP=netdev
update_config=1
country=AT

network={
    ssid="$NORMAL_SSID"
    psk="$NORMAL_PSK"
}
EOF

    # Restart networking
    systemctl restart networking
    systemctl restart wpa_supplicant@wlan0
    
    # Remove all commissioning flags
    rm -f /etc/msh/commissioning
    rm -f /etc/msh/client_commissioning
    rm -f /etc/msh/auto_commissioning
    
    echo "Switched to normal mode"
}

# Function to switch to client commissioning mode (SAFE - maintains connectivity)
switch_to_client_commissioning_mode() {
    echo "Switching to client commissioning mode (maintains connectivity)..."
    
    # Ensure we're in client mode (should already be)
    if ! systemctl is-active --quiet wpa_supplicant@wlan0; then
        echo "Starting wpa_supplicant for client mode..."
        systemctl start wpa_supplicant@wlan0
    fi
    
    # Create client commissioning flag
    mkdir -p /etc/msh
    touch /etc/msh/client_commissioning
    
    # Remove other commissioning flags
    rm -f /etc/msh/commissioning
    rm -f /etc/msh/auto_commissioning
    
    echo "Switched to client commissioning mode"
    echo "Pi remains on main network (${PI_IP}) for safe BLE commissioning"
    echo "GUI remains accessible at msh.local:8083"
}

# Function to switch to AP commissioning mode (temporary - for device control)
switch_to_ap_commissioning_mode() {
    echo "Switching to AP commissioning mode (temporary)..."
    
    # Stop client mode
    systemctl stop wpa_supplicant@wlan0
    
    # Configure hostapd
    cat > /etc/hostapd/hostapd.conf << EOF
interface=wlan0
driver=nl80211
ssid=$AP_SSID
hw_mode=g
channel=$AP_CHANNEL
wmm_enabled=0
macaddr_acl=0
auth_algs=1
ignore_broadcast_ssid=0
wpa=2
wpa_passphrase=$AP_PSK
wpa_key_mgmt=WPA-PSK
wpa_pairwise=TKIP
rsn_pairwise=CCMP
EOF

    # Configure dnsmasq
    cat > /etc/dnsmasq.conf << EOF
interface=wlan0
dhcp-range=$AP_NETWORK,12h
EOF

    # Configure static IP
    cat > /etc/dhcpcd.conf << EOF
interface wlan0
    static ip_address=$AP_IP/24
    nohook wpa_supplicant
EOF

    # Create commissioning flag
    mkdir -p /etc/msh
    touch /etc/msh/commissioning
    
    # Remove other commissioning flags
    rm -f /etc/msh/client_commissioning
    rm -f /etc/msh/auto_commissioning
    
    # Restart services
    systemctl restart networking
    systemctl restart hostapd
    systemctl restart dnsmasq
    
    echo "Switched to AP commissioning mode"
    echo "Pi is now on AP network ($AP_IP) - devices can join for control"
    echo "Note: GUI access temporarily unavailable during AP mode"
}

# Function to switch to auto commissioning mode (GUI-driven workflow)
switch_to_auto_commissioning_mode() {
    echo "Switching to auto commissioning mode (GUI-driven workflow)..."
    
    # NEVER stop wpa_supplicant - this would break connectivity!
    # Only start it if it's not running
    if ! systemctl is-active --quiet wpa_supplicant@wlan0; then
        echo "Starting wpa_supplicant for client mode..."
        sudo systemctl start wpa_supplicant@wlan0
    else
        echo "wpa_supplicant already running - maintaining connectivity"
    fi
    
    # Ensure we're NOT in AP mode
    if systemctl is-active --quiet hostapd; then
        echo "Stopping hostapd to ensure client mode..."
        sudo systemctl stop hostapd
        sudo systemctl stop dnsmasq
    fi
    
    # Create auto commissioning flag
    mkdir -p /etc/msh
    touch /etc/msh/auto_commissioning
    
    # Remove other commissioning flags
    rm -f /etc/msh/commissioning
    rm -f /etc/msh/client_commissioning
    
    echo "Switched to auto commissioning mode"
    echo "✅ GUI remains accessible at msh.local:8083"
    echo "✅ Pi stays connected to main network"
    echo "✅ BLE commissioning will work in client mode"
    echo "✅ After successful commissioning, mode will auto-switch back to client"
}

# Function to complete commissioning and return to client mode
complete_commissioning() {
    echo "Completing commissioning and returning to client mode..."
    
    # Switch back to client mode
    switch_to_normal_mode
    
    echo "Commissioning completed successfully"
    echo "Pi is back in normal client mode"
    echo "GUI accessible at msh.local:8083"
}

# Function to get current mode status
get_mode_status() {
    echo "=== Network Mode Status ==="
    if is_commissioning_mode; then
        echo "Current Mode: AP Commissioning Mode"
        echo "IP Address: $AP_IP"
        echo "Network: $AP_NETWORK"
        echo "SSID: $AP_SSID"
        echo "GUI Access: Temporarily unavailable"
    elif is_client_commissioning_mode; then
        echo "Current Mode: Client Commissioning Mode"
        echo "Status: Connected to main network (safe for BLE commissioning)"
        echo "GUI Access: Available at msh.local:8083"
        echo "Use this mode for device commissioning"
    elif is_auto_commissioning_mode; then
        echo "Current Mode: Auto Commissioning Mode"
        echo "Status: Connected to main network (GUI-driven workflow)"
        echo "GUI Access: Available at msh.local:8083"
        echo "BLE commissioning active - will auto-return to client mode"
    else
        echo "Current Mode: Normal Client Mode"
        echo "Status: Connected to main network"
        echo "GUI Access: Available at msh.local:8083"
    fi
    
    echo ""
    echo "Available Commands:"
    echo "  $0 normal              - Switch to normal client mode"
    echo "  $0 auto-commissioning  - Start GUI-driven commissioning workflow"
    echo "  $0 commissioning-client - Switch to client commissioning mode (SAFE)"
    echo "  $0 commissioning-ap    - Switch to AP commissioning mode (temporary)"
    echo "  $0 complete            - Complete commissioning and return to client mode"
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
    "commissioning-ap")
        switch_to_ap_commissioning_mode
        ;;
    "commissioning")
        echo "Warning: Direct AP mode switching may cause connectivity loss!"
        echo "Recommended: Use 'auto-commissioning' for GUI-driven workflow"
        echo "Or use 'commissioning-client' for safe commissioning"
        read -p "Continue with AP mode? (y/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            switch_to_ap_commissioning_mode
        else
            echo "Cancelled. Use 'auto-commissioning' for GUI-driven workflow."
        fi
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
        echo "Usage: $0 [normal|auto-commissioning|commissioning-client|commissioning-ap|commissioning|complete|status]"
        echo ""
        echo "Recommended GUI-Driven Workflow:"
        echo "1. $0 auto-commissioning  # Start commissioning with GUI access"
        echo "2. Use web GUI (msh.local:8083) for commissioning"
        echo "3. BLE commissioning works in client mode"
        echo "4. After success: $0 complete  # Auto-return to client mode"
        echo ""
        echo "Alternative Manual Workflow:"
        echo "1. $0 commissioning-client  # Safe BLE commissioning"
        echo "2. Commission devices via BLE"
        echo "3. $0 commissioning-ap      # Switch to AP mode for device control"
        echo "4. $0 normal               # Return to normal mode when done"
        exit 1
        ;;
esac 