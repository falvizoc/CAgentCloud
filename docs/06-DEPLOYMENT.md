# Guía de Despliegue

> **Versión:** 1.0
> **Fecha:** 2025-12-22
> **Estado:** Definición
> **Plataforma:** Azure

---

## 1. Arquitectura de Despliegue

### 1.1 Ambientes

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        AMBIENTES                                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  DESARROLLO (Local)                                                      │
│  ─────────────────                                                       │
│  Docker Compose + Hot Reload                                             │
│  Base de datos local                                                     │
│  Conector de prueba: bitmovil.ddns.net:5000                             │
│                                                                          │
│  STAGING (Azure)                                                         │
│  ───────────────                                                         │
│  Container Apps (min scale: 0)                                           │
│  PostgreSQL Flex (Basic tier)                                           │
│  Dominio: staging.cobranzacloud.com                                     │
│                                                                          │
│  PRODUCCIÓN (Azure)                                                      │
│  ────────────────                                                        │
│  Container Apps (auto-scale)                                             │
│  PostgreSQL Flex (General Purpose)                                      │
│  Front Door (CDN + WAF)                                                  │
│  Dominio: app.cobranzacloud.com                                         │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

### 1.2 Infraestructura Azure (Producción)

```
                                   ┌─────────────────────┐
                                   │    Azure Front      │
                                   │    Door + WAF       │
                                   └──────────┬──────────┘
                                              │
                        ┌─────────────────────┼─────────────────────┐
                        │                     │                     │
                        ▼                     ▼                     ▼
               ┌─────────────────┐   ┌─────────────────┐   ┌─────────────────┐
               │  Container App  │   │  Container App  │   │  Container App  │
               │   (Frontend)    │   │   (Backend)     │   │   (Worker)      │
               │   Next.js       │   │   .NET 8        │   │   Background    │
               └────────┬────────┘   └────────┬────────┘   └────────┬────────┘
                        │                     │                     │
                        │                     │                     │
               ┌────────┴─────────────────────┴─────────────────────┴────────┐
               │                        VNET                                  │
               │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
               │  │   PostgreSQL    │  │     Redis       │  │  Key Vault  │ │
               │  │   Flexible      │  │   (Cache)       │  │  (Secrets)  │ │
               │  └─────────────────┘  └─────────────────┘  └─────────────┘ │
               └──────────────────────────────────────────────────────────────┘
```

---

## 2. Setup de Desarrollo Local

### 2.1 Prerrequisitos

```bash
# Herramientas requeridas
- Docker Desktop >= 4.x
- .NET SDK 8.0
- Node.js >= 20.x
- Git
- VS Code (recomendado) o JetBrains Rider
```

### 2.2 Clonar y Configurar

```bash
# Clonar repositorio
git clone https://github.com/tu-org/cobranza-cloud.git
cd cobranza-cloud

# Copiar variables de entorno
cp .env.example .env

# Editar .env con valores locales
```

### 2.3 Variables de Entorno (.env.example)

```bash
# Database
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DB=cobranzacloud
POSTGRES_USER=postgres
POSTGRES_PASSWORD=localdev123

# Redis
REDIS_HOST=localhost
REDIS_PORT=6379

# JWT
JWT_KEY=your-super-secret-key-min-32-chars-long!!
JWT_ISSUER=cobranzacloud-dev
JWT_AUDIENCE=cobranzacloud-api

# OAuth (opcional en desarrollo)
OAUTH_GOOGLE_CLIENT_ID=
OAUTH_GOOGLE_CLIENT_SECRET=
OAUTH_MICROSOFT_CLIENT_ID=
OAUTH_MICROSOFT_CLIENT_SECRET=

# URLs
FRONTEND_URL=http://localhost:3000
API_URL=http://localhost:5000

# Email (desarrollo: usar Mailpit)
SMTP_HOST=localhost
SMTP_PORT=1025
```

### 2.4 Docker Compose Local

