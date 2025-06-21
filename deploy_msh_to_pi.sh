#!/bin/bash

# Konfiguration
PI_USER="chregg"
PI_IP="192.168.0.104"
APP_NAME="msh-app"
IMAGE_NAME="msh-app-prod"
DB_CONTAINER="matter-dev-db-15"
DB_PASSWORD="84jfapa0d8f09qzhf22t"
DB_NAME="matter_prod"
APP_PORT="8080"

# 1. Image bauen (ARM64 für Raspberry Pi)
echo "🛠️  Baue Docker-Image für ARM64..."
docker buildx build --platform linux/arm64 -t $IMAGE_NAME -f src/MSH.Web/Dockerfile . || {
    echo "❌ Build fehlgeschlagen"; exit 1
}

# 2. Image lokal speichern
echo "💾 Speichere Image als TAR-Datei..."
docker save -o $IMAGE_NAME.tar $IMAGE_NAME || {
    echo "❌ Speichern fehlgeschlagen"; exit 1
}

# 3. Image auf Raspberry Pi übertragen
echo "📤 Übertrage Image zum Raspberry Pi..."
scp $IMAGE_NAME.tar $PI_USER@$PI_IP:~/msh/ || {
    echo "❌ Übertragung fehlgeschlagen"; exit 1
}

# 4. Auf dem Raspberry Pi: Image laden und starten
echo "🚀 Starte Container auf dem Raspberry Pi..."
ssh $PI_USER@$PI_IP << EOF
    cd ~/msh

    # Image laden
    echo "🔍 Lade Docker-Image..."
    docker load -i $IMAGE_NAME.tar || {
        echo "❌ Laden des Images fehlgeschlagen"; exit 1
    }

    # Alte Container stoppen/entfernen
    echo "🧹 Alte Container bereinigen..."
    docker stop $APP_NAME || true
    docker rm $APP_NAME || true
    docker stop $DB_CONTAINER || true
    docker rm $DB_CONTAINER || true

    # PostgreSQL-Container starten
    echo "🐘 Starte PostgreSQL-Container..."
    docker run -d \
        --name $DB_CONTAINER \
        -e POSTGRES_PASSWORD=$DB_PASSWORD \
        -e POSTGRES_DB=$DB_NAME \
        -p 5432:5432 \
        -v ~/msh/postgres-data:/var/lib/postgresql/data \
        postgres:15 || {
        echo "❌ PostgreSQL-Start fehlgeschlagen"; exit 1
    }

    # App-Container starten (Port 8080)
    echo "⚡ Starte MSH-App-Container..."
    docker run -d \
        --name $APP_NAME \
        --network host \
        -e ASPNETCORE_ENVIRONMENT=Production \
        -e ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=$DB_NAME;Username=postgres;Password=$DB_PASSWORD" \
        $IMAGE_NAME || {
        echo "❌ App-Start fehlgeschlagen"; exit 1
    }

    echo "✅ Deployment abgeschlossen!"
EOF

# 5. Verifikation
echo "🔍 Prüfe Container-Status..."
ssh $PI_USER@$PI_IP "docker ps --filter 'name=$APP_NAME\|$DB_CONTAINER'"

echo "🌐 Teste App-Erreichbarkeit..."
curl -v http://$PI_IP:$APP_PORT || {
    echo "⚠️  App nicht erreichbar – prüfe Logs mit: ssh $PI_USER@$PI_IP 'docker logs $APP_NAME'"
}

# Aufräumen
rm $IMAGE_NAME.tar
echo "✨ Skript beendet."