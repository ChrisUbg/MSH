#!/usr/bin/env python3
"""
MSH Commissioning Server - Main Application
Handles BLE device discovery, commissioning, and credential transfer to Pi
"""

import asyncio
import logging
from fastapi import FastAPI, HTTPException, WebSocket, WebSocketDisconnect
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import Optional, Dict, Any, List
import json
from datetime import datetime
import time

from commissioning_server.core.config import Config
from commissioning_server.core.matter_client import MatterClient
from commissioning_server.core.ble_scanner import BLEScanner
from commissioning_server.core.credential_store import CredentialStore
from commissioning_server.core.device_manager import DeviceManager

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Initialize FastAPI app
app = FastAPI(
    title="MSH Commissioning Server",
    description="PC-based commissioning server for Matter devices",
    version="1.0.0"
)

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Initialize components
config = Config()
matter_client = MatterClient(config)
ble_scanner = BLEScanner(config)
credential_store = CredentialStore(config)
device_manager = DeviceManager(config)

# Pydantic models
class CommissioningRequest(BaseModel):
    device_name: str
    device_type: str
    qr_code: str
    network_ssid: str
    network_password: str
    pin: Optional[str] = "20202021"
    pi_ip: str
    pi_user: str = "chregg"

class BLEScanRequest(BaseModel):
    scan_timeout: Optional[int] = 30

class CredentialTransferRequest(BaseModel):
    device_id: str
    pi_ip: str
    pi_user: str = "chregg"

class DeviceControlRequest(BaseModel):
    device_id: str
    cluster: str
    command: str
    endpoint: str = "1"

# WebSocket connections
active_connections: List[WebSocket] = []

@app.on_event("startup")
async def startup_event():
    """Initialize components on startup"""
    logger.info("Starting MSH Commissioning Server...")
    
    # Initialize components
    await credential_store.initialize()
    
    # Initialize Matter client (optional)
    try:
        await matter_client.initialize()
        logger.info("Matter SDK initialized successfully")
    except Exception as e:
        logger.warning(f"Matter SDK initialization failed: {e}")
        logger.info("Server will run in limited mode (BLE scanning only)")
    
    logger.info("MSH Commissioning Server started successfully")

@app.on_event("shutdown")
async def shutdown_event():
    """Cleanup on shutdown"""
    logger.info("Shutting down MSH Commissioning Server...")

@app.get("/api/status")
async def get_status():
    """Get server status"""
    return {
        "status": "running",
        "service": "MSH Commissioning Server",
        "version": "1.0.0",
        "timestamp": datetime.utcnow().isoformat(),
        "components": {
            "matter_client": await matter_client.get_status(),
            "ble_scanner": "available",
            "credential_store": "initialized",
            "device_manager": "available"
        }
    }

@app.post("/api/devices/scan-ble")
async def scan_ble_devices(request: BLEScanRequest):
    """Scan for BLE Matter devices"""
    try:
        logger.info(f"Starting BLE scan with timeout: {request.scan_timeout}s")
        
        devices = await ble_scanner.scan_devices(timeout=request.scan_timeout)
        
        return {
            "status": "success",
            "devices_found": len(devices),
            "devices": devices,
            "scan_timeout": request.scan_timeout
        }
        
    except Exception as e:
        logger.error(f"BLE scan failed: {e}")
        raise HTTPException(status_code=500, detail=f"BLE scan failed: {str(e)}")

@app.post("/commission")
async def commission_device(request: CommissioningRequest):
    """Commission a Matter device"""
    try:
        logger.info(f"Commissioning request received: {request}")
        
        # Validate request
        if not request.qr_code:
            raise HTTPException(status_code=400, detail="QR code is required")
        if not request.network_ssid:
            raise HTTPException(status_code=400, detail="Network SSID is required")
        if not request.network_password:
            raise HTTPException(status_code=400, detail="Network password is required")
        
        # Generate device ID
        device_id = f"device_{int(time.time())}"
        
        # Prepare commissioning data
        commissioning_data = {
            "device_name": request.device_name,
            "device_type": request.device_type,
            "qr_code": request.qr_code,
            "network_ssid": request.network_ssid,
            "network_password": request.network_password
        }
        
        # Perform commissioning
        logger.info(f"Starting commissioning for device {device_id}")
        commissioning_result = await matter_client.commission_device(
            device_id=device_id,
            commissioning_type="ble",
            qr_code=request.qr_code,
            network_ssid=request.network_ssid,
            network_password=request.network_password
        )
        
        if commissioning_result.get("success"):
            logger.info(f"Commissioning successful for device {device_id}")
            
            # Add commissioning data to result for transfer
            commissioning_result.update({
                "device_id": device_id,
                "device_name": request.device_name,
                "device_type": request.device_type,
                "qr_code": request.qr_code,
                "network_ssid": request.network_ssid,
                "network_password": request.network_password
            })
            
            # Transfer credentials to Pi if specified
            if request.pi_ip:
                logger.info(f"Transferring credentials to Pi at {request.pi_ip}")
                transfer_result = await device_manager.transfer_credentials_to_pi(
                    device_id=device_id,
                    commissioning_result=commissioning_result,
                    pi_ip=request.pi_ip
                )
                
                if transfer_result.get("success"):
                    logger.info(f"Successfully transferred device {device_id} to Pi")
                    commissioning_result["transfer"] = transfer_result
                else:
                    logger.warning(f"Failed to transfer device {device_id} to Pi: {transfer_result.get('message')}")
                    commissioning_result["transfer"] = transfer_result
            
            return {
                "success": True,
                "device_id": device_id,
                "message": "Device commissioned successfully",
                "commissioning_result": commissioning_result
            }
        else:
            logger.error(f"Commissioning failed for device {device_id}: {commissioning_result}")
            raise HTTPException(
                status_code=500, 
                detail=f"Commissioning failed: {commissioning_result.get('message', 'Unknown error')}"
            )
            
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error during commissioning: {e}")
        raise HTTPException(status_code=500, detail=f"Commissioning failed: {str(e)}")

