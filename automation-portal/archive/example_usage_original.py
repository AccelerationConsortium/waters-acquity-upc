"""
Example usage of the Waters Acquity UPC Driver

This script demonstrates various ways to use the driver to control
the Waters Acquity system.
"""

import time
import sys
import os

# Add the current directory to the path so we can import our driver
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from automation_portal_driver import AutomationPortalDriver, AutomationPortalError
import config


def basic_connection_test():
    """Test basic connection to the Waters Acquity system."""
    print("=== Basic Connection Test ===")
    
    driver = AutomationPortalDriver(
        port=config.DEFAULT_PORT,
        baudrate=config.DEFAULT_BAUDRATE,
        timeout=config.DEFAULT_TIMEOUT
    )
    
    try:
        if driver.connect():
            print("✓ Connection successful!")
            
            # Get and display status
            status = driver.get_status()
            print(f"✓ System status retrieved: {status}")
            
        else:
            print("✗ Connection failed!")
            
    except Exception as e:
        print(f"✗ Error during connection test: {e}")
    finally:
        driver.disconnect()
        print("✓ Disconnected")


def parameter_setting_test():
    """Test setting various system parameters."""
    print("\n=== Parameter Setting Test ===")
    
    with AutomationPortalDriver() as driver:
        try:
            # Test flow rate setting
            flow_rates = [0.5, 1.0, 1.5, 2.0]
            for flow_rate in flow_rates:
                if config.MIN_FLOW_RATE <= flow_rate <= config.MAX_FLOW_RATE:
                    success = driver.set_flow_rate(flow_rate)
                    status = "✓" if success else "✗"
                    print(f"{status} Set flow rate to {flow_rate} mL/min")
                    time.sleep(1)
            
            # Test temperature setting
            temperatures = [25.0, 40.0, 60.0]
            for temp in temperatures:
                if config.MIN_TEMPERATURE <= temp <= config.MAX_TEMPERATURE:
                    success = driver.set_temperature(temp)
                    status = "✓" if success else "✗"
                    print(f"{status} Set temperature to {temp}°C")
                    time.sleep(1)
                    
        except AutomationPortalError as e:
            print(f"✗ Waters Acquity error: {e}")
        except Exception as e:
            print(f"✗ Unexpected error: {e}")


def data_collection_test():
    """Test data collection from the system."""
    print("\n=== Data Collection Test ===")
    
    with AutomationPortalDriver() as driver:
        try:
            # Collect some data
            data = driver.get_data()
            print(f"✓ Collected data with shape: {data.shape}")
            print(f"✓ Data columns: {list(data.columns)}")
            
            # Display some statistics
            print("\nData Statistics:")
            print(data.describe())
            
        except Exception as e:
            print(f"✗ Error during data collection: {e}")


def method_run_simulation():
    """Simulate running a chromatography method."""
    print("\n=== Method Run Simulation ===")
    
    with AutomationPortalDriver() as driver:
        try:
            # Set up initial conditions
            print("Setting up initial conditions...")
            driver.set_flow_rate(config.DEFAULT_FLOW_RATE)
            driver.set_temperature(config.DEFAULT_TEMPERATURE)
            
            # Start a method run
            method_name = "example_method"
            if driver.start_run(method_name):
                print(f"✓ Started method run: {method_name}")
                
                # Simulate monitoring the run
                print("Monitoring run progress...")
                for i in range(5):
                    time.sleep(2)
                    status = driver.get_status()
                    print(f"  Step {i+1}: {status}")
                
                # Stop the run
                if driver.stop_run():
                    print("✓ Method run stopped successfully")
                else:
                    print("✗ Failed to stop method run")
            else:
                print("✗ Failed to start method run")
                
        except Exception as e:
            print(f"✗ Error during method run: {e}")


