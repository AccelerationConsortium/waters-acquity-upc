#!/usr/bin/env python3
"""
Waters Acquity UPC Driver - Working Demo
Demonstrates successful communication with Waters Automation Portal instrument
"""

from automation_portal_driver import AutomationPortalDriver
import config

def demo_working_connection():
    """Demonstrate working Waters instrument communication"""
    print("=== Waters Acquity UPC Driver Demo ===")
    
    # Create driver instance
    driver = AutomationPortalDriver()
    
    try:
        # Connect to instrument
        print(f"Connecting to {config.PORTAL_COMM_SETTINGS['SERIAL_PORT']} at {config.PORTAL_COMM_SETTINGS['SERIAL_BAUDRATE']} baud...")
        if driver.connect():
            print("✅ Connected successfully!")
            print(f"   Instrument ID: {driver.instrument_id}")
            print(f"   Available modules: {driver.available_modules}")
            
            # Test working commands
            print("\n=== Testing Waters Automation Portal Commands ===")
            
            # 1. GetStatus - Shows system state
            print("\n1. GetStatus Command:")
            status = driver.portal_get_status()
            if status['success']:
                print("   ✅ GetStatus successful!")
                print(f"   System State: {status.get('system_state', 'Unknown')}")
                print(f"   Mode: {status.get('mode', 'Unknown')}")
                print(f"   Status: {status.get('status', 'Unknown')}")
                print(f"   MAC Address: {status.get('mac_address', 'Unknown')}")
            else:
                print(f"   ⚠️  GetStatus result: {status}")
            
            # 2. ReportVersion - Shows instrument information
            print("\n2. ReportVersion Command:")
            version = driver.portal_report_version()
            if version['success']:
                print("   ✅ ReportVersion successful!")
                print(f"   Version Info: {version['version_info']}")
            else:
                print(f"   ⚠️  ReportVersion result: {version}")
            
            # 3. ResetSystem - Tests system reset
            print("\n3. ResetSystem Command:")
            reset = driver.portal_reset_system()
            if reset['success']:
                print("   ✅ ResetSystem successful!")
                print(f"   Status: {reset['status']}")
            else:
                print(f"   ⚠️  ResetSystem result: {reset}")
            
            print("\n✅ All tests completed!")
            
        else:
            print("❌ Failed to connect to instrument")
            
    except Exception as e:
        print(f"❌ Error during demo: {e}")
        
    finally:
        driver.disconnect()
        print("✅ Disconnected successfully")

def show_system_info():
    """Show current system configuration"""
    print("\n=== System Configuration ===")
    print(f"Serial Port: {config.PORTAL_COMM_SETTINGS['SERIAL_PORT']}")
    print(f"Baudrate: {config.PORTAL_COMM_SETTINGS['SERIAL_BAUDRATE']}")
    print(f"Data Bits: {config.PORTAL_COMM_SETTINGS['SERIAL_DATABITS']}")
    print(f"Parity: {config.PORTAL_COMM_SETTINGS['SERIAL_PARITY']}")
    print(f"Stop Bits: {config.PORTAL_COMM_SETTINGS['SERIAL_STOPBITS']}")
    print(f"Timeout: {config.PORTAL_COMM_SETTINGS['SERIAL_TIMEOUT']}s")
    print(f"Command Terminator: {repr(config.COMMAND_TERMINATOR)}")

def main():
    """Main entry point for console script"""
    show_system_info()
    demo_working_connection()

if __name__ == "__main__":
    main()
