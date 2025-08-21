#!/usr/bin/env python3
"""
STF (Sample Transfer File) Processor
Handles Waters STF specification 715008535 JSON file creation and processing
"""

import json
from datetime import datetime
from pathlib import Path
from typing import List, Dict, Optional

class STFProcessor:
    """Process STF files for Empower sample set automation"""
    
    def __init__(self, stf_directory: str = "C:\\STF"):
        self.stf_directory = Path(stf_directory)
        self.stf_directory.mkdir(exist_ok=True)
    
    def create_stf_json(
        self,
        sample_set_names: List[str],
        project_path: str = "Waters GPC Training",
        database: str = "Waters GPC Training",
        username: str = "system",
        password: str = "manager",
        system: str = "ARC HPLC",
        node: str = "Waters-h4q6k34"
    ) -> Dict:
        """
        Create STF JSON following Waters specification
        
        Args:
            sample_set_names: List of sample set names to execute
            project_path: Empower project path
            database: Empower database name
            username: Empower username
            password: Empower password
            system: System name (e.g., "ARC HPLC")
            node: Node name (e.g., "Waters-h4q6k34")
            
        Returns:
            Dict: STF JSON structure
        """
        
        # Create sample set details
        sample_set_details = []
        for i, sample_set_name in enumerate(sample_set_names):
            detail = {
                "SampleSetName": sample_set_name,
                "Action": "Execute",
                "New": False,
                "ExperimentId": 1000 + i,
                "Status": None,
                "ExecutionReport": None
            }
            sample_set_details.append(detail)
        
        # Create STF structure
        stf_json = {
            "HeaderFields": {
                "EmpowerProject": project_path,
                "EmpowerDatabase": database,
                "EmpowerUn": username,
                "EmpowerPw": password,
                "System": system,
                "Node": node,
                "SampleSets": len(sample_set_names),
                "Action": "ExecuteExisting"
            },
            "SampleSetDetails": sample_set_details,
            "TrailerReport": {
                "FileVerified": False,
                "FileProcessed": False,
                "FileStatus": "Created",
                "FileProcessReport": None,
                "CreatedAt": datetime.now().isoformat()
            }
        }
        
        return stf_json
    
    def save_stf_file(self, stf_data: Dict, filename_prefix: str = "STF") -> Path:
        """
        Save STF JSON to .new.json file
        
        Args:
            stf_data: STF JSON data
            filename_prefix: Prefix for filename
            
        Returns:
            Path: Path to created file
        """
        timestamp = datetime.now().strftime("%y%m%d_%H%M")
        filename = f"{filename_prefix}_001_{timestamp}.new.json"
        file_path = self.stf_directory / filename
        
        with open(file_path, 'w', encoding='utf-8') as f:
            json.dump(stf_data, f, indent=2, ensure_ascii=False)
        
        print(f"✅ STF file created: {file_path}")
        return file_path
    
    def process_stf_file(self, stf_file_path: Path) -> Dict:
        """
        Process a .new.json STF file
        
        Args:
            stf_file_path: Path to STF file to process
            
        Returns:
            Dict: Processing results
        """
        try:
            # Load STF file
            with open(stf_file_path, 'r') as f:
                stf_data = json.load(f)
            
            # Update trailer report
            stf_data["TrailerReport"]["FileProcessed"] = True
            stf_data["TrailerReport"]["FileStatus"] = "Processed"
            stf_data["TrailerReport"]["ProcessedAt"] = datetime.now().isoformat()
            
            # Create processed filename
            processed_file = stf_file_path.with_suffix('.prc.json')
            processed_file = processed_file.with_name(
                processed_file.name.replace('.new.prc.json', '.prc.json')
            )
            
            # Save processed file
            with open(processed_file, 'w') as f:
                json.dump(stf_data, f, indent=2)
            
            # Remove original .new file
            stf_file_path.unlink()
            
            print(f"✅ STF file processed: {processed_file}")
            
            return {
                "success": True,
                "processed_file": str(processed_file),
                "sample_sets": [detail["SampleSetName"] for detail in stf_data.get("SampleSetDetails", [])]
            }
            
        except Exception as e:
            print(f"❌ STF processing failed: {e}")
            return {
                "success": False,
                "error": str(e)
            }
    
    def process_pending_files(self) -> List[Dict]:
        """
        Process all pending .new.json STF files
        
        Returns:
            List[Dict]: List of processing results
        """
        pending_files = list(self.stf_directory.glob("*.new.json"))
        results = []
        
        print(f"🔍 Found {len(pending_files)} pending STF files")
        
        for stf_file in pending_files:
            print(f"📄 Processing: {stf_file.name}")
            result = self.process_stf_file(stf_file)
            results.append(result)
        
        return results
    
    def create_stf_for_sample_set(self, sample_set_name: str, **kwargs) -> Path:
        """
        Convenience method to create STF for a single sample set
        
        Args:
            sample_set_name: Name of sample set to execute
            **kwargs: Additional parameters for create_stf_json
            
        Returns:
            Path: Path to created STF file
        """
        stf_data = self.create_stf_json([sample_set_name], **kwargs)
        return self.save_stf_file(stf_data, f"Execute_{sample_set_name.replace(' ', '_')}")

def main():
    """Test STF processor"""
    print("🔍 Testing STF Processor...")
    
    processor = STFProcessor()
    
    # Create test STF file
    stf_file = processor.create_stf_for_sample_set("test cjs")
    print(f"📄 Created: {stf_file}")
    
    # Process pending files
    results = processor.process_pending_files()
    print(f"📊 Processing results: {results}")

if __name__ == "__main__":
    main()
