# Hybrid Network Approach

## Overview
This document describes the simplified hybrid approach for managing the smart home network, combining the A1 modem's WiFi with the Raspberry Pi's capabilities for Matter device commissioning.

## Network Modes

### Normal Mode (Default)
- Raspberry Pi connected to A1 modem's WiFi
- Internet access available
- Regular smart home operations
- BLE communication with devices

### Commissioning Mode
- Raspberry Pi acts as Access Point
- Used only for Matter device commissioning
- Temporary mode
- Manual activation when needed

## Implementation

### Mode Switching
- Simple manual switch through UI
- Basic status indicator
- No automatic switching needed
- Clear user feedback

### User Interface
- Mode status display
- Simple mode switch button
- Commissioning instructions
- Basic error messages

### Commissioning Process
1. User activates Commissioning Mode
2. System switches to Access Point mode
3. User commissions new device
4. System returns to Normal Mode

## Requirements

### Hardware
- Raspberry Pi with WiFi and BLE
- A1 modem (existing)
- No additional hardware needed

### Software
- Mode switching script
- Basic UI controls
- Matter protocol implementation
- Network configuration management

## Benefits
- Low energy consumption
- No additional hardware
- Simple user management
- Single network for all devices
- Full control when needed

## Limitations
- Manual mode switching
- Temporary network changes during commissioning
- Basic error handling
- No automatic recovery

## User Instructions

### Normal Operation
- System runs in Normal Mode
- All devices connected to A1 network
- Regular smart home functionality

### Adding New Devices
1. Access smart home UI
2. Switch to Commissioning Mode
3. Follow device-specific commissioning steps
4. System returns to Normal Mode automatically

### Troubleshooting
- Manual mode switch if needed
- Basic error messages guide recovery
- Simple retry options
- Clear status indicators 