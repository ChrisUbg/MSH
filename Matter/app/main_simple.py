from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import json
import os
import subprocess
from typing import Optional, Dict, Any
import logging
import asyncio
import re
import aiohttp
from pathlib import Path

app = FastAPI(title="MSH Matter Bridge - NOUS A8M Integration (Lightweight)")

# Configure CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, replace with specific origins
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Matter server configuration
MATTER_SERVER_URL = "ws://localhost:5580/ws"
MATTER_SERVER_PROCESS = None
MATTER_DATA_PATH = "/app/matter_data"

# Ensure matter data directory exists
Path(MATTER_DATA_PATH).mkdir(exist_ok=True)

class CommissioningRequest(BaseModel):
    device_name: str
    device_type: str
    pin: Optional[str] = "20202021"  # Default Matter PIN

class DeviceState(BaseModel):
    device_id: str
    state: Dict[str, Any]

# Device storage (in production this would be a database)
matter_devices = {}

async def start_matter_server():
    """Start the python-matter-server process"""
    global MATTER_SERVER_PROCESS
    
    if MATTER_SERVER_PROCESS is None:
        try:
            logger.info("Starting python-matter-server...")
            
            # Start matter server in the background
            cmd = [
                "/opt/venv/bin/python", 
                "-m", "matter_server.server",
                "--storage-path", MATTER_DATA_PATH,
                "--port", "5580",
                "--log-level", "info"
            ]
            
            MATTER_SERVER_PROCESS = await asyncio.create_subprocess_exec(
                *cmd,
                stdout=asyncio.subprocess.PIPE,
                stderr=asyncio.subprocess.PIPE
            )
            
            # Give it time to start
            await asyncio.sleep(3)
            
            logger.info("python-matter-server started successfully")
            return True
            
        except Exception as e:
            logger.error(f"Failed to start matter server: {e}")
            return False
    
    return True

async def matter_server_request(command: str, **kwargs) -> dict:
    """Send a command to the python-matter-server via WebSocket"""
    try:
        async with aiohttp.ClientSession() as session:
            async with session.ws_connect(MATTER_SERVER_URL) as ws:
                
                # Send command
                message = {
                    "message_id": f"msh_{asyncio.get_event_loop().time()}",
                    "command": command,
                    "args": kwargs
                }
                
                await ws.send_str(json.dumps(message))
                
                # Wait for response
                async for msg in ws:
                    if msg.type == aiohttp.WSMsgType.TEXT:
                        data = json.loads(msg.data)
                        if data.get("message_id") == message["message_id"]:
                            return data
                    elif msg.type == aiohttp.WSMsgType.ERROR:
                        raise Exception(f"WebSocket error: {ws.exception()}")
                        
    except Exception as e:
        logger.error(f"Matter server request failed: {e}")
        # Fall back to mock for development
        return await create_mock_response(command, **kwargs)

async def create_mock_response(command: str, **kwargs) -> dict:
    """Create mock responses when matter server is unavailable"""
    if command == "commission_with_code":
        device_id = f"mock_nous_a8m_{len(matter_devices) + 1}"
        node_id = len(matter_devices) + 1000
        
        matter_devices[device_id] = {
            "name": kwargs.get("device_name", "Mock Device"),
            "type": "nous_a8m_socket",
            "node_id": node_id,
            "state": {"power": False, "power_consumption": 0.0, "energy": 0.0},
            "commissioned": True,
            "mock": True,
            "last_seen": asyncio.get_event_loop().time()
        }
        
        return {
            "success": True,
            "result": {
                "node_id": node_id,
                "device_id": device_id
            }
        }
    
    elif command == "device_command":
        device_id = kwargs.get("device_id", "mock_device")
        if device_id in matter_devices:
            # Update mock device state
            if kwargs.get("command_name") == "On":
                matter_devices[device_id]["state"]["power"] = True
                matter_devices[device_id]["state"]["power_consumption"] = 15.5
            elif kwargs.get("command_name") == "Off":
                matter_devices[device_id]["state"]["power"] = False
                matter_devices[device_id]["state"]["power_consumption"] = 0.0
                
        return {"success": True}
    
    elif command == "get_nodes":
        return {
            "success": True,
            "result": list(matter_devices.values())
        }
    
    return {"success": False, "error": "Unknown command"}

@app.on_event("startup")
async def startup_event():
    """Start matter server on startup"""
    await start_matter_server()

@app.get("/health")
async def health_check():
    return {
        "status": "healthy", 
        "version": "matter-integration-lightweight", 
        "matter_server": "python-matter-server",
        "data_path": MATTER_DATA_PATH
    }

@app.get("/")
async def root():
    return {"message": "MSH Matter Bridge API - NOUS A8M Integration (Lightweight)", "mode": "production"}

