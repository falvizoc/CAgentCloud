# CLAUDE-DEVOPS.md - Contexto del Agente DevOps

> **Área:** Infraestructura, Docker, CI/CD, Azure
> **Responsabilidad:** Containerización, Deployment, Monitoreo
> **Última actualización:** 2025-12-23

---

## 1. Contexto del Proyecto

Infraestructura de **CobranzaCloud**, plataforma SaaS multi-tenant de gestión de cobranza.

### Stack de Infraestructura

| Componente | Tecnología | Propósito |
|------------|------------|-----------|
| Containers | Docker | Desarrollo y producción |
| Orchestration | Azure Container Apps | Hosting serverless |
| Database | Azure Database for PostgreSQL | Datos persistentes |
| Cache | Azure Cache for Redis | Sesiones, queue |
| CDN/WAF | Azure Front Door | Protección y caché |
| Secrets | Azure Key Vault | Gestión de secretos |
| CI/CD | GitHub Actions | Automatización |
| Monitoring | Azure Monitor + App Insights | Observabilidad |
| Registry | Azure Container Registry | Imágenes Docker |

### Documentos de Referencia

- [CLAUDE.md](../CLAUDE.md) - Contexto general del proyecto
- [docs/06-DEPLOYMENT.md](../docs/06-DEPLOYMENT.md) - Guía de despliegue
- [docs/03-SEGURIDAD.md](../docs/03-SEGURIDAD.md) - Políticas de seguridad

---

## 2. Estructura de Infraestructura

```
docker/
├── CLAUDE-DEVOPS.md              # Este archivo
├── docker-compose.yml            # Desarrollo local
├── docker-compose.override.yml   # Overrides de desarrollo
├── docker-compose.prod.yml       # Producción
├── docker-compose.test.yml       # Tests de integración
│
├── dockerfiles/
│   ├── backend.Dockerfile
│   ├── frontend.Dockerfile
│   └── nginx.Dockerfile
│
├── scripts/
│   ├── init-db.sh               # Inicialización de BD
│   ├── backup-db.sh             # Backup de BD
│   └── health-check.sh          # Health checks
│
└── config/
    ├── nginx/
    │   └── nginx.conf
    ├── postgres/
    │   └── init.sql
    └── redis/
        └── redis.conf

.github/
└── workflows/
    ├── ci.yml                   # Build y tests
    ├── cd-staging.yml           # Deploy a staging
    ├── cd-production.yml        # Deploy a producción
    └── security-scan.yml        # Escaneo de seguridad

infrastructure/
├── bicep/                       # IaC con Bicep
│   ├── main.bicep
│   ├── modules/
│   │   ├── container-app.bicep
│   │   ├── database.bicep
│   │   ├── redis.bicep
│   │   ├── keyvault.bicep
│   │   └── frontdoor.bicep
│   └── environments/
│       ├── dev.bicepparam
│       ├── staging.bicepparam
│       └── prod.bicepparam
└── scripts/
    └── deploy.sh
```

---

## 3. Docker Compose - Desarrollo Local

### 3.1 docker-compose.yml Base

```yaml
# docker/docker-compose.yml
version: '3.8'

services:
  # ============ FRONTEND ============
  frontend:
    build:
      context: ../src/frontend
      dockerfile: ../../docker/dockerfiles/frontend.Dockerfile
      target: development
    ports:
      - "3000:3000"
    environment:
      - NEXT_PUBLIC_API_URL=http://localhost:5000
      - NEXT_PUBLIC_CLERK_PUBLISHABLE_KEY=${CLERK_PUBLISHABLE_KEY}
    volumes:
      - ../src/frontend:/app
      - /app/node_modules
      - /app/.next
    depends_on:
      - backend
    networks:
      - cobranza-net

  # ============ BACKEND ============
  backend:
    build:
      context: ../src/backend
      dockerfile: ../../docker/dockerfiles/backend.Dockerfile
      target: development
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5000
      - ConnectionStrings__Default=Host=postgres;Database=cobranzacloud;Username=postgres;Password=postgres
      - ConnectionStrings__Redis=redis:6379
      - Jwt__Key=${JWT_SECRET_KEY}
      - Clerk__SecretKey=${CLERK_SECRET_KEY}
    volumes:
      - ../src/backend:/app
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - cobranza-net

  # ============ DATABASE ============
  postgres:
    image: postgres:16-alpine
    ports:
      - "5432:5432"
    environment:
      - POSTGRES_DB=cobranzacloud
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./config/postgres/init.sql:/docker-entrypoint-initdb.d/init.sql
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 5s
      timeout: 5s
      retries: 5
    networks:
      - cobranza-net

  # ============ CACHE ============
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
      - ./config/redis/redis.conf:/usr/local/etc/redis/redis.conf
    command: redis-server /usr/local/etc/redis/redis.conf
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 5s
      timeout: 3s
      retries: 5
    networks:
      - cobranza-net

  # ============ ADMINER (Dev Only) ============
  adminer:
    image: adminer:latest
    ports:
      - "8080:8080"
    depends_on:
      - postgres
    networks:
      - cobranza-net
    profiles:
      - debug

networks:
  cobranza-net:
    driver: bridge

volumes:
  postgres_data:
  redis_data:
```