```yaml
# docker-compose.yml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    ports:
      - "5432:5432"
    environment:
      POSTGRES_DB: cobranzacloud
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: localdev123
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 5s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

  mailpit:
    image: axllent/mailpit
    ports:
      - "1025:1025"   # SMTP
      - "8025:8025"   # Web UI
    environment:
      MP_SMTP_AUTH_ACCEPT_ANY: 1
      MP_SMTP_AUTH_ALLOW_INSECURE: 1

  backend:
    build:
      context: ./src/backend
      dockerfile: Dockerfile.dev
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Host=postgres;Database=cobranzacloud;Username=postgres;Password=localdev123
      - ConnectionStrings__Redis=redis:6379
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_started
    volumes:
      - ./src/backend:/app
      - /app/bin
      - /app/obj

  frontend:
    build:
      context: ./src/frontend
      dockerfile: Dockerfile.dev
    ports:
      - "3000:3000"
    environment:
      - NEXT_PUBLIC_API_URL=http://localhost:5000
    depends_on:
      - backend
    volumes:
      - ./src/frontend:/app
      - /app/node_modules
      - /app/.next

volumes:
  postgres_data:
  redis_data:
```

### 2.5 Comandos de Desarrollo

```bash
# Levantar todo
docker-compose up -d

# Ver logs
docker-compose logs -f backend
docker-compose logs -f frontend

# Aplicar migraciones
docker-compose exec backend dotnet ef database update

# Acceder a PostgreSQL
docker-compose exec postgres psql -U postgres -d cobranzacloud

# Ver emails enviados
open http://localhost:8025

# Detener
docker-compose down

# Limpiar todo (incluyendo volúmenes)
docker-compose down -v
```

---

## 3. Dockerfiles

### 3.1 Backend Dockerfile (Producción)

```dockerfile
# src/backend/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj y restaurar
COPY ["CobranzaCloud.Api/CobranzaCloud.Api.csproj", "CobranzaCloud.Api/"]
COPY ["CobranzaCloud.Core/CobranzaCloud.Core.csproj", "CobranzaCloud.Core/"]
COPY ["CobranzaCloud.Data/CobranzaCloud.Data.csproj", "CobranzaCloud.Data/"]
RUN dotnet restore "CobranzaCloud.Api/CobranzaCloud.Api.csproj"

# Copiar resto y build
COPY . .
WORKDIR "/src/CobranzaCloud.Api"
RUN dotnet build -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000

# Usuario no-root
RUN adduser --disabled-password --gecos '' appuser
USER appuser

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CobranzaCloud.Api.dll"]
```

### 3.2 Backend Dockerfile (Desarrollo)

```dockerfile
# src/backend/Dockerfile.dev
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
EXPOSE 5000

# Hot reload
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENTRYPOINT ["dotnet", "watch", "run", "--project", "CobranzaCloud.Api/CobranzaCloud.Api.csproj", "--urls", "http://0.0.0.0:5000"]
```

### 3.3 Frontend Dockerfile (Producción)

```dockerfile
# src/frontend/Dockerfile
FROM node:20-alpine AS deps
WORKDIR /app
COPY package*.json ./
RUN npm ci

FROM node:20-alpine AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Build args para variables de entorno en build time
ARG NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL

RUN npm run build

FROM node:20-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production

# Usuario no-root
RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

COPY --from=builder /app/public ./public
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

USER nextjs
EXPOSE 3000
ENV PORT 3000

CMD ["node", "server.js"]
```

### 3.4 Frontend Dockerfile (Desarrollo)

```dockerfile
# src/frontend/Dockerfile.dev
FROM node:20-alpine
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
EXPOSE 3000

CMD ["npm", "run", "dev"]
```

---

## 4. CI/CD con GitHub Actions

### 4.1 Workflow Principal

