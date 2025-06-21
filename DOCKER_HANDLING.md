c# Or override with absolute path
$env:DOCKERFILE_PATH="C:\Users\Dev\source\repos\MSH\Docker\Dockerfile.prod"
docker-compose -f Docker\docker-compose-prod.yml build

# Docker Konzept für Swarm-Deployment

## Dev environment
docker run --name msh-postgres -e POSTGRES_PASSWORD=devpassword -e POSTGRES_DB=matter_dev -p 5432:5432 -d postgres:latest

## at src/MSH.Web
dotnet restore

## db-update
dotnet ef database update --project src/MSH.Infrastructure --startup-project src/MSH.Web

## run the project
dotnet run --project src/MSH.Web

## 1. Image-Build und Push (auf Entwicklungs-PC)
```bash
# Im Solution-Root-Verzeichnis ausführen
docker buildx build --platform linux/arm64 -t chrislinux11x/msh-app-prod -f src/MSH.Web/Dockerfile .

# Image auf Docker Hub pushen
docker push chrislinux11x/msh-app-prod:latest
```
## 2. Swarm-Setup (auf Raspberry Pi)
```bash
# Swarm initialisieren (falls nicht bereits geschehen)
sudo docker swarm init

# Secrets erstellen
echo "84jfapa0d8f09qzhf22t" | sudo docker secret create postgres_password -
echo "20202021" | sudo docker secret create matter_pin -

# Secrets verifizieren
sudo docker secret ls
```
## 3. Stack-Deployment
```bash

# local Docker build
docker buildx build \
  --platform linux/arm64 \
  -t msh-app-dev \
  -f src/MSH.Web/Dockerfile \
  .

# docker rebuild at the dev environment
docker buildx build --platform linux/arm64 -t chrislinux11x/msh-app-prod -f src/MSH.Web/Dockerfile . --push

# docker-compose.swarm.yml übertragen
scp docker-compose.swarm.yml chregg@192.168.0.104:~/msh/

# Stack deployen
ssh chregg@192.168.0.104 "cd ~/msh && docker stack deploy -c docker-compose.swarm.yml msh"
```
## 4. Verifikation
```bash
# Services prüfen
ssh chregg@192.168.0.104 "docker service ls"

# App-Logs anzeigen
ssh chregg@192.168.0.104 "docker service logs msh_app -f"

# API testen
curl http://192.168.0.104:8080/network-diag
```
## 5. Backup-Strategie (aktualisiert)
### PostgreSQL-Backup
```bash
# 1. Backup direkt im Container erstellen
ssh chregg@192.168.0.104 << 'EOF'
  PGPASSWORD=$(docker exec $(docker ps -q -f name=msh_db) cat /run/secrets/postgres_password)
  docker exec $(docker ps -q -f name=msh_db) pg_dump -U postgres matter_prod > ~/msh/postgres_backup_$(date +%Y%m%d_%H%M%S).sql
EOF

# 2. Backup auf Samba-Freigabe kopieren (falls eingerichtet)
ssh chregg@192.168.0.104 "cp ~/msh/postgres_backup_*.sql /mnt/backup/"
```
### Samba-Einrichtung (optional)
```bash
# 1. Samba-Client installieren
ssh chregg@192.168.0.104 "sudo apt update && sudo apt install -y cifs-utils"

# 2. Freigabe einbinden (manuell)
ssh chregg@192.168.0.104 "sudo mkdir -p /mnt/backup && sudo mount -t cifs //192.168.0.101/backup /mnt/backup -o username=Dev,password=IHR_PASSWORT"
```
## Wichtige Hinweise
- **pgAdmin Zugriff**: 
  - Host: `192.168.0.104`
  - Port: `5432`
  - User: `postgres`
  - Password: Aus dem Docker Secret (`postgres_password`)

- **Volumes**: 
  - PostgreSQL-Daten: `/mnt/postgres-data` (lokal auf dem Pi)
  - Backups: `/mnt/backup` (Samba) oder `~/msh/` (lokal)

- **Troubleshooting**:
  ```bash
  # Netzwerkverbindung testen
  ssh chregg@192.168.0.104 "docker exec $(docker ps -q -f name=msh_app) sh -c 'timeout 2 bash -c \"</dev/tcp/db/5432\" && echo \"✅ Success\" || echo \"❌ Failed\"'"
  ```

## Dateistruktur
msh/
├── docker-compose.swarm.yml    # Swarm-Konfiguration (PostgreSQL + App)
└── .env                        # Zentrale Konfiguration (optional)