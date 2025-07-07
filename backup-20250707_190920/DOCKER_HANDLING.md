# Docker Handling for MSH Project

## Environment Setup

### Windows PowerShell (Windows-specific)
```powershell
# Set environment variable for Dockerfile path
$env:DOCKERFILE_PATH="C:/Users/Dev/source/repos/MSH/Docker/Dockerfile.prod"
docker-compose -f Docker/docker-compose-prod.yml build
```

### Linux/WSL2 (Cross-platform)
```bash
# Set environment variable for Dockerfile path
export DOCKERFILE_PATH="$(pwd)/Docker/Dockerfile.prod"
docker-compose -f Docker/docker-compose-prod.yml build
```

## Docker Concept for Swarm Deployment

### Dev Environment
```bash
docker run --name msh-postgres -e POSTGRES_PASSWORD=devpassword -e POSTGRES_DB=matter_dev -p 5432:5432 -d postgres:latest
```

### At src/MSH.Web
```bash
dotnet restore
```

### Database Update
```bash
dotnet ef database update --project src/MSH.Infrastructure --startup-project src/MSH.Web
```

### Run the Project
```bash
dotnet run --project src/MSH.Web
```

## 1. Image Build and Push (Development PC)

### Cross-platform Build Command
```bash
# Run in solution root directory
docker buildx build --platform linux/arm64 -t chrislinux11x/msh-app-prod -f src/MSH.Web/Dockerfile .

# Push image to Docker Hub
docker push chrislinux11x/msh-app-prod:latest
```

## 2. Swarm Setup (on Raspberry Pi)
```bash
# Initialize swarm (if not already done)
sudo docker swarm init

# Create secrets
echo "84jfapa0d8f09qzhf22t" | sudo docker secret create postgres_password -
echo "20202021" | sudo docker secret create matter_pin -

# Verify secrets
sudo docker secret ls
```

## 3. Stack Deployment

### Local Docker Build
```bash
docker buildx build /
  --platform linux/arm64 /
  -t msh-app-dev /
  -f src/MSH.Web/Dockerfile /
  .
```

### Docker Rebuild at Dev Environment
```bash
docker buildx build --platform linux/arm64 -t chrislinux11x/msh-app-prod -f src/MSH.Web/Dockerfile . --push
```

### Transfer and Deploy (Cross-platform)
```bash
# Transfer docker-compose.swarm.yml
scp docker-compose.swarm.yml ${PI_USER}@${PI_IP}:~/msh/

# Deploy stack
ssh ${PI_USER}@${PI_IP} "cd ~/msh && docker stack deploy -c docker-compose.swarm.yml msh"
```

## 4. Verification
```bash
# Check services
ssh ${PI_USER}@${PI_IP} "docker service ls"

# Show app logs
ssh ${PI_USER}@${PI_IP} "docker service logs msh_app -f"

# Test API
curl http://${PI_IP}:8080/network-diag
```

## 5. Backup Strategy (Updated)

### PostgreSQL Backup
```bash
# 1. Create backup directly in container
ssh ${PI_USER}@${PI_IP} << 'EOF'
  PGPASSWORD=$(docker exec $(docker ps -q -f name=msh_db) cat /run/secrets/postgres_password)
  docker exec $(docker ps -q -f name=msh_db) pg_dump -U postgres matter_prod > ~/msh/postgres_backup_$(date +%Y%m%d_%H%M%S).sql
EOF

# 2. Copy backup to Samba share (if configured)
ssh ${PI_USER}@${PI_IP} "cp ~/msh/postgres_backup_*.sql /mnt/backup/"
```

### Samba Setup (Optional)
```bash
# 1. Install Samba client
ssh ${PI_USER}@${PI_IP} "sudo apt update && sudo apt install -y cifs-utils"

# 2. Mount share (manual)
ssh ${PI_USER}@${PI_IP} "sudo mkdir -p /mnt/backup && sudo mount -t cifs //192.168.0.101/backup /mnt/backup -o username=Dev,password=YOUR_PASSWORD"
```

## Important Notes

### pgAdmin Access
- **Host**: `${PI_IP}`
- **Port**: `5432`
- **User**: `postgres`
- **Password**: From Docker Secret (`postgres_password`)

### Volumes
- **PostgreSQL Data**: `/mnt/postgres-data` (local on Pi)
- **Backups**: `/mnt/backup` (Samba) or `~/msh/` (local)

### Troubleshooting
```bash
# Test network connection
ssh ${PI_USER}@${PI_IP} "docker exec $(docker ps -q -f name=msh_app) sh -c 'timeout 2 bash -c /"</dev/tcp/db/5432/" && echo /"✅ Success/" || echo /"❌ Failed/"'"
```

## File Structure
```
msh/
├── docker-compose.swarm.yml    # Swarm configuration (PostgreSQL + App)
└── .env                        # Central configuration (optional)
```

## Cross-platform Path Handling

### Windows PowerShell
```powershell
# Use Windows-style paths
$env:DOCKERFILE_PATH="C:/Users/Dev/source/repos/MSH/Docker/Dockerfile.prod"
docker-compose -f Docker/docker-compose-prod.yml build
```

### Linux/WSL2
```bash
# Use Unix-style paths
export DOCKERFILE_PATH="$(pwd)/Docker/Dockerfile.prod"
docker-compose -f Docker/docker-compose-prod.yml build
```

### WSL2 Integration
```bash
# Access Windows files from WSL2
cp /mnt/c/Users/Dev/source/repos/MSH/docker-compose.swarm.yml ~/msh/

# Access WSL2 files from Windows PowerShell
cp "//wsl$/Ubuntu-22.04/home/chris/msh/docker-compose.swarm.yml" "C:/temp/"
```