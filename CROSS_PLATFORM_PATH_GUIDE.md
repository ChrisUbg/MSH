# Cross-Platform Path and Naming Convention Guide

## 🚨 **Critical Issues Identified**

### **1. Path Separator Inconsistencies**
- **Linux**: `/` (forward slash)
- **Windows**: `/` (backslash)
- **Mixed usage**: Some files use both, causing failures

### **2. Project Structure Naming**
- **MSH.Web**, **MSH.Infrastructure**, **MSH.Core** (dot notation)
- **Matter/** directory (simple naming)
- **src/** directory (standard convention)

### **3. File Naming Issues**
- **Spaces in filenames**: `Matter-SDK-ARM64-Build-Instructions.md`
- **Special characters**: `MSH.sln.DotSettings.user`
- **Mixed case**: `MSH.Web` vs `matter-server`

### **4. Hardcoded Path References**
- **Absolute paths**: `C:/Users/Dev/source/repos/MSH/`
- **Relative paths**: `../docker-compose.prod-msh.yml`
- **Mixed references**: `src/MSH.Web/Dockerfile`

## ✅ **Recommended Solutions**

### **1. Standardize Path Separators**

#### **Use Forward Slashes Everywhere**
```bash
# ✅ Correct (works on both Windows and Linux)
dockerfile: src/MSH.Web/Dockerfile
cp src/MSH.Web/bin/Debug/net8.0/* publish/

# ❌ Avoid (Windows-specific)
dockerfile: src/MSH.Web/Dockerfile
cp src/MSH.Web/bin/Debug/net8.0/* publish/
```

#### **Cross-Platform Path Handling**
```bash
# Detect OS and use appropriate separators
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" ]]; then
    # Windows
    SEPARATOR="//"
    PROJECT_ROOT="C://Users//Dev//source//repos//MSH"
else
    # Linux/WSL2
    SEPARATOR="/"
    PROJECT_ROOT="$(pwd)"
fi
```

### **2. Standardize Project Structure**

#### **Recommended Structure**
```
MSH/
├── src/
│   ├── MSH.Web/
│   ├── MSH.Infrastructure/
│   ├── MSH.Core/
│   └── MSH.Matter/
├── Matter/
│   ├── app/
│   ├── Dockerfile
│   └── requirements.txt
├── docker/
│   ├── docker-compose.yml
│   └── Dockerfile.prod
├── scripts/
│   ├── build.sh
│   └── deploy.sh
└── docs/
    └── readme.md
```

#### **Consistent Naming Convention**
```bash
# ✅ Use consistent naming
MSH.Web/          # C# project (dot notation)
MSH.Infrastructure/
MSH.Core/
Matter/            # Python project (simple)
docker/            # Docker files
scripts/           # Shell scripts
docs/              # Documentation
```

### **3. Fix File Naming**

#### **Remove Spaces from Filenames**
```bash
# ❌ Current (problematic)
Matter-SDK-ARM64-Build-Instructions.md

# ✅ Fixed (cross-platform)
Instructions-for-Building-Matter-SDK-for-ARM64.md
# OR
Matter-SDK-ARM64-Build-Instructions.md
```

#### **Use Consistent Case**
```bash
# ✅ Consistent lowercase for directories
matter/
docker/
scripts/
docs/

# ✅ Consistent case for files
matter-sdk-build-instructions.md
docker-handling.md
build-and-deploy-matter.sh
```

### **4. Environment-Specific Path Configuration**

#### **Create Environment Configuration**
```bash
# config/environment.sh
#!/bin/bash

# Detect OS
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" ]]; then
    OS="windows"
    SEPARATOR="//"
    PROJECT_ROOT="C://Users//Dev//source//repos//MSH"
else
    OS="linux"
    SEPARATOR="/"
    PROJECT_ROOT="$(pwd)"
fi

# Export variables
export OS
export SEPARATOR
export PROJECT_ROOT
```

#### **Use in Scripts**
```bash
#!/bin/bash
source config/environment.sh

# Use environment variables
docker-compose -f ${PROJECT_ROOT}${SEPARATOR}docker${SEPARATOR}docker-compose.yml up -d
```

### **5. Cross-Platform Script Templates**

#### **Windows PowerShell Template**
```powershell
# build.ps1
$PROJECT_ROOT = "C:/Users/Dev/source/repos/MSH"
$SEPARATOR = "/"

# Use variables
docker-compose -f "${PROJECT_ROOT}${SEPARATOR}docker${SEPARATOR}docker-compose.yml" up -d
```

#### **Linux/WSL2 Template**
```bash
#!/bin/bash
# build.sh
PROJECT_ROOT="$(pwd)"
SEPARATOR="/"

# Use variables
docker-compose -f "${PROJECT_ROOT}${SEPARATOR}docker${SEPARATOR}docker-compose.yml" up -d
```

#### **Cross-Platform Template**
```bash
#!/bin/bash
# build.sh

# Detect OS
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" ]]; then
    OS="windows"
    SEPARATOR="//"
    PROJECT_ROOT="C://Users//Dev//source//repos//MSH"
else
    OS="linux"
    SEPARATOR="/"
    PROJECT_ROOT="$(pwd)"
fi

# Use detected values
docker-compose -f "${PROJECT_ROOT}${SEPARATOR}docker${SEPARATOR}docker-compose.yml" up -d
```

## 🔧 **Implementation Plan**

### **Phase 1: Fix Critical Path Issues**

#### **1. Update Docker Compose Files**
```yaml
# docker-compose.yml
version: '3.8'
services:
  web:
    build:
      context: .
      dockerfile: src/MSH.Web/Dockerfile  # ✅ Forward slash
    volumes:
      - ./src/MSH.Web:/app/src/MSH.Web    # ✅ Forward slash
```

#### **2. Update Build Scripts**
```bash
#!/bin/bash
# build-and-deploy-matter.sh

# Use forward slashes everywhere
cp network-config.sh Matter/
scp -r . ${PI_USER}@${PI_IP}:~/MSH/Matter/
scp ../docker-compose.prod-msh.yml ${PI_USER}@${PI_IP}:~/MSH/
```

#### **3. Update Documentation**
```markdown
# Use consistent paths in documentation
- **Project Root**: `MSH/`
- **Source Code**: `src/MSH.Web/`
- **Matter Bridge**: `Matter/`
- **Docker Files**: `docker/`
```

### **Phase 2: Standardize File Names**

#### **1. Rename Problematic Files**
```bash
# Rename files with spaces
mv "Matter-SDK-ARM64-Build-Instructions.md" "Matter-SDK-ARM64-Build-Instructions.md"
mv "docker-handling.md" "docker-handling.md"
mv "PI-CONFIG-readme.md" "pi-config-readme.md"
```

#### **2. Update References**
```bash
# Update all references to renamed files
find . -name "*.md" -o -name "*.sh" -o -name "*.yml" | xargs sed -i 's/Instructions for Building Matter SDK for ARM64/.md/Matter-SDK-ARM64-Build-Instructions.md/g'
```

### **Phase 3: Create Environment Configuration**

#### **1. Create Environment Script**
```bash
# config/environment.sh
#!/bin/bash

# OS Detection
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" ]]; then
    OS="windows"
    SEPARATOR="//"
    PROJECT_ROOT="C://Users//Dev//source//repos//MSH"
    USER_HOME="C://Users//Dev"
else
    OS="linux"
    SEPARATOR="/"
    PROJECT_ROOT="$(pwd)"
    USER_HOME="$HOME"
fi

# Export for use in scripts
export OS SEPARATOR PROJECT_ROOT USER_HOME
```

#### **2. Update All Scripts**
```bash
#!/bin/bash
# All scripts should start with:
source config/environment.sh

# Then use variables:
docker-compose -f "${PROJECT_ROOT}${SEPARATOR}docker${SEPARATOR}docker-compose.yml" up -d
```

## 📋 **File Renaming Checklist**

### **Files to Rename**
- [ ] `Matter-SDK-ARM64-Build-Instructions.md` → `Matter-SDK-ARM64-Build-Instructions.md`
- [ ] `docker-handling.md` → `docker-handling.md`
- [ ] `PI-CONFIG-readme.md` → `pi-config-readme.md`
- [ ] `real-commissioning-test-guide.md` → `real-commissioning-test-guide.md`
- [ ] `project-roadmap.md` → `project-roadmap.md`
- [ ] `project-definition.md` → `project-definition.md`
- [ ] `debian-setup.md` → `debian-setup.md`
- [ ] `postgres-setup.md` → `postgres-setup.md`
- [ ] `network-config.md` → `network-config.md`
- [ ] `dev-process.md` → `dev-process.md`
- [ ] `hybrid-approach.md` → `hybrid-approach.md`
- [ ] `migration.md` → `migration.md`
- [ ] `tasmoda.md` → `tasmoda.md`

### **Directories to Standardize**
- [ ] `MSH.Infrastructure/` → Keep (C# convention)
- [ ] `MSH.Web/` → Keep (C# convention)
- [ ] `MSH.Core/` → Keep (C# convention)
- [ ] `Matter/` → Keep (Python convention)
- [ ] `src/` → Keep (standard convention)

## 🎯 **Success Criteria**

✅ **All paths use forward slashes** (works on both Windows and Linux)
✅ **No spaces in filenames** (cross-platform compatibility)
✅ **Consistent naming convention** (predictable structure)
✅ **Environment-specific configuration** (automatic OS detection)
✅ **All scripts use environment variables** (no hardcoded paths)

## 🚀 **Benefits**

1. **✅ Windows VS2022 Compatible**: All paths work correctly in Windows environment
2. **✅ Linux Compatible**: Maintains full compatibility with native Linux
3. **✅ WSL2 Optimized**: Proper path handling in WSL2 environment
4. **✅ Predictable Structure**: Consistent naming and organization
5. **✅ Easy Maintenance**: Environment variables for path management

---

**Created**: 2024-01-28  
**Purpose**: Standardize path and naming conventions for cross-platform compatibility  
**Target**: Windows VS2022 and Linux compatibility 