#!/usr/bin/env python3
"""
Environment Test for MSH Commissioning Server
"""

import sys
import os
from pathlib import Path

def test_imports():
    """Test if all required modules can be imported"""
    print("🔍 Testing Python imports...")
    
    required_modules = [
        'fastapi',
        'uvicorn', 
        'pydantic',
        'yaml',
        'asyncio',
        'sqlite3'
    ]
    
    failed_imports = []
    for module in required_modules:
        try:
            __import__(module)
            print(f"✅ {module}")
        except ImportError as e:
            print(f"❌ {module}: {e}")
            failed_imports.append(module)
    
    if failed_imports:
        print(f"\n❌ Failed imports: {failed_imports}")
        print("Run: pip install -r requirements.txt")
        return False
    
    return True

def test_project_structure():
    """Test if project files exist"""
    print("\n📁 Testing project structure...")
    
    required_files = [
        'main.py',
        'requirements.txt',
        'setup.py',
        'commissioning_server/__init__.py',
        'commissioning_server/core/config.py',
        'commissioning_server/core/matter_client.py',
        'commissioning_server/core/ble_scanner.py',
        'commissioning_server/core/credential_store.py',
        'commissioning_server/core/device_manager.py'
    ]
    
    missing_files = []
    for file in required_files:
        if Path(file).exists():
            print(f"✅ {file}")
        else:
            print(f"❌ {file}")
            missing_files.append(file)
    
    if missing_files:
        print(f"\n❌ Missing files: {missing_files}")
        return False
    
    return True

def test_environment():
    """Test environment variables and configuration"""
    print("\n🌍 Testing environment...")
    
    # Check virtual environment
    if 'VIRTUAL_ENV' in os.environ:
        print(f"✅ Virtual environment: {os.environ['VIRTUAL_ENV']}")
    else:
        print("⚠️  No virtual environment detected")
    
    # Check .env file
    if Path('.env').exists():
        print("✅ .env file found")
        # Load and check environment variables
        with open('.env', 'r') as f:
            env_vars = {}
            for line in f:
                if '=' in line and not line.startswith('#'):
                    key, value = line.strip().split('=', 1)
                    env_vars[key] = value
        
        expected_vars = ['MSH_ENVIRONMENT', 'MSH_MODE', 'PI_IP']
        for var in expected_vars:
            if var in env_vars:
                print(f"✅ {var}: {env_vars[var]}")
            else:
                print(f"⚠️  {var}: not set")
    else:
        print("⚠️  No .env file found")
    
    # Check config directory
    config_dir = Path.home() / '.msh'
    if config_dir.exists():
        print(f"✅ Config directory: {config_dir}")
    else:
        print(f"⚠️  Config directory not found: {config_dir}")
    
    return True

def test_configuration():
    """Test configuration loading"""
    print("\n⚙️  Testing configuration...")
    
    try:
        from commissioning_server.core.config import Config
        config = Config()
        print("✅ Configuration loaded successfully")
        
        # Test basic config sections
        sections = ['server', 'matter', 'bluetooth', 'storage', 'security', 'pi']
        for section in sections:
            if config.get(section):
                print(f"✅ {section} configuration")
            else:
                print(f"⚠️  {section} configuration missing")
        
        return True
    except Exception as e:
        print(f"❌ Configuration error: {e}")
        return False

def test_api_creation():
    """Test if FastAPI app can be created"""
    print("\n🚀 Testing API creation...")
    
    try:
        from main import app
        print("✅ FastAPI app created successfully")
        
        # Test basic endpoints
        from fastapi.testclient import TestClient
        client = TestClient(app)
        
        response = client.get("/")
        if response.status_code == 200:
            print("✅ Health endpoint working")
        else:
            print(f"⚠️  Health endpoint returned {response.status_code}")
        
        return True
    except Exception as e:
        print(f"❌ API creation error: {e}")
        return False

def main():
    """Run all environment tests"""
    print("🔧 MSH Commissioning Server - Environment Test")
    print("=" * 50)
    
    tests = [
        ("Python Imports", test_imports),
        ("Project Structure", test_project_structure),
        ("Environment", test_environment),
        ("Configuration", test_configuration),
        ("API Creation", test_api_creation)
    ]
    
    results = {}
    passed = 0
    total = len(tests)
    
    for test_name, test_func in tests:
        print(f"\n{'='*20} {test_name} {'='*20}")
        try:
            if test_func():
                results[test_name] = "PASS"
                passed += 1
            else:
                results[test_name] = "FAIL"
        except Exception as e:
            print(f"❌ Error in {test_name}: {e}")
            results[test_name] = "ERROR"
    
    # Print summary
    print(f"\n{'='*50}")
    print("📊 TEST SUMMARY")
    print(f"{'='*50}")
    
    for test_name, result in results.items():
        status = "✅ PASS" if result == "PASS" else "❌ FAIL"
        print(f"{test_name:<25} {status}")
    
    print(f"\nOverall: {passed}/{total} tests passed")
    
    if passed == total:
        print("🎉 Environment is ready!")
        print("\nNext steps:")
        print("1. Run hardware test: python test-hardware.py")
        print("2. Start server: python main.py --debug")
        print("3. Open web interface: http://localhost:8080")
    else:
        print("⚠️  Some tests failed. Please fix the issues before proceeding.")
        print("\nCommon fixes:")
        print("1. Install dependencies: pip install -r requirements.txt")
        print("2. Create virtual environment: python3 -m venv venv")
        print("3. Create .env file with required variables")
        print("4. Set up configuration directory: mkdir -p ~/.msh")
    
    return passed == total

if __name__ == "__main__":
    success = main()
    sys.exit(0 if success else 1) 