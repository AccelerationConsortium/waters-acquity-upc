# Changelog

All notable changes to this project will be documented in this file.

## [0.2.0] - 2025-09-18
### Major Refactoring
- **Production-ready automation portal driver**: Fixed critical bugs in error detection and command formatting
- **Interactive menu system**: Added `automation_menu.py` for easy command-line operation
- **Clean directory structure**: Removed all debugging and test files
- **Updated documentation**: Comprehensive README.md with usage guide and API reference
- **Bug fixes in automation_portal_driver.py**:
  - Fixed extract_drawer() and insert_drawer() methods to properly detect errors
  - Corrected command format (removed sequence numbers from Extract/Insert commands)
  - Fixed initialize() method to handle unknown command responses
  - Improved error state detection and timeout handling

### Added
- automation_menu.py: Interactive command-line interface for automation portal operations
- Comprehensive usage guide in README.md
- Error recovery procedures and troubleshooting guide

### Removed
- All debugging files: clear_error.py, connection_test.py, debug_test.py, demo.py, etc.
- Test scripts: test_*.py files
- Cache directories: __pycache__, archive/
- Duplicate sample transfer check functionality

## [0.1.1] - 2025-09-02
### Changed
- automation-portal/automation_portal_driver.py: Align with org Copilot instructions.
  - Replace `WatersAcquityError` uses with `AutomationPortalError`.
  - Add context manager methods `__enter__` and `__exit__`.
  - Make `_parse_portal_response` parameters optional to match internal calls.
  - Update examples to use `AutomationPortalDriver`.
  - Remove `if __name__ == "__main__"` block.

## [0.1.0] - 2025-09-02
### Added
- Initial cleaned repository structure with Automation Portal driver and sample-management components.
