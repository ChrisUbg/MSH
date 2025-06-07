#!/bin/bash

# Error handling
error_exit() {
    echo "❌ Fehler: $1"
    exit 1
}

# Get PostgreSQL password from Docker secret
PGPASSWORD=$(docker exec $(docker ps -q -f name=msh_db) cat /run/secrets/postgres_password)

# Create backup
echo "🔍 Erstelle Datenbank-Backup..."
docker exec $(docker ps -q -f name=msh_db) pg_dump -U postgres matter_prod > ~/msh/postgres_backup_$(date +%Y%m%d_%H%M%S).sql || error_exit "pg_dump fehlgeschlagen."

# Copy backup to Samba share (if configured)
if [ -d "/mnt/backup" ]; then
    echo "📤 Kopiere Backup auf Samba-Freigabe..."
    cp ~/msh/postgres_backup_*.sql /mnt/backup/ || error_exit "Kopieren auf Samba-Freigabe fehlgeschlagen."
fi

echo "✅ Backup erfolgreich erstellt"