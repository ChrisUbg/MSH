#!/bin/bash

# MSH mDNS Test Script
# This script tests the mDNS configuration

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${YELLOW}Testing MSH mDNS configuration...${NC}"

# Test 1: Check if msh.local resolves
echo -e "${YELLOW}1. Testing msh.local hostname resolution...${NC}"
if ping -c 1 msh.local > /dev/null 2>&1; then
    echo -e "${GREEN}✓ msh.local is reachable${NC}"
    MSH_IP=$(ping -c 1 msh.local | grep PING | cut -d'(' -f2 | cut -d')' -f1)
    echo -e "${GREEN}  Resolved to: $MSH_IP${NC}"
else
    echo -e "${RED}✗ msh.local is not reachable${NC}"
fi

# Test 2: Check if port 8083 is accessible
echo -e "${YELLOW}2. Testing port 8083 accessibility...${NC}"
if curl -s --connect-timeout 5 http://msh.local:8083 > /dev/null 2>&1; then
    echo -e "${GREEN}✓ http://msh.local:8083 is accessible${NC}"
else
    echo -e "${YELLOW}⚠ http://msh.local:8083 is not accessible (app might not be running)${NC}"
fi

# Test 3: Check Avahi service registration
echo -e "${YELLOW}3. Checking Avahi service registration...${NC}"
if command -v avahi-browse > /dev/null 2>&1; then
    if avahi-browse -at | grep -q "MSH"; then
        echo -e "${GREEN}✓ MSH service is registered with Avahi${NC}"
        avahi-browse -at | grep -A 5 "MSH"
    else
        echo -e "${YELLOW}⚠ MSH service not found in Avahi browse${NC}"
    fi
else
    echo -e "${YELLOW}⚠ avahi-browse not available${NC}"
fi

# Test 4: Check if we can resolve the service
echo -e "${YELLOW}4. Testing service resolution...${NC}"
if command -v avahi-resolve > /dev/null 2>&1; then
    if avahi-resolve -n msh.local > /dev/null 2>&1; then
        echo -e "${GREEN}✓ msh.local resolves via Avahi${NC}"
    else
        echo -e "${RED}✗ msh.local does not resolve via Avahi${NC}"
    fi
else
    echo -e "${YELLOW}⚠ avahi-resolve not available${NC}"
fi

echo ""
echo -e "${GREEN}✓ mDNS test completed!${NC}"
echo -e "${YELLOW}If all tests pass, you can access your MSH app at:${NC}"
echo -e "${GREEN}  http://msh.local:8083${NC}"

# Next steps
echo -e "${YELLOW}Next steps:${NC}"
echo -e "${GREEN}1. Apply database migrations inside the web container${NC}"
echo -e "${GREEN}2. After migration, the health check should pass and the app should become healthy${NC}"
echo -e "${GREEN}  docker exec msh_web_1 dotnet MSH.Web.dll --migrate${NC}" 