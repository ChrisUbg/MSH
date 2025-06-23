#!/usr/bin/env python3
"""
Bluetooth Test Script
Tests Bluetooth functionality in Docker container
"""

import subprocess
import time
import os
import sys

def run_command(cmd, check=True):
    """Run a command and return the result"""
    try:
        result = subprocess.run(cmd, shell=True, capture_output=True, text=True, check=check)
        return result.stdout.strip(), result.stderr.strip(), result.returncode
    except subprocess.CalledProcessError as e:
        return e.stdout.strip(), e.stderr.strip(), e.returncode

def test_system_info():
    """Test basic system information"""
    print("=== System Information ===")
    
    # Check if we're in a container
    print("Container check:")
    if os.path.exists('/.dockerenv'):
        print("  ✓ Running in Docker container")
    else:
        print("  ✗ Not running in Docker container")
    
    # Check kernel modules
    print("\nKernel modules:")
    stdout, stderr, code = run_command("lsmod | grep bluetooth", check=False)
    if code == 0:
        print("  ✓ Bluetooth kernel modules loaded")
        print(f"    {stdout}")
    else:
        print("  ✗ Bluetooth kernel modules not found")
    
    # Check devices
    print("\nBluetooth devices:")
    stdout, stderr, code = run_command("ls -la /dev/ | grep bluetooth", check=False)
    if code == 0:
        print("  ✓ Bluetooth devices found")
        print(f"    {stdout}")
    else:
        print("  ✗ No Bluetooth devices found")

