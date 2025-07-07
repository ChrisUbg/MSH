#!/bin/bash

# Quick Build Status Check
echo "=== Matter SDK Build Status ==="
echo "Time: $(date)"
echo ""

# Check build processes
echo "--- Build Processes ---"
BUILD_PROCESSES=$(ps aux | grep -E "(ninja|gcc|g\+\+|build_python)" | grep -v grep)
if [ -n "$BUILD_PROCESSES" ]; then
    echo "✅ Build is running:"
    echo "$BUILD_PROCESSES" | head -3
else
    echo "❌ No build processes found"
fi

# Check build directory
echo ""
echo "--- Build Directory ---"
if [ -d "out/python_lib" ]; then
    echo "✅ Build directory exists"
    echo "Contents:"
    ls -la out/python_lib/ | head -5
else
    echo "❌ Build directory not created yet"
fi

# Check build log
echo ""
echo "--- Build Log ---"
if [ -f "out/python_lib/.ninja_log" ]; then
    echo "✅ Build log exists"
    echo "Size: $(ls -lh out/python_lib/.ninja_log | awk '{print $5}')"
    echo "Last entry:"
    tail -n 1 out/python_lib/.ninja_log 2>/dev/null
else
    echo "❌ Build log not created yet"
fi

# Check key executables
echo ""
echo "--- Key Executables ---"
if [ -f "out/python_lib/chip-repl" ]; then
    echo "✅ chip-repl: EXISTS ($(ls -lh out/python_lib/chip-repl | awk '{print $5}')"
else
    echo "❌ chip-repl: NOT FOUND"
fi

if [ -f "out/python_lib/chip-tool" ]; then
    echo "✅ chip-tool: EXISTS ($(ls -lh out/python_lib/chip-tool | awk '{print $5}')"
else
    echo "❌ chip-tool: NOT FOUND"
fi

# System resources
echo ""
echo "--- System Resources ---"
echo "CPU Load: $(uptime | awk -F'load average:' '{print $2}')"
echo "Memory: $(free -h | grep Mem | awk '{print $3"/"$2}')"
echo "Disk: $(df -h . | tail -1 | awk '{print $4}') available"

echo ""
echo "=== Quick Commands ==="
echo "Monitor build: ./monitor_build.sh"
echo "Check status: ./check_build_status.sh"
echo "Transfer to Pi: ./transfer_to_pi.sh (when complete)" 