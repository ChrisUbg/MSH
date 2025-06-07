#!/bin/bash
set -e

# Wait for PostgreSQL
until nc -z db 5432; do
  echo "Waiting for PostgreSQL..."
  sleep 2
done

# Run migrations
echo "Running database migrations..."
dotnet MSH.Web.dll --migrate

# Start the application
echo "Starting application..."
exec dotnet MSH.Web.dll
