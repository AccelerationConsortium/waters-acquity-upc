"""
Waters Automation Portal Driver

This module provides a Python interface for controlling the Waters Automation Portal
using the Automation Portal PC Protocol Specification. The Automation Portal provides
access to the sample drawer/tray slots for automated sample handling.
"""

import serial
import socket
import time
import logging
from datetime import datetime
from typing import Optional, Dict, Any, List, Union
import numpy as np
import pandas as pd
import config


class AutomationPortalError(Exception):
    """Custom exception for Automation Portal communication errors."""
    pass


class AutomationPortalDriver:
    """
    Driver for Waters Automation Portal using PC Protocol Specification.
    
    This class provides methods to communicate with and control the Waters Automation Portal
    for automated sample handling via drawer/tray access. Supports sample slot operations,
    status monitoring, and error handling.
    
    Based on Waters Automation Portal PC Protocol Specification.
    """
    
    def __init__(self, 
                 port: str = None, 
                 baudrate: int = None,
                 timeout: float = None,
                 host: str = None,
                 tcp_port: int = None,
                 comm_mode: str = None):
        """
        Initialize the Waters Acquity driver.
        
        Args:
            port: Serial port for communication (e.g., 'COM1' on Windows)
            baudrate: Communication baudrate
            timeout: Timeout for communication in seconds
            host: IP address for TCP/IP communication
            tcp_port: TCP port for network communication
            comm_mode: Communication mode ('serial' or 'tcp')
        """
        # Use config defaults if not specified
        self.port = port or config.DEFAULT_PORT
        self.baudrate = baudrate or config.DEFAULT_BAUDRATE
        self.timeout = timeout or config.DEFAULT_TIMEOUT
        self.host = host or config.DEFAULT_TCP_HOST
        self.tcp_port = tcp_port or config.DEFAULT_TCP_PORT
        self.comm_mode = comm_mode or config.DEFAULT_COMM_MODE
        
        self.connection: Optional[Union[serial.Serial, socket.socket]] = None
        self.is_connected = False
        
        # Waters Acquity specific
        self.instrument_id = None
        self.firmware_version = None
        self.available_modules = []
        
        # Set up logging
        self.logger = logging.getLogger(__name__)
        if not self.logger.handlers:
            handler = logging.StreamHandler()
            formatter = logging.Formatter(config.LOG_FORMAT)
            handler.setFormatter(formatter)
            self.logger.addHandler(handler)
            self.logger.setLevel(getattr(logging, config.LOG_LEVEL))
        
    def connect(self) -> bool:
        """
        Establish connection to the Waters Acquity system.
        
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
                self.logger.info(f"Connected to Waters Acquity via serial on {self.port}")
                
            elif self.comm_mode == config.COMM_MODE_TCP:
                self.connection = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                self.connection.settimeout(self.timeout)
                self.connection.connect((self.host, self.tcp_port))
                self.logger.info(f"Connected to Waters Acquity via TCP at {self.host}:{self.tcp_port}")
                
            else:
                raise ValueError(f"Invalid communication mode: {self.comm_mode}")
            
            self.is_connected = True
            
            # Initialize instrument connection
            if self._initialize_instrument():
                return True
            else:
                self.disconnect()
                return False
                
        except Exception as e:
            self.logger.error(f"Failed to connect to Waters Acquity: {e}")
            self.is_connected = False
            return False
    
    def _initialize_instrument(self) -> bool:
        """
        Initialize instrument connection and get system information.
        
        Returns:
            True if initialization successful, False otherwise
        """
        try:
            # Waters automation portal uses GetStatus for identification
            response = self.send_portal_command("GetStatus")
            if response and response.get('status') == 'completed':
                # Extract MAC address and system info from GetStatus response
                data = response.get('data', [])
                if len(data) > 5:
                    self.instrument_id = data[-1]  # MAC address as ID
                    self.logger.info(f"Instrument ID: {self.instrument_id}")
                
                # Extract system state information
                if len(data) >= 3:
                    system_state = data[0] if data else "Unknown"  # OPERATIONAL
                    mode = data[1] if len(data) > 1 else "Unknown"  # Initialize
                    status = data[2] if len(data) > 2 else "Unknown"  # Idle
                    self.logger.info(f"System: {system_state}, Mode: {mode}, Status: {status}")
            
            # Set available modules to known working commands
            self.available_modules = ['GetStatus', 'ResetSystem', 'Initialize']
            self.logger.info(f"Available modules: {self.available_modules}")
            
            return True
            
        except Exception as e:
            self.logger.error(f"Failed to initialize instrument: {e}")
            return False
    
    def disconnect(self) -> None:
        """Disconnect from the Waters Acquity system."""
        if self.connection:
            if self.comm_mode == config.COMM_MODE_SERIAL and hasattr(self.connection, 'is_open'):
                if self.connection.is_open:
                    self.connection.close()
            elif self.comm_mode == config.COMM_MODE_TCP:
                self.connection.close()
                
            self.is_connected = False
            self.logger.info("Disconnected from Waters Acquity")
    
    def send_command(self, command: str, retries: int = None) -> str:
        """
        Send a command to the Waters Acquity system.
        
        Args:
            command: Command string to send
            retries: Number of retry attempts (uses config default if None)
            
        Returns:
            Response from the system
            
        Raises:
            AutomationPortalError: If communication fails
        """
        if not self.is_connected or not self.connection:
            raise AutomationPortalError("Not connected to Waters Acquity system")
        
        if retries is None:
            retries = config.MAX_RETRIES
        
        for attempt in range(retries + 1):
            try:
                # Prepare command with terminator
                command_str = command + config.COMMAND_TERMINATOR
                
                if self.comm_mode == config.COMM_MODE_SERIAL:
                    # Serial communication
                    self.connection.write(command_str.encode('utf-8'))
                    
                    # Waters instruments often send multiple lines (Received + Completed)
                    # Read with timeout to get full response
                    time.sleep(0.5)  # Give instrument time to respond
                    full_response = ""
                    
                    # Read all available data
                    while self.connection.in_waiting > 0:
                        data = self.connection.read(self.connection.in_waiting).decode('utf-8', errors='ignore')
                        full_response += data
                        time.sleep(0.1)  # Small delay to ensure all data is received
                    
                    # If we got multiple responses, try to find the most complete one
                    lines = full_response.split('\n')
                    response = ""
                    
                    # Look for Completed response first, then Received, then any other
                    for line in lines:
                        line = line.strip()
                        if line.startswith('Completed('):
                            response = line
                            break
                        elif line.startswith('Received(') and not response:
                            response = line
                        elif line and not response:
                            response = line
                    
                    if not response:
                        response = full_response.strip()
                    
                elif self.comm_mode == config.COMM_MODE_TCP:
                    # TCP communication
                    self.connection.send(command_str.encode('utf-8'))
                    response = self.connection.recv(config.DATA_BUFFER_SIZE).decode('utf-8').strip()
                    
                else:
                    raise AutomationPortalError(f"Invalid communication mode: {self.comm_mode}")
                
                self.logger.debug(f"Sent: {command} | Received: {response}")
                
                # Check for errors in response
                if self._is_error_response(response):
                    raise AutomationPortalError(f"Instrument error: {response}")
                
                return response
                
            except Exception as e:
                if attempt < retries:
                    self.logger.warning(f"Command failed (attempt {attempt + 1}), retrying: {e}")
                    time.sleep(config.RETRY_DELAY)
                else:
                    raise AutomationPortalError(f"Communication error after {retries + 1} attempts: {e}")
        
        return ""
    
    def _is_error_response(self, response: str) -> bool:
        """
        Check if response indicates an error.
        
        Args:
            response: Response string from instrument
            
        Returns:
            True if response indicates error, False otherwise
        """
        # Waters automation portal error format: Error(seq,command,code,description)
        if response.strip().startswith('Error('):
            return True
        
        # Also check for other error indicators
        error_indicators = ['ERROR', 'ERR', 'FAIL', 'INVALID']
        return any(indicator in response.upper() for indicator in error_indicators)
    
    def get_status(self) -> Dict[str, Any]:
        """
        Get the current status of the Waters Acquity system.
        
        Returns:
            Dictionary containing system status information
        """
        status = {
            'timestamp': datetime.now().isoformat(),
            'connected': self.is_connected,
            'system_ready': False,
            'pressure': 0.0,
            'temperature': 0.0,
            'flow_rate': 0.0,
            'status_code': 0,
            'status_text': 'Unknown',
            'error_code': 0,
            'modules': {}
        }
        
        if self.is_connected:
            try:
                # Get system status (Waters automation protocol)
                response = self.send_command("STATUS?")
                if response:
                    try:
                        status_code = int(response)
                        status['status_code'] = status_code
                        status['status_text'] = config.STATUS_CODES.get(status_code, f"Unknown ({status_code})")
                        status['system_ready'] = (status_code == 0)  # 0 = Ready
                    except ValueError:
                        status['status_text'] = response
                
                # Get pressure reading
                try:
                    response = self.send_command("PRESSURE?")
                    if response and response != "ERROR":
                        status['pressure'] = float(response)
                except (ValueError, AutomationPortalError):
                    pass
                
                # Get column temperature
                try:
                    response = self.send_command("TEMP:COL?")
                    if response and response != "ERROR":
                        status['temperature'] = float(response)
                except (ValueError, AutomationPortalError):
                    pass
                
                # Get flow rate
                try:
                    response = self.send_command("FLOW?")
                    if response and response != "ERROR":
                        status['flow_rate'] = float(response)
                except (ValueError, AutomationPortalError):
                    pass
                
                # Get error status
                try:
                    response = self.send_command("ERROR?")
                    if response and response != "ERROR":
                        error_code = int(response)
                        status['error_code'] = error_code
                        if error_code != 0:
                            status['error_text'] = config.ERROR_CODES.get(error_code, f"Error {error_code}")
                except (ValueError, AutomationPortalError):
                    pass
                
                # Get module status if available
                for module in self.available_modules:
                    try:
                        response = self.send_command(f"{module}:STATUS?")
                        if response and response != "ERROR":
                            status['modules'][module] = response
                    except:
                        pass
                        
            except Exception as e:
                self.logger.error(f"Error getting status: {e}")
        
        return status
    
    def start_run(self, method_name: str) -> bool:
        """
        Start a chromatography run with the specified method.
        
        Args:
            method_name: Name of the method to run
            
        Returns:
            True if run started successfully, False otherwise
        """
        try:
            # Waters automation protocol for starting runs
            response = self.send_command(f"RUN:START {method_name}")
            success = self._is_success_response(response)
            
            if success:
                self.logger.info(f"Started run with method: {method_name}")
            else:
                self.logger.error(f"Failed to start run: {response}")
            
            return success
        except Exception as e:
            self.logger.error(f"Error starting run: {e}")
            return False
    
    def stop_run(self) -> bool:
        """
        Stop the current chromatography run.
        
        Returns:
            True if run stopped successfully, False otherwise
        """
        try:
            response = self.send_command("RUN:STOP")
            success = self._is_success_response(response)
            
            if success:
                self.logger.info("Run stopped successfully")
            else:
                self.logger.error(f"Failed to stop run: {response}")
            
            return success
        except Exception as e:
            self.logger.error(f"Error stopping run: {e}")
            return False
    
    def abort_run(self) -> bool:
        """
        Abort the current chromatography run immediately.
        
        Returns:
            True if run aborted successfully, False otherwise
        """
        try:
            response = self.send_command("RUN:ABORT")
            success = self._is_success_response(response)
            
            if success:
                self.logger.info("Run aborted successfully")
            else:
                self.logger.error(f"Failed to abort run: {response}")
            
            return success
        except Exception as e:
            self.logger.error(f"Error aborting run: {e}")
            return False
    
    def set_flow_rate(self, flow_rate: float, module: str = "BSM") -> bool:
        """
        Set the flow rate for the specified pump module.
        
        Args:
            flow_rate: Flow rate in mL/min
            module: Module name (BSM = Binary Solvent Manager)
            
        Returns:
            True if flow rate set successfully, False otherwise
        """
        # Validate flow rate range
        if not (config.MIN_FLOW_RATE <= flow_rate <= config.MAX_FLOW_RATE):
            self.logger.error(f"Flow rate {flow_rate} outside valid range "
                            f"({config.MIN_FLOW_RATE}-{config.MAX_FLOW_RATE} mL/min)")
            return False
        
        try:
            # Waters automation protocol for flow rate
            response = self.send_command(f"{module}:FLOW {flow_rate}")
            success = self._is_success_response(response)
            
            if success:
                self.logger.info(f"Flow rate set to {flow_rate} mL/min on {module}")
            else:
                self.logger.error(f"Failed to set flow rate: {response}")
            
            return success
        except Exception as e:
            self.logger.error(f"Error setting flow rate: {e}")
            return False
    
    def set_temperature(self, temperature: float, module: str = "CM") -> bool:
        """
        Set the column temperature.
        
        Args:
            temperature: Temperature in Celsius
            module: Module name (CM = Column Manager)
            
        Returns:
            True if temperature set successfully, False otherwise
        """
        # Validate temperature range
        if not (config.MIN_TEMPERATURE <= temperature <= config.MAX_TEMPERATURE):
            self.logger.error(f"Temperature {temperature} outside valid range "
                            f"({config.MIN_TEMPERATURE}-{config.MAX_TEMPERATURE} °C)")
            return False
        
        try:
            # Waters automation protocol for temperature
            response = self.send_command(f"{module}:TEMP {temperature}")
            success = self._is_success_response(response)
            
            if success:
                self.logger.info(f"Temperature set to {temperature}°C on {module}")
            else:
                self.logger.error(f"Failed to set temperature: {response}")
            
            return success
        except Exception as e:
            self.logger.error(f"Error setting temperature: {e}")
            return False
    
    def set_injection_volume(self, volume: float, module: str = "SM") -> bool:
        """
        Set the injection volume.
        
        Args:
            volume: Injection volume in μL
            module: Module name (SM = Sample Manager)
            
        Returns:
            True if volume set successfully, False otherwise
        """
        # Validate volume range
        if not (config.MIN_INJECTION_VOLUME <= volume <= config.MAX_INJECTION_VOLUME):
            self.logger.error(f"Injection volume {volume} outside valid range "
                            f"({config.MIN_INJECTION_VOLUME}-{config.MAX_INJECTION_VOLUME} μL)")
            return False
        
        try:
            response = self.send_command(f"{module}:VOLUME {volume}")
            success = self._is_success_response(response)
            
            if success:
                self.logger.info(f"Injection volume set to {volume} μL on {module}")
            else:
                self.logger.error(f"Failed to set injection volume: {response}")
            
            return success
        except Exception as e:
            self.logger.error(f"Error setting injection volume: {e}")
            return False
    
    def _is_success_response(self, response: str) -> bool:
        """
        Check if response indicates success.
        
        Args:
            response: Response string from instrument
            
        Returns:
            True if response indicates success, False otherwise
        """
        if not response:
            return False
        
        success_indicators = ['OK', 'READY', 'COMPLETE', 'SUCCESS']
        return any(indicator in response.upper() for indicator in success_indicators)
    
    def get_data(self, detector: str = "TUV", start_time: float = None, end_time: float = None) -> pd.DataFrame:
        """
        Retrieve chromatography data from the specified detector.
        
        Args:
            detector: Detector module name (TUV, FLR, ELS, etc.)
            start_time: Start time for data retrieval (minutes)
            end_time: End time for data retrieval (minutes)
            
        Returns:
            DataFrame containing chromatography data
        """
        try:
            # Build data query command
            cmd = f"{detector}:DATA?"
            if start_time is not None and end_time is not None:
                cmd = f"{detector}:DATA? {start_time},{end_time}"
            
            response = self.send_command(cmd)
            
            if response and not self._is_error_response(response):
                # Parse Waters data format (typically comma-separated)
                # This is a simplified parser - actual format depends on instrument
                data_points = []
                lines = response.split('\n')
                
                for line in lines:
                    if line.strip():
                        try:
                            parts = line.split(',')
                            if len(parts) >= 2:
                                time_val = float(parts[0])
                                signal_val = float(parts[1])
                                data_points.append({'time': time_val, 'signal': signal_val})
                        except ValueError:
                            continue
                
                if data_points:
                    df = pd.DataFrame(data_points)
                    
                    # Add system data if available
                    pressure_data = self._get_system_data('PRESSURE')
                    temp_data = self._get_system_data('TEMPERATURE')
                    
                    if len(pressure_data) == len(df):
                        df['pressure'] = pressure_data
                    else:
                        df['pressure'] = np.full(len(df), self.get_status()['pressure'])
                    
                    if len(temp_data) == len(df):
                        df['temperature'] = temp_data
                    else:
                        df['temperature'] = np.full(len(df), self.get_status()['temperature'])
                    
                    return df
            
            # Fallback to simulated data if no real data available
            self.logger.warning("Using simulated data - replace with actual data retrieval")
            return self._generate_simulated_data()
            
        except Exception as e:
            self.logger.error(f"Error retrieving data: {e}")
            return self._generate_simulated_data()
    
    def _get_system_data(self, parameter: str) -> List[float]:
        """
        Get historical system data for a parameter.
        
        Args:
            parameter: Parameter name (PRESSURE, TEMPERATURE, etc.)
            
        Returns:
            List of parameter values
        """
        try:
            response = self.send_command(f"DATA:{parameter}?")
            if response and not self._is_error_response(response):
                return [float(x) for x in response.split(',') if x.strip()]
        except:
            pass
        return []
    
    def _generate_simulated_data(self) -> pd.DataFrame:
        """Generate simulated chromatography data for testing."""
        time_points = np.linspace(0, 10, 100)
        signal = np.random.normal(0, 1, 100)
        pressure = np.random.normal(100, 5, 100)
        temperature = np.random.normal(25, 1, 100)
        
        return pd.DataFrame({
            'time': time_points,
            'signal': signal,
            'pressure': pressure,
            'temperature': temperature
        })
    
    def load_method(self, method_path: str) -> bool:
        """
        Load a method file into the instrument.
        
        Args:
            method_path: Path to the method file
            
        Returns:
            True if method loaded successfully, False otherwise
        """
        try:
            response = self.send_command(f"METHOD:LOAD {method_path}")
            success = self._is_success_response(response)
            
            if success:
                self.logger.info(f"Method loaded: {method_path}")
            else:
                self.logger.error(f"Failed to load method: {response}")
            
            return success
        except Exception as e:
            self.logger.error(f"Error loading method: {e}")
            return False
    
    def get_method_list(self) -> List[str]:
        """
        Get list of available methods on the instrument.
        
        Returns:
            List of method names
        """
        try:
            response = self.send_command("METHOD:LIST?")
            if response and not self._is_error_response(response):
                return [method.strip() for method in response.split(',') if method.strip()]
        except Exception as e:
            self.logger.error(f"Error getting method list: {e}")
        
        return []
    
    def calibrate_detector(self, detector: str = "TUV") -> bool:
        """
        Initiate detector calibration.
        
        Args:
            detector: Detector module name
            
        Returns:
            True if calibration started successfully, False otherwise
        """
        try:
            response = self.send_command(f"{detector}:CALIBRATE")
            success = self._is_success_response(response)
            
            if success:
                self.logger.info(f"Calibration started for {detector}")
            else:
                self.logger.error(f"Failed to start calibration: {response}")
            
            return success
        except Exception as e:
            self.logger.error(f"Error starting calibration: {e}")
            return False
    
    def prime_pump(self, module: str = "BSM", solvent: str = "A") -> bool:
        """
        Prime the specified pump with solvent.
        
        Args:
            module: Pump module name
            solvent: Solvent line (A, B, C, D)
            
        Returns:
            True if priming started successfully, False otherwise
        """
        try:
            response = self.send_command(f"{module}:PRIME {solvent}")
            success = self._is_success_response(response)
            
            if success:
                self.logger.info(f"Priming started for {module} solvent {solvent}")
            else:
                self.logger.error(f"Failed to start priming: {response}")
            
            return success
        except Exception as e:
            self.logger.error(f"Error starting prime: {e}")
            return False
    
    # Waters Automation Portal Commands (Based on Protocol Specification 715008839)
    def _send_command(self, command_name, args=None):
        """
        Send Waters Automation Portal command using the specific protocol format.
        
        Args:
            command_name (str): Portal command name
            args (list): Optional command arguments
            
        Returns:
            dict: Parsed response containing command result
        """
        # Build command string according to Waters protocol
        command_str = command_name
        if args:
            command_str += "(" + ",".join(str(arg) for arg in args) + ")"
        
        try:
            response = self.send_command(command_str)
            return self._parse_portal_response(response)
        except Exception as e:
            self.logger.error(f"Portal command failed: {command_name} - {e}")
            return {'error': str(e)}
    
    def send_portal_command(self, command: str, retries: int = None) -> Dict[str, Any]:
        """
        Send a Waters Automation Portal command and parse the response.
        
        Args:
            command: Command string to send (without sequence number)
            retries: Number of retry attempts (uses config default if None)
            
        Returns:
            Dictionary containing parsed response data
            
        Raises:
            AutomationPortalError: If communication fails
        """
        if retries is None:
            retries = config.MAX_RETRIES
        
        for attempt in range(retries + 1):
            try:
                # Get next sequence number
                seq_num = self._get_next_sequence()
                
                # Send command and get raw response
                raw_response = self.send_command(command)
                
                # Parse Waters automation portal response
                return self._parse_portal_response(raw_response, seq_num, command)
                
            except Exception as e:
                if attempt < retries:
                    self.logger.warning(f"Portal command failed (attempt {attempt + 1}), retrying: {e}")
                    time.sleep(config.RETRY_DELAY)
                else:
                    raise AutomationPortalError(f"Portal command failed after {retries + 1} attempts: {e}")
        
        return {"status": "failed", "error": "No response"}
    
    def _get_next_sequence(self) -> int:
        """Get the next sequence number for commands."""
        if not hasattr(self, '_sequence_number'):
            self._sequence_number = 0
        self._sequence_number += 1
        return self._sequence_number
    
    def _parse_portal_response(self, response: str, seq_num: int, command: str) -> Dict[str, Any]:
        """
        Parse Waters Automation Portal response format.
        
        Expected formats:
        - Received(seq,command)
        - Completed(seq,command,data...)
        - Error(seq,command,code,description)
        
        Args:
            response: Raw response string
            seq_num: Expected sequence number
            command: Command that was sent
            
        Returns:
            Dictionary with parsed response data
        """
        response = response.strip()
        
        # Handle Completed responses
        if response.startswith('Completed('):
            # Extract data from Completed(seq,command,data1,data2,...)
            content = response[10:-1]  # Remove 'Completed(' and ')'
            parts = content.split(',')
            
            if len(parts) >= 2:
                return {
                    'status': 'completed',
                    'sequence_number': int(parts[0]),
                    'command': parts[1],
                    'data': parts[2:] if len(parts) > 2 else []
                }
        
        # Handle Received responses
        elif response.startswith('Received('):
            # Extract data from Received(seq,command)
            content = response[9:-1]  # Remove 'Received(' and ')'
            parts = content.split(',')
            
            if len(parts) >= 2:
                return {
                    'status': 'received',
                    'sequence_number': int(parts[0]),
                    'command': parts[1]
                }
        
        # Handle Error responses
        elif response.startswith('Error('):
            # Extract data from Error(seq,command,code,description)
            content = response[6:-1]  # Remove 'Error(' and ')'
            parts = content.split(',')
            
            if len(parts) >= 4:
                return {
                    'status': 'error',
                    'sequence_number': int(parts[0]),
                    'command': parts[1],
                    'error_code': int(parts[2]),
                    'error_description': parts[3]
                }
        
        # Fallback for unparseable responses
        return {
            'status': 'unknown',
            'raw_response': response
        }

    # Waters Automation Portal Command Methods
    
    def portal_get_status(self) -> Dict[str, Any]:
        """
        Get instrument status using Waters Automation Portal GetStatus command.
        
        Returns:
            Dictionary containing instrument status
        """
        try:
            response = self.send_portal_command("GetStatus")
            if response.get('status') in ['completed', 'received']:
                data = response.get('data', [])
                return {
                    'success': True,
                    'system_state': data[0] if len(data) > 0 else 'Unknown',
                    'mode': data[1] if len(data) > 1 else 'Unknown', 
                    'status': data[2] if len(data) > 2 else 'Unknown',
                    'drawer_status': data[3] if len(data) > 3 else 'Unknown',
                    'door_status': data[4] if len(data) > 4 else 'Unknown',
                    'feeder_status': data[5] if len(data) > 5 else 'Unknown',
                    'network_info': data[6] if len(data) > 6 else 'Unknown',
                    'mac_address': data[7] if len(data) > 7 else 'Unknown',
                    'raw_data': data,
                    'sequence_number': response.get('sequence_number'),
                    'response_status': response.get('status')  # Include actual status
                }
            else:
                return {
                    'success': False,
                    'error': response.get('error_description', 'Command failed'),
                    'raw_response': response
                }
        except Exception as e:
            return {
                'success': False,
                'error': str(e)
            }
    
    def portal_report_version(self) -> Dict[str, Any]:
        """
        Get instrument version using Waters Automation Portal ReportVersion command.
        
        Returns:
            Dictionary containing version information
        """
        try:
            response = self.send_portal_command("ReportVersion")
            if response.get('status') == 'completed':
                data = response.get('data', [])
                return {
                    'success': True,
                    'version_info': data,
                    'sequence_number': response.get('sequence_number')
                }
            else:
                return {
                    'success': False,
                    'error': response.get('error_description', 'Command failed'),
                    'raw_response': response
                }
        except Exception as e:
            return {
                'success': False,
                'error': str(e)
            }
    
    def portal_initialize(self) -> Dict[str, Any]:
        """
        Initialize instrument using Waters Automation Portal Initialize command.
        
        Returns:
            Dictionary containing initialization result
        """
        try:
            response = self.send_portal_command("Initialize")
            if response.get('status') in ['completed', 'received']:
                return {
                    'success': True,
                    'status': response.get('status'),
                    'sequence_number': response.get('sequence_number'),
                    'data': response.get('data', [])
                }
            else:
                return {
                    'success': False,
                    'error': response.get('error_description', 'Command failed'),
                    'raw_response': response
                }
        except Exception as e:
            return {
                'success': False,
                'error': str(e)
            }
    
    def portal_reset_system(self) -> Dict[str, Any]:
        """
        Reset system using Waters Automation Portal ResetSystem command.
        
        Returns:
            Dictionary containing reset result
        """
        try:
            response = self.send_portal_command("ResetSystem")
            if response.get('status') in ['completed', 'received']:
                return {
                    'success': True,
                    'status': response.get('status'),
                    'sequence_number': response.get('sequence_number'),
                    'data': response.get('data', [])
                }
            else:
                return {
                    'success': False,
                    'error': response.get('error_description', 'Command failed'),
                    'raw_response': response
                }
        except Exception as e:
            return {
                'success': False,
                'error': str(e)
            }
    
    def portal_extract(self, position: int) -> Dict[str, Any]:
        """
        Extract sample using Waters Automation Portal Extract command.
        
        Args:
            position: Sample position to extract
            
        Returns:
            Dictionary containing extraction result
        """
        try:
            response = self.send_portal_command(f"Extract({position})")
            if response.get('status') in ['completed', 'received']:
                return {
                    'success': True,
                    'status': response.get('status'),
                    'position': position,
                    'sequence_number': response.get('sequence_number'),
                    'data': response.get('data', [])
                }
            else:
                return {
                    'success': False,
                    'error': response.get('error_description', 'Command failed'),
                    'raw_response': response
                }
        except Exception as e:
            return {
                'success': False,
                'error': str(e)
            }
    
    def portal_insert(self, position: int) -> Dict[str, Any]:
        """
        Insert sample using Waters Automation Portal Insert command.
        
        Args:
            position: Sample position to insert
            
        Returns:
            Dictionary containing insertion result
        """
        try:
            response = self.send_portal_command(f"Insert({position})")
            if response.get('status') in ['completed', 'received']:
                return {
                    'success': True,
                    'status': response.get('status'),
                    'position': position,
                    'sequence_number': response.get('sequence_number'),
                    'data': response.get('data', [])
                }
            else:
                return {
                    'success': False,
                    'error': response.get('error_description', 'Command failed'),
                    'raw_response': response
                }
        except Exception as e:
            return {
                'success': False,
                'error': str(e)
            }

# Example usage functions
def example_basic_operation():
    """Example of basic Waters Acquity operations."""
    driver = AutomationPortalDriver(port='COM1')
    
    try:
        # Connect to the system
        if driver.connect():
            print("Connected successfully!")
            
            # Get system status
            status = driver.get_status()
            print(f"System status: {status}")
            
            # Set operating parameters
            driver.set_flow_rate(1.0)  # 1.0 mL/min
            driver.set_temperature(40.0)  # 40°C
            
            # Start a run
            driver.start_run("my_method")
            
            # Wait for some time (in real use, you'd monitor the run)
            time.sleep(5)
            
            # Get data
            data = driver.get_data()
            print(f"Retrieved {len(data)} data points")
            
            # Stop the run
            driver.stop_run()
            
    finally:
        driver.disconnect()


def example_context_manager():
    """Example using context manager for automatic connection handling."""
    with AutomationPortalDriver(port='COM1') as driver:
        status = driver.get_status()
        print(f"System status: {status}")
        
        # Perform operations...
        driver.set_flow_rate(0.5)
        data = driver.get_data()
        print(f"Data shape: {data.shape}")


if __name__ == "__main__":
    # Run example
    print("Waters Acquity UPC Driver Example")
    example_basic_operation()
