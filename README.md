# Waters Acquity UPC Python Driver

A production-ready Python driver for controlling Waters Acquity Ultra Performance Convergence Chromatography (UPC) systems, implementing the official **Waters Automation Portal PC Protocol Specification (715008839)** for automated sample handling operations.

## 🚀 Key Features

### Waters Automation Portal Protocol Implementation
- **Complete implementation** of all 6 core Waters automation commands:
  - `GetStatus` - Retrieve system mode, movement state, and sensor status
  - `Initialize` - Initialize system components and detect tray positions  
  - `Extract(position)` - Extract drawer from sample manager position
  - `Insert(position)` - Insert drawer into sample manager position
  - `ReportVersion` - Get firmware and hardware version information
  - `ResetSystem` - Reset automation portal system

### Comprehensive Driver Architecture
- **Dual Communication Support**: RS-232 serial and TCP/IP networking
- **Robust Error Handling**: All 30 Waters-specific error codes implemented
- **Response Parsing**: Complete parsing of Waters protocol responses
- **Status Monitoring**: Real-time system state and sensor monitoring
- **Workflow Automation**: High-level extract-insert cycle operations

### System Capabilities
- **Hardware Status Monitoring**: Door/Feeder/Drawer detection with real-time feedback
- **Network Integration**: IP address and MAC address reporting
- **Error Recovery**: Automatic retry logic with exponential backoff
- **Production Ready**: Comprehensive logging and diagnostic capabilities

## 📋 System Requirements

- **Python**: 3.7+ with serial port access
- **Hardware**: Waters Acquity UPC with Automation Portal
- **Operating System**: Windows/Linux
- **Network**: Ethernet connection (for IP-based monitoring)
- **Permissions**: Serial port access (default: COM4 at 38400 baud)

## 🛠 Installation

### Quick Install
```bash
git clone https://github.com/kelvinchow23/waters-acquity-upc.git
cd waters-acquity-upc
pip install -r requirements.txt
```

### Package Install
```bash
pip install -e .
```

### Dependencies
```
pyserial>=3.5          # Serial communication
pandas>=1.3.0          # Data manipulation  
numpy>=1.21.0          # Numerical operations
PyPDF2>=3.0.0          # PDF documentation extraction
```

## 🚦 Quick Start

### Basic Connection Test
```python
from waters_acquity_driver import WatersAcquityDriver

# Create driver instance (default: COM4, 38400 baud)
driver = WatersAcquityDriver()

try:
    # Connect to instrument
    if driver.connect():
        print("✅ Connected successfully!")
        
        # Get system status
        status = driver.portal_get_status()
        print(f"System State: {status['system_state']}")
        print(f"Door Status: {status['door_status']}")
        print(f"MAC Address: {status['mac_address']}")
        
    else:
        print("❌ Connection failed")
        
finally:
    driver.disconnect()
```

### Sample Handling Workflow
```python
from waters_acquity_driver import WatersAcquityDriver

driver = WatersAcquityDriver(port='COM4', baudrate=38400)

try:
    driver.connect()
    
    # Initialize system (if needed)
    init_result = driver.portal_initialize()
    
    # Check system status
    status = driver.portal_get_status()
    if status['system_state'] == 'OPERATIONAL':
        
        # Extract drawer from position 1
        extract_result = driver.portal_extract(1)
        if extract_result['success']:
            print("✅ Sample extracted successfully")
            
            # Wait for operation to complete
            driver.portal_wait_for_idle(timeout=60)
            
            # Insert to position 0
            insert_result = driver.portal_insert(0)
            if insert_result['success']:
                print("✅ Sample inserted successfully")
        
finally:
    driver.disconnect()
```

## 🔧 Configuration

### Serial Communication (Default)
```python
# Default settings (configured for Waters Automation Portal)
PORT = 'COM4'
BAUDRATE = 38400
DATA_BITS = 8
PARITY = 'N' (None)
STOP_BITS = 1
TIMEOUT = 2 seconds
COMMAND_TERMINATOR = '\r' (Carriage Return)
```

### TCP/IP Communication
```python
driver = WatersAcquityDriver(
    host='192.168.1.100',
    tcp_port=34567,
    comm_mode='tcp'
)
```

## 📖 API Reference

### Core Portal Commands

#### System Status and Control
```python
# Get comprehensive system status
status = driver.portal_get_status()
# Returns: system_state, mode, door_status, feeder_status, network_info, etc.

# Get instrument version information
version = driver.portal_report_version()
# Returns: firmware version, hardware info

# Initialize automation portal
init_result = driver.portal_initialize()
# Returns: success status and sequence number

# Reset system (use with caution)
reset_result = driver.portal_reset_system()
```

