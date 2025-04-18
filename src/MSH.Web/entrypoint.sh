#!/bin/bash
set -e

# Nur PostgreSQL-Check und Migration
until nc -z db 5432; do
  echo "Waiting for PostgreSQL..."
  sleep 2
done

dotnet MSH.Web.dll --migrate
exec dotnet MSH.Web.dll