def error_handling_demo():
    """Demonstrate error handling capabilities."""
    print("\n=== Error Handling Demo ===")
    
    # Test with invalid port
    driver = AutomationPortalDriver(port='INVALID_PORT')
    
    try:
        driver.connect()
        print("✗ Should have failed to connect to invalid port")
    except:
        print("✓ Correctly handled invalid port connection")
    
    # Test command without connection
    try:
        driver.send_command("TEST")
        print("✗ Should have failed to send command without connection")
    except AutomationPortalError:
        print("✓ Correctly handled command without connection")
    
    # Test invalid parameter ranges
    with AutomationPortalDriver() as driver:
        # Try setting flow rate outside valid range
        invalid_flow = config.MAX_FLOW_RATE + 1.0
        try:
            # Note: In a real implementation, this should validate the range
            result = driver.set_flow_rate(invalid_flow)
            print(f"Flow rate setting result for {invalid_flow}: {result}")
        except Exception as e:
            print(f"✓ Correctly handled invalid flow rate: {e}")


def advanced_features_demo():
    """Demonstrate advanced Waters Acquity features."""
    print("\n=== Advanced Features Demo ===")
    
    # Test both communication modes
    for comm_mode in ['serial', 'tcp']:
        print(f"\n--- Testing {comm_mode.upper()} Communication ---")
        
        try:
            if comm_mode == 'tcp':
                driver = AutomationPortalDriver(
                    host='192.168.1.100',  # Replace with actual IP
                    tcp_port=34567,
                    comm_mode='tcp'
                )
            else:
                driver = AutomationPortalDriver(comm_mode='serial')
            
            with driver:
                # Get instrument information
                print(f"✓ Instrument ID: {driver.instrument_id}")
                print(f"✓ Firmware Version: {driver.firmware_version}")
                print(f"✓ Available Modules: {driver.available_modules}")
                
                # Advanced parameter setting with modules
                success = driver.set_flow_rate(1.5, module="BSM")
                print(f"{'✓' if success else '✗'} Set BSM flow rate to 1.5 mL/min")
                
                success = driver.set_temperature(45.0, module="CM")
                print(f"{'✓' if success else '✗'} Set CM temperature to 45°C")
                
                success = driver.set_injection_volume(10.0, module="SM")
                print(f"{'✓' if success else '✗'} Set SM injection volume to 10.0 μL")
                
                # Method management
                methods = driver.get_method_list()
                print(f"✓ Available methods: {methods}")
                
                if methods:
                    success = driver.load_method(methods[0])
                    print(f"{'✓' if success else '✗'} Loaded method: {methods[0]}")
                
                # Detector operations
                success = driver.calibrate_detector("TUV")
                print(f"{'✓' if success else '✗'} Started TUV calibration")
                
                # Pump operations
                success = driver.prime_pump("BSM", "A")
                print(f"{'✓' if success else '✗'} Started BSM priming for solvent A")
                
                # Data collection with specific detector
                data = driver.get_data("TUV", start_time=0.0, end_time=5.0)
                print(f"✓ Collected TUV data: {data.shape}")
                
                # Advanced status with modules
                status = driver.get_status()
                print(f"✓ System Status: {status['status_text']}")
                if status['modules']:
                    print(f"✓ Module Status: {status['modules']}")
                
        except Exception as e:
            print(f"✗ Error with {comm_mode} communication: {e}")
            if comm_mode == 'tcp':
                print("  Note: TCP connection requires actual instrument IP address")


