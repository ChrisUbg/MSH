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

app = FastAPI(title="MSH Matter Bridge - NOUS A8M Integration")

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

# Matter configuration - Try to find chip-tool in common locations
CHIP_TOOL_PATHS = [
    "/usr/local/bin/chip-tool",
    "/usr/bin/chip-tool", 
    "/opt/chip-tool/chip-tool",
    "/matter-sdk/out/linux-x64-all-clusters/chip-tool",
    "chip-tool"  # Try system PATH
]

CHIP_TOOL_PATH = None
for path in CHIP_TOOL_PATHS:
    if os.path.exists(path) or path == "chip-tool":
        CHIP_TOOL_PATH = path
        break

if not CHIP_TOOL_PATH:
    logger.warning("chip-tool not found in common locations, using 'chip-tool' from PATH")
    CHIP_TOOL_PATH = "chip-tool"

logger.info(f"Using chip-tool at: {CHIP_TOOL_PATH}")

class CommissioningRequest(BaseModel):
    device_name: str
    device_type: str
    pin: Optional[str] = "20202021"  # Default Matter PIN

class DeviceState(BaseModel):
    device_id: str
    state: Dict[str, Any]

# Device storage (in production this would be a database)
matter_devices = {}

async def run_chip_tool_command(command_args: list, timeout: int = 30) -> dict:
    """Run a chip-tool command and return parsed results"""
    try:
        cmd = [CHIP_TOOL_PATH] + command_args
        logger.info(f"Running chip-tool command: {' '.join(cmd)}")
        
        # Run the command
        process = await asyncio.create_subprocess_exec(
            *cmd,
            stdout=asyncio.subprocess.PIPE,
            stderr=asyncio.subprocess.PIPE
        )
        
        stdout, stderr = await asyncio.wait_for(process.communicate(), timeout=timeout)
        
        stdout_str = stdout.decode('utf-8') if stdout else ""
        stderr_str = stderr.decode('utf-8') if stderr else ""
        
        logger.info(f"chip-tool stdout: {stdout_str}")
        if stderr_str:
            logger.warning(f"chip-tool stderr: {stderr_str}")
        
        return {
            "success": process.returncode == 0,
            "stdout": stdout_str,
            "stderr": stderr_str,
            "return_code": process.returncode
        }
        
    except asyncio.TimeoutError:
        logger.error(f"chip-tool command timed out after {timeout} seconds")
        return {
            "success": False,
            "stdout": "",
            "stderr": f"Command timed out after {timeout} seconds",
            "return_code": -1
        }
    except Exception as e:
        logger.error(f"Error running chip-tool command: {e}")
        return {
            "success": False,
            "stdout": "",
            "stderr": str(e),
            "return_code": -1
        }

@app.get("/health")
async def health_check():
    return {"status": "healthy", "version": "matter-integration", "chip_tool": CHIP_TOOL_PATH}

@app.get("/")
async def root():
    return {"message": "MSH Matter Bridge API - NOUS A8M Integration", "mode": "production"}

