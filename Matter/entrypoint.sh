#!/bin/bash
set -e

echo "Starting D-Bus daemon..."
# Kill any existing dbus processes
pkill -f dbus-daemon || true
# Clean up any existing PID files
rm -f /run/dbus/pid /var/run/dbus/pid /var/run/dbus.pid /run/dbus.pid
# Create the dbus directory
mkdir -p /var/run/dbus /run/dbus
# Start dbus-daemon
dbus-daemon --system --fork

echo "Starting Avahi daemon..."
# Start avahi-daemon
avahi-daemon --daemonize

echo "Starting Matter Bridge (Simplified)..."
# Start simplified app 
export PYTHONPATH=/app
exec uvicorn app.main_simple:app --host 0.0.0.0 --port 8084 --reload 