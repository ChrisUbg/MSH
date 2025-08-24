#!/bin/bash
# Test script for Device Action Delay API endpoints
set -e

API_BASE_URL="http://localhost:8083/api"
LOG_FILE="device_action_delay_api_test.log"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test counter
TESTS_PASSED=0
TESTS_FAILED=0

# Log function
log() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a "$LOG_FILE"
}

# Test function
test_api() {
    local test_name="$1"
    local method="$2"
    local endpoint="$3"
    local data="$4"
    local expected_status="$5"
    
    log "Testing: $test_name"
    
    if [ "$method" = "GET" ]; then
        response=$(curl -s -w "\n%{http_code}" "$API_BASE_URL$endpoint")
    elif [ "$method" = "POST" ]; then
        response=$(curl -s -w "\n%{http_code}" -X POST -H "Content-Type: application/json" -d "$data" "$API_BASE_URL$endpoint")
    elif [ "$method" = "PUT" ]; then
        response=$(curl -s -w "\n%{http_code}" -X PUT -H "Content-Type: application/json" -d "$data" "$API_BASE_URL$endpoint")
    elif [ "$method" = "DELETE" ]; then
        response=$(curl -s -w "\n%{http_code}" -X DELETE "$API_BASE_URL$endpoint")
    fi
    
    # Extract status code (last line)
    status_code=$(echo "$response" | tail -n1)
    # Extract response body (all lines except last)
    response_body=$(echo "$response" | head -n -1)
    
    if [ "$status_code" = "$expected_status" ]; then
        echo -e "${GREEN}✓ PASS${NC} - $test_name (Status: $status_code)"
        ((TESTS_PASSED++))
    else
        echo -e "${RED}✗ FAIL${NC} - $test_name (Expected: $expected_status, Got: $status_code)"
        echo "Response: $response_body"
        ((TESTS_FAILED++))
    fi
    
    log "Response: $response_body"
    echo ""
}

# Start test log
log "Starting Device Action Delay API tests"
log "API Base URL: $API_BASE_URL"

# Test 1: Get all action delays
test_api "Get all action delays" "GET" "/deviceactiondelay" "" "200"

# Test 2: Create a device action delay
test_api "Create device action delay" "POST" "/deviceactiondelay" '{
    "deviceId": "00000000-0000-0000-0000-000000000001",
    "actionType": "on",
    "delaySeconds": 10,
    "isEnabled": true,
    "priority": 1
}' "201"

# Test 3: Create a group action delay
test_api "Create group action delay" "POST" "/deviceactiondelay" '{
    "deviceGroupId": "00000000-0000-0000-0000-000000000001",
    "actionType": "off",
    "delaySeconds": 30,
    "isEnabled": true,
    "priority": 2
}' "201"

# Test 4: Schedule device action
test_api "Schedule device action" "POST" "/deviceactiondelay/schedule/device" '{
    "deviceId": "00000000-0000-0000-0000-000000000001",
    "actionType": "toggle",
    "delaySeconds": 15,
    "parameters": "{\"brightness\": 50}"
}' "200"

# Test 5: Schedule group action
test_api "Schedule group action" "POST" "/deviceactiondelay/schedule/group" '{
    "deviceGroupId": "00000000-0000-0000-0000-000000000001",
    "actionType": "brightness",
    "delaySeconds": 20,
    "parameters": "{\"brightness\": 75}"
}' "200"

# Test 6: Get action delays for device
test_api "Get action delays for device" "GET" "/deviceactiondelay/device/00000000-0000-0000-0000-000000000001" "" "200"

# Test 7: Get action delays for group
test_api "Get action delays for group" "GET" "/deviceactiondelay/group/00000000-0000-0000-0000-000000000001" "" "200"

# Test 8: Execute all pending actions
test_api "Execute all pending actions" "POST" "/deviceactiondelay/execute-all" "" "200"

# Test 9: Get action delay status (will fail with 404 for non-existent ID)
test_api "Get action delay status (non-existent)" "GET" "/deviceactiondelay/00000000-0000-0000-0000-000000000999/status" "" "404"

# Test 10: Cancel action delay (will fail with 404 for non-existent ID)
test_api "Cancel action delay (non-existent)" "POST" "/deviceactiondelay/00000000-0000-0000-0000-000000000999/cancel" "" "404"

# Summary
echo "=========================================="
echo "Device Action Delay API Test Summary"
echo "=========================================="
echo -e "${GREEN}Tests Passed: $TESTS_PASSED${NC}"
echo -e "${RED}Tests Failed: $TESTS_FAILED${NC}"
echo "Total Tests: $((TESTS_PASSED + TESTS_FAILED))"

if [ $TESTS_FAILED -eq 0 ]; then
    echo -e "${GREEN}All tests passed! 🎉${NC}"
    exit 0
else
    echo -e "${RED}Some tests failed! ❌${NC}"
    exit 1
fi
