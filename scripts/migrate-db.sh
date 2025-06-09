#!/bin/bash

# Exit on error
set -e

echo "Starting database migration..."

# Check if dotnet-ef is installed
if ! dotnet tool list --global | grep -q "dotnet-ef"; then
    echo "Installing dotnet-ef tools..."
    dotnet tool install --global dotnet-ef
fi

# Build the infrastructure project first
echo "Building MSH.Infrastructure..."
dotnet build src/MSH.Infrastructure/MSH.Infrastructure.csproj

# Run the migration
echo "Running database migration..."
dotnet ef database update \
    --project src/MSH.Infrastructure/MSH.Infrastructure.csproj \
    --startup-project src/MSH.Web/MSH.Web.csproj

echo "Migration completed successfully!" 