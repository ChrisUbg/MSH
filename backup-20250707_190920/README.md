# MSH Smart Home System

A comprehensive, local-first smart home management system built with .NET 8 Blazor Server and running on Raspberry Pi, featuring native Matter protocol integration through python-matter-server.

## Current Status: Production Ready 🚀

- ✅ **Full System Deployed** - Running on Raspberry Pi with Docker
- ✅ **Matter Integration Operational** - python-matter-server (officially certified)
- ✅ **Real Device Control Ready** - WebSocket-based commissioning and control
- ✅ **C# Blazor UI Complete** - Device management, room organization, user auth
- ✅ **PostgreSQL Database** - Entity Framework with migrations
- ✅ **Production Monitoring** - Health checks, logging, diagnostics

## Architecture

```
Blazor UI → HTTP API → FastAPI Bridge → WebSocket → python-matter-server → Matter Devices
```

**Ready for NOUS A8M commissioning and control**

## Documentation

- [Network Configuration](network-config.md): Details on network modes, configuration script, switching, and troubleshooting.
- [API Documentation](api.md): REST API endpoints for network and device management.

... (rest of your README) 

##
docker-compose -f docker-compose.dev-msh.yml up --build