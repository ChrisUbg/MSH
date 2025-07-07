#!/usr/bin/env python3
"""
MSH Matter Bridge - FastAPI HTTP Bridge for python-matter-server
Updated for Google Home Commissioning Approach

Port Configuration:
- FastAPI Bridge: 8085 (external), 8084 (internal)
- python-matter-server: 8084 (WebSocket)
- C# Web App: 8083 (external), 8082 (internal)
- Database: 5435 (external), 5432 (internal)

New Workflow:
1. User commissions device with Google Home app
2. Device joins WiFi network automatically
3. Our system discovers device via mDNS
4. User adds device to our system via web UI
"""

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import json
import os
from typing import Optional, Dict, Any, List
import logging
import asyncio
import aiohttp
from pathlib import Path
import websockets
from datetime import datetime

app = FastAPI(title="MSH Matter Bridge - Google Home Commissioning Approach")

# Configure CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Matter server configuration
MATTER_SERVER_URL = "ws://localhost:8084/ws"
MATTER_DATA_PATH = "/home/chregg/MSH/matter_data"

# Web app configuration
WEB_APP_URL = "http://172.17.0.1:8083"

# Ensure matter data directory exists
Path(MATTER_DATA_PATH).mkdir(exist_ok=True)

# Device storage
matter_devices = {}
discovered_devices = {}

class DeviceDiscoveryRequest(BaseModel):
    """Request to discover devices on the network"""
    scan_timeout: Optional[int] = 30

class DeviceAddRequest(BaseModel):
    """Request to add a discovered device to our system"""
    device_name: str
    device_type: str
    node_id: int
    room_id: Optional[int] = None

class DeviceState(BaseModel):
    device_id: str
    state: Dict[str, Any]

async def matter_server_request(command: str, **kwargs) -> dict:
    """Send a command to the python-matter-server via WebSocket"""
    try:
        async with aiohttp.ClientSession() as session:
            async with session.ws_connect(MATTER_SERVER_URL) as ws:
                # Send command
                message = {
                    "command": command,
                    **kwargs
                }
                await ws.send_str(json.dumps(message))
                
                # Wait for response
                response = await ws.receive_str()
                return json.loads(response)
                
    except Exception as e:
        logger.error(f"Matter server request failed: {e}")
        return {"success": False, "error": str(e)}

async def notify_webapp_of_device_update(device_id: str, device_data: dict):
    """Notify the C# web app of device updates"""
    try:
        async with aiohttp.ClientSession() as session:
            async with session.post(
                f"{WEB_APP_URL}/api/devices/update",
                json={"device_id": device_id, "device_data": device_data}
            ) as response:
                if response.status == 200:
                    logger.info(f"Web app notified of device update: {device_id}")
                else:
                    logger.warning(f"Web app notification failed: {response.status}")
    except Exception as e:
        logger.error(f"Failed to notify web app: {e}")

@app.on_event("startup")
async def startup_event():
    """Startup event - verify matter-server is running"""
    logger.info("MSH Matter Bridge starting up...")
    
    # Test connection to matter-server
    try:
        result = await matter_server_request("get_nodes")
        if result.get("success"):
            logger.info("✅ Connected to matter-server successfully")
        else:
            logger.warning("⚠️ Matter server connection test failed")
    except Exception as e:
        logger.error(f"❌ Failed to connect to matter-server: {e}")

@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {"status": "healthy", "timestamp": datetime.now().isoformat()}

@app.get("/")
async def root():
    return {"message": "MSH Matter Bridge API - Google Home Commissioning Approach", "mode": "production"}

@app.post("/devices/discover")
async def discover_devices(request: DeviceDiscoveryRequest):
    """Discover Matter devices on the network that were commissioned via Google Home"""
    try:
        logger.info(f"Starting device discovery with timeout: {request.scan_timeout}s")
        
        # Use matter-server to discover devices on the network
        result = await matter_server_request("get_nodes")
        
        if result.get("success"):
            nodes = result.get("response", [])
            discovered_devices.clear()
            
            for node in nodes:
                node_id = node.get("node_id")
                if node_id:
                    device_id = f"discovered_{node_id}"
                    discovered_devices[device_id] = {
                        "node_id": node_id,
                        "device_id": device_id,
                        "name": f"Device {node_id}",
                        "type": "unknown",
                        "discovered_at": datetime.now().isoformat(),
                        "status": "available"
                    }
            
            logger.info(f"Discovered {len(discovered_devices)} devices on network")
            return {
                "status": "success",
                "message": f"Discovered {len(discovered_devices)} devices",
                "devices": list(discovered_devices.values()),
                "count": len(discovered_devices)
            }
        else:
            logger.warning("Device discovery failed, returning empty list")
            return {
                "status": "success",
                "message": "No devices discovered (discovery may have failed)",
                "devices": [],
                "count": 0
            }
            
    except Exception as e:
        logger.error(f"Device discovery error: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Device discovery failed: {str(e)}")

@app.get("/devices/available")
async def get_available_devices():
    """Get list of discovered devices available for addition"""
    try:
        return {
            "status": "success",
            "devices": list(discovered_devices.values()),
            "count": len(discovered_devices)
        }
    except Exception as e:
        logger.error(f"Get available devices error: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Failed to get available devices: {str(e)}")

