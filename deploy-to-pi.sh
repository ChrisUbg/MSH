#!/bin/bash

# Deployment script for MSH to Raspberry Pi
# Usage: ./deploy-to-pi.sh [pi-ip-address]

set -e

# Default Pi IP (will be auto-detected if not provided)
PI_IP=${1:-""}
PI_USER="chregg"
PROJECT_DIR="~/MSH"

# Function to detect Pi IP automatically
detect_pi_ip() {
    echo "🔍 Auto-detecting Pi IP address..." >&2
    
    # Try common Pi IP ranges
    local possible_ips=(
        "192.168.0.102"  # Your current IP
        "192.168.0.104"  # Previously seen
        "192.168.0.106"  # Previously seen
        "192.168.1.100"
        "192.168.1.101"
        "192.168.1.102"
        "192.168.0.100"
        "192.168.0.101"
        "192.168.0.103"
        "192.168.0.105"
    )
    
    for ip in "${possible_ips[@]}"; do
        echo "  Testing $ip..." >&2
        if ping -c 1 -W 1 "$ip" > /dev/null 2>&1; then
            # Test SSH connection
            if ssh -o ConnectTimeout=3 -o StrictHostKeyChecking=accept-new "$PI_USER@$ip" "echo 'SSH test'" > /dev/null 2>&1; then
                echo "✅ Found Pi at $ip" >&2
                echo "$ip"
                return 0
            fi
        fi
    done
    
    echo "❌ Could not auto-detect Pi IP" >&2
    return 1
}

# Function to scan network for Pi
scan_network() {
    echo "🔍 Scanning network for Pi..." >&2
    
    # Get local network prefix
    local local_ip=$(hostname -I | awk '{print $1}')
    local network_prefix=$(echo "$local_ip" | cut -d. -f1-3)
    
    echo "  Scanning $network_prefix.0/24..." >&2
    
    # Scan common Pi IPs in the same network
    for i in {100..110}; do
        local test_ip="$network_prefix.$i"
        echo -n "  Testing $test_ip... " >&2
        
        if ping -c 1 -W 1 "$test_ip" > /dev/null 2>&1; then
            if ssh -o ConnectTimeout=2 -o StrictHostKeyChecking=accept-new "$PI_USER@$test_ip" "echo 'SSH test'" > /dev/null 2>&1; then
                echo "✅ Found Pi!" >&2
                echo "$test_ip"
                return 0
            fi
        fi
        echo "❌" >&2
    done
    
    return 1
}

# Main script
echo "🚀 Deploying MSH to Raspberry Pi"

# Auto-detect IP if not provided
if [ -z "$PI_IP" ]; then
    echo "📡 No IP provided, auto-detecting..."
    DETECTED_IP=$(detect_pi_ip)
    
    if [ $? -eq 0 ]; then
        PI_IP="$DETECTED_IP"
    else
        echo "🔍 Trying network scan..."
        DETECTED_IP=$(scan_network)
        
        if [ $? -eq 0 ]; then
            PI_IP="$DETECTED_IP"
        else
            echo "❌ Could not find Pi automatically"
            echo "Please provide the IP address manually:"
            echo "  ./deploy-to-pi.sh [pi-ip-address]"
            exit 1
        fi
    fi
fi

echo "🎯 Using Pi IP: $PI_IP"

# Check if we can reach the Pi
echo "📡 Checking connection to Pi..."
if ! ping -c 1 $PI_IP > /dev/null 2>&1; then
    echo "❌ Cannot reach Pi at $PI_IP"
    echo "Please check the IP address and network connection"
    exit 1
fi

echo "✅ Pi is reachable"

# Create project directory on Pi
echo "📁 Creating project directory on Pi..."
ssh $PI_USER@$PI_IP "mkdir -p $PROJECT_DIR"

# Copy project files to Pi
echo "📋 Copying project files to Pi..."
rsync -avz --exclude='.git' --exclude='bin' --exclude='obj' --exclude='node_modules' \
    --exclude='*.log' --exclude='.vs' --exclude='.vscode' \
    ./ $PI_USER@$PI_IP:$PROJECT_DIR/

# Stop any existing MSH containers on Pi
echo "🛑 Stopping existing MSH containers..."
ssh $PI_USER@$PI_IP "cd $PROJECT_DIR && docker-compose -f docker-compose.prod-msh.yml down || true"

# Build and start containers on Pi
echo "🔨 Building and starting containers on Pi..."
ssh $PI_USER@$PI_IP "cd $PROJECT_DIR && docker-compose -f docker-compose.prod-msh.yml up -d --build"

# Setup mDNS configuration on Pi
echo "🌐 Setting up mDNS configuration..."
ssh $PI_USER@$PI_IP "cd $PROJECT_DIR && sudo ./setup-mdns.sh"

# Wait for containers to be healthy
echo "⏳ Waiting for containers to be healthy..."
sleep 30

# Check container status
echo "📊 Checking container status..."
ssh $PI_USER@$PI_IP "cd $PROJECT_DIR && docker-compose -f docker-compose.prod-msh.yml ps"

echo ""
echo "🎉 Deployment complete!"
echo ""
echo "📱 Access your MSH application:"
echo "   Web UI: http://$PI_IP:8083"
echo "   Web UI (mDNS): http://msh.local:8083"
echo "   Matter Bridge: http://$PI_IP:8085"
echo ""
echo "📋 Container ports:"
echo "   - Web: 8083 (avoiding conflict with haustagebuch_web_1 on 8080)"
echo "   - Database: 5435 (avoiding conflict with existing postgres on 5432)"
echo "   - Matter Bridge: 8085"
echo ""
echo "🌐 mDNS Configuration:"
echo "   - Hostname: msh.local"
echo "   - Port: 8083"
echo "   - Service: MSH Smart Home Web App"
echo ""
echo "🔧 Useful commands:"
echo "   View logs: ssh $PI_USER@$PI_IP 'cd $PROJECT_DIR && docker-compose -f docker-compose.prod-msh.yml logs -f'"
echo "   Stop services: ssh $PI_USER@$PI_IP 'cd $PROJECT_DIR && docker-compose -f docker-compose.prod-msh.yml down'"
echo "   Restart services: ssh $PI_USER@$PI_IP 'cd $PROJECT_DIR && docker-compose -f docker-compose.prod-msh.yml restart'"
echo "   Check mDNS: ssh $PI_USER@$PI_IP 'avahi-browse -at | grep MSH'"
echo ""
echo "🔄 For future deployments with changing IP:"
echo "   ./deploy-to-pi.sh  # Auto-detect IP"
echo "   ./deploy-to-pi.sh [specific-ip]  # Use specific IP"
echo ""
echo "💡 Tip: Use http://msh.local:8083 for consistent access regardless of IP changes!" 