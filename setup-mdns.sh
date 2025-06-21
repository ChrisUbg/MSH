#!/bin/bash

# MSH mDNS Setup Script
# This script sets up Avahi mDNS service for the MSH Smart Home application

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${YELLOW}Setting up mDNS configuration for MSH Smart Home...${NC}"

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    echo -e "${RED}This script must be run as root (use sudo)${NC}"
    exit 1
fi

# Install Avahi daemon if not already installed
echo -e "${YELLOW}Installing Avahi daemon...${NC}"
apt-get update
apt-get install -y avahi-daemon avahi-utils

# Create the service file
echo -e "${YELLOW}Creating Avahi service configuration...${NC}"
cat > /etc/avahi/services/msh.service << 'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE service-group SYSTEM "avahi-service.dtd">
<service-group>
  <name>MSH</name>
  <service>
    <name>MSH Smart Home Web App</name>
    <type>_http._tcp</type>
    <port>8083</port>
    <txt-record>description=MSH Smart Home Blazor Application</txt-record>
    <txt-record>version=1.0</txt-record>
  </service>
</service-group>
EOF

# Set proper permissions
chmod 644 /etc/avahi/services/msh.service

# Restart Avahi daemon
echo -e "${YELLOW}Restarting Avahi daemon...${NC}"
systemctl restart avahi-daemon

# Enable Avahi daemon to start on boot
systemctl enable avahi-daemon

# Verify the service is running
echo -e "${YELLOW}Verifying Avahi daemon status...${NC}"
if systemctl is-active --quiet avahi-daemon; then
    echo -e "${GREEN}✓ Avahi daemon is running${NC}"
else
    echo -e "${RED}✗ Avahi daemon failed to start${NC}"
    exit 1
fi

# Check if the service is properly registered
echo -e "${YELLOW}Checking mDNS service registration...${NC}"
sleep 2
if avahi-browse -at | grep -q "MSH"; then
    echo -e "${GREEN}✓ MSH service is registered with mDNS${NC}"
else
    echo -e "${YELLOW}⚠ MSH service not found in mDNS browse (this is normal if the app isn't running yet)${NC}"
fi

echo -e "${GREEN}✓ mDNS configuration completed successfully!${NC}"
echo -e "${YELLOW}Your MSH application will be accessible at:${NC}"
echo -e "${GREEN}  http://msh.local:8083${NC}"
echo -e "${YELLOW}Note: The application must be running for the mDNS service to be fully active.${NC}" 