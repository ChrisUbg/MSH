#!/bin/bash

# Pi IP Broadcast Script
# Run this on the Pi to broadcast its IP address

set -e

PI_HOSTNAME=$(hostname)
PI_IP=$(hostname -I | awk '{print $1}')
PI_USER=$(whoami)
TIMESTAMP=$(date '+%Y-%m-%d %H:%M:%S')

echo "📡 Broadcasting Pi IP address..."
echo "Hostname: $PI_HOSTNAME"
echo "IP: $PI_IP"
echo "User: $PI_USER"
echo "Time: $TIMESTAMP"

# Create a simple HTTP server to broadcast IP
echo "🌐 Starting IP broadcast server on port 8080..."

# Create a simple HTML page with Pi info
cat > /tmp/pi-info.html << EOF
<!DOCTYPE html>
<html>
<head>
    <title>Pi Information</title>
    <meta charset="utf-8">
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; }
        .info { background: #f0f0f0; padding: 20px; border-radius: 5px; }
        .ip { font-size: 24px; color: #0066cc; font-weight: bold; }
    </style>
</head>
<body>
    <h1>🍓 Raspberry Pi Information</h1>
    <div class="info">
        <p><strong>Hostname:</strong> $PI_HOSTNAME</p>
        <p><strong>IP Address:</strong> <span class="ip">$PI_IP</span></p>
        <p><strong>User:</strong> $PI_USER</p>
        <p><strong>Last Updated:</strong> $TIMESTAMP</p>
    </div>
    <h2>🚀 MSH Services</h2>
    <div class="info">
        <p><strong>Web UI:</strong> <a href="http://$PI_IP:8083">http://$PI_IP:8083</a></p>
        <p><strong>Matter Bridge:</strong> <a href="http://$PI_IP:8085">http://$PI_IP:8085</a></p>
        <p><strong>Database:</strong> $PI_IP:5435</p>
    </div>
    <h2>📊 Container Status</h2>
    <div class="info">
        <pre>$(docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" 2>/dev/null || echo "Docker not available")</pre>
    </div>
</body>
</html>
EOF

# Start a simple HTTP server
echo "📡 IP broadcast server running at: http://$PI_IP:8080"
echo "Press Ctrl+C to stop"

# Run the server
cd /tmp && python3 -m http.server 8080 2>/dev/null || python -m SimpleHTTPServer 8080 2>/dev/null || echo "Python not available, using netcat" && while true; do echo -e "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n$(cat pi-info.html)" | nc -l 8080; done 