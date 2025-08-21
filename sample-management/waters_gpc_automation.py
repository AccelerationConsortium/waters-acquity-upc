#!/usr/bin/env python3
"""
Waters GPC Training Automation
Main automation script for Waters GPC Training project
Combines COM connection and STF processing for sample set execution
"""

from empower_com_interface import EmpowerConnection
from stf_processor import STFProcessor
from datetime import datetime
import time
from typing import Dict, List

class WatersGPCAutomation:
    """Main automation class for Waters GPC Training project"""
    
    # Project configuration
    PROJECT_NAME = "Waters GPC Training"
    SYSTEM_NAME = "ARC HPLC"
    NODE_NAME = "Waters-h4q6k34"
    DATABASE_NAME = "Waters GPC Training"
    
    def __init__(self, stf_directory: str = "C:\\STF"):
        self.empower = EmpowerConnection(stf_directory)
        self.stf_processor = STFProcessor(stf_directory)
        self.stf_directory = stf_directory
        
    def verify_empower_connection(self, retry_count: int = 3) -> bool:
        """
        Verify connection to Empower with retry logic (adapted from DataCiphCode patterns)
        
        Args:
            retry_count: Number of connection attempts
            
        Returns:
            bool: True if connection successful
        """
        print("🔍 Verifying Empower connection...")
        
        for attempt in range(retry_count):
            if self.empower.connect():
                info = self.empower.get_connection_info()
                print(f"✅ Empower connection verified: {info['status']}")
                
                # Additional connection validation (DataCiphCode style)
                try:
                    # Test that the connection object is accessible
                    if self.empower.connection:
                        connection_test = str(self.empower.connection)
                        if "Millennium.Project" in connection_test:
                            print(f"📡 Connection validated: {connection_test}")
                            return True
                        else:
                            print(f"⚠️ Connection object unexpected: {connection_test}")
                    else:
                        print("⚠️ Connection object is None")
                except Exception as e:
                    print(f"⚠️ Connection validation failed: {e}")
                    
            print(f"❌ Connection attempt {attempt + 1}/{retry_count} failed")
            if attempt < retry_count - 1:
                print("⏳ Retrying in 1 second...")
                time.sleep(1)
        
        print("❌ All Empower connection attempts failed")
        return False
    
    def execute_sample_set_with_monitoring(self, sample_set_name: str) -> Dict:
        """
        Execute sample set with enhanced monitoring (DataCiphCode inspired)
        
        Args:
            sample_set_name: Name of sample set to execute
            
        Returns:
            Dict: Enhanced execution results with detailed status
        """
        print(f"🎯 Executing sample set with monitoring: '{sample_set_name}'")
        
        execution_log = {
            "sample_set": sample_set_name,
            "start_time": datetime.now(),
            "steps_completed": [],
            "errors_encountered": [],
            "final_status": "pending"
        }
        
        try:
            # Step 1: Verify Empower connection with retry
            print("📡 Step 1: Verifying Empower connection...")
            if not self.verify_empower_connection(retry_count=3):
                execution_log["errors_encountered"].append("Empower connection failed after retries")
                execution_log["final_status"] = "connection_failed"
                return {
                    "success": False,
                    "error": "Failed to connect to Empower after multiple attempts",
                    "execution_log": execution_log
                }
            execution_log["steps_completed"].append("empower_connection_verified")
            
            # Step 2: Create STF file with validation
            print("📄 Step 2: Creating and validating STF file...")
            stf_file = self.stf_processor.create_stf_for_sample_set(
                sample_set_name=sample_set_name,
                project_path=self.PROJECT_NAME,
                database=self.DATABASE_NAME,
                system=self.SYSTEM_NAME,
                node=self.NODE_NAME
            )
            
            # Validate STF file was created
            if not stf_file.exists():
                execution_log["errors_encountered"].append(f"STF file not created: {stf_file}")
                execution_log["final_status"] = "stf_creation_failed"
                return {
                    "success": False,
                    "error": f"STF file creation failed: {stf_file}",
                    "execution_log": execution_log
                }
            execution_log["steps_completed"].append("stf_file_created")
            execution_log["stf_file_path"] = str(stf_file)
            
            # Step 3: Process STF file with status monitoring
            print("🔄 Step 3: Processing STF file...")
            result = self.stf_processor.process_stf_file(stf_file)
            execution_log["steps_completed"].append("stf_file_processed")
            
            # Step 4: Validate processing results
            if result["success"]:
                execution_log["steps_completed"].append("stf_processing_successful")
                execution_log["processed_file"] = result["processed_file"]
                execution_log["final_status"] = "completed_successfully"
                
                print(f"✅ Sample set execution completed successfully: {sample_set_name}")
                
                # Enhanced return with execution log
                return {
                    "success": True,
                    "sample_set": sample_set_name,
                    "stf_file": str(stf_file),
                    "processed_file": result["processed_file"],
                    "execution_time": (datetime.now() - execution_log["start_time"]).total_seconds(),
                    "execution_log": execution_log,
                    "timestamp": datetime.now().isoformat()
                }
            else:
                execution_log["errors_encountered"].append(f"STF processing failed: {result.get('error', 'Unknown error')}")
                execution_log["final_status"] = "stf_processing_failed"
                
                return {
                    "success": False,
                    "error": result.get("error", "STF processing failed"),
                    "execution_log": execution_log
                }
                
        except Exception as e:
            error_msg = f"Execution exception: {str(e)}"
            execution_log["errors_encountered"].append(error_msg)
            execution_log["final_status"] = "exception_occurred"
            
            print(f"❌ Execution failed with exception: {e}")
            return {
                "success": False,
                "error": error_msg,
                "execution_log": execution_log
            }
            
        finally:
            # Always disconnect (DataCiphCode pattern)
            print("🔌 Disconnecting from Empower...")
            self.empower.disconnect()
            execution_log["end_time"] = datetime.now()
        """
        Execute a sample set using STF automation
        
        Args:
            sample_set_name: Name of sample set to execute
            
        Returns:
            Dict: Execution results
        """
        print(f"🎯 Executing sample set: '{sample_set_name}'")
        
        # Verify Empower connection
        if not self.verify_empower_connection():
            return {
                "success": False,
                "error": "Failed to connect to Empower",
                "sample_set": sample_set_name
            }
        
        try:
            # Create STF file for the sample set
            print("📄 Creating STF file...")
            stf_file = self.stf_processor.create_stf_for_sample_set(
                sample_set_name=sample_set_name,
                project_path=self.PROJECT_NAME,
                database=self.DATABASE_NAME,
                system=self.SYSTEM_NAME,
                node=self.NODE_NAME
            )
            
            # Process the STF file
            print("🔄 Processing STF file...")
            result = self.stf_processor.process_stf_file(stf_file)
            
            # Disconnect from Empower
            self.empower.disconnect()
            
            if result["success"]:
                print(f"✅ Sample set execution initiated: {sample_set_name}")
                return {
                    "success": True,
                    "sample_set": sample_set_name,
                    "stf_file": str(stf_file),
                    "processed_file": result["processed_file"],
                    "timestamp": datetime.now().isoformat()
                }
            else:
                print(f"❌ STF processing failed: {result['error']}")
                return {
                    "success": False,
                    "error": result["error"],
                    "sample_set": sample_set_name
                }
                
        except Exception as e:
            print(f"❌ Execution failed: {e}")
            self.empower.disconnect()
            return {
                "success": False,
                "error": str(e),
                "sample_set": sample_set_name
            }
    
    def execute_multiple_sample_sets(self, sample_set_names: List[str]) -> List[Dict]:
        """
        Execute multiple sample sets
        
        Args:
            sample_set_names: List of sample set names to execute
            
        Returns:
            List[Dict]: List of execution results
        """
        print(f"🎯 Executing {len(sample_set_names)} sample sets...")
        
        results = []
        for sample_set_name in sample_set_names:
            result = self.execute_sample_set(sample_set_name)
            results.append(result)
        
        # Summary
        successful = sum(1 for r in results if r["success"])
        print(f"📊 Execution Summary: {successful}/{len(results)} successful")
        
        return results
    
    def get_system_status(self) -> Dict:
        """
        Get current system status
        
        Returns:
            Dict: System status information
        """
        status = {
            "project": self.PROJECT_NAME,
            "system": self.SYSTEM_NAME,
            "node": self.NODE_NAME,
            "empower_connected": False,
            "stf_directory": self.stf_directory,
            "timestamp": datetime.now().isoformat()
        }
        
        # Test Empower connection
        if self.empower.connect():
            status["empower_connected"] = True
            status["connection_info"] = self.empower.get_connection_info()
            self.empower.disconnect()
        
        return status

