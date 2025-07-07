#!/bin/bash

# Matter Server Startup Script with Single Instance Protection
# This script ensures only one instance of matter-server runs at a time

# Configuration
PID_FILE="/home/chregg/MSH/matter_server.pid"
LOG_FILE="/home/chregg/MSH/matter_server.log"
STORAGE_PATH="/home/chregg/MSH/matter_data"
PORT="8084"
PYTHON_ENV="/home/chregg/MSH/matter_host_env"

# Function to check if matter-server is already running
check_running() {
    if [ -f "$PID_FILE" ]; then
        PID=$(cat "$PID_FILE")
        if ps -p "$PID" > /dev/null 2>&1; then
            echo "Matter server is already running with PID $PID"
            return 0
        else
            echo "Stale PID file found, removing..."
            rm -f "$PID_FILE"
        fi
    fi
    return 1
}

# Function to start matter-server
start_server() {
    echo "Starting matter-server..."
    
    # Activate Python environment
    source "$PYTHON_ENV/bin/activate"
    
    # Start matter-server in background and capture PID
    matter-server --storage-path "$STORAGE_PATH" --port "$PORT" > "$LOG_FILE" 2>&1 &
    PID=$!
    
    # Save PID to file
    echo "$PID" > "$PID_FILE"
    
    echo "Matter server started with PID $PID"
    echo "Log file: $LOG_FILE"
    echo "PID file: $PID_FILE"
}

# Function to stop matter-server
stop_server() {
    if [ -f "$PID_FILE" ]; then
        PID=$(cat "$PID_FILE")
        if ps -p "$PID" > /dev/null 2>&1; then
            echo "Stopping matter-server (PID $PID)..."
            kill "$PID"
            rm -f "$PID_FILE"
            echo "Matter server stopped"
        else
            echo "Matter server not running"
            rm -f "$PID_FILE"
        fi
    else
        echo "No PID file found, trying to kill any matter-server processes..."
        pkill -f "matter-server.*--port $PORT" 2>/dev/null
        echo "Killed any running matter-server processes"
    fi
}

# Function to show status
show_status() {
    if check_running; then
        PID=$(cat "$PID_FILE")
        echo "Matter server is running (PID $PID)"
        echo "Port: $PORT"
        echo "Storage: $STORAGE_PATH"
        echo "Log: $LOG_FILE"
    else
        echo "Matter server is not running"
    fi
}

# Function to show logs
show_logs() {
    if [ -f "$LOG_FILE" ]; then
        tail -f "$LOG_FILE"
    else
        echo "Log file not found: $LOG_FILE"
    fi
}

# Main script logic
case "$1" in
    start)
        if check_running; then
            exit 0
        fi
        start_server
        ;;
    stop)
        stop_server
        ;;
    restart)
        stop_server
        sleep 2
        start_server
        ;;
    status)
        show_status
        ;;
    logs)
        show_logs
        ;;
    *)
        echo "Usage: $0 {start|stop|restart|status|logs}"
        echo ""
        echo "Commands:"
        echo "  start   - Start matter-server (single instance)"
        echo "  stop    - Stop matter-server"
        echo "  restart - Restart matter-server"
        echo "  status  - Show server status"
        echo "  logs    - Show live logs"
        exit 1
        ;;
esac 