#!/bin/bash

# Transfer Matter SDK tools to Raspberry Pi
# This script packages and transfers the built Matter tools

# Source configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [[ -f "$SCRIPT_DIR/pi-config.sh" ]]; then
    source "$SCRIPT_DIR/pi-config.sh"
else
    echo "Error: pi-config.sh not found in $SCRIPT_DIR"
    exit 1
fi

# Use configuration variables
PI_HOST="$PI_IP"
PI_USER="$PI_USER"
PI_PATH="$PI_MATTER_DIR"

echo "=== Packaging Matter SDK Tools ==="

# Create a temporary directory for packaging
TEMP_DIR=$(mktemp -d)
PACKAGE_DIR="$TEMP_DIR/matter-sdk-tools"
mkdir -p "$PACKAGE_DIR"

# Copy the built Python packages
echo "Copying Python packages..."
cp dev_connectedhomeip/out/python_lib/controller/python/*.whl "$PACKAGE_DIR/"

# Copy the Python virtual environment
echo "Copying Python virtual environment..."
cp -r dev_connectedhomeip/out/python_lib/python-venv "$PACKAGE_DIR/"

# Create installation script
cat > "$PACKAGE_DIR/install.sh" << 'EOF'
#!/bin/bash

echo "=== Installing Matter SDK Tools on Pi ==="

# Install the wheel packages
cd "$(dirname "$0")"
source python-venv/bin/activate
pip install *.whl

echo "✅ Matter SDK tools installed successfully!"
echo "To use chip-repl:"
echo "  source python-venv/bin/activate"
echo "  chip-repl"
EOF

chmod +x "$PACKAGE_DIR/install.sh"

# Create a test script
cat > "$PACKAGE_DIR/test-chip-repl.sh" << 'EOF'
#!/bin/bash

echo "=== Testing chip-repl ==="
source python-venv/bin/activate
echo "import chip" | chip-repl
echo "✅ chip-repl test completed!"
EOF

chmod +x "$PACKAGE_DIR/test-chip-repl.sh"

# Create package
PACKAGE_FILE="matter-sdk-tools-$(date +%Y%m%d-%H%M%S).tar.gz"
cd "$TEMP_DIR"
tar -czf "$PACKAGE_FILE" matter-sdk-tools/

echo "=== Transferring to Raspberry Pi ==="
echo "Package: $TEMP_DIR/$PACKAGE_FILE"
echo "Target: $PI_USER@$PI_HOST:$PI_PATH"

# Transfer to Pi
ssh -i "$PI_SSH_KEY" "$PI_USER@$PI_HOST" "mkdir -p $PI_PATH"
scp -i "$PI_SSH_KEY" "$TEMP_DIR/$PACKAGE_FILE" "$PI_USER@$PI_HOST:$PI_PATH/"

echo "=== Installing on Pi ==="
ssh -i "$PI_SSH_KEY" "$PI_USER@$PI_HOST" "cd $PI_PATH && tar -xzf $PACKAGE_FILE && cd matter-sdk-tools && ./install.sh"

echo "✅ Transfer and installation completed!"
echo "To test on Pi:"
echo "  ssh -i $PI_SSH_KEY $PI_USER@$PI_HOST"
echo "  cd $PI_PATH/matter-sdk-tools"
echo "  ./test-chip-repl.sh"

# Cleanup
rm -rf "$TEMP_DIR" 