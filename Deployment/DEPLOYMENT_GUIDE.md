# MSH Deployment Guide

This guide documents the successful migration and deployment process for the MSH (Matter Smart Home) application on Raspberry Pi.

## Prerequisites

- Raspberry Pi with Docker installed
- Local development machine with Docker and .NET SDK
- SSH access to Raspberry Pi (user: `chregg`)
- Network connectivity between local machine and Pi

## Architecture Overview

### Container Setup
- **msh_db**: PostgreSQL 16 database container
- **msh_web**: ASP.NET Core web application container
- **Network**: `msh-network` for container communication
- **Ports**: 
  - Database: 5435 (external) -> 5432 (internal)
  - Web App: 8083 (external) -> 8082 (internal)

### Database Configuration
- **Database Name**: `matter_dev`
- **Username**: `postgres`
- **Password**: `postgres`
- **Connection String**: `Host=msh_db;Port=5432;Database=matter_dev;Username=postgres;Password=postgres`

## Deployment Process

### 1. Build and Deploy Application

Use the `deploy_msh_to_pi.sh` script:

```bash
./deploy_msh_to_pi.sh
```

This script:
1. Builds ARM64 Docker image locally
2. Saves image as TAR file
3. Transfers to Raspberry Pi
4. Loads image on Pi
5. Starts database and web containers
6. Verifies deployment

### 2. Database Setup

Use the `setup_fresh_db.sh` script:

```bash
./setup_fresh_db.sh
```

This script:
1. Waits for database to be ready
2. Runs Entity Framework migrations
3. Seeds database with master data
4. Verifies seeded data

## Entity Framework Configuration

### Key Model Relationships

#### BaseEntity Configuration
All entities inherit from `BaseEntity` with user tracking:

```csharp
public String CreatedById { get; set; } = "bb1be326-f26e-4684-bbf5-5c3df450dc61";
public String? UpdatedById { get; set; } = "bb1be326-f26e-4684-bbf5-5c3df450dc61";
public User CreatedBy { get; set; } = null!;
public User? UpdatedBy { get; set; }
```

#### Navigation Properties
- **Room.Devices**: One-to-many relationship (restored)
- **Device.Room**: Many-to-one relationship
- **DeviceGroupMember**: Junction entity for many-to-many relationships

#### Important Notes
- **Computed Properties Removed**: Removed `Device.DeviceGroups` and `DeviceGroup.Devices` computed properties to prevent EF query issues
- **Explicit Relationships**: All relationships explicitly configured in `ApplicationDbContext`
- **Migration History**: Uses `20250620142356_FreshStartWithStringUserIds` migration

### ApplicationDbContext Configuration

```csharp
// BaseEntity relationships
modelBuilder.Entity(entityType.ClrType)
    .HasOne(typeof(User), nameof(BaseEntity.CreatedBy))
    .WithMany()
    .HasForeignKey(nameof(BaseEntity.CreatedById))
    .OnDelete(DeleteBehavior.Restrict);

// Room-Device relationship
modelBuilder.Entity<Device>()
    .HasOne(d => d.Room)
    .WithMany(r => r.Devices)
    .HasForeignKey(d => d.RoomId)
    .OnDelete(DeleteBehavior.Restrict);
```

## Docker Configuration

### Dockerfile
- Uses `mcr.microsoft.com/dotnet/aspnet:8.0` base image
- Builds for ARM64 architecture
- Includes `--migrate` flag in ENTRYPOINT for automatic migration

### Docker Compose
- Uses `postgres:16` for database
- Maps port 5435:5432 for database access
- Maps port 8083:8082 for web application
- Uses `msh-network` for container communication

## Troubleshooting

### Common Issues

1. **"column d0.DeviceGroupId does not exist"**
   - **Cause**: Computed properties generating incorrect SQL
   - **Solution**: Remove computed properties from Device and DeviceGroup entities

2. **"exec format error"**
   - **Cause**: Architecture mismatch (x86_64 vs ARM64)
   - **Solution**: Use `docker buildx build --platform linux/arm64`

3. **"network msh_network not found"**
   - **Cause**: Docker network not created
   - **Solution**: `docker network create msh-network`

4. **"Host is already in use"**
   - **Cause**: Port conflicts
   - **Solution**: Stop existing containers before starting new ones

### Verification Commands

```bash
# Check container status
docker ps

# Check application logs
docker logs msh_web

# Check database logs
docker logs msh_db

# Test database connection
docker exec msh_db psql -U postgres -d matter_dev -c "SELECT 1;"

# Check migration history
docker exec msh_db psql -U postgres -d matter_dev -c "SELECT * FROM \"__EFMigrationsHistory\";"
```

## Master Data Seeding

The `seed_database.sql` script creates:

1. **System User**: Required for all entity relationships
2. **Admin User**: For application administration
3. **Default Rooms**: Living Room, Kitchen, Bedroom, Bathroom
4. **Device Groups**: Smart Plugs, Smart Lighting, Smart Sensors
5. **Device Types**: Smart Plug, Smart Switch, Smart Bulb
6. **ASP.NET Core Identity**: Roles and admin user

## Migration Process

1. **Automatic Migration**: Application runs migrations on startup with `--migrate` flag
2. **Manual Migration**: Use `dotnet ef database update` from `src` directory
3. **Migration History**: Stored in `__EFMigrationsHistory` table

## Security Considerations

- Database password is `postgres` (change for production)
- Application runs in Production environment
- Network isolation using Docker networks
- Health checks configured for containers

## Performance Notes

- Database uses PostgreSQL 16 with JSONB support
- Application includes health checks
- Container restart policies configured
- Volume persistence for database data

## Maintenance

### Backup Database
```bash
docker exec msh_db pg_dump -U postgres matter_dev > backup.sql
```

### Restore Database
```bash
docker exec -i msh_db psql -U postgres matter_dev < backup.sql
```

### Update Application
1. Build new image: `docker buildx build --platform linux/arm64 -t msh-web`
2. Deploy using `deploy_msh_to_pi.sh`
3. Database migrations run automatically

This deployment process has been tested and verified to work correctly with the current MSH application configuration. 