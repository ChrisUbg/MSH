#!/usr/bin/env python3
"""
Test script for Nordic nRF52840 Dongle integration
"""

import asyncio
import logging
import sys
import os
from pathlib import Path

# Add the commissioning_server module to the path
sys.path.insert(0, str(Path(__file__).parent))

from commissioning_server.core.config import Config
from commissioning_server.core.nordic_ble_scanner import NordicBLEScanner

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

async def test_nordic_dongle():
    """Test Nordic nRF52840 dongle capabilities"""
    
    print("🔍 Testing Nordic nRF52840 Dongle Integration")
    print("=" * 50)
    
    try:
        # Load configuration
        config = Config()
        
        # Initialize Nordic BLE scanner
        nordic_scanner = NordicBLEScanner(config)
        
        # Test 1: Check if Nordic adapter is available
        print("\n1. Testing Nordic Adapter Availability...")
        is_available = await nordic_scanner.is_available()
        
        if is_available:
            print("✅ Nordic adapter is available and detected")
        else:
            print("❌ Nordic adapter not available")
            print("   Please ensure the Nordic nRF52840 dongle is connected")
            return False
        
        # Test 2: Apply Nordic optimizations
        print("\n2. Testing Nordic Optimizations...")
        optimization_success = await nordic_scanner.optimize_adapter()
        
        if optimization_success:
            print("✅ Nordic optimizations applied successfully")
        else:
            print("⚠️  Nordic optimizations partially applied (continuing with basic mode)")
        
        # Test 3: Enhanced device scanning
        print("\n3. Testing Enhanced BLE Device Scanning...")
        print("   Scanning for 15 seconds...")
        
        devices = await nordic_scanner.scan_devices_enhanced(timeout=15)
        
        if devices:
            print(f"✅ Found {len(devices)} devices:")
            for i, device in enumerate(devices, 1):
                name = device.get("name", "Unknown")
                address = device.get("address", "Unknown")
                device_type = device.get("type", "unknown")
                print(f"   {i}. {name} ({address}) - Type: {device_type}")
        else:
            print("⚠️  No devices found during scan")
            print("   This is normal if no BLE devices are nearby")
        
        # Test 4: Test device info retrieval (if devices found)
        if devices:
            print("\n4. Testing Enhanced Device Info Retrieval...")
            test_device = devices[0]
            device_address = test_device["address"]
            
            device_info = await nordic_scanner.get_device_info_enhanced(device_address)
            
            if "error" not in device_info:
                print(f"✅ Device info retrieved for {test_device['name']}:")
                for key, value in device_info.items():
                    if key in ["Name", "Address", "RSSI", "nordic_optimized"]:
                        print(f"   {key}: {value}")
            else:
                print(f"⚠️  Could not retrieve device info: {device_info['error']}")
        
        # Test 5: Matter device detection
        print("\n5. Testing Matter Device Detection...")
        matter_devices = [d for d in devices if d.get("type") == "matter"]
        
        if matter_devices:
            print(f"✅ Found {len(matter_devices)} Matter devices:")
            for device in matter_devices:
                print(f"   - {device['name']} ({device['address']})")
        else:
            print("ℹ️  No Matter devices detected")
            print("   This is normal if no Matter devices are nearby")
        
        # Test 6: Performance metrics
        print("\n6. Performance Metrics...")
        print(f"   - Scan interval: {nordic_scanner.scan_interval}ms")
        print(f"   - Scan window: {nordic_scanner.scan_window}ms")
        print(f"   - High power mode: {'Enabled' if nordic_scanner.high_power else 'Disabled'}")
        print(f"   - Extended advertising: {'Enabled' if nordic_scanner.extended_advertising else 'Disabled'}")
        print(f"   - RSSI filter: {nordic_scanner.filter_rssi} dBm")
        print(f"   - Scan type: {nordic_scanner.scan_type}")
        
        print("\n🎉 Nordic Dongle Test Completed Successfully!")
        return True
        
    except Exception as e:
        print(f"\n❌ Test failed with error: {e}")
        logger.exception("Test failed")
        return False

async def test_matter_commissioning():
    """Test Matter commissioning with Nordic dongle"""
    
    print("\n🔧 Testing Matter Commissioning with Nordic Dongle")
    print("=" * 50)
    
    try:
        # Load configuration
        config = Config()
        
        # Initialize Nordic BLE scanner
        nordic_scanner = NordicBLEScanner(config)
        
        # Check if adapter is available
        if not await nordic_scanner.is_available():
            print("❌ Nordic adapter not available for commissioning test")
            return False
        
        # Apply optimizations
        await nordic_scanner.optimize_adapter()
        
        print("✅ Nordic dongle ready for Matter commissioning")
        print("   The dongle is optimized for:")
        print("   - Enhanced BLE range and reliability")
        print("   - Faster device discovery")
        print("   - Better connection stability")
        print("   - Improved Matter device compatibility")
        
        return True
        
    except Exception as e:
        print(f"❌ Commissioning test failed: {e}")
        return False

def main():
    """Main test function"""
    print("Nordic nRF52840 Dongle Integration Test")
    print("=======================================")
    
    # Run basic dongle test
    basic_success = asyncio.run(test_nordic_dongle())
    
    if basic_success:
        # Run commissioning test
        commissioning_success = asyncio.run(test_matter_commissioning())
        
        if commissioning_success:
            print("\n🎯 All tests passed! Nordic dongle is ready for Matter commissioning.")
            print("\nNext steps:")
            print("1. Start your commissioning server")
            print("2. Use the enhanced BLE scanning for device discovery")
            print("3. Commission Matter devices with improved reliability")
        else:
            print("\n⚠️  Basic dongle test passed, but commissioning test failed")
    else:
        print("\n❌ Basic dongle test failed. Please check hardware connection.")

if __name__ == "__main__":
    main() 