def portal_connection_test():
    """Test connection to the Waters Automation Portal."""
    print("=== Waters Automation Portal Connection Test ===")
    
    driver = AutomationPortalDriver(
        port=config.DEFAULT_PORT,
        baudrate=config.DEFAULT_BAUDRATE,
        timeout=config.DEFAULT_TIMEOUT
    )
    
    try:
        if driver.connect():
            print("✓ Connection successful!")
            print(f"  Port: {driver.port}")
            print(f"  Baudrate: {driver.baudrate}")
            
            # Test Waters Automation Portal commands
            print("\nTesting Waters Automation Portal commands...")
            
            # Test GetStatus command
            try:
                print("Testing GetStatus command...")
                status_result = driver.portal_get_status()
                print(f"✓ GetStatus response: {status_result}")
            except Exception as e:
                print(f"✗ GetStatus failed: {e}")
            
            # Test ReportVersion command
            try:
                print("Testing ReportVersion command...")
                version_result = driver.portal_report_version()
                print(f"✓ ReportVersion response: {version_result}")
            except Exception as e:
                print(f"✗ ReportVersion failed: {e}")
            
            # Test Initialize command
            try:
                print("Testing Initialize command...")
                init_result = driver.portal_initialize()
                print(f"✓ Initialize response: {init_result}")
            except Exception as e:
                print(f"✗ Initialize failed: {e}")
                
        else:
            print("✗ Connection failed!")
            
    except Exception as e:
        print(f"✗ Error during portal connection test: {e}")
    finally:
        driver.disconnect()
        print("✓ Disconnected")


def instrument_identification_test():
    """Test to identify what type of Waters instrument this is."""
    print("=== Waters Instrument Identification Test ===")
    
    driver = AutomationPortalDriver(
        port=config.DEFAULT_PORT,
        baudrate=config.DEFAULT_BAUDRATE,
        timeout=config.DEFAULT_TIMEOUT
    )
    
    try:
        if driver.connect():
            print("✓ Connection successful!")
            print(f"  Port: {driver.port}")
            print(f"  Baudrate: {driver.baudrate}")
            
            # Try different command formats to identify the instrument
            test_commands = [
                "?",           # Simple query
                "HELP",        # Help command
                "VER",         # Version (short)
                "VERSION",     # Version (long)
                "STATUS",      # Status
                "INFO",        # Information
                "ID",          # ID
                "IDENT",       # Identification
                "*IDN?",       # SCPI identification
                "*VER?",       # SCPI version
                "GetStatus",   # Portal GetStatus
                "ReportVersion", # Portal ReportVersion
            ]
            
            print("\nTesting different command formats...")
            for cmd in test_commands:
                try:
                    print(f"\nTesting command: '{cmd}'")
                    response = driver.send_command(cmd)
                    print(f"  Response: '{response}'")
                except Exception as e:
                    print(f"  Error: {e}")
                    
                # Small delay between commands
                time.sleep(0.5)
                
        else:
            print("✗ Connection failed!")
            
    except Exception as e:
        print(f"✗ Error during identification test: {e}")
    finally:
        driver.disconnect()
        print("✓ Disconnected")


def waters_protocol_test():
    """Test Waters protocol with proper sequence numbers."""
    print("=== Waters Protocol with Sequence Numbers Test ===")
    
    driver = AutomationPortalDriver(
        port=config.DEFAULT_PORT,
        baudrate=config.DEFAULT_BAUDRATE,
        timeout=config.DEFAULT_TIMEOUT
    )
    
    try:
        if driver.connect():
            print("✓ Connection successful!")
            
            # The instrument seems to expect commands with sequence numbers
            # Let's try the proper Waters format: Command(seq_num, ...)
            test_commands = [
                "GetStatus(1)",      # GetStatus with sequence 1
                "ReportVersion(2)",  # ReportVersion with sequence 2  
                "Initialize(3)",     # Initialize with sequence 3
            ]
            
            print("\nTesting Waters protocol commands with sequence numbers...")
            for cmd in test_commands:
                try:
                    print(f"\nSending command: '{cmd}'")
                    # Send raw command without driver processing
                    if driver.comm_mode == config.COMM_MODE_SERIAL:
                        command_str = cmd + config.COMMAND_TERMINATOR
                        driver.connection.write(command_str.encode('utf-8'))
                        response = driver.connection.readline().decode('utf-8').strip()
                        print(f"  Raw response: '{response}'")
                    
                except Exception as e:
                    print(f"  Error: {e}")
                
                time.sleep(1)  # Wait between commands
            
            # Also try without parentheses
            print("\nTesting simple commands...")
            simple_commands = ["GetStatus", "ReportVersion", "Initialize"]
            for cmd in simple_commands:
                try:
                    print(f"\nSending simple command: '{cmd}'")
                    if driver.comm_mode == config.COMM_MODE_SERIAL:
                        command_str = cmd + config.COMMAND_TERMINATOR
                        driver.connection.write(command_str.encode('utf-8'))
                        response = driver.connection.readline().decode('utf-8').strip()
                        print(f"  Raw response: '{response}'")
                        
                except Exception as e:
                    print(f"  Error: {e}")
                
                time.sleep(1)
                
        else:
            print("✗ Connection failed!")
            
    except Exception as e:
        print(f"✗ Error during protocol test: {e}")
    finally:
        driver.disconnect()
        print("✓ Disconnected")


