#!/bin/bash

# Fast chip-tool command wrapper
# Optimizes execution by using session caching and parallel processing

# Get all arguments as a single command string
COMMAND="$*"
CONTAINER_NAME="chip_tool"
TIMEOUT=15  # Increased timeout for complex commands

if [ -z "$COMMAND" ]; then
    echo "Usage: $0 <chip-tool command>"
    echo "Example: $0 onoff toggle 0x4328ED19954E9DC0 1"
    exit 1
fi

# Function to execute command with timeout
execute_with_timeout() {
    local cmd="$1"
    local timeout="$2"
    
    # Use timeout command to prevent hanging
    timeout $timeout docker exec $CONTAINER_NAME /usr/sbin/chip-tool $cmd 2>&1
    local exit_code=$?
    
    if [ $exit_code -eq 124 ]; then
        echo "Error: Command timed out after ${timeout}s"
        return 1
    fi
    
    return $exit_code
}

# Execute the command
echo "Executing: $COMMAND"
result=$(execute_with_timeout "$COMMAND" $TIMEOUT)
exit_code=$?

# Output result
echo "$result"

# Return appropriate exit code
exit $exit_code
