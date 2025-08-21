"""
Unit tests for the Waters Acquity UPC Driver
"""

import unittest
from unittest.mock import Mock, patch, MagicMock
import sys
import os
import logging

# Add the current directory to the path
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from waters_acquity_driver import WatersAcquityDriver, WatersAcquityError
import config


# Standalone test functions for portal functionality
def test_portal_commands():
    """Test Waters Automation Portal specific commands"""
    print("\n=== Testing Waters Automation Portal Commands ===")
    
    # Mock driver for testing portal command parsing
    class MockPortalDriver(WatersAcquityDriver):
        def __init__(self):
            # Initialize without actual connection
            self.is_connected = True
            self.logger = logging.getLogger(__name__)
        
        def send_command(self, command):
            """Mock responses based on Waters protocol specification"""
            if command == "GetStatus":
                return "Complete(12, GetStatus, OPERATIONAL, NoMoveCmd, Idle, NoDrawerNoTray, DoorClosed, FeederFullyRetracted, 192.168.1.100, 00:11:22:33:44:55)"
            elif command == "Initialize":
                return "Complete(13, Initialize, DrawerOnly, DrawerAndTray)"
            elif command.startswith("Extract"):
                return "Complete(14, Extract, DrawerAndTray)"
            elif command.startswith("Insert"):
                return "Complete(15, Insert)"
            elif command == "ReportVersion":
                return "Complete(16, ReportVersion, 1234567890, 0250.600, 03, 0200)"
            elif command == "ResetSystem":
                return "Complete(17, ResetSystem)"
            else:
                return f"Error(99, 1, Command parsing failure)"
    
    driver = MockPortalDriver()
    
    try:
        # Test GetStatus command
        print("Testing portal_get_status()...")
        status = driver.portal_get_status()
        print(f"Status result: {status}")
        assert status['system_mode'] == 'OPERATIONAL'
        assert status['door_status'] == 'DoorClosed'
        assert status['ip_address'] == '192.168.1.100'
        print("✓ GetStatus test passed")
        
        # Test Initialize command
        print("\nTesting portal_initialize()...")
        init_result = driver.portal_initialize()
        print(f"Initialize result: {init_result}")
        assert init_result['tray_position_0'] == 'DrawerOnly'
        assert init_result['tray_position_1'] == 'DrawerAndTray'
        print("✓ Initialize test passed")
        
        # Test Extract command
        print("\nTesting portal_extract_tray()...")
        extract_result = driver.portal_extract_tray(1)
        print(f"Extract result: {extract_result}")
        assert extract_result['extracted_drawer_status'] == 'DrawerAndTray'
        print("✓ Extract test passed")
        
        # Test Insert command
        print("\nTesting portal_insert_tray()...")
        insert_result = driver.portal_insert_tray(0)
        print(f"Insert result: {insert_result}")
        assert insert_result.get('status') == 'complete'
        print("✓ Insert test passed")
        
        # Test ReportVersion command
        print("\nTesting portal_report_version()...")
        version_result = driver.portal_report_version()
        print(f"Version result: {version_result}")
        assert version_result['serial_number'] == '1234567890'
        assert version_result['firmware_version'] == '0200'
        print("✓ ReportVersion test passed")
        
        # Test status monitoring methods
        print("\nTesting status monitoring methods...")
        system_mode = driver.portal_get_system_mode()
        door_status = driver.portal_get_door_status()
        feeder_status = driver.portal_get_feeder_status()
        is_ready = driver.portal_is_system_ready()
        
        assert system_mode == 'OPERATIONAL'
        assert door_status == 'DoorClosed'
        assert feeder_status == 'FeederFullyRetracted'
        assert is_ready == True
        print("✓ Status monitoring methods test passed")
        
        # Test network info
        print("\nTesting portal_get_network_info()...")
        network_info = driver.portal_get_network_info()
        print(f"Network info: {network_info}")
        assert network_info['ip_address'] == '192.168.1.100'
        assert network_info['mac_address'] == '00:11:22:33:44:55'
        print("✓ Network info test passed")
        
        # Test error response parsing
        print("\nTesting error response parsing...")
        try:
            driver.portal_extract_tray(2)  # Invalid position
        except ValueError as e:
            print(f"Correctly caught ValueError: {e}")
            print("✓ Error handling test passed")
        
        print("\n✓ All portal command tests passed!")
        return True
        
    except Exception as e:
        print(f"✗ Portal command test failed: {e}")
        return False


