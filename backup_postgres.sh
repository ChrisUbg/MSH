#!/bin/bash

# Konfiguration
BACKUP_DIR="/home/chregg/msh/postgres-backups"
DB_NAME="postgres"
DB_USER="postgres"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_FILE="$BACKUP_DIR/${DB_NAME}_backup_$TIMESTAMP.sql"
TARGET_PC="Dev@192.168.0.101:/c/backup/storage/"  # Windows-Pfad als Unix-Style
SSH_KEY="/home/chregg/.ssh/backup_key"

# Funktion für Fehlerbehandlung
error_exit() {
    echo "❌ Fehler: $1" >&2
    exit 1
}

# Backup-Verzeichnis erstellen
mkdir -p "$BACKUP_DIR" || error_exit "Backup-Verzeichnis konnte nicht erstellt werden."

# PostgreSQL-Backup erstellen
echo "🔍 Erstelle Datenbank-Backup..."
docker exec -t matter-dev-db-15 pg_dumpall -U "$DB_USER" > "$BACKUP_FILE" || error_exit "pg_dumpall fehlgeschlagen."

# Backup komprimieren
echo "🗜️ Komprimiere Backup..."
gzip "$BACKUP_FILE" || error_exit "Komprimierung fehlgeschlagen."

# Backup übertragen (mit SSH-Key)
echo "📤 Übertrage Backup zum PC..."
scp -i "$SSH_KEY" "${BACKUP_FILE}.gz" "$TARGET_PC" || error_exit "SCP-Übertragung fehlgeschlagen."

# Lokale Backups aufräumen (älter als 7 Tage)
echo "🧹 Räume alte Backups auf..."
find "$BACKUP_DIR" -type f -name "*.gz" -mtime +7 -delete

echo "✅ Backup erfolgreich: ${BACKUP_FILE}.gz → ${TARGET_PC}"