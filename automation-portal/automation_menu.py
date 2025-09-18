#!/usr/bin/env python3
"""
Waters Automation Portal - Interactive Command Menu
Provides an easy-to-use interface for automation portal operations
"""

import sys
import logging
from automation_portal_driver import AutomationPortalDriver

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')

class AutomationPortalMenu:
    def __init__(self):
        self.driver = AutomationPortalDriver()
        self.connected = False
    
    def clear_screen(self):
        """Clear the terminal screen"""
        import os
        os.system('cls' if os.name == 'nt' else 'clear')
    
    def print_header(self):
        """Print the menu header"""
        print("=" * 60)
        print("    Waters Automation Portal - Interactive Menu")
        print("=" * 60)
        
    def print_status_bar(self):
        """Print current connection status"""
        status_msg = "🟢 Connected" if self.connected else "🔴 Disconnected"
        print(f"Status: {status_msg}")
        print("-" * 60)
    
    def get_current_status(self):
        """Get and display current system status"""
        if not self.connected:
            print("❌ Not connected. Please connect first.")
            return None
            
        try:
            status = self.driver.get_status()
            print("\n📊 Current System Status:")
            print(f"   System State: {status.get('system_state')}")
            print(f"   Door Status: {status.get('door_status')}")
            print(f"   Drawer/Tray: {status.get('drawer_tray_status')}")
            print(f"   Current Mode: {status.get('mode')}")
            print(f"   Movement: {status.get('status')}")
            return status
        except Exception as e:
            print(f"❌ Error getting status: {e}")
            return None
    
    def connect(self):
        """Connect to automation portal"""
        try:
            if self.driver.connect():
                self.connected = True
                print("✅ Connected to Automation Portal")
                return True
            else:
                print("❌ Failed to connect")
                return False
        except Exception as e:
            print(f"❌ Connection error: {e}")
            return False
    
    def disconnect(self):
        """Disconnect from automation portal"""
        try:
            self.driver.disconnect()
            self.connected = False
            print("✅ Disconnected from Automation Portal")
        except Exception as e:
            print(f"❌ Disconnect error: {e}")
    
    def initialize_system(self):
        """Initialize the automation portal system"""
        if not self.connected:
            print("❌ Not connected. Please connect first.")
            return
            
        print("🔧 Initializing system... (this may take up to 2 minutes)")
        try:
            result = self.driver.initialize()
            if result:
                print("✅ System initialized successfully")
            else:
                print("❌ Initialization failed")
            self.get_current_status()
        except Exception as e:
            print(f"❌ Initialization error: {e}")
    
    def extract_sample(self):
        """Extract sample from specified position"""
        if not self.connected:
            print("❌ Not connected. Please connect first.")
            return
            
        status = self.get_current_status()
        if not status or status.get('system_state') != 'OPERATIONAL':
            print("⚠️  System not operational. Initialize first.")
            return
            
        print("\nAvailable positions:")
        print("  0 - Position 0 (Drawer 2)")
        print("  1 - Position 1 (Drawer 1)")
        
        try:
            pos = input("Enter position (0 or 1): ").strip()
            if pos not in ['0', '1']:
                print("❌ Invalid position. Must be 0 or 1.")
                return
                
            print(f"🔄 Extracting sample from position {pos}...")
            result = self.driver.extract_drawer(int(pos))
            if result:
                print(f"✅ Sample extracted from position {pos}")
            else:
                print(f"❌ Extract failed from position {pos}")
            self.get_current_status()
        except Exception as e:
            print(f"❌ Extract error: {e}")
    
    def insert_sample(self):
        """Insert sample to specified position"""
        if not self.connected:
            print("❌ Not connected. Please connect first.")
            return
            
        status = self.get_current_status()
        if not status or status.get('system_state') != 'OPERATIONAL':
            print("⚠️  System not operational. Initialize first.")
            return
            
        print("\nAvailable positions:")
        print("  0 - Position 0 (Drawer 2)")
        print("  1 - Position 1 (Drawer 1)")
        
        try:
            pos = input("Enter position (0 or 1): ").strip()
            if pos not in ['0', '1']:
                print("❌ Invalid position. Must be 0 or 1.")
                return
                
            print(f"🔄 Inserting sample to position {pos}...")
            result = self.driver.insert_drawer(int(pos))
            if result:
                print(f"✅ Sample inserted to position {pos}")
            else:
                print(f"❌ Insert failed to position {pos}")
            self.get_current_status()
        except Exception as e:
            print(f"❌ Insert error: {e}")
    
    def get_version_info(self):
        """Get system version information"""
        if not self.connected:
            print("❌ Not connected. Please connect first.")
            return
            
        try:
            version = self.driver.report_version()
            print(f"📋 System Version: {version}")
        except Exception as e:
            print(f"❌ Version error: {e}")
    
    def show_menu(self):
        """Display the main menu"""
        print("\n📋 Available Operations:")
        print("  1. Connect to Automation Portal")
        print("  2. Get Current Status")
        print("  3. Initialize System")
        print("  4. Extract Sample (from instrument)")
        print("  5. Insert Sample (to instrument)")
        print("  6. Get Version Info")
        print("  7. Disconnect")
        print("  8. Clear Screen")
        print("  9. Exit")
        print()
    
    def run(self):
        """Run the interactive menu"""
        self.clear_screen()
        
        while True:
            self.print_header()
            self.print_status_bar()
            self.show_menu()
            
            try:
                choice = input("Enter your choice (1-9): ").strip()
                
                if choice == '1':
                    self.connect()
                elif choice == '2':
                    self.get_current_status()
                elif choice == '3':
                    self.initialize_system()
                elif choice == '4':
                    self.extract_sample()
                elif choice == '5':
                    self.insert_sample()
                elif choice == '6':
                    self.get_version_info()
                elif choice == '7':
                    self.disconnect()
                elif choice == '8':
                    self.clear_screen()
                    continue
                elif choice == '9':
                    if self.connected:
                        self.disconnect()
                    print("👋 Goodbye!")
                    sys.exit(0)
                else:
                    print("❌ Invalid choice. Please enter 1-9.")
                
                input("\nPress Enter to continue...")
                
            except KeyboardInterrupt:
                print("\n\n👋 Goodbye!")
                if self.connected:
                    self.disconnect()
                sys.exit(0)
            except Exception as e:
                print(f"❌ Unexpected error: {e}")
                input("\nPress Enter to continue...")

if __name__ == "__main__":
    menu = AutomationPortalMenu()
    menu.run()
