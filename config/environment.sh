#!/bin/bash
# Cross-Platform Environment Configuration
# This script detects the OS and sets appropriate path variables

# OS Detection
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" ]]; then
    OS="windows"
    SEPARATOR="//"
    PROJECT_ROOT="C://Users//Dev//source//repos//MSH"
    USER_HOME="C://Users//Dev"
    PYTHON_CMD="python"
elif [[ "$OSTYPE" == "linux-gnu" ]]; then
    OS="linux"
    SEPARATOR="/"
    PROJECT_ROOT="$(pwd)"
    USER_HOME="$HOME"
    PYTHON_CMD="python3"
else
    # Default to Linux-style for WSL2 and other Unix-like systems
    OS="linux"
    SEPARATOR="/"
    PROJECT_ROOT="$(pwd)"
    USER_HOME="$HOME"
    PYTHON_CMD="python3"
fi

# Raspberry Pi Configuration
PI_IP="${PI_IP}"
PI_USER="chregg"
PI_HOSTNAME="msh.local"

# Development Machine Configuration
DEV_USER="chris"
DEV_IP="192.168.0.100"  # Your development machine IP

# Project Paths
SRC_DIR="${PROJECT_ROOT}${SEPARATOR}src"
MATTER_DIR="${PROJECT_ROOT}${SEPARATOR}Matter"
DOCKER_DIR="${PROJECT_ROOT}${SEPARATOR}docker"
SCRIPTS_DIR="${PROJECT_ROOT}${SEPARATOR}scripts"
DOCS_DIR="${PROJECT_ROOT}${SEPARATOR}docs"

# C# Project Paths
MSH_WEB_DIR="${SRC_DIR}${SEPARATOR}MSH.Web"
MSH_INFRASTRUCTURE_DIR="${SRC_DIR}${SEPARATOR}MSH.Infrastructure"
MSH_CORE_DIR="${SRC_DIR}${SEPARATOR}MSH.Core"
MSH_MATTER_DIR="${SRC_DIR}${SEPARATOR}MSH.Matter"

# Docker Compose Files
DOCKER_COMPOSE_DEV="${PROJECT_ROOT}${SEPARATOR}docker-compose.dev-msh.yml"
DOCKER_COMPOSE_PROD="${PROJECT_ROOT}${SEPARATOR}docker-compose.prod-msh.yml"
DOCKER_COMPOSE_PI="${PROJECT_ROOT}${SEPARATOR}docker-compose.prod-pi.yml"

# Build Output Paths
PUBLISH_DIR="${PROJECT_ROOT}${SEPARATOR}publish"
BUILD_DIR="${PROJECT_ROOT}${SEPARATOR}build"

# Matter SDK Paths
MATTER_SDK_DIR="${PROJECT_ROOT}${SEPARATOR}dev_connectedhomeip"
MATTER_BUILD_DIR="${MATTER_SDK_DIR}${SEPARATOR}out"

# Export all variables for use in scripts
export OS SEPARATOR PROJECT_ROOT USER_HOME PYTHON_CMD
export PI_IP PI_USER PI_HOSTNAME DEV_USER DEV_IP
export SRC_DIR MATTER_DIR DOCKER_DIR SCRIPTS_DIR DOCS_DIR
export MSH_WEB_DIR MSH_INFRASTRUCTURE_DIR MSH_CORE_DIR MSH_MATTER_DIR
export DOCKER_COMPOSE_DEV DOCKER_COMPOSE_PROD DOCKER_COMPOSE_PI
export PUBLISH_DIR BUILD_DIR MATTER_SDK_DIR MATTER_BUILD_DIR

# Function to get cross-platform path
get_path() {
    local path="$1"
    if [[ "$OS" == "windows" ]]; then
        echo "$path" | sed 's///////g'
    else
        echo "$path"
    fi
}

# Function to check if running in WSL2
is_wsl2() {
    if [[ -n "$WSL_DISTRO_NAME" ]]; then
        return 0
    else
        return 1
    fi
}

# Function to get WSL2 username
get_wsl_username() {
    if is_wsl2; then
        echo "$USER"
    else
        echo "chris"  # Default fallback
    fi
}

# Function to get Windows path from WSL2
get_windows_path() {
    local wsl_path="$1"
    if is_wsl2; then
        echo "$wsl_path" | sed 's|^/mnt/c/|C://|' | sed 's|/|//|g'
    else
        echo "$wsl_path"
    fi
}

# Function to get WSL2 path from Windows
get_wsl_path() {
    local windows_path="$1"
    if is_wsl2; then
        echo "$windows_path" | sed 's|^C://|/mnt/c/|' | sed 's|//|/|g'
    else
        echo "$windows_path"
    fi
}

# Export utility functions
export -f get_path is_wsl2 get_wsl_username get_windows_path get_wsl_path

# Print configuration for debugging
if [[ "${DEBUG:-false}" == "true" ]]; then
    echo "=== Cross-Platform Environment Configuration ==="
    echo "OS: $OS"
    echo "Separator: $SEPARATOR"
    echo "Project Root: $PROJECT_ROOT"
    echo "User Home: $USER_HOME"
    echo "Python Command: $PYTHON_CMD"
    echo "Pi IP: $PI_IP"
    echo "Pi User: $PI_USER"
    echo "Pi Hostname: $PI_HOSTNAME"
    echo "Dev User: $DEV_USER"
    echo "Dev IP: $DEV_IP"
    echo "WSL2: $(is_wsl2 && echo 'Yes' || echo 'No')"
    echo "WSL Username: $(get_wsl_username)"
    echo "=============================================="
fi 