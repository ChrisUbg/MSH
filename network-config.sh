#!/bin/bash

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
    
    # Remove commissioning flag
    rm -f /etc/msh/commissioning
    
    echo "Switched to normal mode"
}

# Function to switch to commissioning mode
switch_to_commissioning_mode() {
    echo "Switching to commissioning mode..."
    
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
    
    # Restart services
    systemctl restart networking
    systemctl restart hostapd
    systemctl restart dnsmasq
    
    echo "Switched to commissioning mode"
}

# Main script
if [ "$1" == "commissioning" ]; then
    switch_to_commissioning_mode
elif [ "$1" == "normal" ]; then
    switch_to_normal_mode
else
    echo "Usage: $0 [commissioning|normal]"
    exit 1
fi 