def waters_compliant_test():
    """Test using the exact Waters protocol format from the qualification script."""
    print("=== Waters Compliant Protocol Test ===")
    
    driver = AutomationPortalDriver(
        port=config.DEFAULT_PORT,
        baudrate=config.DEFAULT_BAUDRATE,
        timeout=config.DEFAULT_TIMEOUT
    )
    
    try:
        if driver.connect():
            print("✓ Connection successful!")
            print(f"  Port: {driver.port}")
            print(f"  Baudrate: {driver.baudrate}")
            
            # Test the Waters commands in order from the qualification script
            print("\nTesting Waters Automation Portal commands...")
            
            # 1. Test ReportVersion (like the qualification script)
            print("\n1. Testing ReportVersion...")
            try:
                version_result = driver.portal_report_version()
                print(f"   Result: {version_result}")
                if version_result.get('status') == 'completed':
                    print(f"   ✓ Serial Number: {version_result.get('serial_number')}")
                    print(f"   ✓ PCBA Model: {version_result.get('pcba_model')}")
                    print(f"   ✓ PCBA Revision: {version_result.get('pcba_revision')}")
                    print(f"   ✓ Firmware Version: {version_result.get('firmware_version')}")
            except Exception as e:
                print(f"   ✗ ReportVersion failed: {e}")
            
            # 2. Test GetStatus
            print("\n2. Testing GetStatus...")
            try:
                status_result = driver.portal_get_status()
                print(f"   Result: {status_result}")
                if status_result.get('status') == 'completed':
                    print(f"   ✓ System Mode: {status_result.get('system_mode')}")
                    print(f"   ✓ Current State: {status_result.get('move_state')}")
                    print(f"   ✓ Door Status: {status_result.get('door_status')}")
                    print(f"   ✓ Feeder Status: {status_result.get('feeder_status')}")
                    print(f"   ✓ IP Address: {status_result.get('ip_address')}")
                    print(f"   ✓ MAC Address: {status_result.get('mac_address')}")
            except Exception as e:
                print(f"   ✗ GetStatus failed: {e}")
            
            # 3. Test Initialize (if system needs it)
            print("\n3. Testing Initialize...")
            try:
                init_result = driver.portal_initialize()
                print(f"   Result: {init_result}")
                if init_result.get('status') == 'completed':
                    print(f"   ✓ Tray Position 0: {init_result.get('tray_position_0')}")
                    print(f"   ✓ Tray Position 1: {init_result.get('tray_position_1')}")
                elif init_result.get('status') == 'received':
                    print(f"   ⏳ Initialize command received - waiting for completion...")
                    # In a real scenario, we'd poll GetStatus until complete
            except Exception as e:
                print(f"   ✗ Initialize failed: {e}")
                
        else:
            print("✗ Connection failed!")
            
    except Exception as e:
        print(f"✗ Error during Waters compliant test: {e}")
    finally:
        driver.disconnect()
        print("✓ Disconnected")


