import serial
import time
from automation_portal_driver import AutomationPortalDriver

def test_getstatus_only():
    """Test just the GetStatus command to see what's happening"""
    print("=== RAW SERIAL GetStatus TEST ===")
    
    try:
        # Raw serial test first
        port = serial.Serial('COM4', 38400, timeout=2)
        time.sleep(0.5)
        
        print("Sending raw GetStatus command...")
        port.write(b'GetStatus\r')
        time.sleep(1)
        
        response = port.read(2000).decode('utf-8', errors='ignore')
        print(f"Raw Response: {repr(response)}")
        
        port.close()
        
    except Exception as e:
        print(f"Raw serial error: {e}")
    
    print("\n=== DRIVER GetStatus TEST ===")
    
    try:
        # Driver test
        driver = AutomationPortalDriver(port='COM4', baudrate=38400)
        driver.connect()
        
        print("Testing driver portal_get_status()...")
        status = driver.portal_get_status()
        print(f"Driver Status Result: {status}")
        
        print("\nTesting driver send_portal_command('GetStatus')...")
        response = driver.send_portal_command("GetStatus")
        print(f"Portal Command Result: {response}")
        
        driver.disconnect()
        
    except Exception as e:
        print(f"Driver error: {e}")

if __name__ == "__main__":
    test_getstatus_only()
