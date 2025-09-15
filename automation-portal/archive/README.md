# Archive

This directory contains the original files before the automation portal was focused on sample transfer operations only.

## Files:

- **automation_portal_driver_original.py** - Original driver that incorrectly mixed sample transfer with chromatography functions
- **example_usage_original.py** - Original examples that included non-portal functionality  
- **CLEANUP_SUMMARY.md** - Documentation of what was changed during the refactoring

## What Changed:

The automation portal was refactored to focus only on its actual purpose: **sample transfer operations** (extract/insert drawers, status monitoring). All chromatography-related functions were removed as they don't belong in an automation portal driver.

The current automation portal driver now only handles:
- Extract/Insert drawer operations
- System status monitoring  
- Portal initialization and control
- Door and tray status checking
