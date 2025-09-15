"""
Waters Automation Portal Driver - Sample Transfer Only

This module provides a Python interface for controlling the Waters Automation Portal
for sample transfer operations only. The Automation Portal provides access to sample 
drawer/tray slots for automated sample handling.

Based on Waters Automation Portal PC Protocol Specification (715008839).
"""

import serial
import socket
import time
import logging
from datetime import datetime
from typing import Optional, Dict, Any, List, Union
import config


class AutomationPortalError(Exception):
    """Custom exception for Automation Portal communication errors."""
    pass


class AutomationPortalDriver:
    """
    Driver for Waters Automation Portal - Sample Transfer Operations Only.
    
    This class provides methods to communicate with and control the Waters Automation Portal
    for sample transfer operations: Extract, Insert, GetStatus, Initialize, etc.
    
    Based on Waters Automation Portal PC Protocol Specification (715008839).
    """
    
    def __init__(self, 
                 port: str = None, 
                 baudrate: int = None,
                 timeout: float = None,
                 host: str = None,
                 tcp_port: int = None,
                 comm_mode: str = None):
        """
        Initialize the Waters Automation Portal driver.
        
        Args:
            port: Serial port for communication (e.g., 'COM1' on Windows)
            baudrate: Communication baudrate (default from config)
            timeout: Timeout for communication in seconds
            host: IP address for TCP/IP communication
            tcp_port: TCP port for network communication
            comm_mode: Communication mode ('serial' or 'tcp')
        """
        # Communication settings
        self.port = port or config.DEFAULT_PORT
        self.baudrate = baudrate or config.DEFAULT_BAUDRATE
        self.timeout = timeout or config.DEFAULT_TIMEOUT
        self.host = host or config.DEFAULT_TCP_HOST
        self.tcp_port = tcp_port or config.DEFAULT_TCP_PORT
        self.comm_mode = comm_mode or config.COMM_MODE_SERIAL
        
        # Connection state
        self.connection = None
        self.is_connected = False
        self.sequence_number = 0
        
        # Setup logging
        self.logger = logging.getLogger(__name__)
        if not self.logger.handlers:
            handler = logging.StreamHandler()
            formatter = logging.Formatter('%(asctime)s - %(name)s - %(levelname)s - %(message)s')
            handler.setFormatter(formatter)
            self.logger.addHandler(handler)
            self.logger.setLevel(logging.INFO)
    
    def connect(self) -> bool:
        """
        Establish connection to the Automation Portal.
        
        Returns:
            True if connection successful, False otherwise
        """
        try:
            if self.comm_mode == config.COMM_MODE_SERIAL:
                self.connection = serial.Serial(
                    port=self.port,
                    baudrate=self.baudrate,
                    timeout=self.timeout,
                    bytesize=serial.EIGHTBITS,
                    parity=serial.PARITY_NONE,
                    stopbits=serial.STOPBITS_ONE
                )
                self.logger.info(f"Connected via serial to {self.port}")
                
            elif self.comm_mode == config.COMM_MODE_TCP:
                self.connection = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                self.connection.settimeout(self.timeout)
                self.connection.connect((self.host, self.tcp_port))
                self.logger.info(f"Connected via TCP to {self.host}:{self.tcp_port}")
                
            else:
                raise AutomationPortalError(f"Invalid communication mode: {self.comm_mode}")
            
            self.is_connected = True
            return True
            
        except Exception as e:
            self.logger.error(f"Connection failed: {e}")
            self.is_connected = False
            return False
    
    def disconnect(self) -> None:
        """Disconnect from the Automation Portal."""
        if self.connection:
            try:
                self.connection.close()
                self.logger.info("Disconnected from Automation Portal")
            except Exception as e:
                self.logger.error(f"Error during disconnect: {e}")
            finally:
                self.connection = None
                self.is_connected = False
    
    def _get_next_sequence(self) -> int:
        """Get the next sequence number for commands."""
        self.sequence_number = (self.sequence_number % 255) + 1
        return self.sequence_number
    
    def _send_command(self, command: str, retries: int = None) -> str:
        """
        Send a command to the Automation Portal and return the response.
        
        Args:
            command: Command string to send
            retries: Number of retry attempts
            
        Returns:
            Response from the system
            
        Raises:
            AutomationPortalError: If communication fails
        """
        if not self.is_connected or not self.connection:
            raise AutomationPortalError("Not connected to Automation Portal")
        
        if retries is None:
            retries = config.MAX_RETRIES
        
        for attempt in range(retries + 1):
            try:
                # Prepare command with terminator
                command_str = command + config.COMMAND_TERMINATOR
                
                if self.comm_mode == config.COMM_MODE_SERIAL:
                    # Serial communication
                    self.connection.write(command_str.encode('utf-8'))
                    response = self.connection.readline().decode('utf-8').strip()
                    
                elif self.comm_mode == config.COMM_MODE_TCP:
                    # TCP communication
                    self.connection.send(command_str.encode('utf-8'))
                    response = self.connection.recv(config.DATA_BUFFER_SIZE).decode('utf-8').strip()
                    
                else:
                    raise AutomationPortalError(f"Invalid communication mode: {self.comm_mode}")
                
                self.logger.debug(f"Sent: {command} | Received: {response}")
                return response
                
            except Exception as e:
                if attempt < retries:
                    self.logger.warning(f"Command failed (attempt {attempt + 1}), retrying: {e}")
                    time.sleep(config.RETRY_DELAY)
                else:
                    raise AutomationPortalError(f"Communication error after {retries + 1} attempts: {e}")
        
        return ""
    
    def get_status(self) -> Dict[str, Any]:
        """
        Get the current status of the Automation Portal.
        
        Returns:
            Dictionary containing status information including:
            - system_mode: Current system mode
            - move_cmd: Current movement command
            - move_state: Movement state  
            - drawer_tray_status: Status of drawer and tray
            - door_status: Door position status
            - feeder_status: Feeder status
            - ip_address: Network IP address
            - mac_address: Hardware MAC address
        """
        try:
            response = self._send_command("GetStatus")
            # Parse the response according to the protocol specification
            # Format: Status(seq,system_mode,move_cmd,move_state,drawer_tray_status,door_status,feeder_status,ip_address,mac_address)
            
            status = {}
            if response.startswith("Status("):
                # Extract the content between parentheses
                content = response[7:-1]  # Remove "Status(" and ")"
                parts = content.split(',')
                
                if len(parts) >= 8:
                    status = {
                        'sequence': int(parts[0]) if parts[0].isdigit() else 0,
                        'system_mode': parts[1],
                        'move_cmd': parts[2],
                        'move_state': parts[3],
                        'drawer_tray_status': parts[4],
                        'door_status': parts[5],
                        'feeder_status': parts[6],
                        'ip_address': parts[7] if len(parts) > 7 else '',
                        'mac_address': parts[8] if len(parts) > 8 else ''
                    }
                else:
                    status = {'error': 'Invalid status response format'}
            else:
                status = {'error': f'Unexpected response: {response}'}
                
            return status
            
        except Exception as e:
            self.logger.error(f"Error getting status: {e}")
            return {'error': str(e)}
    
    def initialize(self) -> bool:
        """
        Initialize the Automation Portal system.
        
        Must be called after startup or after an error to switch to operational mode.
        
        Returns:
            True if initialization successful, False otherwise
        """
        try:
            seq = self._get_next_sequence()
            response = self._send_command(f"Initialize({seq})")
            
            # Wait for completion
            while True:
                status_response = self._send_command("GetStatus")
                if "Completed" in status_response or "Error" in status_response:
                    break
                time.sleep(0.5)
            
            success = "Completed" in status_response
            if success:
                self.logger.info("Initialization completed successfully")
            else:
                self.logger.error(f"Initialization failed: {status_response}")
            
            return success
            
        except Exception as e:
            self.logger.error(f"Error during initialization: {e}")
            return False
    
    def extract_drawer(self, tray_position: int) -> bool:
        """
        Extract a drawer from the specified sample manager tray position.
        
        Args:
            tray_position: Tray position (0 or 1)
            
        Returns:
            True if extraction successful, False otherwise
        """
        if tray_position not in [0, 1]:
            raise ValueError("Tray position must be 0 or 1")
        
        try:
            seq = self._get_next_sequence()
            response = self._send_command(f"Extract({seq},{tray_position})")
            
            # Wait for completion
            while True:
                status_response = self._send_command("GetStatus")
                if "Completed" in status_response or "Error" in status_response:
                    break
                time.sleep(0.5)
            
            success = "Completed" in status_response
            if success:
                self.logger.info(f"Drawer extracted successfully from position {tray_position}")
            else:
                self.logger.error(f"Drawer extraction failed: {status_response}")
            
            return success
            
        except Exception as e:
            self.logger.error(f"Error extracting drawer: {e}")
            return False
    
    def insert_drawer(self, tray_position: int) -> bool:
        """
        Insert a drawer into the specified sample manager tray position.
        
        Args:
            tray_position: Tray position (0 or 1)
            
        Returns:
            True if insertion successful, False otherwise
        """
        if tray_position not in [0, 1]:
            raise ValueError("Tray position must be 0 or 1")
        
        try:
            seq = self._get_next_sequence()
            response = self._send_command(f"Insert({seq},{tray_position})")
            
            # Wait for completion
            while True:
                status_response = self._send_command("GetStatus")
                if "Completed" in status_response or "Error" in status_response:
                    break
                time.sleep(0.5)
            
            success = "Completed" in status_response
            if success:
                self.logger.info(f"Drawer inserted successfully to position {tray_position}")
            else:
                self.logger.error(f"Drawer insertion failed: {status_response}")
            
            return success
            
        except Exception as e:
            self.logger.error(f"Error inserting drawer: {e}")
            return False
    
    def report_version(self) -> str:
        """
        Get the system version information.
        
        Returns:
            Version information string
        """
        try:
            response = self._send_command("ReportVersion")
            return response
        except Exception as e:
            self.logger.error(f"Error getting version: {e}")
            return f"Error: {e}"
    
    def reset_system(self) -> bool:
        """
        Reset the Automation Portal system.
        
        Returns:
            True if reset successful, False otherwise
        """
        try:
            response = self._send_command("ResetSystem")
            success = "Completed" in response
            if success:
                self.logger.info("System reset completed")
            else:
                self.logger.error(f"System reset failed: {response}")
            return success
        except Exception as e:
            self.logger.error(f"Error resetting system: {e}")
            return False
    
    def is_drawer_present(self) -> Optional[bool]:
        """
        Check if a drawer is currently present.
        
        Returns:
            True if drawer present, False if not, None if status unknown
        """
        try:
            status = self.get_status()
            drawer_status = status.get('drawer_tray_status', '')
            
            if drawer_status in ['DrawerAndTray', 'DrawerOnly']:
                return True
            elif drawer_status == 'NoDrawerNoTray':
                return False
            else:
                return None
                
        except Exception as e:
            self.logger.error(f"Error checking drawer presence: {e}")
            return None
    
    def is_door_open(self) -> Optional[bool]:
        """
        Check if the door is currently open.
        
        Returns:
            True if door open, False if closed, None if status unknown
        """
        try:
            status = self.get_status()
            door_status = status.get('door_status', '')
            
            if door_status == 'DoorOpened':
                return True
            elif door_status == 'DoorClosed':
                return False
            else:
                return None
                
        except Exception as e:
            self.logger.error(f"Error checking door status: {e}")
            return None
    
    def __enter__(self):
        """Context manager entry."""
        if self.connect():
            return self
        else:
            raise AutomationPortalError("Failed to connect to Automation Portal")
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        """Context manager exit."""
        self.disconnect()