#### Sample Handling Operations
```python
# Extract drawer from position (0 or 1)
extract_result = driver.portal_extract(position=1)

# Insert drawer to position (0 or 1)  
insert_result = driver.portal_insert(position=0)

# Wait for operation completion
driver.portal_wait_for_idle(timeout=60)

# Complete transfer cycle
cycle_result = driver.portal_extract_insert_cycle(from_pos=1, to_pos=0)
```

#### Status Monitoring
```python
# Check if system is ready for operations
is_ready = driver.portal_is_system_ready()

# Get specific status components
system_mode = driver.portal_get_system_mode()  # OPERATIONAL/UNINIT/ERROR
door_status = driver.portal_get_door_status()  # Open/Closed/Intermediate
feeder_status = driver.portal_get_feeder_status()  # In SM/Retracted/etc
```

### Response Format

All portal commands return dictionaries with consistent structure:

```python
{
    'success': True/False,           # Operation success status
    'status': 'completed',           # Waters response status
    'sequence_number': 123,          # Command sequence number
    'data': [...],                   # Response data (if any)
    'error_code': 0,                 # Error code (if error)
    'error_description': 'message'   # Error description (if error)
}
```

### System Status Fields

```python
status = driver.portal_get_status()
{
    'success': True,
    'system_state': 'OPERATIONAL',              # System ready state
    'mode': 'Initialize',                       # Current operation mode  
    'status': 'Idle',                          # Movement status
    'drawer_status': 'NoDrawerNoTray',         # Drawer detection
    'door_status': 'DoorClosed',               # Door position
    'feeder_status': 'FeederFullyRetracted',   # Feeder position
    'network_info': '172.16.0.4',             # IP address
    'mac_address': '00:00:C4:06:01:67',        # Hardware MAC
    'response_status': 'completed'              # Waters response type
}
```

## 🧪 Testing

### Run Built-in Tests
```bash
python test_driver.py
```

### Demo Script
```bash
python demo.py
```

### Example Usage
```bash
python example_usage.py
```

## 🔍 Troubleshooting

### Common Issues

#### Connection Problems
```python
# Check COM port
import serial.tools.list_ports
ports = serial.tools.list_ports.comports()
for port in ports:
    print(f"{port.device}: {port.description}")

# Test basic connectivity
driver = WatersAcquityDriver(port='COM4', baudrate=38400)
if driver.connect():
    print("✅ Connection successful")
    status = driver.portal_get_status()
    print(f"System: {status['system_state']}")
else:
    print("❌ Connection failed - check port and cables")
```

#### Error Responses
Waters instruments return specific error codes:
- **Error Code 1**: Unknown command
- **Error Code 4**: Unavailable command for this system mode
- **Error Code 8**: Hardware malfunction

```python
result = driver.portal_extract(1)
if not result['success']:
    print(f"Error {result.get('error_code', 'Unknown')}: {result.get('error_description', 'No description')}")
```

#### Status Interpretation
- **OPERATIONAL**: System ready for extract/insert operations
- **UNINIT**: System powered-on but not initialized
- **ERROR**: Problem detected during execution
- **Idle**: Not currently performing operations
- **DoorClosed**: Door properly secured
- **FeederFullyRetracted**: Feeder in safe position

## 📁 Project Structure

```
waters-acquity-upc/
├── waters_acquity_driver.py      # Main driver implementation
├── config.py                     # Configuration and constants  
├── example_usage.py               # Usage examples and demos
├── demo.py                       # Working demonstration script
├── test_driver.py                # Comprehensive test suite
├── data_analysis.py              # Data processing utilities
├── pdf_extractor.py              # Documentation extraction tool
├── requirements.txt              # Python dependencies
├── setup.py                      # Package installation
├── README.md                     # This documentation
└── docs/
    ├── documentation_index.md     # Documentation overview
    └── proprietary/               # Waters proprietary documentation
        ├── 715008839 Automation Portal PC Protocol Specification.pdf
        ├── 715008839 Automation Portal PC Protocol Specification_extracted.txt
        ├── 715008535v00 Sample Transfer File Guide.pdf
        └── 715008535v00 Sample Transfer File Guide_extracted.txt
```

## 🔒 Waters Protocol Compliance

This driver implements the exact Waters Automation Portal PC Protocol Specification:

### Communication Protocol
- **Serial**: RS-232, 38400 baud, 8N1, no flow control
- **Command Format**: Waters-specific protocol with sequence numbers
- **Response Format**: `Completed(seq, command, data...)` or `Error(seq, command, code, desc)`
- **Timeouts**: Command-specific (5s for status, 120s for initialization)

