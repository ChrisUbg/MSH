#!/bin/bash

# Emergency Network Recovery Script for MSH
# This script will ALWAYS restore the Pi to normal client mode

echo "🚨 EMERGENCY NETWORK RECOVERY 🚨"
echo "Restoring Pi to normal client mode..."

# Configuration variables
NORMAL_SSID="08-TvM6xr-FQ" 
NORMAL_PSK="Kartoffelernte"

# Force stop all network services that might interfere
echo "Stopping all network services..."
sudo systemctl stop hostapd 2>/dev/null || true
sudo systemctl stop dnsmasq 2>/dev/null || true
sudo systemctl stop wpa_supplicant@wlan0 2>/dev/null || true

# Wait a moment
sleep 2

# Configure wlan0 for client mode
echo "Configuring WiFi client mode..."
sudo cat > /etc/wpa_supplicant/wpa_supplicant-wlan0.conf << EOF
ctrl_interface=DIR=/var/run/wpa_supplicant GROUP=netdev
update_config=1
country=AT

network={
    ssid="$NORMAL_SSID"
    psk="$NORMAL_PSK"
}
EOF

# Configure dhcpcd for normal operation
echo "Configuring DHCP client..."
sudo cat > /etc/dhcpcd.conf << EOF
# A sample configuration for dhcpcd.
# See dhcpcd.conf(5) for details.

# Allow users of this group to interact with dhcpcd via the control socket.
#controlgroup wheel

# Inform the DHCP server of our hostname for DDNS.
hostname

# Use the hardware address of the interface for the Client ID.
clientid
# or
# Use the same DUID + IAID as set in DHCPv6 for DHCPv4 ClientID as per RFC4361.
# Some non-RFC compliant DHCP servers do not reply with this set.
# In this case, comment out duid and enable clientid above.
#duid

# Persist interface configuration when dhcpcd exits.
persistent

# Rapid commit support.
# Safe to enable by default because it requires the equivalent option set
# on the server to actually work.
option rapid_commit

# A list of options to request from the DHCP server.
option domain_name_servers, domain_name, domain_search, host_name
option classless_static_routes
# Most distributions have NTP support.
option ntp_servers
# Respect the network MTU. This is applied to DHCP routes.
option interface_mtu

# A ServerID is required by RFC2131.
require dhcp_server_identifier

# Generate Stable Private IPv6 Addresses instead of hardware based ones
slaac private

# Example static IP configuration:
#interface eth0
#static ip_address=192.168.1.10/24
#static ip6_address=fd51:42f8:caae:d92e::ff/64
#static routers=192.168.1.1
#static domain_name_servers=192.168.1.1 8.8.8.8 fd51:42f8:caae:d92e::1

# It is possible to fall back to a static IP if DHCP fails:
# define static profile
#profile static_eth0
#static ip_address=192.168.1.23/24
#static routers=192.168.1.1
#static domain_name_servers=192.168.1.1

# fallback to static profile on eth0
#interface eth0
#fallback static_eth0
EOF

# Remove all commissioning flags
echo "Removing commissioning flags..."
sudo rm -f /etc/msh/commissioning
sudo rm -f /etc/msh/client_commissioning
sudo rm -f /etc/msh/auto_commissioning

# Restart networking services
echo "Restarting networking services..."
sudo systemctl restart networking
sudo systemctl restart wpa_supplicant@wlan0

# Wait for network to stabilize
echo "Waiting for network to stabilize..."
sleep 10

# Check if we're connected
echo "Checking network connection..."
if ping -c 1 8.8.8.8 >/dev/null 2>&1; then
    echo "✅ SUCCESS: Pi is connected to the internet"
else
    echo "⚠️  WARNING: Pi may not be connected to the internet"
fi

# Show current IP
echo "Current network status:"
ip addr show wlan0 | grep "inet " || echo "No IP address assigned yet"

echo ""
echo "🎉 RECOVERY COMPLETE!"
echo "The Pi should now be reachable on your main network."
echo "Try connecting to: msh.local:8083 or the Pi's IP address"
echo ""
echo "If you still can't reach the Pi, try:"
echo "1. Wait a few more minutes for network to fully stabilize"
echo "2. Check your router's DHCP client list for the Pi's new IP"
echo "3. Try connecting to the Pi directly via HDMI/monitor" 