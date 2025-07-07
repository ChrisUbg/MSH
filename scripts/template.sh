#!/bin/bash
# Script Template for Cross-Platform Compatibility

set -e

# Source environment configuration
source config/environment.sh

# Script variables
SCRIPT_NAME="$(basename "$0")"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Function to log messages
log() {
    echo "[$(date +'%Y-%m-%d %H:%M:%S')] $1"
}

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Function to check if file exists
file_exists() {
    [[ -f "$1" ]]
}

# Function to check if directory exists
dir_exists() {
    [[ -d "$1" ]]
}

# Main script logic
main() {
    log "Starting $SCRIPT_NAME"
    
    # Your script logic here
    log "Script completed successfully"
}

# Run main function
main "$@"
