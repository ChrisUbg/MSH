#!/bin/bash
source config/environment.sh

# MSH Database Authentication Fix Script
# Run this script when you encounter PostgreSQL authentication issues

echo "🔧 MSH Database Authentication Fix"
echo "======================================"

# Check if we're on the Pi
if [[ $(hostname) == *"raspberrypi"* ]] || [[ $(hostname) == *"pi"* ]]; then
    echo "✅ Running on Raspberry Pi"
    PI_HOST="localhost"
    
    # Function to run commands locally
    run_on_pi() {
        eval "$1"
    }
    
    # Function to run docker commands locally
    docker_on_pi() {
        docker-compose -f docker-compose.prod-msh.yml $1
    }
else
    echo "🌐 Running remotely, connecting to Pi"
    PI_HOST="${PI_IP}"
    PI_USER="chregg"
    
    # Function to run commands on Pi
    run_on_pi() {
        ssh $PI_USER@$PI_HOST "$1"
    }
    
    # Function to run docker commands on Pi
    docker_on_pi() {
        run_on_pi "cd /${PROJECT_ROOT} && docker-compose -f docker-compose.prod-msh.yml $1"
    }
fi

echo "🔍 Checking current container status..."
if [[ $(hostname) == *"raspberrypi"* ]] || [[ $(hostname) == *"pi"* ]]; then
    docker-compose -f docker-compose.prod-msh.yml ps
else
    ssh $PI_USER@$PI_HOST "cd /${PROJECT_ROOT} && docker-compose -f docker-compose.prod-msh.yml ps"
fi

echo ""
echo "🛑 Stopping containers..."
docker_on_pi "down"

echo ""
echo "🧹 Cleaning up old containers and networks..."
if [[ $(hostname) == *"raspberrypi"* ]] || [[ $(hostname) == *"pi"* ]]; then
    docker system prune -f
    docker network prune -f
else
    run_on_pi "docker system prune -f && docker network prune -f"
fi

echo ""
echo "🚀 Starting containers with fresh configuration..."
docker_on_pi "up -d --build"

echo ""
echo "⏳ Waiting for database to be ready..."
sleep 30

echo ""
echo "🔧 Ensuring database authentication is correct..."
if [[ $(hostname) == *"raspberrypi"* ]] || [[ $(hostname) == *"pi"* ]]; then
    docker exec msh_db_1 psql -U postgres -d msh -c "ALTER USER postgres PASSWORD 'postgres';" 2>/dev/null || echo "Database not ready yet, will retry..."
    sleep 10
    docker exec msh_db_1 psql -U postgres -d msh -c "ALTER USER postgres PASSWORD 'postgres';"
else
    run_on_pi "docker exec msh_db_1 psql -U postgres -d msh -c /"ALTER USER postgres PASSWORD 'postgres';/" 2>/dev/null || echo 'Database not ready yet, will retry...'"
    sleep 10
    run_on_pi "docker exec msh_db_1 psql -U postgres -d msh -c /"ALTER USER postgres PASSWORD 'postgres';/""
fi

echo ""
echo "🔄 Restarting web application..."
docker_on_pi "restart web"

echo ""
echo "⏳ Waiting for services to be healthy..."
sleep 20

echo ""
echo "📊 Final status check..."
docker_on_pi "ps"

echo ""
echo "✅ Database authentication fix completed!"
echo ""
echo "🌐 Your MSH application should now be accessible at:"
if [[ $(hostname) == *"raspberrypi"* ]] || [[ $(hostname) == *"pi"* ]]; then
    echo "   http://localhost:8083"
else
    echo "   http://$PI_HOST:8083"
fi
echo ""
echo "🔧 If you still have issues, check the logs:"
echo "   docker-compose -f docker-compose.prod-msh.yml logs web"
echo "   docker-compose -f docker-compose.prod-msh.yml logs db" 