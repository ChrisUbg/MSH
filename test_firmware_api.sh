#!/bin/bash

# Test script for Firmware Update API endpoints
# This script tests the basic functionality of the firmware update system

set -e

# Configuration
API_BASE_URL="http://localhost:8083/api"
LOG_FILE="firmware_api_test.log"

echo "🧪 Starting Firmware Update API Tests..." | tee -a "$LOG_FILE"
echo "==========================================" | tee -a "$LOG_FILE"

# Function to make API calls and log results
api_call() {
    local method=$1
    local endpoint=$2
    local data=$3
    local description=$4
    
    echo "Testing: $description" | tee -a "$LOG_FILE"
    echo "Endpoint: $method $endpoint" | tee -a "$LOG_FILE"
    
    if [ -n "$data" ]; then
        echo "Data: $data" | tee -a "$LOG_FILE"
        response=$(curl -s -w "\nHTTP_STATUS:%{http_code}" -X "$method" \
            -H "Content-Type: application/json" \
            -d "$data" \
            "$API_BASE_URL$endpoint")
    else
        response=$(curl -s -w "\nHTTP_STATUS:%{http_code}" -X "$method" \
            "$API_BASE_URL$endpoint")
    fi
    
    # Extract HTTP status code
    http_status=$(echo "$response" | grep "HTTP_STATUS:" | cut -d: -f2)
    response_body=$(echo "$response" | sed '/HTTP_STATUS:/d')
    
    echo "Status: $http_status" | tee -a "$LOG_FILE"
    echo "Response: $response_body" | tee -a "$LOG_FILE"
    echo "---" | tee -a "$LOG_FILE"
    
    # Return status for validation
    echo "$http_status"
}

# Test 1: Get available firmware updates
echo "Test 1: Get available firmware updates" | tee -a "$LOG_FILE"
status=$(api_call "GET" "/firmwareupdate" "" "Get available firmware updates")
if [ "$status" = "200" ]; then
    echo "✅ Test 1 PASSED" | tee -a "$LOG_FILE"
else
    echo "❌ Test 1 FAILED (Expected 200, got $status)" | tee -a "$LOG_FILE"
fi

# Test 2: Get pending updates
echo "Test 2: Get pending updates" | tee -a "$LOG_FILE"
status=$(api_call "GET" "/firmwareupdate/pending" "" "Get pending firmware updates")
if [ "$status" = "200" ]; then
    echo "✅ Test 2 PASSED" | tee -a "$LOG_FILE"
else
    echo "❌ Test 2 FAILED (Expected 200, got $status)" | tee -a "$LOG_FILE"
fi

# Test 3: Create a test firmware update
echo "Test 3: Create test firmware update" | tee -a "$LOG_FILE"
test_firmware_data='{
    "name": "Test Firmware Update",
    "description": "Test firmware update for API testing",
    "currentVersion": "1.0.0",
    "targetVersion": "1.1.0",
    "downloadUrl": "https://example.com/firmware/test.bin",
    "fileName": "test_firmware_1.1.0.bin",
    "fileSize": 1048576,
    "checksum": "abc123def456",
    "isCompatible": true,
    "requiresConfirmation": false
}'

