# Stage 1: Build .NET app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and projects
COPY ["src/MSH.Web/MSH.Web.csproj", "src/MSH.Web/"]
COPY ["src/MSH.Infrastructure/MSH.Infrastructure.csproj", "src/MSH.Infrastructure/"]
COPY ["src/MSH.Core/MSH.Core.csproj", "src/MSH.Core/"]
RUN dotnet restore "src/MSH.Web/MSH.Web.csproj"

# Copy everything else and publish
COPY . .
RUN dotnet publish "src/MSH.Web/MSH.Web.csproj" -c Release -o /app/publish

# Stage 2: Final image (ohne Matter)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install runtime dependencies (nur .NET-Abhängigkeiten)
RUN apt-get update && \
    apt-get install -y libgdiplus libssl-dev && \
    rm -rf /var/lib/apt/lists/*

# Copy .NET artifacts
COPY --from=build /app/publish .

# Entrypoint (direkter .NET-Start)
ENTRYPOINT ["dotnet", "MSH.Web.dll"]