### 3.2 Dockerfiles

```dockerfile
# docker/dockerfiles/backend.Dockerfile
# ============ BUILD STAGE ============
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copiar csproj y restaurar dependencias
COPY src/CobranzaCloud.Api/*.csproj ./CobranzaCloud.Api/
COPY src/CobranzaCloud.Core/*.csproj ./CobranzaCloud.Core/
COPY src/CobranzaCloud.Application/*.csproj ./CobranzaCloud.Application/
COPY src/CobranzaCloud.Infrastructure/*.csproj ./CobranzaCloud.Infrastructure/
RUN dotnet restore CobranzaCloud.Api/CobranzaCloud.Api.csproj

# Copiar código y compilar
COPY src/ .
RUN dotnet publish CobranzaCloud.Api/CobranzaCloud.Api.csproj -c Release -o /app/publish

# ============ DEVELOPMENT STAGE ============
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS development
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Hot reload
ENTRYPOINT ["dotnet", "watch", "run", "--project", "src/CobranzaCloud.Api/CobranzaCloud.Api.csproj"]

# ============ PRODUCTION STAGE ============
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS production
WORKDIR /app
EXPOSE 5000

# Security: Non-root user
RUN addgroup -g 1000 dotnet && adduser -u 1000 -G dotnet -s /bin/sh -D dotnet
USER dotnet

COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "CobranzaCloud.Api.dll"]
```

```dockerfile
# docker/dockerfiles/frontend.Dockerfile
# ============ BASE ============
FROM node:20-alpine AS base
WORKDIR /app
RUN npm install -g pnpm

# ============ DEPENDENCIES ============
FROM base AS deps
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

# ============ DEVELOPMENT ============
FROM base AS development
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
EXPOSE 3000
ENV NODE_ENV=development
CMD ["pnpm", "dev"]

# ============ BUILD ============
FROM base AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
RUN pnpm build

# ============ PRODUCTION ============
FROM base AS production
WORKDIR /app
ENV NODE_ENV=production

# Security: Non-root user
RUN addgroup -g 1001 -S nodejs && adduser -S nextjs -u 1001

COPY --from=builder /app/public ./public
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

USER nextjs
EXPOSE 3000
ENV PORT 3000
ENV HOSTNAME "0.0.0.0"

CMD ["node", "server.js"]
```

---

## 4. CI/CD con GitHub Actions

### 4.1 CI Pipeline

