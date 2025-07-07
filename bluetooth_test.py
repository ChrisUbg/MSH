#!/usr/bin/env python3
import asyncio
import sys
from bleak import BleakScanner

async def test_bluetooth():
    print("Testing Bluetooth access in Docker container...")
    try:
        print("Creating scanner...")
        scanner = BleakScanner()
        print("Scanner created successfully")
        
        print("Starting scan (5 seconds)...")
        devices = await asyncio.wait_for(scanner.discover(), timeout=5.0)
        print(f"Scan completed! Found {len(devices)} devices:")
        
        for device in devices:
            print(f"  - {device.name or 'Unknown'} ({device.address})")
            
    except Exception as e:
        print(f"Error: {e}")
        return False
    
    return True

if __name__ == "__main__":
    success = asyncio.run(test_bluetooth())
    sys.exit(0 if success else 1) 