### Supported Hardware
- **Waters Acquity UPC** systems with Automation Portal
- **Sample Manager**: Rotary tray positions 0 and 1
- **Door Control**: Automated open/close with sensor feedback
- **Chain Feeder**: Automated drawer extraction/insertion
- **Network Interface**: Ethernet connectivity with DHCP support

### Error Handling
- **30 Specific Error Codes**: From command parsing to hardware malfunctions
- **Automatic Retry**: Configurable retry logic for transient errors
- **Recovery Procedures**: Initialization and reset capabilities
- **Comprehensive Logging**: Detailed logging for troubleshooting

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/improvement`)
3. Commit your changes (`git commit -am 'Add new feature'`)
4. Push to the branch (`git push origin feature/improvement`)
5. Create a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## ⚠️ Disclaimer

This is an independent implementation based on Waters public documentation. Use with proper safety precautions and verify all operations with your specific instrument configuration.

## 🏆 Production Status

**✅ READY FOR PRODUCTION USE**

This driver has been:
- ✅ Tested with actual Waters Acquity UPC hardware
- ✅ Verified against official Waters protocol specification
- ✅ Implemented with comprehensive error handling
- ✅ Designed for reliable automation workflows
- ✅ Documented with complete API reference

The driver successfully communicates with Waters instruments and provides a solid foundation for automated sample handling operations.

---

**Need Help?** Check the [troubleshooting section](#-troubleshooting) or create an issue on GitHub.

## Documentation

Waters proprietary documentation should be placed in the `docs/proprietary/` folder. This folder is excluded from git tracking to protect proprietary information. Use the provided `copy_waters_docs.bat` script to automatically copy files from your Waters documentation folder.

## Quick Start

```python
from waters_acquity_driver import WatersAcquityDriver

# Serial communication
with WatersAcquityDriver(port='COM1', comm_mode='serial') as driver:
    # Module-specific parameter control
    driver.set_flow_rate(1.0, module="BSM")    # Binary Solvent Manager
    driver.set_temperature(40.0, module="CM")  # Column Manager
    driver.set_injection_volume(5.0, module="SM")  # Sample Manager
    
    # Method management
    methods = driver.get_method_list()
    if methods:
        driver.load_method(methods[0])
        driver.start_run(methods[0])
    
    # Multi-detector data collection
    tuv_data = driver.get_data("TUV")  # UV detector
    frl_data = driver.get_data("FLR")  # Fluorescence detector
    
    # System monitoring
    status = driver.get_status()
    print(f"System: {status['status_text']}")
    print(f"Pressure: {status['pressure']} bar")
    print(f"Temperature: {status['temperature']} °C")
    
    driver.stop_run()

# TCP/IP communication (for networked instruments)
with WatersAcquityDriver(host='192.168.1.100', tcp_port=34567, comm_mode='tcp') as driver:
    # Same API works for both communication modes
    driver.prime_pump("BSM", "A")  # Prime solvent A
    driver.calibrate_detector("TUV")  # Calibrate UV detector
```

## Documentation

### Class: WatersAcquityDriver

#### Initialization
```python
# Serial communication
driver = WatersAcquityDriver(
    port='COM1',              # Serial port
    baudrate=9600,           # Communication baudrate
    timeout=5.0,             # Timeout in seconds
    comm_mode='serial'       # Communication mode
)

# TCP/IP communication
driver = WatersAcquityDriver(
    host='192.168.1.100',    # Instrument IP address
    tcp_port=34567,          # TCP port
    timeout=10.0,            # Network timeout
    comm_mode='tcp'          # Communication mode
)
```

#### Connection Management
- `connect()`: Establish connection to the system
- `disconnect()`: Close connection
- Context manager support for automatic connection handling

#### Module-Specific Control
- `set_flow_rate(rate, module="BSM")`: Set flow rate for Binary Solvent Manager
- `set_temperature(temp, module="CM")`: Set temperature for Column Manager
- `set_injection_volume(vol, module="SM")`: Set volume for Sample Manager
- `prime_pump(module="BSM", solvent="A")`: Prime pump with specified solvent
- `calibrate_detector(detector="TUV")`: Calibrate specified detector

#### Method Management
- `load_method(path)`: Load method file into instrument
- `get_method_list()`: Get list of available methods
- `start_run(method_name)`: Start chromatography run with specified method
- `stop_run()`: Stop current run
- `abort_run()`: Abort current run immediately

#### Data and Status
- `get_status()`: Get comprehensive system status including module states
- `get_data(detector, start_time, end_time)`: Retrieve data from specific detector
- `send_command(command)`: Send raw command to system

### Configuration

System limits and defaults are defined in `config.py`:

```python
# Flow rate limits (mL/min)
MAX_FLOW_RATE = 5.0
MIN_FLOW_RATE = 0.1
DEFAULT_FLOW_RATE = 1.0

