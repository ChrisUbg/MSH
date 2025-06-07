#!/bin/bash

# Test script for MSH Docker setup

echo "Testing MSH Docker setup..."

# Function to check if a port is in use
check_port() {
    if lsof -i:$1 > /dev/null 2>&1; then
        echo "Port $1 is in use"
        return 1
    else
        echo "Port $1 is available"
        return 0
    fi
}

# Function to test Docker service
test_service() {
    local service=$1
    local port=$2
    local url=$3

    echo "Testing $service..."
    
    # Check if container is running
    if docker ps | grep -q $service; then
        echo "$service container is running"
    else
        echo "ERROR: $service container is not running"
        return 1
    fi
    
    # Test port availability
    if check_port $port; then
        echo "Port $port is available"
    else
        echo "ERROR: Port $port is in use"
        return 1
    fi
    
    # Test service health
    if curl -s $url/health > /dev/null; then
        echo "$service health check passed"
    else
        echo "ERROR: $service health check failed"
        return 1
    fi
    
    return 0
}

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "ERROR: Docker is not running"
    exit 1
fi

# Check required ports
echo "Checking required ports..."
check_port 8082  # Web service
check_port 5434  # Database
check_port 8084  # Matter bridge

# Test services
echo "Testing services..."
test_service "msh-web" 8082 "http://localhost:8082"
test_service "msh-db" 5434 "http://localhost:5434"
test_service "msh-matter-bridge" 8084 "http://localhost:8084"

# Test network connectivity
echo "Testing network connectivity..."
if ping -c 1 shrouter.local > /dev/null 2>&1; then
    echo "Network connectivity to Raspberry Pi is working"
else
    echo "ERROR: Cannot reach Raspberry Pi"
    exit 1
fi

echo "All tests completed successfully!" 