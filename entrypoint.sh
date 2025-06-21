#!/bin/bash
set -e

# Start dbus
service dbus start

# Start avahi-daemon
service avahi-daemon start

# Start your app (adjust as needed)
exec uvicorn app.main:app --host 0.0.0.0 --port 8084 --reload