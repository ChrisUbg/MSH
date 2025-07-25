"""
Configuration for packaging MSH Commissioning Server
"""

import os
from pathlib import Path

# Package configuration
PACKAGE_CONFIG = {
    "name": "msh-commissioning-server",
    "version": "1.0.0",
    "description": "PC Commissioning Server for Matter devices",
    "author": "MSH Team",
    "license": "MIT",
    
    # Files to include in package
    "include_files": [
        "config.yaml",
        "templates/",
        "static/",
        "README.md",
        "LICENSE"
    ],
    
    # Python dependencies
    "dependencies": [
        "fastapi",
        "uvicorn[standard]",
        "pydantic",
        "websockets",
        "aiofiles",
        "jinja2",
        "python-multipart"
    ],
    
    # Hidden imports for PyInstaller
    "hidden_imports": [
        "uvicorn.logging",
        "uvicorn.loops",
        "uvicorn.loops.auto",
        "uvicorn.protocols",
        "uvicorn.protocols.http",
        "uvicorn.protocols.http.auto",
        "uvicorn.protocols.websockets",
        "uvicorn.protocols.websockets.auto",
        "uvicorn.lifespan.on",
        "uvicorn.lifespan.off",
        "uvicorn.lifespan.auto",
        "fastapi.staticfiles",
        "fastapi.templating",
        "jinja2.ext"
    ],
    
    # Data files to include
    "data_files": [
        ("templates", "templates"),
        ("static", "static"),
        ("config.yaml", "."),
    ]
}

# Build configurations
BUILD_CONFIGS = {
    "linux": {
        "executable_name": "msh-commissioning-server",
        "icon": None,
        "additional_args": [
            "--onefile",
            "--windowed"
        ]
    },
    
    "windows": {
        "executable_name": "msh-commissioning-server.exe",
        "icon": "static/favicon.ico",
        "additional_args": [
            "--onefile",
            "--windowed",
            "--uac-admin"
        ]
    },
    
    "macos": {
        "executable_name": "msh-commissioning-server",
        "icon": "static/favicon.icns",
        "additional_args": [
            "--onefile",
            "--windowed"
        ]
    }
}

def get_platform_config():
    """Get configuration for current platform"""
    import platform
    system = platform.system().lower()
    
    if system == "linux":
        return BUILD_CONFIGS["linux"]
    elif system == "windows":
        return BUILD_CONFIGS["windows"]
    elif system == "darwin":
        return BUILD_CONFIGS["macos"]
    else:
        return BUILD_CONFIGS["linux"]  # Default to Linux

def create_pyinstaller_spec():
    """Create PyInstaller spec file"""
    
    config = get_platform_config()
    
    spec_content = f'''# -*- mode: python ; coding: utf-8 -*-

block_cipher = None

a = Analysis(
    ['main.py'],
    pathex=[],
    binaries=[],
    datas={PACKAGE_CONFIG["data_files"]},
    hiddenimports={PACKAGE_CONFIG["hidden_imports"]},
    hookspath=[],
    hooksconfig={{}},
    runtime_hooks=[],
    excludes=[],
    win_no_prefer_redirects=False,
    win_private_assemblies=False,
    cipher=block_cipher,
    noarchive=False,
)

pyz = PYZ(a.pure, a.zipped_data, cipher=block_cipher)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.zipfiles,
    a.datas,
    [],
    name='{config["executable_name"]}',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    upx_exclude=[],
    runtime_tmpdir=None,
    console=False,
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
)
'''
    
    with open("msh-commissioning-server.spec", 'w') as f:
        f.write(spec_content)
    
    return "msh-commissioning-server.spec" 