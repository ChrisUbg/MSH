from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import subprocess
import json
import os
from typing import Optional, Dict, Any
import logging

app = FastAPI(title="MSH Matter Bridge")

# Configure CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, replace with specific origins
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Matter SDK paths
MATTER_SDK_PATH = "/matter-sdk"
CHIP_TOOL_PATH = f"{MATTER_SDK_PATH}/out/linux-x64-all-clusters-ipv6only/chip-tool"

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

class TasmotaDevice:
    def __init__(self, device_id: str, name: str):
        self.device_id = device_id
        self.name = name
        self.type = "Tasmota Socket"
        self.properties = {
            "power": False,
            "power_consumption": 0.0,
            "voltage": 0.0,
            "current": 0.0,
            "energy": 0.0
        }

    def update_state(self, state: dict):
        self.properties.update(state)

    def get_state(self) -> dict:
        return self.properties

@app.get("/health")
async def health_check():
    return {"status": "healthy"}

@app.get("/")
async def root():
    return {"message": "MSH Matter Bridge API"}

async def verify_device_commissioning(device_id: str, env: dict) -> bool:
    """Verify if a device is properly commissioned by checking its status."""
    try:
        # Try to read the device's basic information
        cmd = [
            CHIP_TOOL_PATH,
            "device",
            "read",
            "basic",
            "vendor-name",
            device_id,
            "1"  # Endpoint ID
        ]
        
        result = subprocess.run(cmd, capture_output=True, text=True, env=env)
        
        # If we can read the device info, it's commissioned
        return result.returncode == 0
    except Exception:
        return False

@app.post("/commission")
async def commission_device():
    """Commission a Matter device using BLE or mDNS discovery"""
    try:
        logger.info("Starting device commissioning...")
        
        # Set up environment
        env = os.environ.copy()
        env["PATH"] = f"{MATTER_SDK_PATH}/out/linux-x64-all-clusters-ipv6only:{env['PATH']}"
        env["MATTER_ROOT"] = MATTER_SDK_PATH
        
        # Try BLE discovery first (for devices like NOUS A8M)
        logger.info("Attempting BLE discovery for Matter devices...")
        ble_discovery_cmd = [
            CHIP_TOOL_PATH,
            "discover",
            "commissionables",
            "--ble-adapter", "0",  # Use first BLE adapter
            "--commissioner-name", "alpha",
            "--commissioner-nodeid", "112233",
            "--discover-once", "1",
            "--only-allow-trusted-cd-keys", "0"
        ]
        
        logger.info(f"BLE discovery command: {' '.join(ble_discovery_cmd)}")
        
        # Run BLE discovery
        ble_result = subprocess.run(ble_discovery_cmd, capture_output=True, text=True, env=env, timeout=30)
        logger.info(f"BLE discovery stdout: {ble_result.stdout}")
        logger.info(f"BLE discovery stderr: {ble_result.stderr}")
        
        if ble_result.returncode == 0 and "Discovered device" in ble_result.stdout:
            logger.info("BLE device discovered, proceeding with commissioning...")
            # Extract device info from BLE discovery
            # This is a simplified approach - in practice you'd parse the output
            device_info = "ble_device"  # Placeholder
        else:
            logger.info("No BLE devices found, trying mDNS discovery...")
            # Fall back to mDNS discovery
            mDNS_discovery_cmd = [
                CHIP_TOOL_PATH,
                "discover",
                "commissionables",
                "--commissioner-name", "alpha",
                "--commissioner-nodeid", "112233",
                "--discover-once", "1",
                "--only-allow-trusted-cd-keys", "0"
            ]
            
            mDNS_result = subprocess.run(mDNS_discovery_cmd, capture_output=True, text=True, env=env, timeout=60)
            logger.info(f"mDNS discovery stdout: {mDNS_result.stdout}")
            logger.info(f"mDNS discovery stderr: {mDNS_result.stderr}")
            
            if mDNS_result.returncode != 0:
                raise HTTPException(status_code=400, detail="Device discovery failed: No devices found via BLE or mDNS")
            
            device_info = "mdns_device"  # Placeholder
        
        # Proceed with commissioning (simplified for now)
        logger.info("Device discovered, commissioning would proceed here...")
        
        return {"status": "success", "message": "Device discovered successfully", "method": "BLE" if "ble_device" in locals() else "mDNS"}
        
    except subprocess.TimeoutExpired:
        logger.error("Device discovery timed out")
        raise HTTPException(status_code=400, detail="Device discovery timed out after 60 seconds. Please ensure your device is in pairing mode and within range.")
    except Exception as e:
        logger.error(f"Unexpected error during commissioning: {e}")
        raise HTTPException(status_code=500, detail=f"Unexpected error during commissioning: {e}")

