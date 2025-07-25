#!/usr/bin/env python3
"""
Test script to verify unique Node ID generation
"""

import sys
import os
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from commissioning_server.core.config import Config
from commissioning_server.core.matter_client import MatterClient

async def test_node_id_generation():
    """Test that different devices get unique Node IDs"""
    
    # Create config and client
    config = Config()
    client = MatterClient(config)
    
    # Test devices
    test_devices = [
        "office-socket-1",
        "office-socket-2", 
        "kitchen-light",
        "bedroom-fan"
    ]
    
    print("Testing unique Node ID generation...")
    print("=" * 50)
    
    node_ids = set()
    
    for device_id in test_devices:
        node_id = client._get_node_id_for_device(device_id)
        node_ids.add(node_id)
        
        print(f"Device: {device_id}")
        print(f"Node ID: {node_id}")
        print(f"Node ID (decimal): {int(node_id, 16)}")
        print("-" * 30)
    
    print(f"\nResults:")
    print(f"Total devices: {len(test_devices)}")
    print(f"Unique Node IDs: {len(node_ids)}")
    print(f"All unique: {'✅ YES' if len(node_ids) == len(test_devices) else '❌ NO'}")
    
    # Test persistence
    print(f"\nTesting persistence...")
    client2 = MatterClient(config)
    
    for device_id in test_devices:
        node_id = client2._get_node_id_for_device(device_id)
        original_node_id = client.device_node_mappings[device_id]
        
        print(f"Device: {device_id}")
        print(f"Original: {original_node_id}")
        print(f"Loaded:   {node_id}")
        print(f"Match: {'✅ YES' if node_id == original_node_id else '❌ NO'}")
        print("-" * 30)

if __name__ == "__main__":
    import asyncio
    asyncio.run(test_node_id_generation()) 