```yaml
# .github/workflows/ci.yml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  DOTNET_VERSION: '8.0.x'
  NODE_VERSION: '20.x'

jobs:
  # ============ BACKEND ============
  backend:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: src/backend

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Run tests
        run: dotnet test --no-build --configuration Release --verbosity normal --collect:"XPlat Code Coverage"

      - name: Check vulnerabilities
        run: dotnet list package --vulnerable --include-transitive

      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: '**/coverage.cobertura.xml'
          flags: backend

  # ============ FRONTEND ============
  frontend:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: src/frontend

    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'pnpm'
          cache-dependency-path: src/frontend/pnpm-lock.yaml

      - name: Install pnpm
        run: npm install -g pnpm

      - name: Install dependencies
        run: pnpm install --frozen-lockfile

      - name: Lint
        run: pnpm lint

      - name: Type check
        run: pnpm type-check

      - name: Run tests
        run: pnpm test --coverage

      - name: Build
        run: pnpm build

      - name: Check bundle size
        run: pnpm analyze

  # ============ DOCKER BUILD ============
  docker:
    runs-on: ubuntu-latest
    needs: [backend, frontend]
    if: github.ref == 'refs/heads/main'

    steps:
      - uses: actions/checkout@v4

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3

      - name: Build backend image
        uses: docker/build-push-action@v5
        with:
          context: ./src/backend
          file: ./docker/dockerfiles/backend.Dockerfile
          target: production
          push: false
          tags: cobranzacloud-api:${{ github.sha }}
          cache-from: type=gha
          cache-to: type=gha,mode=max

      - name: Build frontend image
        uses: docker/build-push-action@v5
        with:
          context: ./src/frontend
          file: ./docker/dockerfiles/frontend.Dockerfile
          target: production
          push: false
          tags: cobranzacloud-web:${{ github.sha }}
          cache-from: type=gha
          cache-to: type=gha,mode=max
```

### 4.2 CD Pipeline (Staging)

```yaml
# .github/workflows/cd-staging.yml
name: CD Staging

on:
  push:
    branches: [develop]

env:
  AZURE_CONTAINER_REGISTRY: cobranzacloud.azurecr.io
  RESOURCE_GROUP: cobranzacloud-staging-rg

jobs:
  deploy:
    runs-on: ubuntu-latest
    environment: staging

    steps:
      - uses: actions/checkout@v4

      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Login to ACR
        run: az acr login --name cobranzacloud

      - name: Build and push backend
        uses: docker/build-push-action@v5
        with:
          context: ./src/backend
          file: ./docker/dockerfiles/backend.Dockerfile
          target: production
          push: true
          tags: |
            ${{ env.AZURE_CONTAINER_REGISTRY }}/api:${{ github.sha }}
            ${{ env.AZURE_CONTAINER_REGISTRY }}/api:staging

      - name: Build and push frontend
        uses: docker/build-push-action@v5
        with:
          context: ./src/frontend
          file: ./docker/dockerfiles/frontend.Dockerfile
          target: production
          push: true
          tags: |
            ${{ env.AZURE_CONTAINER_REGISTRY }}/web:${{ github.sha }}
            ${{ env.AZURE_CONTAINER_REGISTRY }}/web:staging

      - name: Deploy to Container Apps
        run: |
          az containerapp update \
            --name cobranza-api \
            --resource-group ${{ env.RESOURCE_GROUP }} \
            --image ${{ env.AZURE_CONTAINER_REGISTRY }}/api:${{ github.sha }}

          az containerapp update \
            --name cobranza-web \
            --resource-group ${{ env.RESOURCE_GROUP }} \
            --image ${{ env.AZURE_CONTAINER_REGISTRY }}/web:${{ github.sha }}
```

### 4.3 Security Scan

```yaml
# .github/workflows/security-scan.yml
name: Security Scan

on:
  push:
    branches: [main, develop]
  schedule:
    - cron: '0 6 * * 1' # Lunes 6am

jobs:
  dependency-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      # .NET vulnerabilities
      - name: .NET Vulnerability Scan
        run: dotnet list src/backend/CobranzaCloud.sln package --vulnerable --include-transitive

      # npm vulnerabilities
      - name: npm audit
        run: |
          cd src/frontend
          npm audit --audit-level=high

      # Container scan
      - name: Run Trivy vulnerability scanner
        uses: aquasecurity/trivy-action@master
        with:
          scan-type: 'fs'
          scan-ref: '.'
          severity: 'CRITICAL,HIGH'

  code-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Initialize CodeQL
        uses: github/codeql-action/init@v2
        with:
          languages: csharp, javascript

      - name: Perform CodeQL Analysis
        uses: github/codeql-action/analyze@v2
```

---

## 5. Azure Infrastructure (Bicep)

### 5.1 Main Template