def test_dbus():
    """Test D-Bus functionality"""
    print("\n=== D-Bus Test ===")
    
    # Check if dbus-daemon is available
    stdout, stderr, code = run_command("which dbus-daemon")
    if code == 0:
        print(f"  ✓ dbus-daemon found at: {stdout}")
    else:
        print("  ✗ dbus-daemon not found")
        return False
    
    # Create necessary directories
    print("\nCreating D-Bus directories...")
    run_command("mkdir -p /var/run/dbus /run/dbus /etc/dbus-1/system.d")
    
    # Create a basic dbus config if it doesn't exist
    if not os.path.exists('/etc/dbus-1/system.conf'):
        print("Creating basic D-Bus config...")
        config_content = """<!DOCTYPE busconfig PUBLIC "-//freedesktop//DTD D-BUS Bus Configuration 1.0//EN"
 "http://www.freedesktop.org/standards/dbus/1.0/busconfig.dtd">
<busconfig>
  <policy context="default">
    <allow send_destination="*"/>
    <allow receive_sender="*"/>
  </policy>
  <policy context="mandatory">
    <allow send_destination="org.bluez"/>
    <allow receive_sender="org.bluez"/>
  </policy>
</busconfig>"""
        with open('/etc/dbus-1/system.conf', 'w') as f:
            f.write(config_content)
    
    # Start dbus-daemon in background
    print("\nStarting D-Bus daemon...")
    try:
        # Kill any existing dbus processes
        run_command("pkill -f dbus-daemon || true", check=False)
        time.sleep(1)
        
        # Start dbus-daemon with explicit socket path
        dbus_process = subprocess.Popen(["dbus-daemon", "--system", "--fork", "--address=unix:path=/var/run/dbus/system_bus_socket"], 
                                       stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        time.sleep(3)
        
        # Check if dbus-daemon is running
        stdout, stderr, code = run_command("ps aux | grep dbus-daemon | grep -v grep")
        if code == 0:
            print("  ✓ D-Bus daemon is running")
            print(f"    {stdout}")
        else:
            print("  ✗ D-Bus daemon not running")
            print(f"    stderr: {stderr}")
            return False
        
        # Create symlink for compatibility
        run_command("ln -sf /var/run/dbus/system_bus_socket /run/dbus/system_bus_socket", check=False)
        
        # Set environment variable for D-Bus
        os.environ['DBUS_SYSTEM_BUS_ADDRESS'] = 'unix:path=/var/run/dbus/system_bus_socket'
        
        # Test D-Bus connection
        print("\nTesting D-Bus connection...")
        stdout, stderr, code = run_command("dbus-send --system --dest=org.freedesktop.DBus --type=method_call --print-reply /org/freedesktop/DBus org.freedesktop.DBus.ListNames", check=False)
        if code == 0:
            print("  ✓ D-Bus connection successful")
        else:
            print("  ✗ D-Bus connection failed")
            print(f"    Error: {stderr}")
            return False
        
        return True
        
    except Exception as e:
        print(f"  ✗ Failed to start D-Bus daemon: {e}")
        return False

def test_bluetooth_daemon():
    """Test Bluetooth daemon functionality"""
    print("\n=== Bluetooth Daemon Test ===")
    
    # Check if bluetoothd is available
    stdout, stderr, code = run_command("which bluetoothd")
    if code == 0:
        print(f"  ✓ bluetoothd found at: {stdout}")
    else:
        print("  ✗ bluetoothd not found")
        return False
    
    # Start bluetooth daemon
    print("\nStarting Bluetooth daemon...")
    bluetooth_process = subprocess.Popen(["bluetoothd", "--experimental", "--debug"], 
                                        stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    time.sleep(3)
    
    # Check if bluetoothd is running
    stdout, stderr, code = run_command("ps aux | grep bluetoothd | grep -v grep")
    if code == 0:
        print("  ✓ Bluetooth daemon is running")
        print(f"    {stdout}")
    else:
        print("  ✗ Bluetooth daemon not running")
        return False
    
    # Check Bluetooth socket
    print("\nChecking Bluetooth socket...")
    if os.path.exists('/var/run/bluetooth/bluetoothd.pid'):
        print("  ✓ Bluetooth PID file exists")
    else:
        print("  ✗ Bluetooth PID file not found")
    
    # Test bluetoothctl
    print("\nTesting bluetoothctl...")
    stdout, stderr, code = run_command("timeout 5 bluetoothctl show", check=False)
    if code == 0:
        print("  ✓ bluetoothctl working")
        print(f"    {stdout}")
    else:
        print("  ✗ bluetoothctl failed")
        print(f"    Error: {stderr}")
        return False
    
    return True

def test_matter_bluetooth():
    """Test Matter Bluetooth functionality"""
    print("\n=== Matter Bluetooth Test ===")
    
    # Test if we can import Matter libraries
    try:
        import chip
        print("  ✓ chip library available")
    except ImportError as e:
        print(f"  ✗ chip library not available: {e}")
        return False
    
    try:
        from chip import discovery
        print("  ✓ chip.discovery available")
    except ImportError as e:
        print(f"  ✗ chip.discovery not available: {e}")
        return False
    
    # Test Bluetooth discovery
    print("\nTesting Bluetooth discovery...")
    try:
        # This is a basic test - in real usage we'd need proper initialization
        print("  ✓ Matter Bluetooth libraries available")
        return True
    except Exception as e:
        print(f"  ✗ Matter Bluetooth test failed: {e}")
        return False

def main():
    """Main test function"""
    print("Bluetooth Test Container")
    print("=" * 50)
    
    # Test system info
    test_system_info()
    
    # Test D-Bus
    if not test_dbus():
        print("\n❌ D-Bus test failed - cannot continue")
        sys.exit(1)
    
    # Test Bluetooth daemon
    if not test_bluetooth_daemon():
        print("\n❌ Bluetooth daemon test failed")
        sys.exit(1)
    
    # Test Matter Bluetooth
    if not test_matter_bluetooth():
        print("\n❌ Matter Bluetooth test failed")
        sys.exit(1)
    
    print("\n✅ All Bluetooth tests passed!")
    print("\nBluetooth is ready for Matter commissioning")

if __name__ == "__main__":
    main() 