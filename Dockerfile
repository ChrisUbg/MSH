FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/MSH.Web/MSH.Web.csproj", "src/MSH.Web/"]
COPY ["src/MSH.Infrastructure/MSH.Infrastructure.csproj", "src/MSH.Infrastructure/"]
COPY ["src/MSH.Core/MSH.Core.csproj", "src/MSH.Core/"]
COPY ["src/MSH.Matter/MSH.Matter.csproj", "src/MSH.Matter/"]
RUN dotnet restore "src/MSH.Web/MSH.Web.csproj"

# Copy libman.json and restore client-side libraries
COPY ["src/MSH.Web/libman.json", "src/MSH.Web/"]
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli
RUN ~/.dotnet/tools/libman restore --root src/MSH.Web/

# Copy the rest of the code
COPY . .

# Build the application
RUN dotnet build "src/MSH.Web/MSH.Web.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "src/MSH.Web/MSH.Web.csproj" -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for healthcheck, PostgreSQL client tools, and Docker CLI
RUN apt-get update && apt-get install -y curl gnupg2 lsb-release wget ca-certificates && \
    sh -c 'echo "deb http://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list' && \
    wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | apt-key add - && \
    apt-get update && \
    apt-get install -y postgresql-client-16 && \
    apt-get update && \
    apt-get install -y gn && \
    # Install Docker CLI
    curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg && \
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/debian $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null && \
    apt-get update && \
    apt-get install -y docker-ce-cli && \
    rm -rf /var/lib/apt/lists/*

# Install gn (Google build tool)
RUN wget -O /usr/local/bin/gn https://chrome-infra-packages.appspot.com/dl/gn/gn/linux-arm64/+/latest -q --show-progress && \
    chmod +x /usr/local/bin/gn

# Copy published files
COPY --from=publish /app/publish .

# Copy the network configuration script
COPY network-config.sh /app/network-config.sh
RUN chmod +x /app/network-config.sh

# Create directory for DataProtection keys
RUN mkdir -p /root/.aspnet/DataProtection-Keys

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8082

# Expose the port
EXPOSE 8082

# Start the application
CMD ["dotnet", "MSH.Web.dll"] 