# Example usage functions
def example_sample_transfer():
    """Example of basic sample transfer operations."""
    driver = AutomationPortalDriver(port='COM1')
    
    try:
        # Connect to the system
        if driver.connect():
            print("Connected successfully!")
            
            # Initialize the system
            if driver.initialize():
                print("System initialized")
                
                # Get system status
                status = driver.get_status()
                print(f"System status: {status}")
                
                # Extract drawer from position 0
                if driver.extract_drawer(0):
                    print("Drawer extracted from position 0")
                    
                    # Wait for user to load/unload samples
                    input("Load/unload samples, then press Enter...")
                    
                    # Insert drawer back to position 0
                    if driver.insert_drawer(0):
                        print("Drawer inserted back to position 0")
                
    finally:
        driver.disconnect()


def example_context_manager():
    """Example using context manager for automatic connection handling."""
    with AutomationPortalDriver(port='COM1') as driver:
        # Initialize system
        driver.initialize()
        
        # Check status
        status = driver.get_status()
        print(f"System status: {status}")
        
        # Check if drawer is present
        drawer_present = driver.is_drawer_present()
        print(f"Drawer present: {drawer_present}")


if __name__ == "__main__":
    # Run example
    print("Waters Automation Portal Driver - Sample Transfer Example")
    example_sample_transfer()
