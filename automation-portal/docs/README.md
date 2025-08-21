# Documentation Directory

This directory contains documentation for the Waters Acquity UPC Driver project.

## Structure

- `proprietary/` - Waters proprietary documentation (excluded from git)
  - Place Waters protocol specifications and manuals here
  - These files are ignored by git to protect proprietary information

## Instructions for Documentation

1. Copy Waters documentation files to the `proprietary/` folder:
   ```
   copy "C:\Users\Administrator.WS\Documents\waters files\*.pdf" docs\proprietary\
   ```

2. The `.gitignore` file ensures these proprietary files won't be committed to version control

3. Create a local reference file if needed:
   ```
   docs\proprietary\README_LOCAL.txt
   ```

## Files to include in proprietary folder:

- 715008839 Automation Portal PC Protocol Specification.pdf
- Any other Waters manuals or documentation
- Configuration files specific to your system
- Method files and calibration data

Remember: These files contain proprietary information and should not be shared publicly.
