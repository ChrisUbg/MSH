#!/usr/bin/env python3
"""
Test script for the improved API commissioning process
"""

import asyncio
import aiohttp
import json
import sys
import time

# Test device data
DEVICE_1_DATA = {
    "device_name": "office-socket-1",
    "device_type": "NOUS A8M Socket",
    "qr_code": "0150-175-1910",
    "network_ssid": "08-TvM6xr-FQ",
    "network_password": "Kartoffelernte",
    "pi_ip": "192.168.0.107"
}

DEVICE_2_DATA = {
    "device_name": "office-socket-2", 
    "device_type": "NOUS A8M Socket",
    "qr_code": "3096-783-6060",
    "network_ssid": "08-TvM6xr-FQ",
    "network_password": "Kartoffelernte",
    "pi_ip": "192.168.0.107"
}

async def test_api_commissioning(device_data, device_name):
    """Test the API commissioning process"""
    print(f"\n🔧 Testing API commissioning for {device_name}")
    print(f"Device data: {json.dumps(device_data, indent=2)}")
    
    # First, check if server is running
    try:
        async with aiohttp.ClientSession() as session:
            async with session.get("http://localhost:8888/api/status") as response:
                if response.status != 200:
                    print(f"❌ Server not responding (status: {response.status})")
                    return False
                status_data = await response.json()
                print(f"✅ Server status: {status_data}")
    except Exception as e:
        print(f"❌ Cannot connect to server: {e}")
        return False
    
    # Test commissioning
    try:
        async with aiohttp.ClientSession() as session:
            print(f"\n📡 Sending commissioning request...")
            async with session.post(
                "http://localhost:8888/commission",
                json=device_data,
                timeout=aiohttp.ClientTimeout(total=300)  # 5 minutes timeout
            ) as response:
                
                if response.status == 200:
                    result = await response.json()
                    print(f"✅ Commissioning successful!")
                    print(f"Device ID: {result.get('device_id')}")
                    print(f"Node ID: {result.get('node_id')}")
                    print(f"Method used: {result.get('method_used')}")
                    print(f"Discriminator used: {result.get('discriminator_used')}")
                    
                    # Test device control
                    print(f"\n🎛️  Testing device control...")
                    control_data = {
                        "device_id": result.get('device_id'),
                        "cluster": "onoff",
                        "command": "toggle",
                        "endpoint": "1"
                    }
                    
                    async with session.post(
                        "http://localhost:8888/api/devices/control",
                        json=control_data,
                        timeout=aiohttp.ClientTimeout(total=30)
                    ) as control_response:
                        if control_response.status == 200:
                            control_result = await control_response.json()
                            print(f"✅ Device control successful: {control_result}")
                        else:
                            control_error = await control_response.text()
                            print(f"⚠️  Device control failed: {control_error}")
                    
                    return True
                else:
                    error_text = await response.text()
                    print(f"❌ Commissioning failed (status: {response.status})")
                    print(f"Error: {error_text}")
                    return False
                    
    except asyncio.TimeoutError:
        print(f"❌ Commissioning timed out after 5 minutes")
        return False
    except Exception as e:
        print(f"❌ Commissioning error: {e}")
        return False

async def main():
    """Main test function"""
    print("🧪 Testing Improved API Commissioning Process")
    print("=" * 50)
    
    # Test Device 1
    success1 = await test_api_commissioning(DEVICE_1_DATA, "Office Socket 1")
    
    # Wait a bit between tests
    if success1:
        print(f"\n⏳ Waiting 10 seconds before testing Device 2...")
        await asyncio.sleep(10)
    
    # Test Device 2
    success2 = await test_api_commissioning(DEVICE_2_DATA, "Office Socket 2")
    
    # Summary
    print(f"\n📊 Test Results Summary")
    print("=" * 30)
    print(f"Device 1 (Office Socket 1): {'✅ PASS' if success1 else '❌ FAIL'}")
    print(f"Device 2 (Office Socket 2): {'✅ PASS' if success2 else '❌ FAIL'}")
    
    if success1 and success2:
        print(f"\n🎉 All tests passed! API commissioning is working correctly.")
    else:
        print(f"\n⚠️  Some tests failed. Check the logs above for details.")

if __name__ == "__main__":
    asyncio.run(main()) 