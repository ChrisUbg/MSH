# MSH Commissioning Server Installation Script for Windows 11

param(
    [switch]$SkipPrompts
)

Write-Host "🚀 Installing MSH Commissioning Server..." -ForegroundColor Green

# Function to print colored output
function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Check if running as administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "This script should be run as Administrator"
    exit 1
}

# Check Windows version
$osInfo = Get-CimInstance -ClassName Win32_OperatingSystem
if ($osInfo.Caption -notlike "*Windows 11*") {
    Write-Warning "This script is designed for Windows 11. You're running: $($osInfo.Caption)"
    if (-not $SkipPrompts) {
        $continue = Read-Host "Continue anyway? (y/n)"
        if ($continue -ne "y") {
            exit 1
        }
    }
}

# Install Chocolatey if not present
if (-not (Get-Command choco -ErrorAction SilentlyContinue)) {
    Write-Status "Installing Chocolatey package manager..."
    Set-ExecutionPolicy Bypass -Scope Process -Force
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
    iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
}

# Install system dependencies
Write-Status "Installing system dependencies..."
choco install -y python311 git curl wget jq

# Install Visual Studio Build Tools (for Matter SDK compilation)
Write-Status "Installing Visual Studio Build Tools..."
choco install -y visualstudio2022buildtools
choco install -y visualstudio2022-workload-vctools

# Refresh environment variables
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

# Create project directory
$projectDir = "$env:USERPROFILE\msh-commissioning-server"
Write-Status "Creating project directory: $projectDir"
New-Item -ItemType Directory -Force -Path $projectDir | Out-Null
Set-Location $projectDir

# Create virtual environment
Write-Status "Creating Python virtual environment..."
python -m venv venv
& "$projectDir\venv\Scripts\Activate.ps1"

# Upgrade pip
Write-Status "Upgrading pip..."
python -m pip install --upgrade pip

# Install Python dependencies
Write-Status "Installing Python dependencies..."
pip install -r requirements.txt

# Install the commissioning server
Write-Status "Installing MSH Commissioning Server..."
pip install -e .

# Create configuration directory
Write-Status "Creating configuration directory..."
$configDir = "$env:USERPROFILE\.msh"
New-Item -ItemType Directory -Force -Path $configDir | Out-Null

# Create default configuration
Write-Status "Creating default configuration..."
$configContent = @"
server:
  host: "0.0.0.0"
  port: 8080
  debug: false

matter:
  sdk_path: "C:\matter-sdk"
  chip_tool_path: "C:\matter-sdk\out\chip-tool.exe"
  chip_repl_path: "C:\matter-sdk\out\chip-repl.exe"
  fabric_id: "1"
  node_id: "112233"

bluetooth:
  adapter: "hci0"
  timeout: 30
  scan_duration: 10

storage:
  type: "sqlite"
  path: "$configDir\credentials.db"

security:
  api_key_required: false
  allowed_hosts: ["192.168.0.0/24"]
  encrypt_credentials: true

pi:
  default_ip: "192.168.0.104"
  default_user: "chregg"
  ssh_key_path: "$env:USERPROFILE\.ssh\id_ed25519"
"@

$configContent | Out-File -FilePath "$configDir\config.yaml" -Encoding UTF8

# Create Windows Service (optional)
if (-not $SkipPrompts) {
    $installService = Read-Host "Do you want to install as a Windows Service? (y/n)"
    if ($installService -eq "y") {
        Write-Status "Creating Windows Service..."
        
        # Install NSSM (Non-Sucking Service Manager)
        choco install -y nssm
        
        # Create service
        $serviceName = "MSHCommissioningServer"
        $exePath = "$projectDir\venv\Scripts\msh-commissioning-server.exe"
        $args = "--host 0.0.0.0 --port 8080"
        
        nssm install $serviceName $exePath $args
        nssm set $serviceName AppDirectory $projectDir
        nssm set $serviceName Description "MSH Commissioning Server - Native PC application for Matter device commissioning"
        nssm set $serviceName Start SERVICE_AUTO_START
        
        Write-Status "Windows Service installed: $serviceName"
    }
}

# Create desktop shortcut (optional)
if (-not $SkipPrompts) {
    $createShortcut = Read-Host "Do you want to create a desktop shortcut? (y/n)"
    if ($createShortcut -eq "y") {
        Write-Status "Creating desktop shortcut..."
        
        $WshShell = New-Object -comObject WScript.Shell
        $Shortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\MSH Commissioning Server.lnk")
        $Shortcut.TargetPath = "$projectDir\venv\Scripts\msh-commissioning-server.exe"
        $Shortcut.Arguments = "--host 0.0.0.0 --port 8080"
        $Shortcut.WorkingDirectory = $projectDir
        $Shortcut.Description = "MSH Commissioning Server - Native PC application for Matter device commissioning"
        $Shortcut.Save()
        
        Write-Status "Desktop shortcut created"
    }
}

# Print installation summary
Write-Host ""
Write-Status "Installation completed successfully!"
Write-Host ""
Write-Host "📋 Installation Summary:" -ForegroundColor Cyan
Write-Host "  • Python virtual environment: $projectDir\venv"
Write-Host "  • Configuration: $configDir\config.yaml"
Write-Host "  • Database: $configDir\credentials.db"
Write-Host "  • Web interface: http://localhost:8080"
Write-Host "  • API documentation: http://localhost:8080/docs"
Write-Host ""
Write-Host "🚀 To start the server:" -ForegroundColor Cyan
Write-Host "  • Manual: $projectDir\venv\Scripts\Activate.ps1 && msh-commissioning-server"
Write-Host "  • Service: Start-Service MSHCommissioningServer"
Write-Host "  • Desktop: Double-click the desktop shortcut"
Write-Host ""
Write-Host "📖 Next steps:" -ForegroundColor Cyan
Write-Host "  1. Configure your Raspberry Pi IP in $configDir\config.yaml"
Write-Host "  2. Set up SSH keys for Pi communication"
Write-Host "  3. Install Matter SDK tools (chip-tool, chip-repl)"
Write-Host "  4. Test with a Matter device"
Write-Host ""
Write-Warning "Note: You may need to restart your computer for all changes to take effect" 