```yaml
# .github/workflows/ci-cd.yml
name: CI/CD

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME_BACKEND: ${{ github.repository }}/backend
  IMAGE_NAME_FRONTEND: ${{ github.repository }}/frontend

jobs:
  # ===== TEST BACKEND =====
  test-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore src/backend/CobranzaCloud.sln

      - name: Build
        run: dotnet build src/backend/CobranzaCloud.sln --no-restore

      - name: Test
        run: dotnet test src/backend/CobranzaCloud.sln --no-build --verbosity normal

  # ===== TEST FRONTEND =====
  test-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: src/frontend/package-lock.json

      - name: Install dependencies
        run: npm ci
        working-directory: src/frontend

      - name: Lint
        run: npm run lint
        working-directory: src/frontend

      - name: Build
        run: npm run build
        working-directory: src/frontend
        env:
          NEXT_PUBLIC_API_URL: https://api.example.com

      - name: Test
        run: npm test
        working-directory: src/frontend

  # ===== BUILD & PUSH IMAGES =====
  build-and-push:
    needs: [test-backend, test-frontend]
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write

    steps:
      - uses: actions/checkout@v4

      - name: Login to Container Registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Build and push Backend
        uses: docker/build-push-action@v5
        with:
          context: src/backend
          push: true
          tags: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME_BACKEND }}:${{ github.sha }}

      - name: Build and push Frontend
        uses: docker/build-push-action@v5
        with:
          context: src/frontend
          push: true
          tags: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME_FRONTEND }}:${{ github.sha }}
          build-args: |
            NEXT_PUBLIC_API_URL=${{ secrets.PROD_API_URL }}

  # ===== DEPLOY TO AZURE =====
  deploy:
    needs: build-and-push
    runs-on: ubuntu-latest
    environment: production

    steps:
      - name: Login to Azure
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Deploy Backend to Container Apps
        uses: azure/container-apps-deploy-action@v1
        with:
          resourceGroup: cobranza-prod-rg
          containerAppName: cobranza-backend
          imageToDeploy: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME_BACKEND }}:${{ github.sha }}

      - name: Deploy Frontend to Container Apps
        uses: azure/container-apps-deploy-action@v1
        with:
          resourceGroup: cobranza-prod-rg
          containerAppName: cobranza-frontend
          imageToDeploy: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME_FRONTEND }}:${{ github.sha }}
```

---

## 5. Configuración de Azure

### 5.1 Crear Recursos (Azure CLI)

```bash
# Variables
RESOURCE_GROUP=cobranza-prod-rg
LOCATION=eastus
ENVIRONMENT=cobranza-env

# Crear Resource Group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Crear Container Apps Environment
az containerapp env create \
  --name $ENVIRONMENT \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION

# Crear PostgreSQL
az postgres flexible-server create \
  --name cobranza-db \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user dbadmin \
  --admin-password 'SecureP@ssw0rd!' \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --storage-size 32

# Crear Redis
az redis create \
  --name cobranza-cache \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Basic \
  --vm-size c0

# Crear Key Vault
az keyvault create \
  --name cobranza-kv \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION
```

### 5.2 Configurar Secrets en Key Vault

```bash
# Guardar secrets
az keyvault secret set --vault-name cobranza-kv --name "JwtKey" --value "your-secret-key"
az keyvault secret set --vault-name cobranza-kv --name "DbConnectionString" --value "Host=..."
az keyvault secret set --vault-name cobranza-kv --name "OAuthGoogleSecret" --value "..."
```

### 5.3 Crear Container Apps

```bash
# Backend
az containerapp create \
  --name cobranza-backend \
  --resource-group $RESOURCE_GROUP \
  --environment $ENVIRONMENT \
  --image ghcr.io/tu-org/cobranza-cloud/backend:latest \
  --target-port 5000 \
  --ingress external \
  --min-replicas 1 \
  --max-replicas 10 \
  --cpu 0.5 \
  --memory 1.0Gi \
  --secrets "jwt-key=keyvaultref:cobranza-kv/JwtKey,db-conn=keyvaultref:cobranza-kv/DbConnectionString" \
  --env-vars "Jwt__Key=secretref:jwt-key" "ConnectionStrings__Default=secretref:db-conn"

# Frontend
az containerapp create \
  --name cobranza-frontend \
  --resource-group $RESOURCE_GROUP \
  --environment $ENVIRONMENT \
  --image ghcr.io/tu-org/cobranza-cloud/frontend:latest \
  --target-port 3000 \
  --ingress external \
  --min-replicas 1 \
  --max-replicas 5 \
  --cpu 0.25 \
  --memory 0.5Gi
```