@app.post("/devices/add")
async def add_discovered_device(request: DeviceAddRequest):
    """Add a discovered device to our system"""
    try:
        logger.info(f"Adding discovered device: {request.device_name} (node {request.node_id})")
        
        # Check if device is in discovered devices
        device_id = f"discovered_{request.node_id}"
        if device_id not in discovered_devices:
            raise HTTPException(status_code=404, detail="Device not found in discovered devices")
        
        # Create device entry in our system
        device_data = {
            "name": request.device_name,
            "type": request.device_type,
            "node_id": request.node_id,
            "state": {"power": False, "power_consumption": 0.0, "energy": 0.0},
            "commissioned": True,
            "mock": False,
            "method": "google-home-commissioning",
            "room_id": request.room_id,
            "discovered_at": discovered_devices[device_id]["discovered_at"],
            "added_at": datetime.now().isoformat(),
            "last_seen": asyncio.get_event_loop().time()
        }
        
        # Add to our device storage
        matter_devices[device_id] = device_data
        
        # Remove from discovered devices
        del discovered_devices[device_id]
        
        # Notify web app of new device
        try:
            await notify_webapp_of_device_update(device_id, device_data)
        except Exception as db_error:
            logger.error(f"Database integration error: {db_error}")
        
        logger.info(f"Device added successfully: {device_id}")
        return {
            "status": "success",
            "message": f"Device {request.device_name} added successfully",
            "device_id": device_id,
            "node_id": request.node_id
        }
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Add device error: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Failed to add device: {str(e)}")

@app.post("/devices/add-manual")
async def add_device_manually(request: DeviceAddRequest):
    """Add a device manually with QR code data (for devices already commissioned)"""
    try:
        logger.info(f"Adding device manually: {request.device_name} (node {request.node_id})")
        
        device_id = f"manual_{request.node_id}"
        
        # Create device entry
        device_data = {
            "name": request.device_name,
            "type": request.device_type,
            "node_id": request.node_id,
            "state": {"power": False, "power_consumption": 0.0, "energy": 0.0},
            "commissioned": True,
            "mock": False,
            "method": "manual-addition",
            "room_id": request.room_id,
            "added_at": datetime.now().isoformat(),
            "last_seen": asyncio.get_event_loop().time()
        }
        
        # Add to our device storage
        matter_devices[device_id] = device_data
        
        # Notify web app
        try:
            await notify_webapp_of_device_update(device_id, device_data)
        except Exception as db_error:
            logger.error(f"Database integration error: {db_error}")
        
        logger.info(f"Device added manually: {device_id}")
        return {
            "status": "success",
            "message": f"Device {request.device_name} added manually",
            "device_id": device_id,
            "node_id": request.node_id
        }
        
    except Exception as e:
        logger.error(f"Manual add device error: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Failed to add device manually: {str(e)}")

@app.post("/device/{device_id}/power")
async def toggle_device_power(device_id: str):
    """Toggle power state of a device using Matter control"""
    try:
        if device_id not in matter_devices:
            raise HTTPException(status_code=404, detail="Device not found")
        
        device = matter_devices[device_id]
        current_state = device["state"].get("power", False)
        new_state = not current_state
        node_id = device["node_id"]
        
        logger.info(f"Toggling power for device {device_id} (node {node_id}) from {current_state} to {new_state}")
        
        # Use python-matter-server for device control
        command_name = "On" if new_state else "Off"
        result = await matter_server_request(
            "device_command",
            node_id=node_id,
            endpoint_id=1,
            cluster_id=6,  # OnOff cluster
            command_name=command_name,
            device_id=device_id
        )
        
        if result.get("success"):
            # Update local state
            device["state"]["power"] = new_state
            device["state"]["power_consumption"] = 15.5 if new_state else 0.0
            device["last_seen"] = asyncio.get_event_loop().time()
            
            logger.info(f"Device {device_id} power {'ON' if new_state else 'OFF'}")
            return {
                "success": True,
                "device_id": device_id,
                "power_state": "on" if new_state else "off",
                "power_consumption": device["state"]["power_consumption"]
            }
        else:
            logger.error(f"Device control failed: {result.get('error')}")
            raise HTTPException(status_code=500, detail="Failed to toggle device power")
            
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Toggle device power error: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Device control error: {str(e)}")

@app.get("/device/{device_id}/state")
async def get_device_state(device_id: str):
    """Get current state of a device"""
    try:
        if device_id not in matter_devices:
            raise HTTPException(status_code=404, detail="Device not found")
        
        device = matter_devices[device_id]
        return {
            "device_id": device_id,
            "name": device["name"],
            "type": device["type"],
            "state": device["state"],
            "commissioned": device["commissioned"],
            "method": device["method"],
            "last_seen": device["last_seen"]
        }
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Get device state error: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Failed to get device state: {str(e)}")

@app.get("/devices")
async def list_devices():
    """List all devices in our system"""
    try:
        devices = []
        for device_id, device in matter_devices.items():
            devices.append({
                "device_id": device_id,
                "name": device["name"],
                "type": device["type"],
                "state": device["state"],
                "commissioned": device["commissioned"],
                "method": device["method"]
            })
        
        return {
            "status": "success",
            "devices": devices,
            "count": len(devices)
        }
    except Exception as e:
        logger.error(f"List devices error: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Failed to list devices: {str(e)}")

@app.get("/dev/matter-server-test")
async def test_matter_server():
    """Test endpoint to verify matter-server connectivity"""
    try:
        # Test basic connectivity
        result = await matter_server_request("get_nodes")
        
        return {
            "status": "success",
            "message": "python-matter-server test completed",
            "websocket_connection": "✅" if result.get("success") else "❌",
            "command_results": {
                "get_nodes": result
            },
            "matter_server_process": "running"
        }
    except Exception as e:
        logger.error(f"Matter server test error: {str(e)}")
        return {
            "status": "error",
            "message": f"Matter server test failed: {str(e)}",
            "websocket_connection": "❌",
            "command_results": {},
            "matter_server_process": "unknown"
        }

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8085) 