```bicep
// infrastructure/bicep/main.bicep
targetScope = 'subscription'

@description('Environment name')
@allowed(['dev', 'staging', 'prod'])
param environment string

@description('Location for all resources')
param location string = 'eastus2'

@description('Base name for resources')
param baseName string = 'cobranzacloud'

var resourceGroupName = '${baseName}-${environment}-rg'
var tags = {
  Environment: environment
  Project: 'CobranzaCloud'
  ManagedBy: 'Bicep'
}

// Resource Group
resource rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// Modules
module containerApp './modules/container-app.bicep' = {
  scope: rg
  name: 'containerApp'
  params: {
    baseName: baseName
    environment: environment
    location: location
  }
}

module database './modules/database.bicep' = {
  scope: rg
  name: 'database'
  params: {
    baseName: baseName
    environment: environment
    location: location
  }
}

module redis './modules/redis.bicep' = {
  scope: rg
  name: 'redis'
  params: {
    baseName: baseName
    environment: environment
    location: location
  }
}

module keyVault './modules/keyvault.bicep' = {
  scope: rg
  name: 'keyVault'
  params: {
    baseName: baseName
    environment: environment
    location: location
  }
}

module frontDoor './modules/frontdoor.bicep' = {
  scope: rg
  name: 'frontDoor'
  params: {
    baseName: baseName
    environment: environment
  }
}

// Outputs
output apiUrl string = containerApp.outputs.apiUrl
output webUrl string = containerApp.outputs.webUrl
output resourceGroupName string = resourceGroupName
```

---

## 6. Comandos Útiles

```bash
# ============ DOCKER ============
# Levantar entorno de desarrollo
docker-compose up -d

# Ver logs
docker-compose logs -f backend

# Reconstruir imagen específica
docker-compose up -d --build backend

# Limpiar todo
docker-compose down -v --remove-orphans

# ============ DATABASE ============
# Aplicar migraciones
docker-compose exec backend dotnet ef database update

# Crear migración
docker-compose exec backend dotnet ef migrations add NombreMigracion

# Backup
docker-compose exec postgres pg_dump -U postgres cobranzacloud > backup.sql

# ============ AZURE ============
# Login
az login

# Deploy Bicep
az deployment sub create \
  --location eastus2 \
  --template-file infrastructure/bicep/main.bicep \
  --parameters environment=staging

# Ver logs de Container App
az containerapp logs show -n cobranza-api -g cobranzacloud-staging-rg

# ============ DEBUGGING ============
# Shell en container
docker-compose exec backend sh

# Healthcheck
curl http://localhost:5000/health

# Redis CLI
docker-compose exec redis redis-cli
```

---

## 7. Principio FRICTIONLESS en DevOps

### Desarrollo Local

```bash
# ✅ CORRECTO: Un solo comando para levantar todo
docker-compose up

# ❌ INCORRECTO: Múltiples pasos manuales
# 1. Instalar PostgreSQL
# 2. Crear base de datos
# 3. Configurar variables
# 4. Ejecutar migraciones
# 5. Iniciar backend
# 6. Instalar node modules
# 7. Iniciar frontend
```

### Auto-migrate en Desarrollo

```csharp
// Program.cs - Solo en desarrollo
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}
```

### Environment Files

```bash
# .env.example - Sin secretos reales
POSTGRES_USER=postgres
POSTGRES_PASSWORD=change_me_in_production
JWT_SECRET_KEY=change_me_in_production_minimum_32_chars
CLERK_PUBLISHABLE_KEY=pk_test_xxxxx
CLERK_SECRET_KEY=sk_test_xxxxx

# A02 OWASP: Nunca commitear .env con secretos reales
```

---

## 8. Triggers para Intervención de Este Agente

Este agente debe intervenir cuando:

1. **Docker/Containers** - Cambios en Dockerfiles, docker-compose
2. **CI/CD** - Modificaciones a pipelines de GitHub Actions
3. **Infraestructura Azure** - Cambios en Bicep, configuración
4. **Secrets** - Gestión de Key Vault, variables de entorno
5. **Monitoreo** - Alertas, dashboards, logs
6. **Performance infra** - Scaling, optimización de recursos
7. **Seguridad infra** - WAF, networking, firewalls

---

*Documento de contexto para agente DevOps - Actualizar con cada cambio de infraestructura*