@app.get("/api/devices/credentials")
async def get_all_credentials():
    """Get all stored device credentials"""
    try:
        credentials = await credential_store.get_all_credentials()
        return {
            "status": "success",
            "devices_count": len(credentials),
            "devices": credentials
        }
    except Exception as e:
        logger.error(f"Failed to get credentials: {e}")
        raise HTTPException(status_code=500, detail=f"Failed to get credentials: {str(e)}")

@app.get("/api/devices/{device_id}/credentials")
async def get_device_credentials(device_id: str):
    """Get credentials for a specific device"""
    try:
        credentials = await credential_store.get_credentials(device_id)
        if not credentials:
            raise HTTPException(status_code=404, detail=f"Device {device_id} not found")
        
        return {
            "status": "success",
            "device_id": device_id,
            "credentials": credentials
        }
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Failed to get device credentials: {e}")
        raise HTTPException(status_code=500, detail=f"Failed to get device credentials: {str(e)}")

@app.post("/api/devices/transfer-credentials")
async def transfer_credentials(request: CredentialTransferRequest):
    """Transfer existing device credentials to Pi"""
    try:
        # Get device credentials
        credentials = await credential_store.get_credentials(request.device_id)
        if not credentials:
            raise HTTPException(status_code=404, detail=f"Device {request.device_id} not found")
        
        # Transfer to Pi
        transfer_result = await device_manager.transfer_to_pi(
            credentials,
            request.pi_ip,
            request.pi_user
        )
        
        return {
            "status": "success",
            "device_id": request.device_id,
            "transfer_result": transfer_result
        }
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Credential transfer failed: {e}")
        raise HTTPException(status_code=500, detail=f"Credential transfer failed: {str(e)}")

@app.post("/api/devices/control")
async def control_device(request: DeviceControlRequest):
    """Control a Matter device"""
    try:
        if not matter_client.initialized:
            raise HTTPException(status_code=503, detail="Matter SDK not initialized")
        
        # Control device using Matter client
        result = await matter_client.control_device(
            request.device_id,
            request.cluster,
            request.command,
            request.endpoint
        )
        
        return {
            "success": result["success"],
            "message": result.get("message", "Device control command executed"),
            "device_id": request.device_id,
            "cluster": request.cluster,
            "command": request.command,
            "result": result
        }
        
    except Exception as e:
        logger.error(f"Error controlling device {request.device_id}: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/api/test-qr")
async def test_qr_code(qr_code: str):
    """Test endpoint to verify QR code processing"""
    logger.info(f"Testing QR code: '{qr_code}' (length: {len(qr_code)})")
    
    # Strip MT: prefix if present
    if qr_code.startswith("MT:"):
        qr_code = qr_code[3:]
        logger.info(f"Stripped MT: prefix, final code: '{qr_code}' (length: {len(qr_code)})")
    
    # Test the chip-tool command
    cmd = ["/usr/local/bin/chip-tool", "pairing", "code", "112233", qr_code]
    logger.info(f"Test command: {' '.join(cmd)}")
    
    try:
        result = await matter_client._run_command(cmd, timeout=30)
        return {"success": True, "result": result}
    except Exception as e:
        return {"success": False, "error": str(e)}

@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    """WebSocket endpoint for real-time updates"""
    await websocket.accept()
    active_connections.append(websocket)
    
    try:
        while True:
            # Keep connection alive and handle messages
            data = await websocket.receive_text()
            message = json.loads(data)
            
            # Handle different message types
            if message.get("type") == "ping":
                await websocket.send_text(json.dumps({"type": "pong", "timestamp": datetime.utcnow().isoformat()}))
            elif message.get("type") == "scan_ble":
                # Handle BLE scan request
                devices = await ble_scanner.scan_devices(timeout=30)
                await websocket.send_text(json.dumps({
                    "type": "scan_result",
                    "devices": devices
                }))
            else:
                await websocket.send_text(json.dumps({
                    "type": "error",
                    "message": "Unknown message type"
                }))
                
    except WebSocketDisconnect:
        active_connections.remove(websocket)
    except Exception as e:
        logger.error(f"WebSocket error: {e}")
        if websocket in active_connections:
            active_connections.remove(websocket)

if __name__ == "__main__":
    import uvicorn
    # Get server config from the config object
    server_config = config.get_server_config()
    uvicorn.run(
        app, 
        host=server_config.get("host", "0.0.0.0"), 
        port=server_config.get("port", 8888)
    ) 