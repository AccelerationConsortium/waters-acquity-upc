import serial
import time
import config

def test_raw_serial():
    """Test direct serial communication with Waters instrument"""
    try:
        # Test direct serial communication
        port = serial.Serial('COM4', 38400, timeout=2)
        time.sleep(0.5)
        
        print('Testing ReportVersion...')
        port.write(b'ReportVersion\r')
        time.sleep(0.5)
        response = port.read(1000).decode('utf-8', errors='ignore')
        print(f'Response: {repr(response)}')
        
        print('\nTesting GetStatus...')
        port.write(b'GetStatus\r')
        time.sleep(0.5)
        response = port.read(1000).decode('utf-8', errors='ignore')
        print(f'Response: {repr(response)}')
        
        print('\nTesting Initialize...')
        port.write(b'Initialize\r')
        time.sleep(2)  # Initialize might take longer
        response = port.read(1000).decode('utf-8', errors='ignore')
        print(f'Response: {repr(response)}')
        
        print('\nTesting ResetSystem...')
        port.write(b'ResetSystem\r')
        time.sleep(1)
        response = port.read(1000).decode('utf-8', errors='ignore')
        print(f'Response: {repr(response)}')
        
        print('\nTesting Extract(1)...')
        port.write(b'Extract(1)\r')
        time.sleep(1)
        response = port.read(1000).decode('utf-8', errors='ignore')
        print(f'Response: {repr(response)}')
        
        port.close()
        
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    test_raw_serial()
