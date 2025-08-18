"""
Matter SDK client for commissioning operations
"""

import asyncio
import json
import logging
import os
import subprocess
import tempfile
import time
import uuid
from pathlib import Path
from typing import Dict, List, Optional

from .config import Config

logger = logging.getLogger(__name__)

class MatterClient:
    """Client for interacting with Matter SDK tools"""
    
    def __init__(self, config: Config):
        self.config = config
        self.matter_config = config.get_matter_config()
        self.chip_tool_path = self.matter_config.get("chip_tool_path")
        self.chip_repl_path = self.matter_config.get("chip_repl_path")
        self.fabric_id = self.matter_config.get("fabric_id", "1")
        self.base_node_id = self.matter_config.get("node_id", "112233")
        self.initialized = False
        # Store device to Node ID mappings
        self.device_node_mappings = {}
        self.mappings_file = Path("device_node_mappings.json")
        self._load_device_mappings()
        
    def _load_device_mappings(self):
        """Load device-to-Node-ID mappings from file"""
        try:
            if self.mappings_file.exists():
                with open(self.mappings_file, 'r') as f:
                    self.device_node_mappings = json.load(f)
                logger.info(f"Loaded {len(self.device_node_mappings)} device mappings")
        except Exception as e:
            logger.warning(f"Failed to load device mappings: {e}")
            self.device_node_mappings = {}
            
    def _save_device_mappings(self):
        """Save device-to-Node-ID mappings to file"""
        try:
            with open(self.mappings_file, 'w') as f:
                json.dump(self.device_node_mappings, f, indent=2)
            logger.info(f"Saved {len(self.device_node_mappings)} device mappings")
        except Exception as e:
            logger.error(f"Failed to save device mappings: {e}")
        
    def _generate_unique_node_id(self, device_id: str) -> str:
        """Generate a unique Node ID for a device"""
        # Use device_id hash + timestamp to ensure uniqueness
        timestamp = int(time.time() * 1000) % 0xFFFFFFFF  # 32 bits of timestamp
        device_hash = hash(device_id) % 0xFFFFFFFF  # 32 bits of device hash
        # Create a 64-bit Node ID by combining timestamp and device hash
        unique_id = (timestamp << 32) | device_hash
        return f"{unique_id:016X}"  # Format as 16-character hex string (64-bit)
        
    def _get_node_id_for_device(self, device_id: str) -> str:
        """Get or generate Node ID for a device"""
        if device_id not in self.device_node_mappings:
            node_id = self._generate_unique_node_id(device_id)
            self.device_node_mappings[device_id] = node_id
            self._save_device_mappings()  # Save when new mapping is created
            logger.info(f"Generated Node ID {node_id} for device {device_id}")
        return self.device_node_mappings[device_id]
    
    async def initialize(self):
        """Initialize the Matter SDK client"""
        try:
            # Check if Matter tools are available
            tools_available = await self._check_tools()
            
            if not tools_available:
                # Check if Matter SDK is optional
                if self.matter_config.get("optional", False):
                    logger.warning("Matter SDK tools not found, running in limited mode")
                    self.initialized = False
                    return
                else:
                    raise Exception("Matter SDK tools not found")
            
            # Initialize chip-tool
            await self._initialize_chip_tool()
            
            self.initialized = True
            logger.info("Matter SDK client initialized successfully")
            
        except Exception as e:
            logger.error(f"Failed to initialize Matter SDK client: {e}")
            raise
    
    async def _check_tools(self) -> bool:
        """Check if Matter SDK tools are available"""
        try:
            # Check chip-tool
            if not Path(self.chip_tool_path).exists():
                logger.error(f"chip-tool not found at {self.chip_tool_path}")
                return False
            
            # Check chip-repl
            if not Path(self.chip_repl_path).exists():
                logger.error(f"chip-repl not found at {self.chip_repl_path}")
                return False
            
            # Test chip-tool by checking if it's executable
            if not os.access(self.chip_tool_path, os.X_OK):
                logger.error("chip-tool is not executable")
                return False
            
            return True
            
        except Exception as e:
            logger.error(f"Error checking Matter tools: {e}")
            return False
    
    async def _initialize_chip_tool(self):
        """Initialize chip-tool with fabric and node configuration"""
        try:
            # Set up fabric
            await self._run_command([
                self.chip_tool_path,
                "fabric", "add",
                "--fabric-id", self.fabric_id
            ])
            
            # Set up node
            await self._run_command([
                self.chip_tool_path,
                "node", "add",
                "--fabric-id", self.fabric_id,
                "--node-id", self.base_node_id
            ])
            
            logger.info(f"Initialized chip-tool with fabric {self.fabric_id}, node {self.base_node_id}")
            
        except Exception as e:
            logger.error(f"Failed to initialize chip-tool: {e}")
            raise
    
    async def get_status(self) -> Dict:
        """Get Matter SDK status"""
        try:
            # Check if tools are available
            tools_available = await self._check_tools()
            
            # Get fabric info
            fabric_info = {}
            if tools_available:
                try:
                    result = await self._run_command([
                        self.chip_tool_path,
                        "fabric", "list"
                    ])
                    if result["return_code"] == 0:
                        fabric_info = self._parse_fabric_list(result["stdout"])
                except Exception as e:
                    logger.warning(f"Could not get fabric info: {e}")
            
            return {
                "initialized": self.initialized,
                "tools_available": tools_available,
                "chip_tool_path": self.chip_tool_path,
                "chip_repl_path": self.chip_repl_path,
                "fabric_id": self.fabric_id,
                "base_node_id": self.base_node_id,
                "device_mappings_count": len(self.device_node_mappings),
                "fabrics": fabric_info
            }
            
        except Exception as e:
            logger.error(f"Error getting Matter SDK status: {e}")
            return {
                "initialized": False,
                "error": str(e)
            }
    
    async def commission_device(self, device_id: str, commissioning_type: str, 
                              qr_code: Optional[str] = None, manual_code: Optional[str] = None,
                              network_ssid: Optional[str] = None, network_password: Optional[str] = None) -> Dict:
        """Commission a Matter device"""
        try:
            if not self.initialized:
                raise Exception("Matter SDK client not initialized")
            
            if commissioning_type not in ["ble", "wifi"]:
                raise Exception(f"Unsupported commissioning type: {commissioning_type}")
            
            # Prepare commissioning data
            commissioning_data = {}
            if qr_code:
                commissioning_data["qr_code"] = qr_code
            if manual_code:
                commissioning_data["manual_code"] = manual_code
            if network_ssid:
                commissioning_data["network_ssid"] = network_ssid
            if network_password:
                commissioning_data["network_password"] = network_password
            
            if commissioning_type == "ble":
                result = await self._commission_ble(device_id, commissioning_data)
            else:  # wifi
                result = await self._commission_wifi(device_id, commissioning_data)
            
            return {
                "success": True,
                "device_id": device_id,
                "commissioning_type": commissioning_type,
                "credentials": result.get("credentials", {}),
                "details": result
            }
            
        except Exception as e:
            logger.error(f"Error commissioning device {device_id}: {e}")
            return {
                "success": False,
                "device_id": device_id,
                "error": str(e)
            }
    
    async def _commission_ble(self, device_id: str, commissioning_data: Dict) -> Dict:
        """Commission device via BLE using the reliable BLE-WiFi method"""
        try:
            qr_code = commissioning_data.get("qr_code")
            network_ssid = commissioning_data.get("network_ssid")
            network_password = commissioning_data.get("network_password")
            
            if not qr_code:
                raise Exception("No QR code provided")
            if not network_ssid or not network_password:
                raise Exception("Network SSID and password are required for BLE-WiFi commissioning")
            
            # Parse QR code to get passcode
            parse_cmd = [
                self.chip_tool_path,
                "payload", "parse-setup-payload",
                qr_code
            ]
            
            logger.info(f"Parsing QR code with command: {' '.join(parse_cmd)}")
            parse_result = await self._run_command(parse_cmd, timeout=30)
            
            if parse_result["return_code"] != 0:
                raise Exception(f"Failed to parse QR code: {parse_result['stderr']}")
            
            # Extract passcode from parse output
            stdout = parse_result["stdout"]
            passcode = None
            for line in stdout.split('\n'):
                if "Passcode:" in line:
                    passcode = line.split("Passcode:")[1].strip()
                    break
            
            if not passcode:
                raise Exception("Could not extract passcode from QR code")
            
            logger.info(f"QR code parsing - passcode: {passcode}")
            
            # For NOUS devices, we need to detect the actual discriminator via BLE scan
            # Check if this is a NOUS device (based on device type or QR code pattern)
            is_nous_device = (
                commissioning_data.get("device_type", "").lower().find("nous") != -1 or
                qr_code.startswith("0150-")  # NOUS QR code pattern
            )
            
            discriminator = None
            if is_nous_device:
                logger.info("Detected NOUS device - scanning for actual discriminator")
                # Scan for Matter devices to find actual discriminator
                scan_result = await self._scan_for_matter_devices()
                if scan_result and "devices" in scan_result:
                    for device in scan_result["devices"]:
                        if device.get("name", "").find("MATTER") != -1:
                            # Extract discriminator from device name (e.g., "MATTER-0097" -> 97)
                            name = device.get("name", "")
                            if "-" in name:
                                try:
                                    discriminator = name.split("-")[-1]
                                    logger.info(f"Found NOUS device with actual discriminator: {discriminator}")
                                    break
                                except (ValueError, IndexError):
                                    continue
            
            if not discriminator:
                # Fallback: try to extract from QR code parsing
                for line in stdout.split('\n'):
                    if "Short discriminator:" in line:
                        discriminator = line.split("Short discriminator:")[1].split(" ")[0].strip()
                        break
            
            if not discriminator:
                raise Exception("Could not determine discriminator for device")
            
            logger.info(f"Using discriminator: {discriminator}")
            
            # Get or generate Node ID for this device
            node_id = self._get_node_id_for_device(device_id)
            logger.info(f"Using Node ID: {node_id}")
            
            # Use the reliable BLE-WiFi commissioning method
            cmd = [
                self.chip_tool_path,
                "pairing", "ble-wifi",
                node_id,
                network_ssid,
                network_password,
                passcode,
                discriminator,
                "--bypass-attestation-verifier", "true"  # Always bypass for NOUS devices
            ]
            
            logger.info(f"Starting BLE-WiFi commissioning with command: {' '.join(cmd)}")
            result = await self._run_command(cmd, timeout=300)
            
            logger.info(f"Commissioning result - return_code: {result['return_code']}")
            if result["stderr"]:
                logger.warning(f"Commissioning stderr: {result['stderr']}")
            
            if result["return_code"] == 0:
                credentials = self._parse_commissioning_output(result["stdout"])
                logger.info(f"BLE-WiFi commissioning successful! Credentials: {credentials}")
                return {
                    "success": True,
                    "credentials": credentials,
                    "method": "ble-wifi",
                    "discriminator_used": discriminator,
                    "passcode_used": passcode,
                    "node_id": node_id,
                    "is_nous_device": is_nous_device
                }
            else:
                error_msg = result.get("stderr", "Unknown error")
                logger.error(f"BLE-WiFi commissioning failed: {error_msg}")
                raise Exception(f"Commissioning failed: {error_msg}")
                
        except Exception as e:
            logger.error(f"Error in BLE commissioning: {e}")
            return {
                "success": False,
                "error": str(e),
                "method": "ble-wifi"
            }

    async def _scan_for_matter_devices(self) -> Dict:
        """Scan for Matter devices to detect actual discriminators"""
        try:
            # Use bluetoothctl to scan for Matter devices
            cmd = ["bluetoothctl", "scan", "on"]
            await self._run_command(cmd, timeout=10)
            
            # Get scan results
            cmd = ["bluetoothctl", "devices"]
            result = await self._run_command(cmd, timeout=5)
            
            devices = []
            if result["return_code"] == 0:
                for line in result["stdout"].split('\n'):
                    if "MATTER" in line:
                        # Parse device info (e.g., "Device F8:17:2D:7F:BB:0D MATTER-0097")
                        parts = line.split()
                        if len(parts) >= 3:
                            mac = parts[1]
                            name = " ".join(parts[2:])
                            devices.append({
                                "mac": mac,
                                "name": name
                            })
            
            return {"devices": devices}
            
        except Exception as e:
            logger.error(f"Failed to scan for Matter devices: {e}")
            return {"devices": []}
    
    async def _commission_wifi(self, device_id: str, commissioning_data: Dict) -> Dict:
        """Commission device via WiFi"""
        try:
            # Get or generate Node ID for this device
            node_id = self._get_node_id_for_device(device_id)
            
            # Use chip-tool for WiFi commissioning
            cmd = [
                self.chip_tool_path,
                "pairing", "code",
                node_id  # node-id comes before payload
            ]
            
            if "qr_code" in commissioning_data:
                cmd.append(commissioning_data["qr_code"])
            elif "manual_code" in commissioning_data:
                cmd.append(commissioning_data["manual_code"])
            else:
                raise Exception("No QR code or manual code provided")
            
            result = await self._run_command(cmd, timeout=120)  # Increased timeout for commissioning
            
            if result["return_code"] == 0:
                # Parse credentials from output
                credentials = self._parse_commissioning_output(result["stdout"])
                return {
                    "success": True,
                    "credentials": credentials
                }
            else:
                raise Exception(f"Commissioning failed: {result['stderr']}")
                
        except Exception as e:
            logger.error(f"WiFi commissioning failed: {e}")
            raise
    
    async def control_device(self, device_id: str, cluster: str, command: str, 
                           endpoint: str = "1", **kwargs) -> Dict:
        """Control a commissioned device"""
        try:
            if not self.initialized:
                raise Exception("Matter SDK client not initialized")
            
            # Get or generate Node ID for the device
            node_id = self._get_node_id_for_device(device_id)
            
            # Build chip-tool command with correct format
            cmd = [
                self.chip_tool_path,
                cluster,
                command,
                f"0x{node_id}",  # Node ID as positional argument with 0x prefix
                endpoint
            ]
            
            # Add additional arguments
            for key, value in kwargs.items():
                cmd.extend([f"--{key}", str(value)])
            
            logger.info(f"Control command: {' '.join(cmd)}")
            result = await self._run_command(cmd)
            logger.info(f"Control result: {result}")
            
            if result["return_code"] == 0:
                return {
                    "success": True,
                    "device_id": device_id,
                    "cluster": cluster,
                    "command": command,
                    "response": result["stdout"]
                }
            else:
                return {
                    "success": False,
                    "device_id": device_id,
                    "error": result["stderr"]
                }
                
        except Exception as e:
            logger.error(f"Error controlling device {device_id}: {e}")
            return {
                "success": False,
                "device_id": device_id,
                "error": str(e)
            }
    
    async def get_device_info(self, device_id: str) -> Dict:
        """Get information about a commissioned device"""
        try:
            # Get Node ID for the device
            node_id = self._get_node_id_for_device(device_id)
            
            # Use chip-tool to get device info
            cmd = [
                self.chip_tool_path,
                "descriptor", "read",
                "--fabric-id", self.fabric_id,
                "--node-id", node_id,
                "--endpoint", "0"
            ]
            
            result = await self._run_command(cmd)
            
            if result["return_code"] == 0:
                return {
                    "success": True,
                    "device_id": device_id,
                    "info": self._parse_device_info(result["stdout"])
                }
            else:
                return {
                    "success": False,
                    "device_id": device_id,
                    "error": result["stderr"]
                }
                
        except Exception as e:
            logger.error(f"Error getting device info for {device_id}: {e}")
            return {
                "success": False,
                "device_id": device_id,
                "error": str(e)
            }
    
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
    
    def _parse_fabric_list(self, output: str) -> Dict:
        """Parse fabric list output from chip-tool"""
        fabrics = {}
        try:
            lines = output.strip().split('\n')
            for line in lines:
                if 'fabric' in line.lower() and 'id' in line.lower():
                    # Parse fabric information
                    parts = line.split()
                    if len(parts) >= 2:
                        fabric_id = parts[1]
                        fabrics[fabric_id] = {
                            "id": fabric_id,
                            "status": "active"
                        }
        except Exception as e:
            logger.warning(f"Could not parse fabric list: {e}")
        
        return fabrics
    
    def _parse_commissioning_output(self, output: str) -> Dict:
        """Parse commissioning output to extract credentials"""
        credentials = {}
        try:
            # Look for device information in the output
            lines = output.strip().split('\n')
            for line in lines:
                if 'node id' in line.lower():
                    parts = line.split()
                    if len(parts) >= 3:
                        credentials["node_id"] = parts[2]
                elif 'fabric id' in line.lower():
                    parts = line.split()
                    if len(parts) >= 3:
                        credentials["fabric_id"] = parts[2]
                elif 'endpoint' in line.lower():
                    parts = line.split()
                    if len(parts) >= 2:
                        credentials["endpoint"] = parts[1]
        except Exception as e:
            logger.warning(f"Could not parse commissioning output: {e}")
        
        return credentials
    
    def _parse_device_info(self, output: str) -> Dict:
        """Parse device information from chip-tool output"""
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
    
    async def cleanup(self):
        """Cleanup Matter SDK client"""
        self.initialized = False
        logger.info("Matter SDK client cleaned up") 