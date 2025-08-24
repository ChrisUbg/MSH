#!/bin/bash

echo "Testing Mock Matter API endpoints..."
echo "=================================="

# Wait a moment for the mock API to start
sleep 2

# Test health endpoint
echo "1. Testing health endpoint:"
curl -s http://localhost:8000/api/matter/health | jq .
echo ""

# Test device toggle endpoint
echo "2. Testing device toggle endpoint:"
curl -s -X POST http://localhost:8000/api/matter/device/123/toggle | jq .
echo ""

# Test device state endpoint
echo "3. Testing device state endpoint:"
curl -s http://localhost:8000/api/matter/device/123/state | jq .
echo ""

# Test device online endpoint
echo "4. Testing device online endpoint:"
curl -s http://localhost:8000/api/matter/device/123/online | jq .
echo ""

# Test power metrics endpoint
echo "5. Testing power metrics endpoint:"
curl -s http://localhost:8000/api/matter/device/123/power-metrics | jq .
echo ""

echo "Mock API test completed!"
