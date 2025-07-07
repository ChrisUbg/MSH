#!/bin/bash
source config/environment.sh

# Matter SDK Build Monitor
# This script monitors the build progress on the development machine

# Change to the correct directory
cd dev_connectedhomeip 2>/dev/null || echo "Warning: dev_connectedhomeip directory not found"

echo "=== Matter SDK Build Monitor ==="
echo "Started: $(date)"
echo ""

# Function to check build status
check_build() {
    echo "=== Build Status Check ==="
    echo "Time: $(date)"
    
    # Check if build processes are running
    echo "--- Running Processes ---"
    ps aux | grep -E "(ninja|gcc|g/+/+|build_python)" | grep -v grep | head -5
    
    # Check build directory
    echo "--- Build Directory ---"
    if [ -d "out/python_lib" ]; then
        echo "Build directory exists"
        ls -la out/python_lib/ | head -5
    else
        echo "Build directory not created yet"
    fi
    
    # Check build log
    echo "--- Build Log ---"
    if [ -f "out/python_lib/.ninja_log" ]; then
        echo "Build log exists"
        echo "Log size: $(ls -lh out/python_lib/.ninja_log | awk '{print $5}')"
        echo "Last 3 entries:"
        tail -n 3 out/python_lib/.ninja_log 2>/dev/null
    else
        echo "Build log not created yet"
    fi
    
    # Check for key executables
    echo "--- Key Executables ---"
    if [ -f "out/python_lib/chip-repl" ]; then
        echo "✅ chip-repl: EXISTS"
    else
        echo "❌ chip-repl: NOT FOUND"
    fi
    
    if [ -f "out/python_lib/chip-tool" ]; then
        echo "✅ chip-tool: EXISTS"
    else
        echo "❌ chip-tool: NOT FOUND"
    fi
    
    echo ""
    echo "--- System Resources ---"
    echo "CPU Load: $(uptime | awk -F'load average:' '{print $2}')"
    echo "Memory: $(free -h | grep Mem | awk '{print $3"/"$2}')"
    echo "Disk Space: $(df -h . | tail -1 | awk '{print $4}') available"
    echo ""
}

# Function to show transfer instructions
show_transfer_instructions() {
    echo "=== Transfer Instructions ==="
    echo "Once build completes, transfer to Pi using:"
    echo ""
    echo "Method 1: SCP (Recommended)"
    echo "scp -r out/python_lib/ /${PI_USER}@192.168.0.106:~/connectedhomeip/out/"
    echo ""
    echo "Method 2: rsync (More efficient)"
    echo "rsync -avz --progress out/python_lib/ /${PI_USER}@192.168.0.106:~/connectedhomeip/out/python_lib/"
    echo ""
    echo "Method 3: Specific tools only"
    echo "scp out/python_lib/chip-repl /${PI_USER}@192.168.0.106:~/connectedhomeip/out/python_lib/"
    echo "scp out/python_lib/chip-tool /${PI_USER}@192.168.0.106:~/connectedhomeip/out/python_lib/"
    echo ""
}

# Main monitoring loop
echo "Starting build monitoring..."
echo "Press Ctrl+C to stop monitoring"
echo ""

while true; do
    check_build
    
    # Check if build is complete
    if [ -f "out/python_lib/chip-repl" ] && [ -f "out/python_lib/chip-tool" ]; then
        echo "🎉 BUILD COMPLETE! 🎉"
        echo "Both chip-repl and chip-tool are available"
        show_transfer_instructions
        break
    fi
    
    # Wait 30 seconds before next check
    echo "Waiting 30 seconds before next check..."
    sleep 30
done 