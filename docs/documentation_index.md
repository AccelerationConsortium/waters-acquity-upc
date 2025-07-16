# Waters Documentation Files - Local Index

This file helps you keep track of the Waters documentation you have locally.
This file is tracked in git but doesn't contain proprietary information.

## Available Documentation Files

### Protocol Specifications
- [ ] 715008839 Automation Portal PC Protocol Specification.pdf ✓ (Available)
- [ ] 715008535v00 Sample Transfer File Guide.pdf ✓ (Available)

### Method Files
- [ ] Add your method files here (.met, .method)

### Configuration Files
- [ ] Add your configuration files here (.cfg, .ini)

### Additional Documentation
- [ ] User manuals
- [ ] Installation guides
- [ ] Troubleshooting guides

## Notes

### Communication Protocol Summary
Based on the available documentation:
- Communication method: Serial/RS-232 or Ethernet
- Baud rate: Check your system specifications
- Data format: ASCII commands with specific terminators
- Response format: Status codes and data responses

### Key Commands to Implement
(Update this list as you review the documentation)
- [ ] System status queries
- [ ] Flow rate control
- [ ] Temperature control
- [ ] Method start/stop
- [ ] Data retrieval
- [ ] Error handling

### Implementation Status
- [x] Driver framework complete
- [x] Serial communication setup
- [x] Error handling structure
- [ ] Replace placeholder commands with actual Waters commands
- [ ] Test with hardware
- [ ] Validate response parsing

## Next Steps

1. Review the protocol specification PDF in `proprietary/`
2. Identify the exact command syntax for your system
3. Update the placeholder commands in `waters_acquity_driver.py`
4. Test the implementation with your Waters Acquity system

## Quick Reference

Waters Acquity Automation Portal Commands (implemented in driver):

```
Basic Commands:
- Identification: *IDN?
- Version: *VER?
- Module List: MODULES?
- System Status: STATUS?
- Error Status: ERROR?

Pump Commands (Binary Solvent Manager - BSM):
- Set Flow Rate: BSM:FLOW <rate>
- Get Flow Rate: FLOW?
- Prime Solvent: BSM:PRIME <A|B|C|D>

Column Manager Commands (CM):
- Set Temperature: CM:TEMP <temperature>
- Get Temperature: TEMP:COL?

Sample Manager Commands (SM):
- Set Injection Volume: SM:VOLUME <volume>

Run Control:
- Start Run: RUN:START <method_name>
- Stop Run: RUN:STOP
- Abort Run: RUN:ABORT

Method Management:
- Load Method: METHOD:LOAD <path>
- List Methods: METHOD:LIST?

Data Acquisition:
- Get Detector Data: <DETECTOR>:DATA? [start_time,end_time]
- Get System Data: DATA:<PARAMETER>?
- Detector Calibration: <DETECTOR>:CALIBRATE

System Monitoring:
- Pressure Reading: PRESSURE?
- Module Status: <MODULE>:STATUS?
```

### Communication Protocol Details

**Serial Communication:**
- Baud Rate: 9600 (typical)
- Data Bits: 8
- Parity: None
- Stop Bits: 1
- Terminator: \r\n

**TCP/IP Communication:**
- Default Port: 34567
- Protocol: ASCII over TCP
- Terminator: \r\n

### Response Codes

**Status Codes:**
- 0: Ready
- 1: Running
- 2: Error
- 3: Standby
- 4: Maintenance
- 5: Initializing

**Success Indicators:**
- OK, READY, COMPLETE, SUCCESS

**Error Indicators:**
- ERROR, ERR, FAIL, INVALID