@app.post("/commission")
async def commission_device(request: CommissioningRequest):
    """Commission a NOUS A8M Matter device"""
    try:
        logger.info(f"Starting Matter device commissioning for {request.device_name}")
        
        # First, discover commissionable devices
        logger.info("Discovering Matter devices...")
        discovery_result = await run_chip_tool_command([
            "discover", "commissionables"
        ], timeout=60)
        
        if not discovery_result["success"]:
            # If chip-tool isn't available, fall back to mock
            if "No such file or directory" in discovery_result["stderr"]:
                logger.warning("chip-tool not available, creating mock device for development")
                device_id = f"mock_nous_a8m_{len(matter_devices) + 1}"
                matter_devices[device_id] = {
                    "name": request.device_name,
                    "type": "nous_a8m_socket",
                    "node_id": 9999,  # Mock node ID
                    "state": {"power": False, "power_consumption": 0.0, "energy": 0.0},
                    "commissioned": True,
                    "mock": True,
                    "last_seen": asyncio.get_event_loop().time()
                }
                
                return {
                    "status": "success",
                    "message": "Mock NOUS A8M device created (chip-tool not available)",
                    "device_id": device_id,
                    "node_id": 9999,
                    "method": "mock",
                    "note": "Install chip-tool for real Matter functionality"
                }
            else:
                raise HTTPException(
                    status_code=400, 
                    detail=f"Device discovery failed: {discovery_result['stderr']}"
                )
        
        # Look for NOUS A8M or similar devices in discovery output
        discovered_devices = []
        for line in discovery_result["stdout"].split('\n'):
            if "Discovered" in line and ("A8M" in line or "NOUS" in line or "Socket" in line):
                discovered_devices.append(line)
        
        if not discovered_devices:
            logger.warning("No NOUS A8M devices found, proceeding with generic commissioning")
        
        # Generate a new node ID for this device
        node_id = len(matter_devices) + 1000  # Start from 1000 to avoid conflicts
        
        # Commission the device using BLE or mDNS
        logger.info(f"Commissioning device with node ID {node_id}")
        commission_result = await run_chip_tool_command([
            "pairing", "ble-wifi",
            str(node_id),  # Node ID
            "YourWiFiSSID",  # TODO: Make this configurable
            "YourWiFiPassword",  # TODO: Make this configurable
            request.pin,  # Matter PIN
            str(12345)  # Discriminator - should be read from device
        ], timeout=120)
        
        if commission_result["success"]:
            # Store device information
            device_id = f"nous_a8m_{node_id}"
            matter_devices[device_id] = {
                "name": request.device_name,
                "type": "nous_a8m_socket",
                "node_id": node_id,
                "state": {"power": False, "power_consumption": 0.0, "energy": 0.0},
                "commissioned": True,
                "last_seen": asyncio.get_event_loop().time()
            }
            
            logger.info(f"Device commissioned successfully: {device_id}")
            return {
                "status": "success",
                "message": "NOUS A8M device commissioned successfully",
                "device_id": device_id,
                "node_id": node_id,
                "method": "matter"
            }
        else:
            # Fall back to mock for development if real commissioning fails
            logger.warning("Real commissioning failed, creating mock device for development")
            device_id = f"mock_nous_a8m_{len(matter_devices) + 1}"
            matter_devices[device_id] = {
                "name": request.device_name,
                "type": "nous_a8m_socket",
                "node_id": 9999,  # Mock node ID
                "state": {"power": False, "power_consumption": 0.0, "energy": 0.0},
                "commissioned": True,
                "mock": True,
                "last_seen": asyncio.get_event_loop().time()
            }
            
            return {
                "status": "success",
                "message": "Mock NOUS A8M device created for development",
                "device_id": device_id,
                "node_id": 9999,
                "method": "mock",
                "note": "Real commissioning failed, using mock device"
            }
        
    except Exception as e:
        logger.error(f"Commissioning error: {e}")
        raise HTTPException(status_code=500, detail=f"Commissioning failed: {e}")