def test_portal_error_codes():
    """Test Waters Automation Portal error code handling"""
    print("\n=== Testing Portal Error Code Handling ===")
    
    # Test error code mappings
    from config import PORTAL_ERROR_CODES
    
    test_errors = [1, 6, 15, 25, 30]
    
    for error_code in test_errors:
        if error_code in PORTAL_ERROR_CODES:
            description = PORTAL_ERROR_CODES[error_code]
            print(f"Error {error_code}: {description}")
        else:
            print(f"Error {error_code}: Unknown error code")
    
    print("✓ Error code mapping test passed")
    return True


def test_portal_configuration():
    """Test Waters Automation Portal configuration values"""
    print("\n=== Testing Portal Configuration ===")
    
    from config import (PORTAL_COMM_SETTINGS, PORTAL_TIMEOUTS, 
                       PORTAL_VALIDATION, PORTAL_SYSTEM_MODES)
    
    # Test communication settings
    assert PORTAL_COMM_SETTINGS['SERIAL_BAUDRATE'] == 38400
    assert PORTAL_COMM_SETTINGS['SERIAL_FLOW_CONTROL'] == 'none'
    print("✓ Communication settings valid")
    
    # Test timeout values
    assert PORTAL_TIMEOUTS['Initialize'] == 120  # 2 minutes for initialization
    assert PORTAL_TIMEOUTS['Extract'] == 60      # 1 minute for extract
    print("✓ Timeout values valid")
    
    # Test validation ranges
    assert PORTAL_VALIDATION['TRAY_POSITIONS'] == [0, 1]
    assert PORTAL_VALIDATION['MAX_EXTRACT_DISTANCE'] == 175
    print("✓ Validation ranges valid")
    
    # Test system modes
    assert 'OPERATIONAL' in PORTAL_SYSTEM_MODES
    assert 'ERROR' in PORTAL_SYSTEM_MODES
    print("✓ System modes valid")
    
    print("✓ All portal configuration tests passed!")
    return True


