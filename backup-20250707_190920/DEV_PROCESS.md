# Matter Smart Home - Development Process

## 1. Docker Environment Setup
### Development
```dockerfile
# For local development (hot reload, debugging)
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /src
COPY . .
RUN dotnet restore
CMD ["dotnet", "watch", "--project", "src/MSH.Web"]
```

### Production
```dockerfile
# For production deployments
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "MSH.Web.dll"]
```
# Build Commands
| Development | docker-compose -f docker-compose.yml up --build |
| Production | docker-compose -f docker-compose.prod.yml build |
| CI/CD | docker build -f docker/Dockerfile.prod -t msh-app:latest . |