@app.post("/device/{device_id}/state")
async def set_device_state(device_id: str, state: DeviceState):
    try:
        # Implement device state control
        cmd = [
            CHIP_TOOL_PATH,
            "onoff",
            "toggle",
            device_id,
            "1"  # Endpoint ID
        ]
        
        # Set up environment
        env = os.environ.copy()
        env["PATH"] = f"{MATTER_SDK_PATH}/out/linux-x64-all-clusters-ipv6only:{env['PATH']}"
        env["MATTER_ROOT"] = MATTER_SDK_PATH
        
        result = subprocess.run(cmd, capture_output=True, text=True, env=env)
        
        if result.returncode != 0:
            raise HTTPException(status_code=400, detail=f"Failed to set device state: {result.stderr}")
        
        return {
            "status": "success",
            "message": "Device state updated successfully"
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/device/{device_id}/state")
async def get_device_state(device_id: str):
    try:
        # Get current device state
        cmd = [
            CHIP_TOOL_PATH,
            "onoff",
            "read",
            "on-off",
            device_id,
            "1"  # Endpoint ID
        ]
        
        # Set up environment
        env = os.environ.copy()
        env["PATH"] = f"{MATTER_SDK_PATH}/out/linux-x64-all-clusters-ipv6only:{env['PATH']}"
        env["MATTER_ROOT"] = MATTER_SDK_PATH
        
        result = subprocess.run(cmd, capture_output=True, text=True, env=env)
        
        if result.returncode != 0:
            raise HTTPException(status_code=400, detail=f"Failed to get device state: {result.stderr}")
        
        # Parse the output to get state
        state = "unknown"
        for line in result.stdout.split('\n'):
            if "on-off:" in line:
                state = "on" if "true" in line.lower() else "off"
                break
        
        return {
            "status": "success",
            "device_id": device_id,
            "state": state
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/device/{device_id}/status")
async def get_device_status(device_id: str):
    """Get the current status of a commissioned device."""
    try:
        # Set up environment
        env = os.environ.copy()
        env["PATH"] = f"{MATTER_SDK_PATH}/out/linux-x64-all-clusters-ipv6only:{env['PATH']}"
        env["MATTER_ROOT"] = MATTER_SDK_PATH
        
        # Check if device is commissioned
        is_commissioned = await verify_device_commissioning(device_id, env)
        
        if not is_commissioned:
            return {
                "status": "error",
                "message": "Device is not commissioned or not responding",
                "commissioned": False
            }
        
        # Try to read basic device information
        cmd = [
            CHIP_TOOL_PATH,
            "device",
            "read",
            "basic",
            "vendor-name",
            device_id,
            "1"  # Endpoint ID
        ]
        
        result = subprocess.run(cmd, capture_output=True, text=True, env=env)
        
        if result.returncode != 0:
            return {
                "status": "error",
                "message": "Failed to read device information",
                "commissioned": True,
                "error": result.stderr
            }
        
        return {
            "status": "success",
            "message": "Device is commissioned and responding",
            "commissioned": True,
            "device_info": result.stdout
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/device/{device_id}/tasmota/state")
async def update_tasmota_state(device_id: str, state: dict):
    """Update the state of a Tasmota device."""
    try:
        # Set up environment
        env = os.environ.copy()
        env["PATH"] = f"{MATTER_SDK_PATH}/out/linux-x64-all-clusters-ipv6only:{env['PATH']}"
        env["MATTER_ROOT"] = MATTER_SDK_PATH

        # Update power state if provided
        if "power" in state:
            cmd = [
                CHIP_TOOL_PATH,
                "onoff",
                "toggle" if state["power"] else "off",
                device_id,
                "1"  # Endpoint ID
            ]
            result = subprocess.run(cmd, capture_output=True, text=True, env=env)
            if result.returncode != 0:
                raise HTTPException(status_code=400, detail=f"Failed to update power state: {result.stderr}")

        return {
            "status": "success",
            "message": "Tasmota device state updated successfully"
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/device/{device_id}/tasmota/metrics")
async def get_tasmota_metrics(device_id: str):
    """Get power metrics from a Tasmota device."""
    try:
        # Set up environment
        env = os.environ.copy()
        env["PATH"] = f"{MATTER_SDK_PATH}/out/linux-x64-all-clusters-ipv6only:{env['PATH']}"
        env["MATTER_ROOT"] = MATTER_SDK_PATH

        # Get power measurement
        power_cmd = [
            CHIP_TOOL_PATH,
            "electricalmeasurement",
            "read",
            "measurement-type",
            device_id,
            "1"  # Endpoint ID
        ]
        power_result = subprocess.run(power_cmd, capture_output=True, text=True, env=env)
        
        # Get energy measurement
        energy_cmd = [
            CHIP_TOOL_PATH,
            "electricalmeasurement",
            "read",
            "total-energy",
            device_id,
            "1"  # Endpoint ID
        ]
        energy_result = subprocess.run(energy_cmd, capture_output=True, text=True, env=env)

        metrics = {
            "power": 0.0,
            "energy": 0.0
        }

        # Parse power measurement
        for line in power_result.stdout.split('\n'):
            if "measurement-type:" in line:
                metrics["power"] = float(line.split(':')[1].strip())

        # Parse energy measurement
        for line in energy_result.stdout.split('\n'):
            if "total-energy:" in line:
                metrics["energy"] = float(line.split(':')[1].strip())

        return {
            "status": "success",
            "metrics": metrics
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
