FROM python:3.11-slim

# Install Bluetooth tools and dependencies
RUN apt-get update && apt-get install -y \
    bluez \
    bluez-tools \
    bluetooth \
    dbus \
    dbus-x11 \
    curl \
    procps \
    && rm -rf /var/lib/apt/lists/*

# Create test script
COPY test-bluetooth.py /app/test-bluetooth.py

WORKDIR /app

CMD ["python", "test-bluetooth.py"] 