---

## 6. Monitoreo y Logging

### 6.1 Application Insights

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();

// appsettings.Production.json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=xxx;IngestionEndpoint=..."
  }
}
```

### 6.2 Serilog para Logs Estructurados

```csharp
// Program.cs
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(
        services.GetRequiredService<TelemetryConfiguration>(),
        TelemetryConverter.Traces));
```

### 6.3 Health Checks

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "database")
    .AddRedis(redisConnectionString, name: "redis");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

---

## 7. Backup y Recovery

### 7.1 PostgreSQL Backup

```bash
# Backup automático en Azure (configurado en portal)
# Retención: 7 días (Basic), 35 días (GP)

# Backup manual
az postgres flexible-server backup create \
  --resource-group $RESOURCE_GROUP \
  --server-name cobranza-db \
  --backup-name "manual-$(date +%Y%m%d)"

# Restore
az postgres flexible-server restore \
  --resource-group $RESOURCE_GROUP \
  --name cobranza-db-restored \
  --source-server cobranza-db \
  --restore-time "2025-12-22T15:00:00Z"
```

### 7.2 Disaster Recovery

```
┌─────────────────────────────────────────────────────────────┐
│                  DISASTER RECOVERY PLAN                      │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  RPO (Recovery Point Objective): 1 hora                     │
│  RTO (Recovery Time Objective): 4 horas                     │
│                                                              │
│  Backups:                                                    │
│  • PostgreSQL: Point-in-time recovery (35 días)             │
│  • Blobs: Geo-redundant storage                             │
│  • Secrets: Key Vault backup mensual                        │
│                                                              │
│  Failover:                                                   │
│  • DNS: Azure Front Door (automatic failover)               │
│  • DB: Read replica en región secundaria                    │
│  • Apps: Multi-region deployment (futuro)                   │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 8. Escalabilidad

### 8.1 Auto-scaling Rules

```bash
# Container Apps auto-scale
az containerapp update \
  --name cobranza-backend \
  --resource-group $RESOURCE_GROUP \
  --min-replicas 1 \
  --max-replicas 10 \
  --scale-rule-name http-rule \
  --scale-rule-type http \
  --scale-rule-http-concurrency 100
```

### 8.2 Métricas de Escalado

| Métrica | Threshold | Acción |
|---------|-----------|--------|
| CPU | > 70% | Scale out |
| Memory | > 80% | Scale out |
| HTTP requests | > 100/s | Scale out |
| Response time | > 500ms | Alert |

---

## 9. Checklist de Despliegue

### Pre-Producción
- [ ] Variables de entorno configuradas
- [ ] Secrets en Key Vault
- [ ] SSL/TLS configurado
- [ ] Health checks funcionando
- [ ] Logs centralizados
- [ ] Backups verificados
- [ ] DNS configurado

### Post-Despliegue
- [ ] Smoke tests pasando
- [ ] Métricas llegando
- [ ] Alertas configuradas
- [ ] Documentación actualizada
- [ ] Runbook de operaciones

---

## 10. Runbook de Operaciones

### 10.1 Rollback

```bash
# Ver revisiones anteriores
az containerapp revision list \
  --name cobranza-backend \
  --resource-group $RESOURCE_GROUP

# Activar revisión anterior
az containerapp revision activate \
  --name cobranza-backend \
  --resource-group $RESOURCE_GROUP \
  --revision cobranza-backend--xxxxxx
```

### 10.2 Escalar Manualmente

```bash
# Escalar a 5 réplicas
az containerapp update \
  --name cobranza-backend \
  --resource-group $RESOURCE_GROUP \
  --min-replicas 5 \
  --max-replicas 10
```

### 10.3 Ver Logs en Tiempo Real

```bash
az containerapp logs show \
  --name cobranza-backend \
  --resource-group $RESOURCE_GROUP \
  --follow
```

---

*Guía de despliegue - Actualizar con cada cambio de infraestructura*
