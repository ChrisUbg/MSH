#!/bin/bash
source config/environment.sh

# Deployment script for MSH Matter Bridge with Real Commissioning
# Usage: ./deploy-matter-bridge.sh [pi-ip-address]

set -e

# Default Pi IP (will be auto-detected if not provided)
PI_IP=${1:-""}
PI_USER="chregg"
PROJECT_DIR="/${PROJECT_ROOT}"

# Function to detect Pi IP automatically (reused from deploy-to-pi.sh)
detect_pi_ip() {
    echo "🔍 Auto-detecting Pi IP address..." >&2
    
    # Try common Pi IP ranges
    local possible_ips=(
        "${PI_IP}"  # Your current IP
        "${PI_IP}"  # Previously seen
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

# Function to scan network for Pi (reused from deploy-to-pi.sh)
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
echo "🚀 Deploying MSH Matter Bridge with Real Commissioning"

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
            echo "  ./deploy-matter-bridge.sh [pi-ip-address]"
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

# Check if MSH project exists on Pi
echo "📁 Checking MSH project on Pi..."
if ! ssh $PI_USER@$PI_IP "test -d $PROJECT_DIR"; then
    echo "❌ MSH project not found on Pi at $PROJECT_DIR"
    echo "Please run the full deployment first: ./deploy-to-pi.sh"
    exit 1
fi

echo "✅ MSH project found"

# Create backup of current Matter bridge
echo "💾 Creating backup of current Matter bridge..."
ssh $PI_USER@$PI_IP "cd $PROJECT_DIR && cp -r Matter Matter.backup.$(date +%Y%m%d_%H%M%S) || true"

# Copy only the updated Matter bridge files
echo "📋 Copying updated Matter bridge files..."
rsync -avz --exclude='*.pyc' --exclude='__pycache__' /
    /${MATTER_DIR}/ $PI_USER@$PI_IP:$PROJECT_DIR//${MATTER_DIR}/

# Ensure network-config.sh is executable
echo "🔧 Setting permissions for network configuration..."
ssh $PI_USER@$PI_IP "cd $PROJECT_DIR && chmod +x network-config.sh"

# Stop only the Matter bridge container
echo "🛑 Stopping Matter bridge container..."
ssh $PI_USER@$PI_IP "cd $PROJECT_DIR && docker-compose -f docker-compose.prod-msh.yml stop matter-bridge || true"

# Remove the old Matter bridge container and image
echo "🗑️ Removing old Matter bridge container and image..."
ssh $PI_USER@$PI_IP "cd $PROJECT_DIR && docker-compose -f docker-compose.prod-msh.yml rm -f matter-bridge || true"
ssh $PI_USER@$PI_IP "docker rmi msh-matter-bridge:latest || true"

# Build and start only the Matter bridge container
echo "🔨 Building and starting updated Matter bridge..."
ssh $PI_USER@$PI_IP "cd $PROJECT_DIR && docker-compose -f docker-compose.prod-msh.yml up -d --build matter-bridge"

# Wait for Matter bridge to be ready
echo "⏳ Waiting for Matter bridge to be ready..."
sleep 15

# Test the new Matter bridge
echo "🧪 Testing new Matter bridge..."
echo "Testing health endpoint..."
if ssh $PI_USER@$PI_IP "curl -s http://localhost:8085/health"; then
    echo "✅ Matter bridge health check passed"
else
    echo "⚠️ Matter bridge health check failed, but continuing..."
fi

echo "Testing real commissioning endpoint..."
if ssh $PI_USER@$PI_IP "curl -s http://localhost:8085/dev/real-commissioning-test"; then
    echo "✅ Real commissioning test endpoint available"
else
    echo "⚠️ Real commissioning test endpoint failed, but continuing..."
fi

# Check container status
echo "📊 Checking Matter bridge container status..."
ssh $PI_USER@$PI_IP "cd $PROJECT_DIR && docker-compose -f docker-compose.prod-msh.yml ps matter-bridge"

echo ""
echo "🎉 Matter Bridge deployment complete!"
echo ""
echo "🔧 New Features Deployed:"
echo "   ✅ Real Matter Commissioning (BLE + WiFi)"
echo "   ✅ Network Mode Integration (Access Point switching)"
echo "   ✅ QR Code Parsing (MT: format)"
echo "   ✅ Device Control (Real Matter clusters)"
echo "   ✅ Error Handling (Multi-layer fallback)"
echo "   ✅ Testing Endpoints (/dev/real-commissioning-test)"
echo ""
echo "📱 Access your updated Matter Bridge:"
echo "   Matter Bridge: http://$PI_IP:8085"
echo "   Health Check: http://$PI_IP:8085/health"
echo "   Real Commissioning Test: http://$PI_IP:8085/dev/real-commissioning-test"
echo ""
echo "🧪 Testing Commands:"
echo "   Test real commissioning: curl http://$PI_IP:8085/dev/real-commissioning-test"
echo "   Test health: curl http://$PI_IP:8085/health"
echo "   View logs: ssh $PI_USER@$PI_IP 'cd $PROJECT_DIR && docker-compose -f docker-compose.prod-msh.yml logs -f matter-bridge'"
echo ""
echo "🔧 Commissioning Flow:"
echo "   1. Device QR Code → Parse MT: format"
echo "   2. BLE Discovery → Establish PASE session"
echo "   3. WiFi Credentials → Exchange via BLE"
echo "   4. Matter Commissioning → Perform handshake"
echo "   5. Device Control → Real cluster commands"
echo ""
echo "🌐 Network Mode Integration:"
echo "   - Normal Mode: Pi connects to main WiFi"
echo "   - Commissioning Mode: Pi becomes Access Point (MSH_Setup)"
echo "   - Automatic switching during commissioning"
echo "   - Error recovery and fallback"
echo ""
echo "🔄 Next Steps:"
echo "   1. Test real commissioning with NOUS A8M device"
echo "   2. Validate network mode switching"
echo "   3. Test device control (power on/off)"
echo "   4. Monitor error handling and fallbacks"
echo ""
echo "💡 Tip: Use the web UI at http://$PI_IP:8083 to commission devices!"
echo ""
echo "🔄 For future Matter bridge updates:"
echo "   ./deploy-matter-bridge.sh  # Auto-detect IP"
echo "   ./deploy-matter-bridge.sh [specific-ip]  # Use specific IP" 