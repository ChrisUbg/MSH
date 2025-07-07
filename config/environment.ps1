# Cross-Platform Environment Configuration for PowerShell

# OS Detection
$OS = "windows"
$SEPARATOR = "\"
$PROJECT_ROOT = "C:\Users\Dev\source\repos\MSH"
$USER_HOME = "C:\Users\Dev"
$PYTHON_CMD = "python"

# Raspberry Pi Configuration
$PI_IP = "192.168.0.102"
$PI_USER = "chregg"
$PI_HOSTNAME = "msh.local"

# Development Machine Configuration
$DEV_USER = "chris"
$DEV_IP = "192.168.0.100"

# Project Paths
$SRC_DIR = "$PROJECT_ROOT\src"
$MATTER_DIR = "$PROJECT_ROOT\Matter"
$DOCKER_DIR = "$PROJECT_ROOT\docker"
$SCRIPTS_DIR = "$PROJECT_ROOT\scripts"
$DOCS_DIR = "$PROJECT_ROOT\docs"

# C# Project Paths
$MSH_WEB_DIR = "$SRC_DIR\MSH.Web"
$MSH_INFRASTRUCTURE_DIR = "$SRC_DIR\MSH.Infrastructure"
$MSH_CORE_DIR = "$SRC_DIR\MSH.Core"
$MSH_MATTER_DIR = "$SRC_DIR\MSH.Matter"

# Docker Compose Files
$DOCKER_COMPOSE_DEV = "$PROJECT_ROOT\docker-compose.dev-msh.yml"
$DOCKER_COMPOSE_PROD = "$PROJECT_ROOT\docker-compose.prod-msh.yml"
$DOCKER_COMPOSE_PI = "$PROJECT_ROOT\docker-compose.prod-pi.yml"

# Build Output Paths
$PUBLISH_DIR = "$PROJECT_ROOT\publish"
$BUILD_DIR = "$PROJECT_ROOT\build"

# Matter SDK Paths
$MATTER_SDK_DIR = "$PROJECT_ROOT\dev_connectedhomeip"
$MATTER_BUILD_DIR = "$MATTER_SDK_DIR\out"

# Function to get cross-platform path
function Get-CrossPlatformPath {
    param([string]$Path)
    return $Path -replace '/', '\'
}

# Function to check if running in WSL2
function Test-WSL2 {
    return $env:WSL_DISTRO_NAME -ne $null
}

# Function to get WSL2 username
function Get-WSLUsername {
    if (Test-WSL2) {
        return $env:USERNAME
    } else {
        return "chris"
    }
}

# Export variables for use in scripts
$env:OS = $OS
$env:SEPARATOR = $SEPARATOR
$env:PROJECT_ROOT = $PROJECT_ROOT
$env:USER_HOME = $USER_HOME
$env:PYTHON_CMD = $PYTHON_CMD
$env:PI_IP = $PI_IP
$env:PI_USER = $PI_USER
$env:PI_HOSTNAME = $PI_HOSTNAME
$env:DEV_USER = $DEV_USER
$env:DEV_IP = $DEV_IP
$env:SRC_DIR = $SRC_DIR
$env:MATTER_DIR = $MATTER_DIR
$env:DOCKER_DIR = $DOCKER_DIR
$env:SCRIPTS_DIR = $SCRIPTS_DIR
$env:DOCS_DIR = $DOCS_DIR
$env:MSH_WEB_DIR = $MSH_WEB_DIR
$env:MSH_INFRASTRUCTURE_DIR = $MSH_INFRASTRUCTURE_DIR
$env:MSH_CORE_DIR = $MSH_CORE_DIR
$env:MSH_MATTER_DIR = $MSH_MATTER_DIR
$env:DOCKER_COMPOSE_DEV = $DOCKER_COMPOSE_DEV
$env:DOCKER_COMPOSE_PROD = $DOCKER_COMPOSE_PROD
$env:DOCKER_COMPOSE_PI = $DOCKER_COMPOSE_PI
$env:PUBLISH_DIR = $PUBLISH_DIR
$env:BUILD_DIR = $BUILD_DIR
$env:MATTER_SDK_DIR = $MATTER_SDK_DIR
$env:MATTER_BUILD_DIR = $MATTER_BUILD_DIR

# Print configuration for debugging
if ($env:DEBUG -eq "true") {
    Write-Host "=== Cross-Platform Environment Configuration ==="
    Write-Host "OS: $OS"
    Write-Host "Separator: $SEPARATOR"
    Write-Host "Project Root: $PROJECT_ROOT"
    Write-Host "User Home: $USER_HOME"
    Write-Host "Python Command: $PYTHON_CMD"
    Write-Host "Pi IP: $PI_IP"
    Write-Host "Pi User: $PI_USER"
    Write-Host "Pi Hostname: $PI_HOSTNAME"
    Write-Host "Dev User: $DEV_USER"
    Write-Host "Dev IP: $DEV_IP"
    Write-Host "WSL2: $(if (Test-WSL2) { 'Yes' } else { 'No' })"
    Write-Host "WSL Username: $(Get-WSLUsername)"
    Write-Host "=============================================="
}
