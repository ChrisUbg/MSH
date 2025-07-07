#!/usr/bin/env python3
import asyncio
import aiohttp
import json

async def test_matter_server():
    """Test python-matter-server WebSocket API"""
    try:
        async with aiohttp.ClientSession() as session:
            async with session.ws_connect("ws://localhost:8084/ws") as ws:
                print("Connected to python-matter-server WebSocket")
                
                # Test get_nodes command
                message = {
                    "message_id": "test_001",
                    "command": "get_nodes"
                }
                print(f"Sending: {json.dumps(message)}")
                await ws.send_str(json.dumps(message))
                
                # Wait for response
                async for msg in ws:
                    if msg.type == aiohttp.WSMsgType.TEXT:
                        data = json.loads(msg.data)
                        print(f"Received: {json.dumps(data, indent=2)}")
                        break
                    elif msg.type == aiohttp.WSMsgType.ERROR:
                        print(f"WebSocket error: {ws.exception()}")
                        break
                
                # Try to get available commands
                help_message = {
                    "message_id": "test_002",
                    "command": "help"
                }
                print(f"\nSending help: {json.dumps(help_message)}")
                await ws.send_str(json.dumps(help_message))
                
                # Wait for response
                async for msg in ws:
                    if msg.type == aiohttp.WSMsgType.TEXT:
                        data = json.loads(msg.data)
                        print(f"Help response: {json.dumps(data, indent=2)}")
                        break
                    elif msg.type == aiohttp.WSMsgType.ERROR:
                        print(f"WebSocket error: {ws.exception()}")
                        break
                
                # Try commissioning with different command format
                commission_message = {
                    "message_id": "test_003",
                    "command": "commission",
                    "args": {
                        "code": "MT:IXA27ACN16GUBE2H910"
                    }
                }
                print(f"\nSending commissioning (alternative): {json.dumps(commission_message)}")
                await ws.send_str(json.dumps(commission_message))
                
                # Wait for response with timeout
                try:
                    async with asyncio.timeout(10):
                        async for msg in ws:
                            if msg.type == aiohttp.WSMsgType.TEXT:
                                data = json.loads(msg.data)
                                print(f"Commissioning response: {json.dumps(data, indent=2)}")
                                break
                            elif msg.type == aiohttp.WSMsgType.ERROR:
                                print(f"WebSocket error: {ws.exception()}")
                                break
                except asyncio.TimeoutError:
                    print("Commissioning command timed out after 10 seconds")
                        
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    asyncio.run(test_matter_server()) 