def report_version_test():
    """Test the ReportVersion command."""
    print("=== ReportVersion Command Test ===")
    
    driver = AutomationPortalDriver(
        port=config.DEFAULT_PORT,
        baudrate=config.DEFAULT_BAUDRATE,
        timeout=config.DEFAULT_TIMEOUT
    )
    
    try:
        if driver.connect():
            print("✓ Connection successful!")
            print(f"  Port: {driver.port}")
            print(f"  Baudrate: {driver.baudrate}")
            
            try:
                version_result = driver.portal_report_version()
                print(f"✓ ReportVersion response: {version_result}")
                if version_result.get('success'):
                    version_info = version_result.get('version_info', [])
                    if len(version_info) >= 4:
                        print(f"  Serial Number: {version_info[0]}")
                        print(f"  PCBA Model: {version_info[1]}")
                        print(f"  PCBA Revision: {version_info[2]}")
                        print(f"  Firmware Version: {version_info[3]}")
                else:
                    print(f"✗ ReportVersion failed: {version_result.get('error')}")
            except Exception as e:
                print(f"✗ ReportVersion failed: {e}")
                
        else:
            print("✗ Connection failed!")
            
    except Exception as e:
        print(f"✗ Error during ReportVersion test: {e}")
    finally:
        driver.disconnect()
        print("✓ Disconnected")


def get_status_test():
    """Test the GetStatus command with detailed tray status information."""
    print("=== GetStatus Command Test ===")
    
    driver = AutomationPortalDriver(
        port=config.DEFAULT_PORT,
        baudrate=config.DEFAULT_BAUDRATE,
        timeout=config.DEFAULT_TIMEOUT
    )
    
    try:
        if driver.connect():
            print("✓ Connection successful!")
            print(f"  Port: {driver.port}")
            print(f"  Baudrate: {driver.baudrate}")
            
            try:
                status_result = driver.portal_get_status()
                print(f"✓ GetStatus response: {status_result}")
                if status_result.get('success'):
                    print(f"\n=== System Status ===")
                    print(f"  System State: {status_result.get('system_state')}")
                    print(f"  Mode: {status_result.get('mode')}")
                    print(f"  Status: {status_result.get('status')}")
                    
                    print(f"\n=== Hardware Status ===")
                    print(f"  Drawer Status: {status_result.get('drawer_status')}")
                    print(f"  Door Status: {status_result.get('door_status')}")
                    print(f"  Feeder Status: {status_result.get('feeder_status')}")
                    
                    print(f"\n=== Network Status ===")
                    print(f"  Network Info: {status_result.get('network_info')}")
                    print(f"  MAC Address: {status_result.get('mac_address')}")
                    
                    # Display raw data to check for additional tray information
                    raw_data = status_result.get('raw_data', [])
                    print(f"\n=== Raw Status Data ===")
                    for i, item in enumerate(raw_data):
                        print(f"  [Index {i}]: {item}")
                    
                    # Check if drawer status contains tray position information
                    drawer_status = status_result.get('drawer_status', '')
                    print(f"\n=== Tray Analysis ===")
                    if 'NoDrawerNoTray' in drawer_status:
                        print(f"  Tray Position 0: No drawer/tray present")
                        print(f"  Tray Position 1: No drawer/tray present")
                    else:
                        # Parse drawer status for more detailed tray information
                        print(f"  Drawer Status Details: {drawer_status}")
                        # Note: Individual tray status might be in separate fields or require 
                        # additional parsing based on the actual response format
                        
                else:
                    print(f"✗ GetStatus failed: {status_result.get('error')}")
            except Exception as e:
                print(f"✗ GetStatus failed: {e}")
                
        else:
            print("✗ Connection failed!")
            
    except Exception as e:
        print(f"✗ Error during GetStatus test: {e}")
    finally:
        driver.disconnect()
        print("✓ Disconnected")