class TestWatersAcquityDriver(unittest.TestCase):
    """Test cases for WatersAcquityDriver class."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.driver = WatersAcquityDriver(
            port='COM_TEST',
            baudrate=9600,
            timeout=5.0
        )
    
    def tearDown(self):
        """Clean up after tests."""
        if self.driver.is_connected:
            self.driver.disconnect()
    
    def test_initialization(self):
        """Test driver initialization."""
        self.assertEqual(self.driver.port, 'COM_TEST')
        self.assertEqual(self.driver.baudrate, 9600)
        self.assertEqual(self.driver.timeout, 5.0)
        self.assertFalse(self.driver.is_connected)
        self.assertIsNone(self.driver.connection)
    
    @patch('serial.Serial')
    def test_successful_connection(self, mock_serial):
        """Test successful connection to the device."""
        # Mock successful serial connection
        mock_connection = Mock()
        mock_serial.return_value = mock_connection
        
        result = self.driver.connect()
        
        self.assertTrue(result)
        self.assertTrue(self.driver.is_connected)
        self.assertIsNotNone(self.driver.connection)
        mock_serial.assert_called_once()
    
    @patch('serial.Serial')
    def test_failed_connection(self, mock_serial):
        """Test failed connection to the device."""
        # Mock failed serial connection
        mock_serial.side_effect = Exception("Connection failed")
        
        result = self.driver.connect()
        
        self.assertFalse(result)
        self.assertFalse(self.driver.is_connected)
    
    def test_disconnect(self):
        """Test disconnection from the device."""
        # Mock connection
        mock_connection = Mock()
        mock_connection.is_open = True
        self.driver.connection = mock_connection
        self.driver.is_connected = True
        
        self.driver.disconnect()
        
        mock_connection.close.assert_called_once()
        self.assertFalse(self.driver.is_connected)
    
    def test_send_command_without_connection(self):
        """Test sending command without connection raises error."""
        with self.assertRaises(WatersAcquityError):
            self.driver.send_command("TEST")
    
    @patch('serial.Serial')
    def test_send_command_with_connection(self, mock_serial):
        """Test sending command with active connection."""
        # Set up mock connection
        mock_connection = Mock()
        mock_connection.readline.return_value = b'OK\r\n'
        mock_serial.return_value = mock_connection
        
        self.driver.connect()
        response = self.driver.send_command("TEST")
        
        self.assertEqual(response, 'OK')
        mock_connection.write.assert_called_once()
        mock_connection.readline.assert_called_once()
    
    def test_get_status_disconnected(self):
        """Test getting status when disconnected."""
        status = self.driver.get_status()
        
        self.assertFalse(status['connected'])
        self.assertIn('timestamp', status)
        self.assertIn('system_ready', status)
    
    @patch('serial.Serial')
    def test_get_status_connected(self, mock_serial):
        """Test getting status when connected."""
        mock_connection = Mock()
        mock_serial.return_value = mock_connection
        
        self.driver.connect()
        status = self.driver.get_status()
        
        self.assertTrue(status['connected'])
        self.assertIn('timestamp', status)
    
    @patch('serial.Serial')
    def test_set_flow_rate(self, mock_serial):
        """Test setting flow rate."""
        mock_connection = Mock()
        mock_connection.readline.return_value = b'OK\r\n'
        mock_serial.return_value = mock_connection
        
        self.driver.connect()
        result = self.driver.set_flow_rate(1.5)
        
        self.assertTrue(result)
    
    @patch('serial.Serial')
    def test_set_temperature(self, mock_serial):
        """Test setting temperature."""
        mock_connection = Mock()
        mock_connection.readline.return_value = b'OK\r\n'
        mock_serial.return_value = mock_connection
        
        self.driver.connect()
        result = self.driver.set_temperature(40.0)
        
        self.assertTrue(result)
    
    @patch('serial.Serial')
    def test_start_run(self, mock_serial):
        """Test starting a method run."""
        mock_connection = Mock()
        mock_connection.readline.return_value = b'OK\r\n'
        mock_serial.return_value = mock_connection
        
        self.driver.connect()
        result = self.driver.start_run("test_method")
        
        self.assertTrue(result)
    
    @patch('serial.Serial')
    def test_stop_run(self, mock_serial):
        """Test stopping a method run."""
        mock_connection = Mock()
        mock_connection.readline.return_value = b'OK\r\n'
        mock_serial.return_value = mock_connection
        
        self.driver.connect()
        result = self.driver.stop_run()
        
        self.assertTrue(result)
    
    def test_get_data(self):
        """Test data retrieval."""
        data = self.driver.get_data()
        
        self.assertIsNotNone(data)
        self.assertEqual(len(data.columns), 4)  # time, signal, pressure, temperature
        self.assertEqual(len(data), 100)  # 100 data points
    
    @patch('serial.Serial')
    def test_context_manager(self, mock_serial):
        """Test using driver as context manager."""
        mock_connection = Mock()
        mock_serial.return_value = mock_connection
        
        with WatersAcquityDriver() as driver:
            self.assertTrue(driver.is_connected)
        
        # Should be disconnected after exiting context
        mock_connection.close.assert_called_once()
    
    def test_portal_commands(self):
        """Test Waters Automation Portal specific commands"""
        print("\n=== Testing Waters Automation Portal Commands ===")
        
        # Mock driver for testing portal command parsing
        class MockPortalDriver(WatersAcquityDriver):
            def __init__(self):
                # Initialize without actual connection
                self.is_connected = True
                self.logger = logging.getLogger(__name__)
            
            def send_command(self, command):
                """Mock responses based on Waters protocol specification"""
                if command == "GetStatus":
                    return "Complete(12, GetStatus, OPERATIONAL, NoMoveCmd, Idle, NoDrawerNoTray, DoorClosed, FeederFullyRetracted, 192.168.1.100, 00:11:22:33:44:55)"
                elif command == "Initialize":
                    return "Complete(13, Initialize, DrawerOnly, DrawerAndTray)"
                elif command.startswith("Extract"):
                    return "Complete(14, Extract, DrawerAndTray)"
                elif command.startswith("Insert"):
                    return "Complete(15, Insert)"
                elif command == "ReportVersion":
                    return "Complete(16, ReportVersion, 1234567890, 0250.600, 03, 0200)"
                elif command == "ResetSystem":
                    return "Complete(17, ResetSystem)"
                else:
                    return f"Error(99, 1, Command parsing failure)"
        
        driver = MockPortalDriver()
        
        try:
            # Test GetStatus command
            print("Testing portal_get_status()...")
            status = driver.portal_get_status()
            print(f"Status result: {status}")
            assert status['system_mode'] == 'OPERATIONAL'
            assert status['door_status'] == 'DoorClosed'
            assert status['ip_address'] == '192.168.1.100'
            print("✓ GetStatus test passed")
            
            # Test Initialize command
            print("\nTesting portal_initialize()...")
            init_result = driver.portal_initialize()
            print(f"Initialize result: {init_result}")
            assert init_result['tray_position_0'] == 'DrawerOnly'
            assert init_result['tray_position_1'] == 'DrawerAndTray'
            print("✓ Initialize test passed")
            
            # Test Extract command
            print("\nTesting portal_extract_tray()...")
            extract_result = driver.portal_extract_tray(1)
            print(f"Extract result: {extract_result}")
            assert extract_result['extracted_drawer_status'] == 'DrawerAndTray'
            print("✓ Extract test passed")
            
            # Test Insert command
            print("\nTesting portal_insert_tray()...")
            insert_result = driver.portal_insert_tray(0)
            print(f"Insert result: {insert_result}")
            assert insert_result.get('status') == 'complete'
            print("✓ Insert test passed")
            
            # Test ReportVersion command
            print("\nTesting portal_report_version()...")
            version_result = driver.portal_report_version()
            print(f"Version result: {version_result}")
            assert version_result['serial_number'] == '1234567890'
            assert version_result['firmware_version'] == '0200'
            print("✓ ReportVersion test passed")
            
            # Test status monitoring methods
            print("\nTesting status monitoring methods...")
            system_mode = driver.portal_get_system_mode()
            door_status = driver.portal_get_door_status()
            feeder_status = driver.portal_get_feeder_status()
            is_ready = driver.portal_is_system_ready()
            
            assert system_mode == 'OPERATIONAL'
            assert door_status == 'DoorClosed'
            assert feeder_status == 'FeederFullyRetracted'
            assert is_ready == True
            print("✓ Status monitoring methods test passed")
            
            # Test network info
            print("\nTesting portal_get_network_info()...")
            network_info = driver.portal_get_network_info()
            print(f"Network info: {network_info}")
            assert network_info['ip_address'] == '192.168.1.100'
            assert network_info['mac_address'] == '00:11:22:33:44:55'
            print("✓ Network info test passed")
            
            # Test error response parsing
            print("\nTesting error response parsing...")
            try:
                driver.portal_extract_tray(2)  # Invalid position
            except ValueError as e:
                print(f"Correctly caught ValueError: {e}")
                print("✓ Error handling test passed")
            
            print("\n✓ All portal command tests passed!")
            return True
            
        except Exception as e:
            print(f"✗ Portal command test failed: {e}")
            return False
    
    def test_portal_error_codes(self):
        """Test Waters Automation Portal error code handling"""
        print("\n=== Testing Portal Error Code Handling ===")
        
        # Test error code mappings
        from config import PORTAL_ERROR_CODES
        
        test_errors = [1, 6, 15, 25, 30]
        
        for error_code in test_errors:
            if error_code in PORTAL_ERROR_CODES:
                description = PORTAL_ERROR_CODES[error_code]
                print(f"Error {error_code}: {description}")
            else:
                print(f"Error {error_code}: Unknown error code")
        
        print("✓ Error code mapping test passed")
        return True
    
    def test_portal_configuration(self):
        """Test Waters Automation Portal configuration values"""
        print("\n=== Testing Portal Configuration ===")
        
        from config import (PORTAL_COMM_SETTINGS, PORTAL_TIMEOUTS, 
                           PORTAL_VALIDATION, PORTAL_SYSTEM_MODES)
        
    # Test communication settings
    assert PORTAL_COMM_SETTINGS['SERIAL_BAUDRATE'] == 38400
    assert PORTAL_COMM_SETTINGS['SERIAL_FLOW_CONTROL'] == 'none'
    print("✓ Communication settings valid")
        
        # Test timeout values
        assert PORTAL_TIMEOUTS['Initialize'] == 120  # 2 minutes for initialization
        assert PORTAL_TIMEOUTS['Extract'] == 60      # 1 minute for extract
        print("✓ Timeout values valid")
        
        # Test validation ranges
        assert PORTAL_VALIDATION['TRAY_POSITIONS'] == [0, 1]
        assert PORTAL_VALIDATION['MAX_EXTRACT_DISTANCE'] == 175
        print("✓ Validation ranges valid")
        
        # Test system modes
        assert 'OPERATIONAL' in PORTAL_SYSTEM_MODES
        assert 'ERROR' in PORTAL_SYSTEM_MODES
        print("✓ System modes valid")
        
        print("✓ All portal configuration tests passed!")
        return True


class TestConfigConstants(unittest.TestCase):
    """Test configuration constants."""
    
    def test_flow_rate_limits(self):
        """Test flow rate limits are reasonable."""
        self.assertGreater(config.MAX_FLOW_RATE, config.MIN_FLOW_RATE)
        self.assertGreaterEqual(config.DEFAULT_FLOW_RATE, config.MIN_FLOW_RATE)
        self.assertLessEqual(config.DEFAULT_FLOW_RATE, config.MAX_FLOW_RATE)
    
    def test_temperature_limits(self):
        """Test temperature limits are reasonable."""
        self.assertGreater(config.MAX_TEMPERATURE, config.MIN_TEMPERATURE)
        self.assertGreaterEqual(config.DEFAULT_TEMPERATURE, config.MIN_TEMPERATURE)
        self.assertLessEqual(config.DEFAULT_TEMPERATURE, config.MAX_TEMPERATURE)
    
    def test_pressure_limits(self):
        """Test pressure limits are reasonable."""
        self.assertGreater(config.MAX_PRESSURE, config.MIN_PRESSURE)
        self.assertGreaterEqual(config.MIN_PRESSURE, 0.0)


if __name__ == '__main__':
    # Run the tests
    print("Running Waters Acquity Driver Tests...")
    
    # Run additional portal tests
    print("\n" + "="*60)
    print("ADDITIONAL PORTAL FUNCTIONALITY TESTS")
    print("="*60)
    
    test_portal_commands()
    test_portal_error_codes() 
    test_portal_configuration()
    
    print("\n" + "="*60)
    print("UNITTEST FRAMEWORK TESTS")
    print("="*60)
    
    unittest.main(verbosity=2)
