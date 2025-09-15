"""
Example usage of the Waters Automation Portal Driver

This script demonstrates sample transfer operations using the Waters Automation Portal.
The portal is designed specifically for moving samples in and out of the instrument.
"""

import time
import sys
import os

# Add the current directory to the path so we can import our driver
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from automation_portal_driver import AutomationPortalDriver, AutomationPortalError
import config


def basic_connection_test():
    """Test basic connection to the Waters Automation Portal."""
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


def initialization_test():
    """Test system initialization."""
    print("\n=== System Initialization Test ===")
    
    with AutomationPortalDriver() as driver:
        try:
            print("Initializing system...")
            if driver.initialize():
                print("✓ System initialized successfully")
                
                # Check status after initialization
                status = driver.get_status()
                print(f"✓ Post-initialization status: {status}")
            else:
                print("✗ Initialization failed")
                
        except AutomationPortalError as e:
            print(f"✗ Error during initialization: {e}")


def sample_extraction_test():
    """Test extracting a sample drawer."""
    print("\n=== Sample Extraction Test ===")
    
    with AutomationPortalDriver() as driver:
        try:
            # Initialize first
            if not driver.initialize():
                print("✗ Failed to initialize system")
                return
            
            # Check current status
            status = driver.get_status()
            print(f"Current status: {status.get('drawer_tray_status', 'Unknown')}")
            
            # Extract drawer from position 0
            print("Extracting drawer from position 0...")
            if driver.extract_drawer(0):
                print("✓ Drawer extracted successfully")
                
                # Check if drawer is present
                drawer_present = driver.is_drawer_present()
                print(f"✓ Drawer present: {drawer_present}")
                
                # Check door status
                door_open = driver.is_door_open()
                print(f"✓ Door open: {door_open}")
                
            else:
                print("✗ Drawer extraction failed")
                
        except Exception as e:
            print(f"✗ Error during extraction: {e}")


def sample_insertion_test():
    """Test inserting a sample drawer."""
    print("\n=== Sample Insertion Test ===")
    
    with AutomationPortalDriver() as driver:
        try:
            # Initialize first
            if not driver.initialize():
                print("✗ Failed to initialize system")
                return
            
            # Insert drawer to position 0
            print("Inserting drawer to position 0...")
            if driver.insert_drawer(0):
                print("✓ Drawer inserted successfully")
                
                # Check final status
                status = driver.get_status()
                print(f"✓ Final status: {status.get('drawer_tray_status', 'Unknown')}")
                
            else:
                print("✗ Drawer insertion failed")
                
        except Exception as e:
            print(f"✗ Error during insertion: {e}")


def full_sample_transfer_workflow():
    """Complete sample transfer workflow."""
    print("\n=== Full Sample Transfer Workflow ===")
    
    try:
        # Use different connection modes based on configuration
        if hasattr(config, 'PREFERRED_COMM_MODE'):
            if config.PREFERRED_COMM_MODE == 'tcp':
                driver = AutomationPortalDriver(comm_mode='tcp')
            else:
                driver = AutomationPortalDriver(comm_mode='serial')
        else:
            driver = AutomationPortalDriver(comm_mode='serial')
        
        with driver:
            print("1. Initializing Automation Portal...")
            if not driver.initialize():
                print("✗ Initialization failed")
                return
            
            print("2. Getting initial status...")
            status = driver.get_status()
            print(f"   System mode: {status.get('system_mode', 'Unknown')}")
            print(f"   Door status: {status.get('door_status', 'Unknown')}")
            print(f"   Drawer/Tray: {status.get('drawer_tray_status', 'Unknown')}")
            
            print("3. Extracting sample drawer from position 0...")
            if driver.extract_drawer(0):
                print("   ✓ Drawer extracted - ready for sample loading/unloading")
                
                # Simulate user interaction
                print("   >> Place your samples in the drawer <<")
                print("   (In real use, user would load/unload samples here)")
                time.sleep(2)  # Simulate sample handling time
                
                print("4. Inserting drawer back to position 0...")
                if driver.insert_drawer(0):
                    print("   ✓ Drawer inserted - samples ready for analysis")
                else:
                    print("   ✗ Failed to insert drawer")
            else:
                print("   ✗ Failed to extract drawer")
            
            print("5. Final system status...")
            final_status = driver.get_status()
            print(f"   ✓ Final drawer/tray status: {final_status.get('drawer_tray_status', 'Unknown')}")
            
    except Exception as e:
        print(f"✗ Error in workflow: {e}")


def system_info_test():
    """Test getting system information."""
    print("\n=== System Information Test ===")
    
    with AutomationPortalDriver() as driver:
        try:
            # Get version information
            version = driver.report_version()
            print(f"✓ System version: {version}")
            
            # Get detailed status
            status = driver.get_status()
            print("✓ Detailed system status:")
            for key, value in status.items():
                print(f"   {key}: {value}")
                
        except Exception as e:
            print(f"✗ Error getting system info: {e}")


def error_handling_test():
    """Test error handling scenarios."""
    print("\n=== Error Handling Test ===")
    
    # Test invalid tray position
    try:
        driver = AutomationPortalDriver(port='INVALID_PORT')
        driver.connect()
        print("✗ Should have failed with invalid port")
    except:
        print("✓ Correctly handled invalid port")
    
    # Test invalid tray position
    with AutomationPortalDriver() as driver:
        try:
            driver.extract_drawer(99)  # Invalid position
            print("✗ Should have failed with invalid position")
        except ValueError:
            print("✓ Correctly handled invalid tray position")
        except AutomationPortalError:
            print("✓ Communication error (expected if not connected to real hardware)")


def status_monitoring_test():
    """Test continuous status monitoring."""
    print("\n=== Status Monitoring Test ===")
    
    with AutomationPortalDriver() as driver:
        try:
            for i in range(3):
                status = driver.get_status()
                print(f"Status check {i+1}:")
                print(f"  System mode: {status.get('system_mode', 'Unknown')}")
                print(f"  Movement state: {status.get('move_state', 'Unknown')}")
                print(f"  Drawer present: {driver.is_drawer_present()}")
                print(f"  Door open: {driver.is_door_open()}")
                
                if i < 2:  # Don't sleep on last iteration
                    time.sleep(1)
                    
        except Exception as e:
            print(f"✗ Error during monitoring: {e}")


def main():
    """Run all example tests."""
    print("Waters Automation Portal Driver - Sample Transfer Examples")
    print("=" * 60)
    
    try:
        # Basic tests
        basic_connection_test()
        initialization_test()
        system_info_test()
        
        # Sample transfer tests
        sample_extraction_test()
        sample_insertion_test()
        
        # Advanced tests
        full_sample_transfer_workflow()
        status_monitoring_test()
        error_handling_test()
        
        print("\n" + "=" * 60)
        print("✓ All tests completed!")
        
    except KeyboardInterrupt:
        print("\n✗ Tests interrupted by user")
    except Exception as e:
        print(f"\n✗ Unexpected error during tests: {e}")


if __name__ == "__main__":
    main()
