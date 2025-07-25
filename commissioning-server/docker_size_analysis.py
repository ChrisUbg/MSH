#!/usr/bin/env python3
"""
Docker size analysis for MSH Commissioning Server
"""

import subprocess
import os
import sys

def analyze_docker_sizes():
    """Analyze Docker image sizes for different base images"""
    
    print("📊 Docker Image Size Analysis")
    print("=" * 50)
    
    # Common base images and their sizes
    base_images = {
        "python:3.11-slim": "~45MB",
        "python:3.11-alpine": "~15MB", 
        "ubuntu:22.04": "~30MB",
        "debian:bullseye-slim": "~25MB",
        "alpine:latest": "~5MB"
    }
    
    print("Base Image Sizes:")
    for image, size in base_images.items():
        print(f"  {image}: {size}")
    
    print("\n📦 Estimated Final Image Sizes:")
    print("=" * 50)
    
    # Python dependencies
    python_deps = [
        "fastapi (~15MB)",
        "uvicorn (~8MB)", 
        "pydantic (~5MB)",
        "websockets (~2MB)",
        "jinja2 (~3MB)",
        "aiofiles (~1MB)",
        "python-multipart (~1MB)"
    ]
    
    total_python_deps = "~35MB"
    
    # System dependencies for Bluetooth
    system_deps = [
        "bluetooth (~5MB)",
        "bluez (~8MB)",
        "libbluetooth-dev (~2MB)",
        "build-essential (~200MB)",
        "cmake (~15MB)",
        "ninja-build (~2MB)"
    ]
    
    total_system_deps = "~232MB"
    
    print("Python Dependencies:")
    for dep in python_deps:
        print(f"  {dep}")
    print(f"  Total: {total_python_deps}")
    
    print("\nSystem Dependencies (Bluetooth):")
    for dep in system_deps:
        print(f"  {dep}")
    print(f"  Total: {total_system_deps}")
    
    print("\n🎯 Estimated Final Sizes:")
    print("=" * 50)
    
    estimates = {
        "Minimal (Alpine + Python deps only)": "~50MB",
        "Standard (Ubuntu + Python deps)": "~80MB", 
        "Full (Ubuntu + all deps)": "~320MB",
        "Optimized (Multi-stage build)": "~60MB"
    }
    
    for scenario, size in estimates.items():
        print(f"  {scenario}: {size}")
    
    return estimates

def compare_packaging_options():
    """Compare different packaging options"""
    
    print("\n📊 Packaging Options Comparison")
    print("=" * 50)
    
    options = {
        "PyInstaller (Single executable)": "~50-80MB",
        "Docker (Minimal Alpine)": "~50MB", 
        "Docker (Standard Ubuntu)": "~80MB",
        "Docker (Full with build tools)": "~320MB",
        "Python Package (pip install)": "~5MB + Python runtime",
        "System Package (deb/rpm)": "~10MB + dependencies"
    }
    
    for option, size in options.items():
        print(f"  {option}: {size}")
    
    print("\n💡 Recommendations:")
    print("=" * 50)
    print("  🥇 PyInstaller: Best for portability")
    print("  🥈 Docker Alpine: Best for consistency") 
    print("  🥉 Python Package: Best for updates")

def create_optimized_dockerfile():
    """Create an optimized Dockerfile for minimal size"""
    
    dockerfile_content = '''# Multi-stage build for minimal size
FROM python:3.11-alpine AS builder

# Install build dependencies
RUN apk add --no-cache \\
    gcc \\
    musl-dev \\
    linux-headers \\
    bluetooth-dev \\
    bluez-dev

# Install Python dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Runtime stage
FROM python:3.11-alpine AS runtime

# Install runtime dependencies only
RUN apk add --no-cache \\
    bluetooth \\
    bluez \\
    && rm -rf /var/cache/apk/*

# Copy Python packages from builder
COPY --from=builder /usr/local/lib/python3.11/site-packages /usr/local/lib/python3.11/site-packages
COPY --from=builder /usr/local/bin /usr/local/bin

# Copy application
COPY . /app
WORKDIR /app

# Create non-root user
RUN adduser -D -s /bin/sh app && chown -R app:app /app
USER app

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \\
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Start application
CMD ["python", "main.py", "--host", "0.0.0.0", "--port", "8080"]
'''
    
    with open("Dockerfile.optimized", "w") as f:
        f.write(dockerfile_content)
    
    print("\n📝 Created optimized Dockerfile.optimized")
    print("   Estimated size: ~60MB")
    print("   Features: Multi-stage build, Alpine base, minimal runtime")

def show_size_breakdown():
    """Show detailed size breakdown"""
    
    print("\n🔍 Detailed Size Breakdown")
    print("=" * 50)
    
    breakdown = {
        "Base Image": {
            "python:3.11-alpine": "15MB",
            "python:3.11-slim": "45MB", 
            "ubuntu:22.04": "30MB"
        },
        "Python Runtime": {
            "Python 3.11": "25MB",
            "pip + setuptools": "5MB"
        },
        "Application Dependencies": {
            "fastapi": "15MB",
            "uvicorn": "8MB",
            "pydantic": "5MB", 
            "websockets": "2MB",
            "jinja2": "3MB",
            "aiofiles": "1MB",
            "python-multipart": "1MB"
        },
        "System Dependencies": {
            "bluetooth": "5MB",
            "bluez": "8MB",
            "build tools (if needed)": "200MB+"
        },
        "Application Code": {
            "Python files": "1MB",
            "Templates": "0.5MB",
            "Static files": "0.5MB",
            "Config files": "0.1MB"
        }
    }
    
    for category, items in breakdown.items():
        print(f"\n{category}:")
        for item, size in items.items():
            print(f"  {item}: {size}")

if __name__ == "__main__":
    analyze_docker_sizes()
    compare_packaging_options()
    create_optimized_dockerfile()
    show_size_breakdown()
    
    print("\n🎯 Summary:")
    print("  • Minimal Docker: ~50-60MB")
    print("  • Standard Docker: ~80-100MB") 
    print("  • PyInstaller: ~50-80MB")
    print("  • Python Package: ~5MB + Python") 