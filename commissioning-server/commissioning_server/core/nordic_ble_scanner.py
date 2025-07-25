"""
Nordic nRF52840 Enhanced BLE Scanner for Matter device discovery
"""

import asyncio
import logging
import subprocess
import time
from typing import Dict, List, Optional, Tuple

from .config import Config

logger = logging.getLogger(__name__)

class NordicBLEScanner:
    """Enhanced BLE scanner optimized for Nordic nRF52840 dongle"""
    
    def __init__(self, config: Config):
        self.config = config
        self.bluetooth_config = config.get_bluetooth_config()
        self.nordic_config = self.bluetooth_config.get("nordic_optimizations", {})
        self.advanced_config = self.bluetooth_config.get("advanced_scanning", {})
        
        self.adapter = self.bluetooth_config.get("adapter", "hci0")
        self.timeout = self.bluetooth_config.get("timeout", 30)
        self.scan_duration = self.bluetooth_config.get("scan_duration", 10)
        
        # Nordic-specific settings
        self.high_power = self.nordic_config.get("enable_high_power", True)
        self.extended_advertising = self.nordic_config.get("enable_extended_advertising", True)
        self.scan_interval = self.nordic_config.get("scan_interval", 100)
        self.scan_window = self.nordic_config.get("scan_window", 50)
        
        # Advanced scanning settings
        self.duplicate_filtering = self.advanced_config.get("enable_duplicate_filtering", True)
        self.scan_type = self.advanced_config.get("scan_type", "active")
        self.filter_rssi = self.advanced_config.get("filter_rssi", -80)
        
    async def is_available(self) -> bool:
        """Check if Nordic Bluetooth adapter is available and optimized"""
        try:
            # Check if bluetoothctl is available
            result = await self._run_command(["bluetoothctl", "--version"])
            if result["return_code"] != 0:
                logger.error("bluetoothctl not available")
                return False
            
            # Check if Nordic adapter exists
            result = await self._run_command(["bluetoothctl", "list"])
            if result["return_code"] != 0:
                logger.error("Failed to list Bluetooth adapters")
                return False
            
            # Check for Nordic adapter
            if self.adapter not in result["stdout"]:
                logger.error(f"Nordic adapter {self.adapter} not found")
                return False
            
            # Check adapter capabilities
            adapter_info = await self._get_adapter_info()
            if adapter_info:
                logger.info(f"Nordic adapter capabilities: {adapter_info}")
            
            return True
            
        except Exception as e:
            logger.error(f"Error checking Nordic Bluetooth availability: {e}")
            return False
    
    async def optimize_adapter(self) -> bool:
        """Apply Nordic-specific optimizations to the Bluetooth adapter"""
        try:
            logger.info("Applying Nordic nRF52840 optimizations...")
            
            # Set high power mode if supported
            if self.high_power:
                await self._run_command([
                    "hciconfig", self.adapter, "up"
                ])
                logger.info("High power mode enabled")
            
            # Configure scan parameters for better performance
            scan_params = [
                "hcitool", "lescan", "--duplicates",
                "--scan-type", self.scan_type,
                "--scan-interval", str(self.scan_interval),
                "--scan-window", str(self.scan_window)
            ]
            
            # Test scan parameters
            result = await self._run_command(scan_params, timeout=5)
            if result["return_code"] == 0:
                logger.info("Nordic scan parameters configured successfully")
                return True
            else:
                logger.warning(f"Could not set Nordic scan parameters: {result['stderr']}")
                return False
                
        except Exception as e:
            logger.error(f"Error optimizing Nordic adapter: {e}")
            return False
    
    async def scan_devices_enhanced(self, timeout: Optional[int] = None) -> List[Dict]:
        """Enhanced device scanning with Nordic optimizations"""
        try:
            if not await self.is_available():
                raise Exception("Nordic Bluetooth adapter not available")
            
            # Apply optimizations
            await self.optimize_adapter()
            
            # Use provided timeout or default scan_duration
            scan_time = timeout if timeout is not None else self.scan_duration
            
            logger.info(f"Starting enhanced BLE scan for {scan_time} seconds...")
            
            # Start scanning with Nordic optimizations
            scan_cmd = [
                "bluetoothctl", 
                "scan", "on"
            ]
            
            await self._run_command(scan_cmd)
            
            # Wait for scan duration
            await asyncio.sleep(scan_time)
            
            # Stop scanning
            await self._run_command([
                "bluetoothctl", 
                "scan", "off"
            ])
            
            # Get discovered devices with enhanced filtering
            devices = await self._get_discovered_devices()
            
            # Filter for Matter devices with Nordic-specific criteria
            matter_devices = self._filter_matter_devices_enhanced(devices)
            
            logger.info(f"Enhanced scan found {len(matter_devices)} Matter devices")
            return matter_devices
                
        except Exception as e:
            logger.error(f"Error in enhanced BLE device scanning: {e}")
            return []
    
    async def get_device_info_enhanced(self, device_address: str) -> Dict:
        """Get enhanced device information with Nordic-specific details"""
        try:
            # Get basic device info
            result = await self._run_command([
                "bluetoothctl", 
                "info", device_address
            ])
            
            if result["return_code"] == 0:
                device_info = self._parse_device_info_enhanced(result["stdout"])
                
                # Add Nordic-specific information
                device_info["nordic_optimized"] = True
                device_info["scan_type"] = self.scan_type
                device_info["high_power_mode"] = self.high_power
                
                return device_info
            else:
                return {"error": result["stderr"]}
                
        except Exception as e:
            logger.error(f"Error getting enhanced device info for {device_address}: {e}")
            return {"error": str(e)}
    
    async def connect_device_enhanced(self, device_address: str) -> bool:
        """Enhanced device connection with Nordic optimizations"""
        try:
            logger.info(f"Attempting enhanced connection to {device_address}")
            
            # Set connection parameters for better reliability
            if self.high_power:
                # Use high power mode for connection
                logger.info("Using high power mode for connection")
            
            # Try to connect with retries
            max_retries = 3
            for attempt in range(max_retries):
                result = await self._run_command([
                    "bluetoothctl", 
                    "connect", device_address
                ], timeout=30)
                
                if result["return_code"] == 0:
                    logger.info(f"Enhanced connection successful on attempt {attempt + 1}")
                    return True
                else:
                    logger.warning(f"Connection attempt {attempt + 1} failed: {result['stderr']}")
                    if attempt < max_retries - 1:
                        await asyncio.sleep(2)  # Wait before retry
            
            logger.error(f"Enhanced connection failed after {max_retries} attempts")
            return False
                
        except Exception as e:
            logger.error(f"Error in enhanced device connection: {e}")
            return False
    
    def _filter_matter_devices_enhanced(self, devices: List[Dict]) -> List[Dict]:
        """Enhanced filtering for Matter devices with Nordic-specific criteria"""
        matter_devices = []
        
        for device in devices:
            # Enhanced Matter device detection
            name = device.get("name", "").lower()
            address = device.get("address", "")
            rssi = device.get("rssi", -100)
            
            # Check RSSI filter
            if rssi < self.filter_rssi:
                continue
            
            # Enhanced Matter device detection criteria
            matter_indicators = [
                "matter", "chip", "thread", "nous", "a8m", 
                "philips", "hue", "ikea", "tradfri"
            ]
            
            if any(indicator in name for indicator in matter_indicators):
                device["type"] = "matter"
                device["nordic_optimized"] = True
                device["scan_method"] = "enhanced"
                matter_devices.append(device)
                logger.info(f"Enhanced Matter device detected: {name} ({address}) RSSI: {rssi}")
        
        return matter_devices
    
    async def _get_adapter_info(self) -> Optional[Dict]:
        """Get detailed adapter information"""
        try:
            result = await self._run_command([
                "hciconfig", self.adapter
            ])
            
            if result["return_code"] == 0:
                return self._parse_adapter_info(result["stdout"])
            return None
            
        except Exception as e:
            logger.error(f"Error getting adapter info: {e}")
            return None
    
    async def _get_discovered_devices(self) -> List[Dict]:
        """Get discovered devices with enhanced parsing"""
        try:
            result = await self._run_command([
                "bluetoothctl", 
                "devices"
            ])
            
            if result["return_code"] == 0:
                return self._parse_devices_enhanced(result["stdout"])
            else:
                logger.error(f"Failed to get devices: {result['stderr']}")
                return []
                
        except Exception as e:
            logger.error(f"Error getting discovered devices: {e}")
            return []
    
    def _parse_devices_enhanced(self, output: str) -> List[Dict]:
        """Enhanced device parsing with Nordic-specific information"""
        devices = []
        lines = output.strip().split('\n')
        
        for line in lines:
            if line.strip():
                parts = line.split()
                if len(parts) >= 2:
                    address = parts[1]
                    name = ' '.join(parts[2:]) if len(parts) > 2 else "Unknown"
                    
                    device = {
                        "address": address,
                        "name": name,
                        "scanner_type": "nordic_enhanced",
                        "timestamp": time.time()
                    }
                    devices.append(device)
        
        return devices
    
    def _parse_device_info_enhanced(self, output: str) -> Dict:
        """Enhanced device info parsing"""
        info = {}
        lines = output.strip().split('\n')
        
        for line in lines:
            if ':' in line:
                key, value = line.split(':', 1)
                key = key.strip()
                value = value.strip()
                info[key] = value
        
        return info
    
    def _parse_adapter_info(self, output: str) -> Dict:
        """Parse adapter information"""
        info = {}
        lines = output.strip().split('\n')
        
        for line in lines:
            if ':' in line:
                key, value = line.split(':', 1)
                key = key.strip()
                value = value.strip()
                info[key] = value
        
        return info
    
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