#!/usr/bin/env python3
"""
Core Empower COM Interface
Simplified, working connection to Waters Empower Personal 7.0
"""

import win32com.client
from datetime import datetime
from pathlib import Path
from typing import Optional, Dict, Any

class EmpowerConnection:
    """Core Empower COM connection manager"""
    
    def __init__(self, log_directory: str = "C:\\STF"):
        self.connection: Optional[Any] = None
        self.log_directory = Path(log_directory)
        self.log_file = self.log_directory / f"empower_com_log_{datetime.now().strftime('%Y%m%d')}.txt"
        
        # Ensure log directory exists
        self.log_directory.mkdir(exist_ok=True)
        
    def _log(self, level: str, message: str, details: Dict = None):
        """Log COM activity"""
        try:
            timestamp = datetime.now().isoformat()
            log_entry = f"{timestamp} [{level}] {message}"
            
            if details:
                log_entry += f"\n  Details: {details}"
            
            log_entry += "\n"
            
            with open(self.log_file, 'a', encoding='utf-8') as f:
                f.write(log_entry)
                
        except Exception as e:
            print(f"⚠️ Logging failed: {e}")
    
    def connect(self) -> bool:
        """
        Connect to Empower via COM
        
        Returns:
            bool: True if connection successful, False otherwise
        """
        try:
            self._log("INFO", "Attempting COM connection to Millennium.Project")
            
            # Create COM connection
            self.connection = win32com.client.Dispatch("Millennium.Project")
            
            # Log successful connection
            connection_info = {
                "connection_object": str(self.connection),
                "connection_type": type(self.connection).__name__,
                "timestamp": datetime.now().isoformat()
            }
            
            self._log("SUCCESS", f"Connected to Empower: {self.connection}", connection_info)
            print(f"✅ Connected to Empower: {self.connection}")
            
            return True
            
        except Exception as e:
            self._log("ERROR", f"Failed to connect to Empower: {e}")
            print(f"❌ Failed to connect to Empower: {e}")
            return False
    
    def disconnect(self):
        """Disconnect from Empower"""
        try:
            if self.connection:
                self._log("INFO", "Disconnecting from Empower")
                self.connection = None
                print("✅ Disconnected from Empower")
        except Exception as e:
            self._log("ERROR", f"Error during disconnect: {e}")
            print(f"⚠️ Disconnect error: {e}")
    
    def is_connected(self) -> bool:
        """Check if connected to Empower"""
        return self.connection is not None
    
    def get_connection_info(self) -> Dict:
        """Get current connection information"""
        if not self.connection:
            return {"status": "disconnected"}
        
        return {
            "status": "connected",
            "connection_object": str(self.connection),
            "connection_type": type(self.connection).__name__,
            "connected_at": datetime.now().isoformat()
        }

def test_connection():
    """Test Empower COM connection"""
    print("🔍 Testing Empower COM Connection...")
    
    empower = EmpowerConnection()
    
    if empower.connect():
        info = empower.get_connection_info()
        print(f"📊 Connection Info: {info}")
        
        empower.disconnect()
        print("✅ Connection test successful!")
        return True
    else:
        print("❌ Connection test failed!")
        return False

if __name__ == "__main__":
    test_connection()
