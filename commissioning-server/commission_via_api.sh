#!/bin/bash

# Commission devices via the improved API
# Usage: ./commission_via_api.sh [device1|device2|both]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Device configurations
DEVICE_1_DATA='{
    "device_name": "office-socket-1",
    "device_type": "NOUS A8M Socket",
    "qr_code": "0150-175-1910",
    "network_ssid": "08-TvM6xr-FQ",
    "network_password": "Kartoffelernte",
    "pi_ip": "192.168.0.107"
}'

DEVICE_2_DATA='{
    "device_name": "office-socket-2",
    "device_type": "NOUS A8M Socket", 
    "qr_code": "3096-783-6060",
    "network_ssid": "08-TvM6xr-FQ",
    "network_password": "Kartoffelernte",
    "pi_ip": "192.168.0.107"
}'

# Function to check if server is running
check_server() {
    echo -e "${BLUE}🔍 Checking if server is running...${NC}"
    if curl -s http://localhost:8888/api/status > /dev/null; then
        echo -e "${GREEN}✅ Server is running${NC}"
        return 0
    else
        echo -e "${RED}❌ Server is not running${NC}"
        echo -e "${YELLOW}Please start the server with: nohup python3 main.py > server.log 2>&1 &${NC}"
        return 1
    fi
}

# Function to commission a device
commission_device() {
    local device_name=$1
    local device_data=$2
    
    echo -e "\n${BLUE}🔧 Commissioning $device_name via API...${NC}"
    echo -e "${YELLOW}Device data:${NC}"
    echo "$device_data" | python3 -m json.tool
    
    # Send commissioning request
    echo -e "\n${BLUE}📡 Sending commissioning request...${NC}"
    response=$(curl -s -w "\n%{http_code}" -X POST http://localhost:8888/commission \
        -H "Content-Type: application/json" \
        -d "$device_data")
    
    # Extract status code and response body
    http_code=$(echo "$response" | tail -n1)
    response_body=$(echo "$response" | head -n -1)
    
    if [ "$http_code" = "200" ]; then
        echo -e "${GREEN}✅ Commissioning successful!${NC}"
        echo -e "${YELLOW}Response:${NC}"
        echo "$response_body" | python3 -m json.tool
        
        # Extract device ID and Node ID for control testing
        device_id=$(echo "$response_body" | python3 -c "import sys, json; print(json.load(sys.stdin)['device_id'])")
        node_id=$(echo "$response_body" | python3 -c "import sys, json; print(json.load(sys.stdin).get('node_id', 'N/A'))")
        
        echo -e "\n${BLUE}🎛️  Testing device control...${NC}"
        control_response=$(curl -s -X POST http://localhost:8888/api/devices/control \
            -H "Content-Type: application/json" \
            -d "{
                \"device_id\": \"$device_id\",
                \"cluster\": \"onoff\",
                \"command\": \"toggle\",
                \"endpoint\": \"1\"
            }")
        
        if echo "$control_response" | python3 -c "import sys, json; exit(0 if json.load(sys.stdin).get('success') else 1)" 2>/dev/null; then
            echo -e "${GREEN}✅ Device control successful!${NC}"
            echo -e "${YELLOW}Control response:${NC}"
            echo "$control_response" | python3 -m json.tool
        else
            echo -e "${YELLOW}⚠️  Device control failed:${NC}"
            echo "$control_response"
        fi
        
        return 0
    else
        echo -e "${RED}❌ Commissioning failed (HTTP $http_code)${NC}"
        echo -e "${YELLOW}Error response:${NC}"
        echo "$response_body"
        return 1
    fi
}

# Function to test device control
test_device_control() {
    local device_id=$1
    local device_name=$2
    
    echo -e "\n${BLUE}🎛️  Testing control for $device_name...${NC}"
    
    # Test toggle
    echo -e "${YELLOW}Testing toggle command...${NC}"
    toggle_response=$(curl -s -X POST http://localhost:8888/api/devices/control \
        -H "Content-Type: application/json" \
        -d "{
            \"device_id\": \"$device_id\",
            \"cluster\": \"onoff\",
            \"command\": \"toggle\",
            \"endpoint\": \"1\"
        }")
    
    if echo "$toggle_response" | python3 -c "import sys, json; exit(0 if json.load(sys.stdin).get('success') else 1)" 2>/dev/null; then
        echo -e "${GREEN}✅ Toggle successful!${NC}"
    else
        echo -e "${RED}❌ Toggle failed:${NC}"
        echo "$toggle_response"
    fi
}

# Main function
main() {
    echo -e "${BLUE}🚀 MSH API Commissioning Test${NC}"
    echo "=================================="
    
    # Check server
    if ! check_server; then
        exit 1
    fi
    
    case "${1:-both}" in
        "device1")
            commission_device "Office Socket 1" "$DEVICE_1_DATA"
            ;;
        "device2")
            commission_device "Office Socket 2" "$DEVICE_2_DATA"
            ;;
        "both")
            echo -e "\n${BLUE}📋 Commissioning both devices...${NC}"
            
            # Commission Device 1
            if commission_device "Office Socket 1" "$DEVICE_1_DATA"; then
                echo -e "\n${YELLOW}⏳ Waiting 10 seconds before commissioning Device 2...${NC}"
                sleep 10
                
                # Commission Device 2
                commission_device "Office Socket 2" "$DEVICE_2_DATA"
            else
                echo -e "${RED}❌ Device 1 commissioning failed, skipping Device 2${NC}"
            fi
            ;;
        *)
            echo -e "${RED}❌ Invalid option. Use: device1, device2, or both${NC}"
            echo -e "${YELLOW}Usage: $0 [device1|device2|both]${NC}"
            exit 1
            ;;
    esac
    
    echo -e "\n${GREEN}🎉 Commissioning test completed!${NC}"
}

# Run main function
main "$@" 