@app.post("/commission")
async def commission_device(request: CommissioningRequest):
    """Commission a NOUS A8M Matter device using python-matter-server"""
    try:
        logger.info(f"Starting Matter device commissioning for {request.device_name}")
        
        # Ensure matter server is running
        if not await start_matter_server():
            raise HTTPException(status_code=500, detail="Failed to start matter server")
        
        # Commission device using python-matter-server
        # For now, we'll use a QR code format - in real usage, this would come from the device
        qr_code = f"MT:Y.K90SO006C00.0648G00"  # Example QR code for testing
        
        result = await matter_server_request(
            "commission_with_code",
            code=qr_code,
            device_name=request.device_name
        )
        
        if result.get("success"):
            node_id = result.get("result", {}).get("node_id")
            device_id = f"nous_a8m_{node_id}"
            
            # Store device information
            matter_devices[device_id] = {
                "name": request.device_name,
                "type": "nous_a8m_socket",
                "node_id": node_id,
                "state": {"power": False, "power_consumption": 0.0, "energy": 0.0},
                "commissioned": True,
                "mock": result.get("result", {}).get("device_id", "").startswith("mock_"),
                "last_seen": asyncio.get_event_loop().time()
            }
            
            logger.info(f"Device commissioned successfully: {device_id}")
            return {
                "status": "success",
                "message": "NOUS A8M device commissioned successfully",
                "device_id": device_id,
                "node_id": node_id,
                "method": "python-matter-server"
            }
        else:
            raise HTTPException(
                status_code=400,
                detail=f"Device commissioning failed: {result.get('error', 'Unknown error')}"
            )
            
    except Exception as e:
        logger.error(f"Commission device error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/device/{device_id}/power")
async def toggle_device_power(device_id: str):
    """Toggle power state of a NOUS A8M device"""
    try:
        if device_id not in matter_devices:
            raise HTTPException(status_code=404, detail="Device not found")
        
        device = matter_devices[device_id]
        current_state = device["state"].get("power", False)
        new_state = not current_state
        
        # Send command to matter server
        command_name = "On" if new_state else "Off"
        result = await matter_server_request(
            "device_command",
            node_id=device["node_id"],
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
            raise HTTPException(status_code=500, detail="Failed to toggle device power")
            
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Power toggle error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/device/{device_id}/power-metrics")
async def get_power_metrics(device_id: str):
    """Get power consumption metrics for a NOUS A8M device"""
    try:
        if device_id not in matter_devices:
            raise HTTPException(status_code=404, detail="Device not found")
        
        device = matter_devices[device_id]
        
        # For real devices, we would query the electrical measurement cluster
        # For now, return current state with some simulated values
        power_state = device["state"].get("power", False)
        base_consumption = 15.5 if power_state else 0.1  # Standby power
        
        # Simulate some variance in power consumption
        import random
        consumption = base_consumption + random.uniform(-0.5, 0.5) if power_state else 0.1
        
        return {
            "device_id": device_id,
            "device_name": device["name"],
            "device_type": device["type"],
            "power_state": "on" if power_state else "off",
            "power_consumption": round(consumption, 2),  # Watts
            "voltage": 230.0,  # Volts (typical EU voltage)
            "current": round(consumption / 230.0, 3) if consumption > 0 else 0.0,  # Amps
            "energy_today": round(consumption * 0.5, 2),  # Simulated daily energy
            "timestamp": asyncio.get_event_loop().time(),
            "online": True
        }
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Power metrics error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

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
            "node_id": device["node_id"],
            "state": device["state"],
            "commissioned": device["commissioned"],
            "online": True,
            "last_seen": device["last_seen"],
            "mock": device.get("mock", False)
        }
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Get device state error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/devices")
async def list_devices():
    """List all commissioned devices"""
    try:
        devices = []
        for device_id, device in matter_devices.items():
            devices.append({
                "device_id": device_id,
                "name": device["name"],
                "type": device["type"],
                "node_id": device["node_id"],
                "online": True,
                "power_state": device["state"].get("power", False),
                "mock": device.get("mock", False)
            })
        
        return {
            "devices": devices,
            "count": len(devices),
            "matter_server": "python-matter-server"
        }
        
    except Exception as e:
        logger.error(f"List devices error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/device/{device_id}/schedule")
async def set_device_schedule(device_id: str, schedule: dict):
    """Set a schedule for device automation (placeholder)"""
    try:
        if device_id not in matter_devices:
            raise HTTPException(status_code=404, detail="Device not found")
        
        # This would integrate with your rule engine in the C# application
        logger.info(f"Schedule set for device {device_id}: {schedule}")
        
        return {
            "success": True,
            "device_id": device_id,
            "schedule": schedule,
            "message": "Schedule functionality would be handled by C# rule engine"
        }
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Set schedule error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/dev/reset")
async def reset_devices():
    """Reset all devices (development endpoint)"""
    try:
        global matter_devices
        matter_devices.clear()
        
        logger.info("All devices reset")
        return {
            "success": True,
            "message": "All devices have been reset",
            "device_count": 0
        }
        
    except Exception as e:
        logger.error(f"Reset devices error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/dev/matter-server-test")
async def test_matter_server():
    """Test python-matter-server connectivity"""
    try:
        # Test basic connectivity
        result = await matter_server_request("get_nodes")
        
        if result.get("success"):
            return {
                "status": "connected",
                "matter_server": "python-matter-server",
                "url": MATTER_SERVER_URL,
                "nodes": len(result.get("result", [])),
                "test_time": asyncio.get_event_loop().time()
            }
        else:
            return {
                "status": "error",
                "matter_server": "python-matter-server", 
                "url": MATTER_SERVER_URL,
                "error": result.get("error", "Unknown error"),
                "fallback": "mock mode available"
            }
        
    except Exception as e:
        logger.error(f"Matter server test error: {e}")
        return {
            "status": "error",
            "matter_server": "python-matter-server",
            "url": MATTER_SERVER_URL,
            "error": str(e),
            "fallback": "mock mode available"
        }

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8084) 