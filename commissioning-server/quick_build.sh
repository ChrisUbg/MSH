#!/bin/bash
# Quick build script for MSH Commissioning Server

echo "🚀 Building MSH Commissioning Server..."

# Check if PyInstaller is installed
if ! command -v pyinstaller &> /dev/null; then
    echo "📦 Installing PyInstaller..."
    pip install pyinstaller
fi

# Create build directory
mkdir -p dist

# Build the executable
echo "🔨 Building executable..."
pyinstaller \
    --onefile \
    --name=msh-commissioning-server \
    --add-data="config.yaml:." \
    --add-data="templates:templates" \
    --add-data="static:static" \
    --hidden-import=uvicorn.logging \
    --hidden-import=uvicorn.loops \
    --hidden-import=uvicorn.protocols \
    --hidden-import=fastapi.staticfiles \
    --hidden-import=fastapi.templating \
    main.py

# Copy additional files
echo "📋 Copying additional files..."
cp config.yaml dist/
cp README.md dist/ 2>/dev/null || echo "# MSH Commissioning Server" > dist/README.md

# Create launcher scripts
echo "📝 Creating launcher scripts..."

# Linux/Mac launcher
cat > dist/start-server.sh << 'EOF'
#!/bin/bash
echo "🚀 Starting MSH Commissioning Server..."
export MSH_CONFIG_PATH="./config.yaml"
export MSH_DATA_PATH="./data"
mkdir -p ./data
./msh-commissioning-server --port 8888 --host 0.0.0.0
EOF

# Windows launcher
cat > dist/start-server.bat << 'EOF'
@echo off
echo 🚀 Starting MSH Commissioning Server...
set MSH_CONFIG_PATH=config.yaml
set MSH_DATA_PATH=data
if not exist data mkdir data
msh-commissioning-server.exe --port 8888 --host 0.0.0.0
pause
EOF

# Make launcher executable
chmod +x dist/start-server.sh

# Create deployment package
echo "📦 Creating deployment package..."
cd dist
tar -czf ../msh-commissioning-server-linux.tar.gz *
cd ..

echo "✅ Build complete!"
echo "📁 Package location: msh-commissioning-server-linux.tar.gz"
echo "🌐 To run: tar -xzf msh-commissioning-server-linux.tar.gz && cd msh-commissioning-server && ./start-server.sh" 