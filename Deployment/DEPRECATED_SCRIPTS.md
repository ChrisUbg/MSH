# Deprecated Deployment Scripts

## Overview

This document lists deployment scripts that are **deprecated** and should **not be used** for new deployments. The recommended approach is to use `deploy-unified.sh`.

## ❌ Deprecated Scripts

### 1. `deploy.sh`
**Status**: ❌ **BROKEN**
**Issues**:
- References `docker-compose.prod.yml` which doesn't exist
- Uses outdated deployment approach
- No ARM64 build support
- Hardcoded configurations

**Replacement**: Use `deploy-unified.sh`

### 2. `deploy-to-pi.sh`
**Status**: ⚠️ **PARTIALLY WORKING**
**Issues**:
- Builds on Pi (causes build issues)
- Uses different database name (`msh` vs `matter_dev`)
- Less reliable than local building approach
- No comprehensive verification

**Replacement**: Use `deploy-unified.sh`

### 3. `deploy_msh_to_pi.sh`
**Status**: ✅ **WORKING** but **DEPRECATED**
**Issues**:
- German comments (not user-friendly)
- Hardcoded values
- No auto-detection of Pi IP
- Less comprehensive than unified script

**Replacement**: Use `deploy-unified.sh`

## ✅ Recommended Scripts

### 1. `deploy-unified.sh` ⭐ **RECOMMENDED**
**Features**:
- ✅ Local ARM64 building
- ✅ Auto-detection of Pi IP
- ✅ Comprehensive verification
- ✅ mDNS setup
- ✅ Consistent database configuration
- ✅ Clear error handling

### 2. `setup_fresh_db.sh`
**Status**: ✅ **WORKING**
**Purpose**: Database setup and seeding
**Usage**: Run after deployment

### 3. `setup-mdns.sh`
**Status**: ✅ **WORKING**
**Purpose**: mDNS configuration
**Usage**: Called automatically by `deploy-unified.sh`

## Migration Guide

### From `deploy.sh`
```bash
# Old (broken)
./deploy.sh

# New (recommended)
./deploy-unified.sh
```

### From `deploy-to-pi.sh`
```bash
# Old (builds on Pi)
./deploy-to-pi.sh

# New (builds locally)
./deploy-unified.sh
```

### From `deploy_msh_to_pi.sh`
```bash
# Old (German script)
./deploy_msh_to_pi.sh

# New (unified approach)
./deploy-unified.sh
```

## Why These Scripts Are Deprecated

### 1. **Inconsistent Database Configuration**
- Some use `msh`, others use `matter_dev`
- Causes confusion and deployment failures

### 2. **Build Issues on Pi**
- Pi has limited resources for building
- ARM64 builds are slow and unreliable on Pi
- Better to build locally and transfer

### 3. **Missing Files**
- `docker-compose.prod.yml` doesn't exist
- Causes deployment failures

### 4. **Poor Error Handling**
- Limited verification and health checks
- Difficult to debug issues

### 5. **Hardcoded Values**
- IP addresses hardcoded in some scripts
- Not flexible for different environments

## Action Items

1. **Use `deploy-unified.sh`** for all new deployments
2. **Keep deprecated scripts** for reference (don't delete yet)
3. **Update documentation** to reference unified approach
4. **Test unified script** thoroughly before removing old scripts

## Future Cleanup

Once `deploy-unified.sh` is proven stable:
1. Archive deprecated scripts to `archive/` directory
2. Update all documentation to reference unified approach
3. Remove references to deprecated scripts from guides
