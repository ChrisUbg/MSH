#!/bin/bash
# Environment Switcher for MSH Project

set -e

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_header() {
    echo -e "${BLUE}$1${NC}"
}

# Function to activate commissioning server environment
activate_commissioning_server() {
    print_header "Switching to Commissioning Server Environment"
    
    # Check if we're in the right directory
    if [[ ! -f "main.py" ]] || [[ ! -d "commissioning_server" ]]; then
        print_error "Not in commissioning-server directory"
        print_status "Please run: cd commissioning-server"
        return 1
    fi
    
    # Check if virtual environment exists
    if [[ ! -d "venv" ]]; then
        print_warning "Virtual environment not found"
        read -p "Create virtual environment? (y/n): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            print_status "Creating virtual environment..."
            python3 -m venv venv
        else
            return 1
        fi
    fi
    
    # Activate virtual environment
    print_status "Activating virtual environment..."
    source venv/bin/activate
    
    # Verify activation
    if [[ -z "$VIRTUAL_ENV" ]]; then
        print_error "Failed to activate virtual environment"
        return 1
    fi
    
    print_status "Virtual environment activated: $VIRTUAL_ENV"
    
    # Check if dependencies are installed
    if ! python -c "import fastapi" 2>/dev/null; then
        print_warning "Dependencies not installed"
        read -p "Install dependencies? (y/n): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            print_status "Installing dependencies..."
            # Try basic requirements first
            if pip install -r requirements-basic.txt; then
                print_status "Basic dependencies installed successfully"
            else
                print_warning "Basic dependencies failed, trying minimal install..."
                pip install fastapi uvicorn pydantic pyyaml
            fi
            pip install -e .
        fi
    fi
    
    # Load environment variables
    if [[ -f ".env" ]]; then
        print_status "Loading environment variables..."
        export $(cat .env | grep -v '^#' | xargs)
    fi
    
    print_status "Commissioning server environment activated!"
    print_status "Current directory: $(pwd)"
    print_status "Python: $(which python)"
    print_status "Virtual env: $VIRTUAL_ENV"
    
    # Show available commands
    echo
    print_header "Available Commands:"
    echo "  python main.py --help          # Start server"
    echo "  python test-hardware.py        # Test hardware"
    echo "  python test-env.py             # Test environment"
    echo "  python -m pytest tests/        # Run tests"
    echo "  deactivate                     # Exit environment"
}

# Function to show current environment
show_current_env() {
    print_header "Current Environment Status"
    
    if [[ -n "$VIRTUAL_ENV" ]]; then
        print_status "Virtual Environment: $VIRTUAL_ENV"
    else
        print_warning "No virtual environment active"
    fi
    
    if [[ -f ".env" ]]; then
        print_status "Environment file: .env"
        print_status "Environment: $MSH_ENVIRONMENT"
        print_status "Mode: $MSH_MODE"
    else
        print_warning "No .env file found"
    fi
    
    if [[ -f "main.py" ]]; then
        print_status "Commissioning Server: Available"
    fi
    
    if [[ -d "venv" ]]; then
        print_status "Virtual Environment: Available"
    else
        print_warning "Virtual Environment: Not found"
    fi
}

# Function to create new environment
create_environment() {
    local env_type=$1
    
    case $env_type in
        "commissioning-server")
            print_header "Creating Commissioning Server Environment"
            
            # Create virtual environment
            if [[ ! -d "venv" ]]; then
                print_status "Creating virtual environment..."
                python3 -m venv venv
            fi
            
            # Create .env file
            if [[ ! -f ".env" ]]; then
                print_status "Creating .env file..."
                cat > .env << EOF
MSH_ENVIRONMENT=commissioning-server
MSH_MODE=development
MSH_LOG_LEVEL=DEBUG
MSH_HOST=0.0.0.0
MSH_PORT=8080
PI_IP=192.168.0.107
PI_USER=chregg
EOF
            fi
            
            # Install dependencies
            print_status "Installing dependencies..."
            source venv/bin/activate
            pip install --upgrade pip
            # Try basic requirements first
            if pip install -r requirements-basic.txt; then
                print_status "Basic dependencies installed successfully"
            else
                print_warning "Basic dependencies failed, trying minimal install..."
                pip install fastapi uvicorn pydantic pyyaml
            fi
            pip install -e .
            
            print_status "Commissioning server environment created!"
            ;;
            
        *)
            print_error "Unknown environment type: $env_type"
            print_status "Available: commissioning-server"
            return 1
            ;;
    esac
}

# Main script logic
case "${1:-}" in
    "commissioning-server"|"server"|"cs")
        activate_commissioning_server
        ;;
        
    "status"|"current")
        show_current_env
        ;;
        
    "create")
        create_environment "$2"
        ;;
        
    "help"|"-h"|"--help")
        print_header "MSH Environment Switcher"
        echo
        echo "Usage: $0 [COMMAND]"
        echo
        echo "Commands:"
        echo "  commissioning-server  Activate commissioning server environment"
        echo "  server               Alias for commissioning-server"
        echo "  cs                   Alias for commissioning-server"
        echo "  status              Show current environment status"
        echo "  create <type>       Create new environment"
        echo "  help                Show this help"
        echo
        echo "Examples:"
        echo "  $0 commissioning-server"
        echo "  $0 status"
        echo "  $0 create commissioning-server"
        ;;
        
    *)
        print_header "MSH Environment Switcher"
        echo
        echo "Available environments:"
        echo "  commissioning-server  PC commissioning server (BLE, Matter SDK)"
        echo
        echo "Usage: $0 [COMMAND]"
        echo "Run: $0 help for more information"
        ;;
esac 