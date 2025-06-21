from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import json
import os
from typing import Optional, Dict, Any
import logging
import asyncio

app = FastAPI(title="MSH Matter Bridge (Simplified)")

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

class CommissioningRequest(BaseModel):
    device_name: str
    device_type: str
    pin: Optional[str] = None

class DeviceState(BaseModel):
    device_id: str
    state: Dict[str, Any]

# Mock device storage (in production this would be a database)
mock_devices = {}

@app.get("/health")
async def health_check():
    return {"status": "healthy", "version": "simple"}

@app.get("/")
async def root():
    return {"message": "MSH Matter Bridge API (Simplified)", "mode": "development"}

@app.post("/commission")
async def commission_device():
    """Mock commission a Matter device"""
    try:
        logger.info("Starting mock device commissioning...")
        
        # Simulate discovery delay
        await asyncio.sleep(2)
        
        # Mock discovered device
        mock_device_id = f"mock_device_{len(mock_devices) + 1}"
        mock_devices[mock_device_id] = {
            "name": f"Mock Device {len(mock_devices) + 1}",
            "type": "smart_plug",
            "state": {"power": False},
            "commissioned": True
        }
        
        logger.info(f"Mock device commissioned: {mock_device_id}")
        
        return {
            "status": "success", 
            "message": "Mock device commissioned successfully", 
            "device_id": mock_device_id,
            "method": "mock"
        }
        
    except Exception as e:
        logger.error(f"Unexpected error during mock commissioning: {e}")
        raise HTTPException(status_code=500, detail=f"Unexpected error during commissioning: {e}")

@app.post("/device/{device_id}/state")
async def set_device_state(device_id: str, state: DeviceState):
    try:
        logger.info(f"Setting mock device state: {device_id}")
        
        if device_id not in mock_devices:
            # Create mock device if it doesn't exist
            mock_devices[device_id] = {
                "name": f"Device {device_id}",
                "type": "unknown",
                "state": {},
                "commissioned": True
            }
        
        # Update device state
        mock_devices[device_id]["state"].update(state.state)
        
        return {
            "status": "success",
            "message": "Mock device state updated successfully",
            "device_id": device_id,
            "new_state": mock_devices[device_id]["state"]
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/device/{device_id}/state")
async def get_device_state(device_id: str):
    try:
        if device_id not in mock_devices:
            raise HTTPException(status_code=404, detail="Device not found")
        
        return {
            "status": "success",
            "device_id": device_id,
            "state": mock_devices[device_id]["state"]
        }
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/device/{device_id}/status")
async def get_device_status(device_id: str):
    try:
        if device_id not in mock_devices:
            raise HTTPException(status_code=404, detail="Device not found")
        
        device = mock_devices[device_id]
        return {
            "status": "success",
            "device_id": device_id,
            "name": device["name"],
            "type": device["type"],
            "commissioned": device["commissioned"],
            "online": True,  # Mock as always online
            "state": device["state"]
        }
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/devices")
async def list_devices():
    """List all mock devices"""
    return {
        "status": "success",
        "devices": mock_devices
    }

@app.post("/device/{device_id}/toggle")
async def toggle_device(device_id: str):
    """Toggle device power state (mock)"""
    try:
        if device_id not in mock_devices:
            raise HTTPException(status_code=404, detail="Device not found")
        
        current_state = mock_devices[device_id]["state"].get("power", False)
        mock_devices[device_id]["state"]["power"] = not current_state
        
        return {
            "status": "success",
            "device_id": device_id,
            "action": "toggle",
            "new_power_state": mock_devices[device_id]["state"]["power"]
        }
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

# Development endpoint to reset mock data
@app.post("/dev/reset")
async def reset_mock_devices():
    """Reset all mock devices (development only)"""
    global mock_devices
    mock_devices = {}
    return {"status": "success", "message": "Mock devices reset"}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8084) 