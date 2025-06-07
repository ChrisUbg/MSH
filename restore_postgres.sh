#!/bin/bash

# Error handling
error_exit() {
    echo "❌ Fehler: $1"
    exit 1
}

# Check arguments
if [ $# -ne 1 ]; then
    echo "Usage: $0 <backup_file>"
    exit 1
fi

BACKUP_FILE="$1"
if [ ! -f "$BACKUP_FILE" ]; then
    error_exit "Backup file not found: $BACKUP_FILE"
fi

# Configuration
DB_NAME="matter_prod"
DB_USER="postgres"

# Check if running in Docker
if [ -f /.dockerenv ]; then
    echo "🔍 Stelle Datenbank wieder her..."
    gunzip -c "$BACKUP_FILE" | psql -U "$DB_USER" "$DB_NAME" || error_exit "Wiederherstellung fehlgeschlagen."
else
    echo "🔍 Stelle Datenbank wieder her..."
    gunzip -c "$BACKUP_FILE" | docker exec -i msh-db psql -U "$DB_USER" "$DB_NAME" || error_exit "Wiederherstellung fehlgeschlagen."
fi

echo "✅ Datenbank erfolgreich wiederhergestellt aus: $BACKUP_FILE" 