@app.post("/device/{device_id}/power")
async def toggle_device_power(device_id: str):
    """Toggle NOUS A8M socket power on/off"""
    try:
        if device_id not in matter_devices:
            raise HTTPException(status_code=404, detail="Device not found")
        
        device = matter_devices[device_id]
        
        # Check if this is a mock device
        if device.get("mock", False):
            # Mock behavior
            current_power = device["state"]["power"]
            new_power = not current_power
            device["state"]["power"] = new_power
            device["state"]["power_consumption"] = 15.5 if new_power else 0.0  # Mock power consumption
            
            return {
                "status": "success",
                "device_id": device_id,
                "action": "toggle",
                "power_state": new_power,
                "method": "mock"
            }
        
        # Real Matter device control
        node_id = device["node_id"]
        current_power = device["state"]["power"]
        new_command = "off" if current_power else "on"
        
        logger.info(f"Turning {new_command} device {device_id} (node {node_id})")
        
        # Use Matter On/Off cluster
        result = await run_chip_tool_command([
            "onoff", new_command,
            str(node_id),  # Node ID
            "1"  # Endpoint ID (usually 1 for main socket)
        ])
        
        if result["success"]:
            device["state"]["power"] = not current_power
            device["last_seen"] = asyncio.get_event_loop().time()
            
            return {
                "status": "success",
                "device_id": device_id,
                "action": "toggle",
                "power_state": device["state"]["power"],
                "method": "matter"
            }
        else:
            raise HTTPException(
                status_code=400, 
                detail=f"Failed to control device: {result['stderr']}"
            )
            
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error controlling device {device_id}: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/device/{device_id}/power-metrics")
async def get_power_metrics(device_id: str):
    """Get power consumption and energy metrics from NOUS A8M"""
    try:
        if device_id not in matter_devices:
            raise HTTPException(status_code=404, detail="Device not found")
        
        device = matter_devices[device_id]
        
        # Check if this is a mock device
        if device.get("mock", False):
            # Return mock metrics
            return {
                "status": "success",
                "device_id": device_id,
                "metrics": {
                    "power_consumption_w": device["state"]["power_consumption"],
                    "energy_kwh": device["state"].get("energy", 0.0),
                    "voltage_v": 230.0,
                    "current_a": device["state"]["power_consumption"] / 230.0 if device["state"]["power_consumption"] > 0 else 0.0
                },
                "method": "mock"
            }
        
        # Real Matter device - read electrical measurement cluster
        node_id = device["node_id"]
        
        # Read power measurement
        power_result = await run_chip_tool_command([
            "electricalmeasurement", "read", "active-power",
            str(node_id), "1"
        ])
        
        # Read energy measurement (if supported)
        energy_result = await run_chip_tool_command([
            "electricalmeasurement", "read", "total-active-power", 
            str(node_id), "1"
        ])
        
        metrics = {
            "power_consumption_w": 0.0,
            "energy_kwh": 0.0,
            "voltage_v": 0.0,
            "current_a": 0.0
        }
        
        # Parse power measurement
        if power_result["success"]:
            for line in power_result["stdout"].split('\n'):
                if "active-power:" in line:
                    try:
                        power_value = float(re.search(r'active-power:\s*(\d+\.?\d*)', line).group(1))
                        metrics["power_consumption_w"] = power_value
                        device["state"]["power_consumption"] = power_value
                    except:
                        pass
        
        # Parse energy measurement
        if energy_result["success"]:
            for line in energy_result["stdout"].split('\n'):
                if "total-active-power:" in line:
                    try:
                        energy_value = float(re.search(r'total-active-power:\s*(\d+\.?\d*)', line).group(1))
                        metrics["energy_kwh"] = energy_value / 1000.0  # Convert to kWh
                        device["state"]["energy"] = metrics["energy_kwh"]
                    except:
                        pass
        
        device["last_seen"] = asyncio.get_event_loop().time()
        
        return {
            "status": "success",
            "device_id": device_id,
            "metrics": metrics,
            "method": "matter"
        }
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error reading power metrics from {device_id}: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/device/{device_id}/state")
async def get_device_state(device_id: str):
    """Get current state of NOUS A8M device"""
    try:
        if device_id not in matter_devices:
            raise HTTPException(status_code=404, detail="Device not found")
        
        device = matter_devices[device_id]
        
        return {
            "status": "success",
            "device_id": device_id,
            "state": device["state"],
            "name": device["name"],
            "type": device["type"],
            "commissioned": device["commissioned"],
            "last_seen": device["last_seen"],
            "method": "mock" if device.get("mock", False) else "matter"
        }
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/devices")
async def list_devices():
    """List all Matter devices"""
    return {
        "status": "success",
        "devices": matter_devices,
        "count": len(matter_devices)
    }

@app.post("/device/{device_id}/schedule")
async def set_device_schedule(device_id: str, schedule: dict):
    """Set power schedule for NOUS A8M (future implementation)"""
    # TODO: Implement scheduling functionality
    return {
        "status": "success",
        "message": "Scheduling feature coming soon",
        "device_id": device_id,
        "schedule": schedule
    }

# Development endpoints
@app.post("/dev/reset")
async def reset_devices():
    """Reset all devices (development only)"""
    global matter_devices
    matter_devices = {}
    return {"status": "success", "message": "All devices reset"}

@app.get("/dev/chip-tool-test")
async def test_chip_tool():
    """Test if chip-tool is working"""
    result = await run_chip_tool_command(["--version"], timeout=10)
    return {
        "chip_tool_path": CHIP_TOOL_PATH,
        "working": result["success"],
        "output": result["stdout"],
        "error": result["stderr"]
    }

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8084) 