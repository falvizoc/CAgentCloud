#!/bin/bash
# =============================================================================
# CobranzaCloud - Database Initialization Script
# =============================================================================

set -e

echo "Initializing CobranzaCloud database..."

# Wait for PostgreSQL to be ready
until pg_isready -h localhost -p 5432 -U postgres; do
    echo "Waiting for PostgreSQL..."
    sleep 2
done

echo "PostgreSQL is ready!"

# Run migrations (from backend container)
if [ -f "/app/src/CobranzaCloud.Api/CobranzaCloud.Api.csproj" ]; then
    echo "Running EF Core migrations..."
    dotnet ef database update -p /app/src/CobranzaCloud.Infrastructure -s /app/src/CobranzaCloud.Api
    echo "Migrations applied successfully!"
else
    echo "Backend project not found, skipping migrations."
fi

echo "Database initialization completed."
