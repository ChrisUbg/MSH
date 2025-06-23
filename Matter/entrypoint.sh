#!/bin/bash
set -e

echo "Starting Matter Bridge (Simplified)..."
echo "Using host D-Bus and Bluetooth daemons"

# Start simplified app 
export PYTHONPATH=/app
exec uvicorn app.main_simple:app --host 0.0.0.0 --port 8084 --reload 