def main():
    """Main automation demonstration with enhanced DataCiphCode patterns"""
    print("=" * 60)
    print("🚀 Waters GPC Training Automation (Enhanced)")
    print("=" * 60)
    
    # Initialize automation
    automation = WatersGPCAutomation()
    
    # Get system status
    print("\n📊 System Status:")
    status = automation.get_system_status()
    for key, value in status.items():
        print(f"  {key}: {value}")
    
    # Test enhanced sample set execution
    print("\n🎯 Testing Enhanced Sample Set Execution:")
    sample_set_name = "test cjs"  # Your actual sample set name
    
    # Use the enhanced method with DataCiphCode patterns
    result = automation.execute_sample_set_with_monitoring(sample_set_name)
    
    print(f"\n📋 Enhanced Execution Result:")
    for key, value in result.items():
        if key == "execution_log":
            print(f"  📝 Execution Log:")
            log = value
            print(f"    ⏱️  Start Time: {log.get('start_time', 'N/A')}")
            print(f"    ⏱️  End Time: {log.get('end_time', 'N/A')}")
            print(f"    ✅ Steps Completed: {len(log.get('steps_completed', []))}")
            for step in log.get('steps_completed', []):
                print(f"      - {step}")
            print(f"    ❌ Errors: {len(log.get('errors_encountered', []))}")
            for error in log.get('errors_encountered', []):
                print(f"      - {error}")
            print(f"    🏁 Final Status: {log.get('final_status', 'unknown')}")
        else:
            print(f"  {key}: {value}")
    
    print("\n" + "=" * 60)
    print("✅ Enhanced automation test complete!")
    print("💡 Check C:\\STF\\ directory for processed files")
    print("💡 Check Empower Audit Trail for connection entries")
    print("💡 Review enhanced_empower_connection.log for detailed logs")
    print("=" * 60)

if __name__ == "__main__":
    main()
