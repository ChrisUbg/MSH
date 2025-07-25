"""
Configuration management for MSH Commissioning Server
"""

import os
import yaml
from pathlib import Path
from typing import Dict, List, Optional

class Config:
    """Configuration manager for the commissioning server"""
    
    def __init__(self, config_path: Optional[str] = None):
        self.config_path = config_path or self._get_default_config_path()
        self.config = self._load_config()
    
    def _get_default_config_path(self) -> str:
        """Get the default configuration file path"""
        # Look for config in current directory, then user home
        possible_paths = [
            "./config.yaml",
            "./config.yml", 
            str(Path.home() / ".msh" / "config.yaml"),
            str(Path.home() / ".msh" / "config.yml")
        ]
        
        for path in possible_paths:
            if os.path.exists(path):
                return path
        
        # Return default path for creation
        return "./config.yaml"
    
    def _load_config(self) -> Dict:
        """Load configuration from file or create default"""
        if os.path.exists(self.config_path):
            try:
                with open(self.config_path, 'r') as f:
                    config = yaml.safe_load(f)
                    if config:
                        return config
            except Exception as e:
                print(f"Warning: Could not load config from {self.config_path}: {e}")
        
        # Create default configuration
        config = self._get_default_config()
        self._save_config(config)
        return config
    
    def _get_default_config(self) -> Dict:
        """Get default configuration"""
        return {
            "server": {
                "host": "0.0.0.0",
                "port": 8080,
                "debug": False
            },
            "matter": {
                "sdk_path": "/usr/local/matter-sdk",
                "chip_tool_path": "/usr/local/bin/chip-tool",
                "chip_repl_path": "/usr/local/bin/chip-repl",
                "fabric_id": "1",
                "node_id": "112233",
                "optional": True  # Make Matter SDK optional
            },
            "bluetooth": {
                "adapter": "hci0",
                "timeout": 30,
                "scan_duration": 10
            },
            "storage": {
                "type": "sqlite",
                "path": "./credentials.db"
            },
            "security": {
                "api_key_required": False,
                "allowed_hosts": ["192.168.0.0/24"],
                "encrypt_credentials": True
            },
            "pi": {
                "default_ip": "192.168.0.107",
                "default_user": "chregg",
                "ssh_key_path": "~/.ssh/id_ed25519"
            }
        }
    
    def _save_config(self, config: Dict):
        """Save configuration to file"""
        try:
            # Ensure directory exists
            config_dir = os.path.dirname(self.config_path)
            if config_dir and not os.path.exists(config_dir):
                os.makedirs(config_dir)
            
            with open(self.config_path, 'w') as f:
                yaml.dump(config, f, default_flow_style=False, indent=2)
        except Exception as e:
            print(f"Warning: Could not save config to {self.config_path}: {e}")
    
    def get(self, key: str, default=None):
        """Get configuration value using dot notation"""
        keys = key.split('.')
        value = self.config
        
        for k in keys:
            if isinstance(value, dict) and k in value:
                value = value[k]
            else:
                return default
        
        return value
    
    def set(self, key: str, value):
        """Set configuration value using dot notation"""
        keys = key.split('.')
        config = self.config
        
        # Navigate to the parent of the target key
        for k in keys[:-1]:
            if k not in config:
                config[k] = {}
            config = config[k]
        
        # Set the value
        config[keys[-1]] = value
        
        # Save the updated configuration
        self._save_config(self.config)
    
    def get_server_config(self) -> Dict:
        """Get server configuration"""
        return self.get("server", {})
    
    def get_matter_config(self) -> Dict:
        """Get Matter SDK configuration"""
        return self.get("matter", {})
    
    def get_bluetooth_config(self) -> Dict:
        """Get Bluetooth configuration"""
        return self.get("bluetooth", {})
    
    def get_storage_config(self) -> Dict:
        """Get storage configuration"""
        return self.get("storage", {})
    
    def get_security_config(self) -> Dict:
        """Get security configuration"""
        return self.get("security", {})
    
    def get_pi_config(self) -> Dict:
        """Get Raspberry Pi configuration"""
        return self.get("pi", {})
    
    def validate(self) -> List[str]:
        """Validate configuration and return list of errors"""
        errors = []
        
        # Check required paths
        matter_config = self.get_matter_config()
        if not os.path.exists(matter_config.get("chip_tool_path", "")):
            errors.append(f"chip-tool not found at {matter_config.get('chip_tool_path')}")
        
        if not os.path.exists(matter_config.get("chip_repl_path", "")):
            errors.append(f"chip-repl not found at {matter_config.get('chip_repl_path')}")
        
        # Check storage path
        storage_config = self.get_storage_config()
        storage_path = storage_config.get("path", "")
        if storage_path:
            storage_dir = os.path.dirname(storage_path)
            if storage_dir and not os.path.exists(storage_dir):
                try:
                    os.makedirs(storage_dir)
                except Exception as e:
                    errors.append(f"Cannot create storage directory {storage_dir}: {e}")
        
        return errors
    
    def reload(self):
        """Reload configuration from file"""
        self.config = self._load_config()
    
    def get_config_path(self) -> str:
        """Get the configuration file path"""
        return self.config_path 