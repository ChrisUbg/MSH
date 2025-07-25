# Environment configuration for MSH project
# PowerShell version

# Pi Configuration
$PI_IP = "192.168.0.107"
$PI_USER = "chregg"
$PI_SSH_KEY = "~/.ssh/id_ed25519"
$PI_HOME = "/home/chregg"
$PI_MATTER_DIR = "$PI_HOME/matter-sdk"
$TRANSFER_TIMEOUT = 300

# Project Configuration
$PROJECT_ROOT = "/home/chris/RiderProjects/MSH"
$MATTER_DIR = "Matter"
$DEV_MATTER_DIR = "dev_connectedhomeip"

# Export variables for use in scripts
$env:PI_IP = $PI_IP
$env:PI_USER = $PI_USER
$env:PI_SSH_KEY = $PI_SSH_KEY
$env:PI_HOME = $PI_HOME
$env:PI_MATTER_DIR = $PI_MATTER_DIR
$env:TRANSFER_TIMEOUT = $TRANSFER_TIMEOUT
$env:PROJECT_ROOT = $PROJECT_ROOT
$env:MATTER_DIR = $MATTER_DIR
$env:DEV_MATTER_DIR = $DEV_MATTER_DIR

Write-Host "Environment variables set:"
Write-Host "PI_IP: $PI_IP"
Write-Host "PI_USER: $PI_USER"
Write-Host "PROJECT_ROOT: $PROJECT_ROOT"
