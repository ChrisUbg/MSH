"""
Credential store for Matter device credentials
"""

import json
import logging
import sqlite3
from pathlib import Path
from typing import Dict, List, Optional

from .config import Config

logger = logging.getLogger(__name__)

class CredentialStore:
    """Secure storage for Matter device credentials"""
    
    def __init__(self, config: Config):
        self.config = config
        self.storage_config = config.get_storage_config()
        self.security_config = config.get_security_config()
        self.db_path = self.storage_config.get("path", "./credentials.db")
        self.encrypt_credentials = self.security_config.get("encrypt_credentials", True)
        self.initialized = False
        
    async def initialize(self):
        """Initialize the credential store"""
        try:
            # Ensure database directory exists
            db_dir = Path(self.db_path).parent
            db_dir.mkdir(parents=True, exist_ok=True)
            
            # Create database and tables
            await self._create_tables()
            
            self.initialized = True
            logger.info("Credential store initialized successfully")
            
        except Exception as e:
            logger.error(f"Failed to initialize credential store: {e}")
            raise
    
    async def _create_tables(self):
        """Create database tables"""
        try:
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()
            
            # Create devices table
            cursor.execute("""
                CREATE TABLE IF NOT EXISTS devices (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    device_id TEXT UNIQUE NOT NULL,
                    name TEXT,
                    type TEXT,
                    commissioning_type TEXT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )
            """)
            
            # Create credentials table
            cursor.execute("""
                CREATE TABLE IF NOT EXISTS credentials (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    device_id TEXT NOT NULL,
                    fabric_id TEXT,
                    node_id TEXT,
                    endpoint TEXT,
                    credentials_data TEXT,
                    encrypted BOOLEAN DEFAULT 0,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (device_id) REFERENCES devices (device_id)
                )
            """)
            
            # Create transfers table
            cursor.execute("""
                CREATE TABLE IF NOT EXISTS transfers (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    device_id TEXT NOT NULL,
                    pi_ip TEXT,
                    pi_user TEXT,
                    transfer_status TEXT,
                    transferred_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (device_id) REFERENCES devices (device_id)
                )
            """)
            
            conn.commit()
            conn.close()
            
        except Exception as e:
            logger.error(f"Error creating database tables: {e}")
            raise
    
    async def store_credentials(self, device_id: str, credentials: Dict, 
                              device_info: Optional[Dict] = None) -> bool:
        """Store device credentials"""
        try:
            if not self.initialized:
                raise Exception("Credential store not initialized")
            
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()
            
            # Store device info
            if device_info:
                cursor.execute("""
                    INSERT OR REPLACE INTO devices 
                    (device_id, name, type, commissioning_type, updated_at)
                    VALUES (?, ?, ?, ?, CURRENT_TIMESTAMP)
                """, (
                    device_id,
                    device_info.get("name", "Unknown"),
                    device_info.get("type", "matter"),
                    device_info.get("commissioning_type", "unknown")
                ))
            
            # Store credentials
            credentials_json = json.dumps(credentials)
            if self.encrypt_credentials:
                credentials_json = self._encrypt_data(credentials_json)
            
            cursor.execute("""
                INSERT OR REPLACE INTO credentials 
                (device_id, fabric_id, node_id, endpoint, credentials_data, encrypted)
                VALUES (?, ?, ?, ?, ?, ?)
            """, (
                device_id,
                credentials.get("fabric_id"),
                credentials.get("node_id"),
                credentials.get("endpoint"),
                credentials_json,
                self.encrypt_credentials
            ))
            
            conn.commit()
            conn.close()
            
            logger.info(f"Stored credentials for device {device_id}")
            return True
            
        except Exception as e:
            logger.error(f"Error storing credentials for device {device_id}: {e}")
            return False
    
    async def get_credentials(self, device_id: str) -> Optional[Dict]:
        """Get stored credentials for a device"""
        try:
            if not self.initialized:
                raise Exception("Credential store not initialized")
            
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()
            
            cursor.execute("""
                SELECT credentials_data, encrypted FROM credentials 
                WHERE device_id = ?
            """, (device_id,))
            
            result = cursor.fetchone()
            conn.close()
            
            if result:
                credentials_data, encrypted = result
                if encrypted:
                    credentials_data = self._decrypt_data(credentials_data)
                
                return json.loads(credentials_data)
            else:
                return None
                
        except Exception as e:
            logger.error(f"Error getting credentials for device {device_id}: {e}")
            return None
    
    async def get_all_credentials(self) -> List[Dict]:
        """Get all stored device credentials"""
        try:
            if not self.initialized:
                raise Exception("Credential store not initialized")
            
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()
            
            cursor.execute("""
                SELECT c.device_id, c.credentials_data, c.encrypted,
                       d.name, d.type, d.commissioning_type
                FROM credentials c
                LEFT JOIN devices d ON c.device_id = d.device_id
            """)
            
            results = cursor.fetchall()
            conn.close()
            
            credentials = []
            for result in results:
                device_id, credentials_data, encrypted, name, device_type, commissioning_type = result
                
                if encrypted:
                    credentials_data = self._decrypt_data(credentials_data)
                
                credentials.append({
                    "device_id": device_id,
                    "name": name or "Unknown",
                    "type": device_type or "matter",
                    "commissioning_type": commissioning_type or "unknown",
                    "credentials": json.loads(credentials_data)
                })
            
            return credentials
            
        except Exception as e:
            logger.error(f"Error getting all credentials: {e}")
            return []
    
    async def delete_credentials(self, device_id: str) -> bool:
        """Delete stored credentials for a device"""
        try:
            if not self.initialized:
                raise Exception("Credential store not initialized")
            
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()
            
            # Delete credentials
            cursor.execute("DELETE FROM credentials WHERE device_id = ?", (device_id,))
            
            # Delete device info
            cursor.execute("DELETE FROM devices WHERE device_id = ?", (device_id,))
            
            # Delete transfer records
            cursor.execute("DELETE FROM transfers WHERE device_id = ?", (device_id,))
            
            conn.commit()
            conn.close()
            
            logger.info(f"Deleted credentials for device {device_id}")
            return True
            
        except Exception as e:
            logger.error(f"Error deleting credentials for device {device_id}: {e}")
            return False
    
    async def record_transfer(self, device_id: str, pi_ip: str, pi_user: str, 
                            status: str = "success") -> bool:
        """Record a credential transfer to Pi"""
        try:
            if not self.initialized:
                raise Exception("Credential store not initialized")
            
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()
            
            cursor.execute("""
                INSERT INTO transfers (device_id, pi_ip, pi_user, transfer_status)
                VALUES (?, ?, ?, ?)
            """, (device_id, pi_ip, pi_user, status))
            
            conn.commit()
            conn.close()
            
            logger.info(f"Recorded transfer for device {device_id} to {pi_user}@{pi_ip}")
            return True
            
        except Exception as e:
            logger.error(f"Error recording transfer for device {device_id}: {e}")
            return False
    
    async def get_transfer_history(self, device_id: Optional[str] = None) -> List[Dict]:
        """Get transfer history"""
        try:
            if not self.initialized:
                raise Exception("Credential store not initialized")
            
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()
            
            if device_id:
                cursor.execute("""
                    SELECT device_id, pi_ip, pi_user, transfer_status, transferred_at
                    FROM transfers WHERE device_id = ?
                    ORDER BY transferred_at DESC
                """, (device_id,))
            else:
                cursor.execute("""
                    SELECT device_id, pi_ip, pi_user, transfer_status, transferred_at
                    FROM transfers
                    ORDER BY transferred_at DESC
                """)
            
            results = cursor.fetchall()
            conn.close()
            
            transfers = []
            for result in results:
                device_id, pi_ip, pi_user, status, transferred_at = result
                transfers.append({
                    "device_id": device_id,
                    "pi_ip": pi_ip,
                    "pi_user": pi_user,
                    "status": status,
                    "transferred_at": transferred_at
                })
            
            return transfers
            
        except Exception as e:
            logger.error(f"Error getting transfer history: {e}")
            return []
    
    def _encrypt_data(self, data: str) -> str:
        """Encrypt data (placeholder - implement proper encryption)"""
        # TODO: Implement proper encryption
        # For now, just return the data as-is
        return data
    
    def _decrypt_data(self, data: str) -> str:
        """Decrypt data (placeholder - implement proper decryption)"""
        # TODO: Implement proper decryption
        # For now, just return the data as-is
        return data
    
    async def cleanup(self):
        """Cleanup credential store"""
        self.initialized = False
        logger.info("Credential store cleaned up") 