def initialize_test():
    """Test the Initialize command."""
    print("=== Initialize Command Test ===")
    
    driver = AutomationPortalDriver(
        port=config.DEFAULT_PORT,
        baudrate=config.DEFAULT_BAUDRATE,
        timeout=config.DEFAULT_TIMEOUT
    )
    
    try:
        if driver.connect():
            print("✓ Connection successful!")
            print(f"  Port: {driver.port}")
            print(f"  Baudrate: {driver.baudrate}")
            
            try:
                # Check current status first
                current_status = driver.portal_get_status()
                system_state = current_status.get('system_state', '')
                print(f"\nCurrent system state: {system_state}")
                
                if system_state in ['UNINIT', 'ERROR']:
                    print(f"System state is {system_state} - attempting initialization...")
                    init_result = driver.portal_initialize()
                    print(f"✓ Initialize response: {init_result}")
                    if init_result.get('success'):
                        print(f"  Status: {init_result.get('status')}")
                        if init_result.get('status') == 'received':
                            print("  ⏳ Initialization started...")
                            print("  Note: Use GetStatus to monitor initialization progress")
                    else:
                        print(f"✗ Initialize failed: {init_result.get('error')}")
                else:
                    print(f"System state is {system_state} - initialization not needed")
                    print("  (Initialize is only required when system is UNINIT or ERROR)")
            except Exception as e:
                print(f"✗ Initialize failed: {e}")
                
        else:
            print("✗ Connection failed!")
            
    except Exception as e:
        print(f"✗ Error during Initialize test: {e}")
    finally:
        driver.disconnect()
        print("✓ Disconnected")


def extract_test(position=None):
    """Test the Extract command for a specific position or both positions.
    
    Args:
        position (int, optional): Position to extract (0 or 1). If None, tests both positions.
    """
    if position is not None:
        print(f"=== Extract Command Test (Position {position}) ===")
        positions = [position]
    else:
        print("=== Extract Command Test (Both Positions) ===")
        positions = [0, 1]
    
    driver = AutomationPortalDriver(
        port=config.DEFAULT_PORT,
        baudrate=config.DEFAULT_BAUDRATE,
        timeout=config.DEFAULT_TIMEOUT
    )
    
    try:
        if driver.connect():
            print("✓ Connection successful!")
            print(f"  Port: {driver.port}")
            print(f"  Baudrate: {driver.baudrate}")
            
            # Test specified position(s)
            for pos in positions:
                print(f"\n--- Testing Extract for position {pos} ---")
                try:
                    extract_result = driver.portal_extract(pos)
                    print(f"✓ Extract({pos}) response: {extract_result}")
                    
                    if extract_result.get('success'):
                        print(f"  Position: {extract_result.get('position')}")
                        print(f"  Status: {extract_result.get('status')}")
                        print(f"  Sequence Number: {extract_result.get('sequence_number')}")
                        if extract_result.get('data'):
                            print(f"  Drawer/Tray Status: {extract_result.get('data')}")
                    else:
                        print(f"  Error: {extract_result.get('error')}")
                        
                except Exception as e:
                    print(f"✗ Extract({pos}) failed: {e}")
                
        else:
            print("✗ Connection failed!")
            
    except Exception as e:
        print(f"✗ Error during Extract test: {e}")
    finally:
        driver.disconnect()
        print("✓ Disconnected")