# Temperature limits (°C)
MAX_TEMPERATURE = 80.0
MIN_TEMPERATURE = 10.0
DEFAULT_TEMPERATURE = 40.0

# Pressure limits (bar)
MAX_PRESSURE = 1000.0
MIN_PRESSURE = 0.0
```

## Examples

### Basic Connection Test
```python
from waters_acquity_driver import WatersAcquityDriver

driver = WatersAcquityDriver(port='COM1')
if driver.connect():
    print("Connected successfully!")
    status = driver.get_status()
    print(f"System status: {status}")
    driver.disconnect()
```

### Parameter Setting
```python
with WatersAcquityDriver() as driver:
    # Set operating parameters
    driver.set_flow_rate(1.5)    # 1.5 mL/min
    driver.set_temperature(45.0)  # 45°C
    
    # Verify settings
    status = driver.get_status()
    print(f"Current settings: {status}")
```

### Method Run with Data Collection
```python
with WatersAcquityDriver() as driver:
    # Set initial conditions
    driver.set_flow_rate(1.0)
    driver.set_temperature(40.0)
    
    # Start method
    if driver.start_run("gradient_method"):
        print("Method started successfully")
        
        # Monitor run (simplified)
        for i in range(10):
            time.sleep(30)  # Wait 30 seconds
            status = driver.get_status()
            print(f"Run status: {status}")
            
            # Collect data
            data = driver.get_data()
            # Process data as needed...
        
        # Stop run
        driver.stop_run()
```

## Testing

Run the test suite:
```bash
python test_driver.py
```

Run example usage:
```bash
python example_usage.py
```

## Error Handling

The driver includes comprehensive error handling:

```python
from waters_acquity_driver import WatersAcquityDriver, WatersAcquityError

try:
    with WatersAcquityDriver() as driver:
        driver.set_flow_rate(1.0)
        driver.start_run("my_method")
except WatersAcquityError as e:
    print(f"Waters Acquity error: {e}")
except Exception as e:
    print(f"Unexpected error: {e}")
```

## Customization

### Adding New Commands

To add new Waters Acquity commands, extend the `WatersAcquityDriver` class:

```python
class ExtendedWatersDriver(WatersAcquityDriver):
    def set_pressure(self, pressure):
        """Set system pressure."""
        try:
            response = self.send_command(f"SET_PRESSURE {pressure}")
            return "OK" in response.upper()
        except Exception as e:
            self.logger.error(f"Error setting pressure: {e}")
            return False
```

### Custom Data Processing

```python
def process_chromatography_data(data):
    """Custom data processing function."""
    # Apply smoothing
    data['smoothed_signal'] = data['signal'].rolling(window=5).mean()
    
    # Find peaks
    from scipy.signal import find_peaks
    peaks, _ = find_peaks(data['smoothed_signal'], height=0.1)
    
    return data, peaks
```

## Requirements

- Python 3.7+
- pyserial
- numpy
- pandas
- logging (built-in)
- datetime (built-in)

## Notes

### Important Customization Required

This driver provides a **template framework** that needs to be customized for your specific Waters Acquity system. The current implementation includes:

1. **Placeholder Commands**: Replace the example commands with actual Waters Acquity commands from your system documentation
2. **Communication Protocol**: Verify and adjust the serial communication parameters
3. **Response Parsing**: Implement proper parsing of system responses
4. **Error Codes**: Add handling for specific Waters Acquity error codes

### Next Steps

1. **Obtain Documentation**: Get the Waters Acquity command reference manual
2. **Replace Placeholders**: Update the `send_command()` calls with actual commands
3. **Test with Hardware**: Test the driver with your actual Waters Acquity system
4. **Validate Responses**: Ensure response parsing matches your system's output format

### Example Command Mapping

```python
# Replace these placeholder commands with actual Waters commands:
# self.send_command("START_RUN") -> actual start command
# self.send_command("STOP_RUN") -> actual stop command  
# self.send_command("SET_FLOW") -> actual flow rate command
# self.send_command("SET_TEMP") -> actual temperature command
```

## License

This project is provided as-is for educational and development purposes.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## Support

For questions or issues:
1. Check the documentation
2. Review the example usage
3. Run the test suite to verify functionality
4. Consult your Waters Acquity system documentation for specific commands
