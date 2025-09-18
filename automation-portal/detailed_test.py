import serial
import time
from automation_portal_driver import AutomationPortalDriver

def test_detailed_getstatus():
    """Test GetStatus in detail to understand the response pattern"""
    print("=== DETAILED GetStatus ANALYSIS ===")
    
    try:
        # Test with raw serial to see all responses
        port = serial.Serial('COM4', 38400, timeout=2)
        time.sleep(0.5)
        
        print("1. Sending GetStatus and reading multiple times...")
        port.write(b'GetStatus\r')
        
        # Read multiple times to see if there are multiple responses
        for i in range(3):
            time.sleep(0.5)
            response = port.read(1000).decode('utf-8', errors='ignore')
            if response:
                print(f"   Read {i+1}: {repr(response)}")
            else:
                print(f"   Read {i+1}: (no data)")
        
        port.close()
        
    except Exception as e:
        print(f"Raw serial error: {e}")
    
    print("\n=== DRIVER ANALYSIS ===")
    
    try:
        driver = AutomationPortalDriver(port='COM4', baudrate=38400)
        driver.connect()
        
        print("2. Testing send_portal_command directly...")
        for i in range(3):
            print(f"\nAttempt {i+1}:")
            response = driver.send_portal_command("GetStatus")
            print(f"   Response: {response}")
            time.sleep(1)
        
        driver.disconnect()
        
    except Exception as e:
        print(f"Driver error: {e}")

if __name__ == "__main__":
    test_detailed_getstatus()
