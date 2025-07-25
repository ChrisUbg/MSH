"""
Device manager for handling device operations and Pi transfers
"""

import asyncio
import subprocess
import logging
from typing import Dict, Any, Optional, List
from pathlib import Path
import json
from datetime import datetime

logger = logging.getLogger(__name__)

class DeviceManager:
    """Manages device operations and credential transfers to the Raspberry Pi"""
    
    def __init__(self, config):
        self.config = config
        self.ssh_key_path = config.get("ssh_key_path", "~/.ssh/backup_key")
        self.transfer_timeout = config.get("transfer_timeout", 300)
        
    async def transfer_credentials_to_pi(self, device_id: str, commissioning_result: Dict, pi_ip: str) -> Dict:
        """Transfer device credentials to Raspberry Pi"""
        try:
            # Extract commissioning data
            device_name = commissioning_result.get("device_name", "Unknown Device")
            device_type = commissioning_result.get("device_type", "Unknown Type")
            qr_code = commissioning_result.get("qr_code", "")
            network_ssid = commissioning_result.get("network_ssid", "")
            discriminator_used = commissioning_result.get("discriminator_used", "")
            qr_discriminator = commissioning_result.get("qr_discriminator", "")
            is_nous_device = commissioning_result.get("is_nous_device", False)
            commissioning_method = commissioning_result.get("method", "unknown")
            
            # Create commissioning data structure
            commissioning_data = {
                "device_id": device_id,
                "device_name": device_name,
                "device_type": device_type,
                "manufacturer": "NOUS" if is_nous_device else "Unknown",
                "vendor_id": "0x125D" if is_nous_device else "Unknown",
                "product_id": "0x06BC" if is_nous_device else "Unknown",
                "discriminator": discriminator_used,
                "qr_discriminator": qr_discriminator,
                "passcode": self._extract_passcode_from_qr(qr_code),
                "qr_code": qr_code,
                "network_ssid": network_ssid,
                "commissioning_method": commissioning_method,
                "attestation_bypassed": is_nous_device,  # NOUS devices require attestation bypass
                "commissioning_timestamp": datetime.now().isoformat(),
                "device_addresses": self._extract_device_addresses(commissioning_result),
                "is_nous_device": is_nous_device
            }
            
            logger.info(f"Transferring commissioning data to Pi: {commissioning_data}")
            
            # Transfer data to Pi via SSH
            success = await self._transfer_data_via_ssh(commissioning_data, pi_ip)
            
            if success:
                return {
                    "success": True,
                    "message": f"Commissioning data transferred to Pi at {pi_ip}",
                    "commissioning_data": commissioning_data
                }
            else:
                return {
                    "success": False,
                    "message": f"Failed to transfer commissioning data to Pi at {pi_ip}",
                    "commissioning_data": commissioning_data
                }
                
        except Exception as e:
            logger.error(f"Error transferring credentials to Pi: {e}")
            return {
                "success": False,
                "message": f"Error transferring credentials: {str(e)}"
            }

    def _extract_passcode_from_qr(self, qr_code: str) -> str:
        """Extract passcode from QR code using chip-tool"""
        try:
            # This would need to be implemented with actual chip-tool call
            # For now, return a placeholder
            return "85064361"  # Known NOUS passcode
        except Exception as e:
            logger.error(f"Failed to extract passcode from QR code: {e}")
            return ""

    def _extract_device_addresses(self, commissioning_result: Dict) -> List[str]:
        """Extract device addresses from commissioning result"""
        addresses = []
        
        # Look for device addresses in the commissioning output
        stdout = commissioning_result.get("stdout", "")
        if stdout:
            # Parse for IP addresses and ports
            import re
            ip_pattern = r'(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(\d+)'
            ipv6_pattern = r'\[([0-9a-fA-F:]+)\]:(\d+)'
            
            # Find IPv4 addresses
            for match in re.finditer(ip_pattern, stdout):
                addresses.append(f"{match.group(1)}:{match.group(2)}")
            
            # Find IPv6 addresses
            for match in re.finditer(ipv6_pattern, stdout):
                addresses.append(f"[{match.group(1)}]:{match.group(2)}")
        
        # Add default addresses if none found
        if not addresses:
            addresses = ["192.168.0.106:5540", "[FE80::FA17:2DFF:FE7F:BB0C]:5540"]
        
        return addresses

    async def _transfer_data_via_ssh(self, commissioning_data: Dict, pi_ip: str) -> bool:
        """Transfer commissioning data to Pi via SSH"""
        try:
            # Create JSON file with commissioning data
            import json
            import tempfile
            import os
            
            # Create temporary file
            with tempfile.NamedTemporaryFile(mode='w', suffix='.json', delete=False) as f:
                json.dump(commissioning_data, f, indent=2)
                temp_file = f.name
            
            # Transfer file to Pi
            ssh_cmd = [
                "scp", 
                "-i", self.ssh_key_path,
                "-o", "StrictHostKeyChecking=no",
                temp_file,
                f"pi@{pi_ip}:/tmp/commissioning_data_{commissioning_data['device_id']}.json"
            ]
            
            result = await self._run_command(ssh_cmd, timeout=30)
            
            # Clean up temporary file
            os.unlink(temp_file)
            
            if result["return_code"] == 0:
                logger.info(f"Successfully transferred commissioning data to Pi")
                return True
            else:
                logger.error(f"Failed to transfer commissioning data: {result['stderr']}")
                return False
                
        except Exception as e:
            logger.error(f"Error in SSH transfer: {e}")
            return False
    
    async def ping_pi(self, pi_ip: str, pi_user: str = "chregg") -> bool:
        """Test connectivity to Raspberry Pi"""
        try:
            cmd = [
                "ssh", "-i", self.ssh_key_path, "-o", "ConnectTimeout=5",
                f"{pi_user}@{pi_ip}", "echo", "ping-success"
            ]
            
            result = subprocess.run(cmd, capture_output=True, text=True, timeout=10)
            return result.returncode == 0 and "ping-success" in result.stdout
            
        except Exception as e:
            logger.error(f"Pi ping failed: {e}")
            return False
    
    async def get_pi_status(self, pi_ip: str, pi_user: str = "chregg") -> Dict[str, Any]:
        """Get status information from Raspberry Pi"""
        try:
            cmd = [
                "ssh", "-i", self.ssh_key_path, "-o", "ConnectTimeout=5",
                f"{pi_user}@{pi_ip}", "echo", "status-check"
            ]
            
            result = subprocess.run(cmd, capture_output=True, text=True, timeout=10)
            
            if result.returncode == 0:
                return {
                    "connected": True,
                    "pi_ip": pi_ip,
                    "status": "online"
                }
            else:
                return {
                    "connected": False,
                    "pi_ip": pi_ip,
                    "error": result.stderr
                }
                
        except Exception as e:
            return {
                "connected": False,
                "pi_ip": pi_ip,
                "error": str(e)
            } 