def insert_test(position=None):
    """Test the Insert command for a specific position or both positions.
    
    Args:
        position (int, optional): Position to insert (0 or 1). If None, tests both positions.
    """
    if position is not None:
        print(f"=== Insert Command Test (Position {position}) ===")
        positions = [position]
    else:
        print("=== Insert Command Test (Both Positions) ===")
        positions = [0, 1]
    
    driver = AutomationPortalDriver(
        port=config.DEFAULT_PORT,
        baudrate=config.DEFAULT_BAUDRATE,
        timeout=config.DEFAULT_TIMEOUT
    )
    
    try:
        if driver.connect():
            print("✓ Connection successful!")
            print(f"  Port: {driver.port}")
            print(f"  Baudrate: {driver.baudrate}")
            
            # Test specified position(s)
            for pos in positions:
                print(f"\n--- Testing Insert for position {pos} ---")
                try:
                    insert_result = driver.portal_insert(pos)
                    print(f"✓ Insert({pos}) response: {insert_result}")
                    
                    if insert_result.get('success'):
                        print(f"  Position: {insert_result.get('position')}")
                        print(f"  Status: {insert_result.get('status')}")
                        print(f"  Sequence Number: {insert_result.get('sequence_number')}")
                        if insert_result.get('data'):
                            print(f"  Data: {insert_result.get('data')}")
                    else:
                        print(f"  Error: {insert_result.get('error')}")
                        
                except Exception as e:
                    print(f"✗ Insert({pos}) failed: {e}")
                
        else:
            print("✗ Connection failed!")
            
    except Exception as e:
        print(f"✗ Error during Insert test: {e}")
    finally:
        driver.disconnect()
        print("✓ Disconnected")



def reset_system_test():
    """Test the ResetSystem command from Table 2-2."""
    print("=== ResetSystem Command Test ===")
    print("⚠️  WARNING: This will reset the Waters Automation Portal system!")
    
    driver = AutomationPortalDriver(
        port=config.DEFAULT_PORT,
        baudrate=config.DEFAULT_BAUDRATE,
        timeout=config.DEFAULT_TIMEOUT
    )
    
    try:
        if driver.connect():
            print("✓ Connection successful!")
            print(f"  Port: {driver.port}")
            print(f"  Baudrate: {driver.baudrate}")
            
            # Check current status first
            try:
                status = driver.portal_get_status()
                print(f"\nCurrent system status: {status.get('system_state', 'Unknown')}")
                print(f"Current mode: {status.get('mode', 'Unknown')}")
            except Exception as e:
                print(f"Could not get status: {e}")
            
            # Execute ResetSystem command
            print("\nExecuting ResetSystem command...")
            try:
                reset_result = driver.portal_reset_system()
                print(f"✓ ResetSystem response: {reset_result}")
                
                if reset_result.get('success'):
                    print("✓ System reset command sent successfully!")
                    print("  The Waters Automation Portal will restart.")
                    print("  System will return to UNINIT state after reset.")
                else:
                    print(f"✗ Reset failed: {reset_result.get('error')}")
                    
            except Exception as e:
                print(f"✗ ResetSystem command failed: {e}")
                
        else:
            print("✗ Connection failed!")
            
    except Exception as e:
        print(f"✗ Error during reset test: {e}")
    finally:
        driver.disconnect()
        print("✓ Disconnected")


def main():
    """Run basic connection test and show available functions."""
    print("Waters Acquity UPC Driver - Example Usage")
    print("=" * 50)
    
    # Run only basic connection test by default
    basic_connection_test()
    
    print("\n" + "=" * 50)
    print("Available test functions:")
    print("=" * 50)
    print("Basic Tests:")
    print("- basic_connection_test()")
    print("- parameter_setting_test()")
    print("- data_collection_test()")
    print("- method_run_simulation()")
    print("- error_handling_demo()")
    print("- advanced_features_demo()")
    print()
    print("Waters Portal Commands (Individual):")
    print("- report_version_test()")
    print("- get_status_test()")
    print("- initialize_test()")
    print("- extract_test(position=None)  # position: 0, 1, or None for both")
    print("- insert_test(position=None)   # position: 0, 1, or None for both")
    print("- reset_system_test()")
    print()
    print("System Identification:")
    print("- portal_connection_test()")
    print("- instrument_identification_test()")
    print("- waters_protocol_test()")
    print("- waters_compliant_test()")
    print()
    print("Run individual functions as needed for specific testing.")
    print("Example: report_version_test() or extract_test(1) or insert_test(0)")


if __name__ == "__main__":
    main()