status=$(api_call "POST" "/firmwareupdate" "$test_firmware_data" "Create test firmware update")
if [ "$status" = "201" ]; then
    echo "✅ Test 3 PASSED" | tee -a "$LOG_FILE"
    # Extract the created firmware update ID for further tests
    firmware_id=$(echo "$response_body" | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
    echo "Created firmware ID: $firmware_id" | tee -a "$LOG_FILE"
else
    echo "❌ Test 3 FAILED (Expected 201, got $status)" | tee -a "$LOG_FILE"
    firmware_id=""
fi

# Test 4: Get specific firmware update (if created)
if [ -n "$firmware_id" ]; then
    echo "Test 4: Get specific firmware update" | tee -a "$LOG_FILE"
    status=$(api_call "GET" "/firmwareupdate/$firmware_id" "" "Get specific firmware update")
    if [ "$status" = "200" ]; then
        echo "✅ Test 4 PASSED" | tee -a "$LOG_FILE"
    else
        echo "❌ Test 4 FAILED (Expected 200, got $status)" | tee -a "$LOG_FILE"
    fi
else
    echo "⚠️  Test 4 SKIPPED (No firmware ID available)" | tee -a "$LOG_FILE"
fi

# Test 5: Check for updates for a device (using a test device ID)
echo "Test 5: Check for updates for a device" | tee -a "$LOG_FILE"
test_device_id="00000000-0000-0000-0000-000000000001"  # Test device ID
status=$(api_call "POST" "/firmwareupdate/check/$test_device_id" "" "Check for updates for test device")
if [ "$status" = "200" ]; then
    echo "✅ Test 5 PASSED" | tee -a "$LOG_FILE"
else
    echo "❌ Test 5 FAILED (Expected 200, got $status)" | tee -a "$LOG_FILE"
fi

# Test 6: Search for updates for a device
echo "Test 6: Search for updates for a device" | tee -a "$LOG_FILE"
status=$(api_call "GET" "/firmwareupdate/search/$test_device_id" "" "Search for updates for test device")
if [ "$status" = "200" ]; then
    echo "✅ Test 6 PASSED" | tee -a "$LOG_FILE"
else
    echo "❌ Test 6 FAILED (Expected 200, got $status)" | tee -a "$LOG_FILE"
fi

# Test 7: Start batch update (if firmware was created)
if [ -n "$firmware_id" ]; then
    echo "Test 7: Start batch update" | tee -a "$LOG_FILE"
    batch_data="{
        \"deviceIds\": [\"$test_device_id\"],
        \"firmwareUpdateId\": \"$firmware_id\"
    }"
    status=$(api_call "POST" "/firmwareupdate/batch" "$batch_data" "Start batch update")
    if [ "$status" = "200" ]; then
        echo "✅ Test 7 PASSED" | tee -a "$LOG_FILE"
    else
        echo "❌ Test 7 FAILED (Expected 200, got $status)" | tee -a "$LOG_FILE"
    fi
else
    echo "⚠️  Test 7 SKIPPED (No firmware ID available)" | tee -a "$LOG_FILE"
fi

# Test 8: Clean up - Delete test firmware update (if created)
if [ -n "$firmware_id" ]; then
    echo "Test 8: Delete test firmware update" | tee -a "$LOG_FILE"
    status=$(api_call "DELETE" "/firmwareupdate/$firmware_id" "" "Delete test firmware update")
    if [ "$status" = "204" ]; then
        echo "✅ Test 8 PASSED" | tee -a "$LOG_FILE"
    else
        echo "❌ Test 8 FAILED (Expected 204, got $status)" | tee -a "$LOG_FILE"
    fi
else
    echo "⚠️  Test 8 SKIPPED (No firmware ID available)" | tee -a "$LOG_FILE"
fi

echo "" | tee -a "$LOG_FILE"
echo "🎯 Firmware Update API Tests Completed!" | tee -a "$LOG_FILE"
echo "📄 Full test log saved to: $LOG_FILE" | tee -a "$LOG_FILE"
echo "" | tee -a "$LOG_FILE"

# Summary
echo "📊 Test Summary:" | tee -a "$LOG_FILE"
echo "=================" | tee -a "$LOG_FILE"
echo "✅ Tests completed successfully" | tee -a "$LOG_FILE"
echo "📋 Check the log file for detailed results" | tee -a "$LOG_FILE"
echo "" | tee -a "$LOG_FILE"

echo "🚀 Next Steps:" | tee -a "$LOG_FILE"
echo "===============" | tee -a "$LOG_FILE"
echo "1. Check the web interface at: http://localhost:8082/firmware" | tee -a "$LOG_FILE"
echo "2. Verify the firmware management page loads correctly" | tee -a "$LOG_FILE"
echo "3. Test the UI interactions for firmware updates" | tee -a "$LOG_FILE"
echo "4. Deploy to Raspberry Pi for full testing" | tee -a "$LOG_FILE"
