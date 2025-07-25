"""
BLE Scanner for Matter device discovery
"""

import asyncio
import logging
import subprocess
from typing import Dict, List, Optional

from .config import Config

logger = logging.getLogger(__name__)

class BLEScanner:
    """Bluetooth Low Energy scanner for Matter devices"""
    
    def __init__(self, config: Config):
        self.config = config
        self.bluetooth_config = config.get_bluetooth_config()
        self.adapter = self.bluetooth_config.get("adapter", "hci0")
        self.timeout = self.bluetooth_config.get("timeout", 30)
        self.scan_duration = self.bluetooth_config.get("scan_duration", 10)
        
    async def is_available(self) -> bool:
        """Check if Bluetooth is available"""
        try:
            # Check if bluetoothctl is available
            result = await self._run_command(["bluetoothctl", "--version"])
            if result["return_code"] != 0:
                return False
            
            # Check if adapter exists
            result = await self._run_command(["bluetoothctl", "list"])
            if result["return_code"] != 0:
                return False
            
            # Check if any controller is available (output contains "Controller")
            if "Controller" not in result["stdout"]:
                return False
            
            return True
            
        except Exception as e:
            logger.error(f"Error checking Bluetooth availability: {e}")
            return False
    
    async def scan_devices(self, timeout: Optional[int] = None) -> List[Dict]:
        """Scan for BLE Matter devices"""
        try:
            if not await self.is_available():
                raise Exception("Bluetooth not available")
            
            # Use provided timeout or default scan_duration
            scan_time = timeout if timeout is not None else self.scan_duration
            
            # Start scanning
            await self._run_command([
                "bluetoothctl", 
                "scan", "on"
            ])
            
            # Wait for scan duration
            await asyncio.sleep(scan_time)
            
            # Stop scanning
            await self._run_command([
                "bluetoothctl", 
                "scan", "off"
            ])
            
            # Get discovered devices
            result = await self._run_command([
                "bluetoothctl", 
                "devices"
            ])
            
            if result["return_code"] == 0:
                devices = self._parse_devices(result["stdout"])
                logger.info(f"Found {len(devices)} total devices: {devices}")
                # Filter for Matter devices
                matter_devices = self._filter_matter_devices(devices)
                logger.info(f"Found {len(matter_devices)} Matter devices: {matter_devices}")
                return matter_devices
            else:
                raise Exception(f"Failed to get devices: {result['stderr']}")
                
        except Exception as e:
            logger.error(f"Error scanning BLE devices: {e}")
            return []
    
    async def get_device_info(self, device_address: str) -> Dict:
        """Get detailed information about a BLE device"""
        try:
            # Get device info
            result = await self._run_command([
                "bluetoothctl", 
                "info", device_address
            ])
            
            if result["return_code"] == 0:
                return self._parse_device_info(result["stdout"])
            else:
                return {"error": result["stderr"]}
                
        except Exception as e:
            logger.error(f"Error getting device info for {device_address}: {e}")
            return {"error": str(e)}
    
    async def connect_device(self, device_address: str) -> bool:
        """Connect to a BLE device"""
        try:
            # Try to connect
            result = await self._run_command([
                "bluetoothctl", 
                "connect", device_address
            ])
            
            if result["return_code"] == 0:
                return True
            else:
                logger.error(f"Failed to connect to {device_address}: {result['stderr']}")
                return False
                
        except Exception as e:
            logger.error(f"Error connecting to device {device_address}: {e}")
            return False
    
    async def disconnect_device(self, device_address: str) -> bool:
        """Disconnect from a BLE device"""
        try:
            result = await self._run_command([
                "bluetoothctl", 
                "disconnect", device_address
            ])
            
            if result["return_code"] == 0:
                return True
            else:
                logger.error(f"Failed to disconnect from {device_address}: {result['stderr']}")
                return False
                
        except Exception as e:
            logger.error(f"Error disconnecting from device {device_address}: {e}")
            return False
    
    def _parse_devices(self, output: str) -> List[Dict]:
        """Parse bluetoothctl devices output"""
        devices = []
        try:
            lines = output.strip().split('\n')
            for line in lines:
                if line.startswith('Device'):
                    # Parse device line
                    parts = line.split()
                    if len(parts) >= 2:
                        address = parts[1]
                        name = ' '.join(parts[2:]) if len(parts) > 2 else "Unknown"
                        devices.append({
                            "address": address,
                            "name": name,
                            "type": "unknown"
                        })
        except Exception as e:
            logger.warning(f"Could not parse devices output: {e}")
        
        return devices
    
    def _parse_device_info(self, output: str) -> Dict:
        """Parse bluetoothctl info output"""
        info = {}
        try:
            lines = output.strip().split('\n')
            for line in lines:
                if ':' in line:
                    key, value = line.split(':', 1)
                    info[key.strip()] = value.strip()
        except Exception as e:
            logger.warning(f"Could not parse device info: {e}")
        
        return info
    
    def _filter_matter_devices(self, devices: List[Dict]) -> List[Dict]:
        """Filter devices to find Matter devices"""
        # For testing, return all devices
        for device in devices:
            device["type"] = "ble"
        return devices
    
    async def _run_command(self, cmd: List[str], timeout: int = 30) -> Dict:
        """Run a command and return the result"""
        try:
            process = await asyncio.create_subprocess_exec(
                *cmd,
                stdout=asyncio.subprocess.PIPE,
                stderr=asyncio.subprocess.PIPE
            )
            
            stdout, stderr = await asyncio.wait_for(
                process.communicate(),
                timeout=timeout
            )
            
            return {
                "return_code": process.returncode,
                "stdout": stdout.decode('utf-8'),
                "stderr": stderr.decode('utf-8')
            }
            
        except asyncio.TimeoutError:
            process.kill()
            return {
                "return_code": -1,
                "stdout": "",
                "stderr": f"Command timed out after {timeout} seconds"
            }
        except Exception as e:
            return {
                "return_code": -1,
                "stdout": "",
                "stderr": str(e)
            } 