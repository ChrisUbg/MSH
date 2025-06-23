echo "Building matter-bridge with simplified Dockerfile on Pi..."
ssh chregg@$PI_HOST "cd ~/MSH/Matter && docker-compose -f docker-compose.prod-msh.yml build --no-cache"

# Add a cache-busting argument for the web build
CACHE_BUSTER=$(date +%s)
echo "Building web with cache buster: $CACHE_BUSTER"
ssh chregg@$PI_HOST "cd ~/MSH && docker-compose -f docker-compose.prod-msh.yml build --build-arg CACHE_BUSTER=$CACHE_BUSTER web"


echo "Starting matter-bridge..."
ssh chregg@$PI_HOST "cd ~/MSH/Matter && docker-compose -f docker-compose.prod-msh.yml up -d"

echo "Checking container status..."

# Add this line to the Dockerfile to receive the build argument
ARG CACHE_BUSTER=

WORKDIR /src
COPY ["src/MSH.Web/MSH.Web.csproj", "src/MSH.Web/"]
COPY ["src/MSH.Infrastructure/MSH.Infrastructure.csproj", "src/MSH.Infrastructure/"]
# ... existing code ... 