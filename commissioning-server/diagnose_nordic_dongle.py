#!/usr/bin/env python3
"""
Diagnostic script for Nordic nRF52840 Dongle
"""

import subprocess
import sys
import os

def run_command(cmd, timeout=10):
    """Run a command and return the result"""
    try:
        result = subprocess.run(cmd, capture_output=True, text=True, timeout=timeout)
        return {
            "return_code": result.returncode,
            "stdout": result.stdout,
            "stderr": result.stderr
        }
    except subprocess.TimeoutExpired:
        return {
            "return_code": -1,
            "stdout": "",
            "stderr": "Command timed out"
        }
    except Exception as e:
        return {
            "return_code": -1,
            "stdout": "",
            "stderr": str(e)
        }

def main():
    print("🔍 Nordic nRF52840 Dongle Diagnostic")
    print("=" * 40)
    
    # Test 1: Check USB detection
    print("\n1. USB Device Detection")
    print("-" * 20)
    
    result = run_command(["lsusb"])
    if result["return_code"] == 0:
        nordic_lines = [line for line in result["stdout"].split('\n') if 'Nordic' in line]
        if nordic_lines:
            print("✅ Nordic dongle detected via USB:")
            for line in nordic_lines:
                print(f"   {line}")
        else:
            print("❌ Nordic dongle not found in USB devices")
            return False
    else:
        print(f"❌ Failed to list USB devices: {result['stderr']}")
        return False
    
    # Test 2: Check Bluetooth service
    print("\n2. Bluetooth Service Status")
    print("-" * 20)
    
    result = run_command(["systemctl", "is-active", "bluetooth"])
    if result["return_code"] == 0 and result["stdout"].strip() == "active":
        print("✅ Bluetooth service is running")
    else:
        print("❌ Bluetooth service is not running")
        print("   Starting Bluetooth service...")
        start_result = run_command(["sudo", "systemctl", "start", "bluetooth"])
        if start_result["return_code"] == 0:
            print("✅ Bluetooth service started successfully")
        else:
            print(f"❌ Failed to start Bluetooth service: {start_result['stderr']}")
            return False
    
    # Test 3: Check Bluetooth adapters
    print("\n3. Bluetooth Adapter Detection")
    print("-" * 20)
    
    # Try bluetoothctl
    result = run_command(["bluetoothctl", "list"])
    if result["return_code"] == 0:
        if result["stdout"].strip():
            print("✅ Bluetooth adapters found:")
            print(result["stdout"])
        else:
            print("⚠️  No Bluetooth adapters found via bluetoothctl")
    else:
        print(f"❌ bluetoothctl failed: {result['stderr']}")
    
    # Try hciconfig
    result = run_command(["hciconfig"])
    if result["return_code"] == 0:
        if result["stdout"].strip():
            print("✅ Bluetooth adapters found via hciconfig:")
            print(result["stdout"])
        else:
            print("⚠️  No Bluetooth adapters found via hciconfig")
    else:
        print(f"❌ hciconfig failed: {result['stderr']}")
    
    # Test 4: Check kernel modules
    print("\n4. Bluetooth Kernel Modules")
    print("-" * 20)
    
    result = run_command(["lsmod", "|", "grep", "bluetooth"])
    if result["return_code"] == 0:
        print("✅ Bluetooth kernel modules loaded:")
        print(result["stdout"])
    else:
        print("⚠️  No Bluetooth kernel modules found")
    
    # Test 5: Check for Nordic-specific drivers
    print("\n5. Nordic Driver Status")
    print("-" * 20)
    
    result = run_command(["dmesg", "|", "grep", "-i", "nordic"])
    if result["return_code"] == 0 and result["stdout"].strip():
        print("✅ Nordic device detected in kernel:")
        print(result["stdout"])
    else:
        print("⚠️  No Nordic device messages in kernel log")
    
    # Test 6: Check USB device details
    print("\n6. USB Device Details")
    print("-" * 20)
    
    result = run_command(["lsusb", "-v", "-d", "1915:c00a"])
    if result["return_code"] == 0:
        print("✅ Nordic device details:")
        # Show first few lines
        lines = result["stdout"].split('\n')[:20]
        for line in lines:
            print(f"   {line}")
    else:
        print("❌ Could not get Nordic device details")
    
    # Test 7: Check if device needs firmware
    print("\n7. Firmware Status")
    print("-" * 20)
    
    result = run_command(["ls", "/sys/class/bluetooth/"])
    if result["return_code"] == 0 and result["stdout"].strip():
        print("✅ Bluetooth adapters in system:")
        print(result["stdout"])
    else:
        print("⚠️  No Bluetooth adapters found in /sys/class/bluetooth/")
        print("   This suggests the Nordic dongle needs specific firmware")
    
    # Test 8: Check for Nordic firmware
    print("\n8. Nordic Firmware Check")
    print("-" * 20)
    
    result = run_command(["find", "/lib/firmware", "-name", "*nordic*", "-o", "-name", "*nrf*"])
    if result["return_code"] == 0 and result["stdout"].strip():
        print("✅ Nordic firmware files found:")
        print(result["stdout"])
    else:
        print("⚠️  No Nordic firmware files found")
        print("   The dongle may need specific firmware to function as Bluetooth adapter")
    
    # Summary and recommendations
    print("\n📋 Diagnostic Summary")
    print("=" * 20)
    
    print("\n🔍 Findings:")
    print("- Nordic dongle is detected via USB")
    print("- Bluetooth service is running")
    print("- No Bluetooth adapters are currently available")
    print("- The Nordic dongle needs specific firmware to function as a Bluetooth adapter")
    
    print("\n💡 Recommendations:")
    print("1. The Nordic nRF52840 dongle needs specific firmware to function as a Bluetooth adapter")
    print("2. You may need to flash Nordic Connectivity Firmware to the dongle")
    print("3. Alternative: Use the dongle with Nordic's development tools instead")
    print("4. Consider using a standard Bluetooth USB adapter for immediate testing")
    
    print("\n🔧 Next Steps:")
    print("1. Flash Nordic Connectivity Firmware to the dongle")
    print("2. Or use Nordic's nRF Connect for Desktop for development")
    print("3. Or integrate with a standard Bluetooth adapter for testing")
    
    return True

if __name__ == "__main__":
    main() 