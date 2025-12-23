#!/bin/bash
# =============================================================================
# CobranzaCloud - Health Check Script
# =============================================================================

set -e

echo "Checking service health..."

# Check PostgreSQL
echo -n "PostgreSQL: "
if pg_isready -h localhost -p 5432 -U postgres > /dev/null 2>&1; then
    echo "✓ Healthy"
else
    echo "✗ Unhealthy"
    exit 1
fi

# Check Redis
echo -n "Redis: "
if redis-cli -h localhost -p 6379 ping > /dev/null 2>&1; then
    echo "✓ Healthy"
else
    echo "✗ Unhealthy"
    exit 1
fi

# Check Backend API
echo -n "Backend API: "
if curl -sf http://localhost:5000/health > /dev/null 2>&1; then
    echo "✓ Healthy"
else
    echo "✗ Unhealthy (may not be started)"
fi

# Check Frontend
echo -n "Frontend: "
if curl -sf http://localhost:3000 > /dev/null 2>&1; then
    echo "✓ Healthy"
else
    echo "✗ Unhealthy (may not be started)"
fi

echo ""
echo "Health check completed."
