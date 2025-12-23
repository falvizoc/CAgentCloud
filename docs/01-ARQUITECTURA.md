# Arquitectura del Sistema

> **Versión:** 1.0
> **Fecha:** 2025-12-22
> **Estado:** Definición

---

## 1. Visión de Arquitectura

### Principios Rectores

> **📋 Documento Normativo:** [08-FRICTIONLESS-MANIFEST.md](./08-FRICTIONLESS-MANIFEST.md)

| Principio | Descripción |
|-----------|-------------|
| **🎯 FRICTIONLESS First** | Usuario obtiene valor en < 5 minutos sin configuración manual |
| **Container-First** | Todo componente corre en Docker para replicabilidad |
| **Security by Design** | Seguridad integrada desde el diseño, no agregada después |
| **Separation of Concerns** | Cada servicio tiene una responsabilidad clara |
| **Outbound-Only** | Conectores nunca exponen puertos, solo conexiones salientes |
| **Multi-Tenant Ready** | Aislamiento lógico de datos desde el MVP |

### Aplicación FRICTIONLESS en Arquitectura

| Componente | Decisión FRICTIONLESS |
|------------|----------------------|
| Frontend | Clerk para auth pre-built, NextStep.js onboarding, cmdk ⌘K |
| Backend | Problem Details con sugerencias, auto-refresh tokens |
| Conector | Outbound-only = sin configurar firewall del cliente |
| Base de datos | Seeders con defaults, plantillas pre-cargadas |
| DevOps | Un solo `docker-compose up` para entorno completo |

---

## 2. Arquitectura de Alto Nivel

```
                                    ┌─────────────────────────┐
                                    │      INTERNET           │
                                    └───────────┬─────────────┘
                                                │
                    ┌───────────────────────────┼───────────────────────────┐
                    │                    AZURE CLOUD                         │
                    │                           │                            │
                    │              ┌────────────▼────────────┐              │
                    │              │    Azure Front Door     │              │
                    │              │    (CDN + WAF + LB)     │              │
                    │              └────────────┬────────────┘              │
                    │                           │                            │
                    │         ┌─────────────────┼─────────────────┐         │
                    │         │                 │                 │         │
                    │         ▼                 ▼                 ▼         │
                    │  ┌────────────┐   ┌────────────┐   ┌────────────┐    │
                    │  │  Frontend  │   │  Backend   │   │   Worker   │    │
                    │  │  Next.js   │   │  .NET 8    │   │  (Emails)  │    │
                    │  │  :3000     │   │  :5000     │   │            │    │
                    │  └─────┬──────┘   └─────┬──────┘   └─────┬──────┘    │
                    │        │                │                 │           │
                    │        │                ▼                 │           │
                    │        │    ┌──────────────────────┐     │           │
                    │        │    │     PostgreSQL       │     │           │
                    │        │    │     (Azure DB)       │     │           │
                    │        │    └──────────────────────┘     │           │
                    │        │                                  │           │
                    │        │    ┌──────────────────────┐     │           │
                    │        └───▶│       Redis          │◀────┘           │
                    │             │  (Sessions/Queue)    │                 │
                    │             └──────────────────────┘                 │
                    │                                                       │
                    └───────────────────────────────────────────────────────┘
                                                ▲
                                                │ HTTPS (443)
                                                │ Outbound Only
                    ┌───────────────────────────┼───────────────────────────┐
                    │           CLIENTE LOCAL   │                           │
                    │                           │                           │
                    │    ┌─────────────────────────────────────────┐       │
                    │    │         Conector ASPEL SAE v2.0         │       │
                    │    │         (Windows Service)                │       │
                    │    │                                          │       │
                    │    │  ┌─────────┐  ┌─────────┐  ┌─────────┐ │       │
                    │    │  │CloudSync│  │SyncEngine│ │StateStore│ │       │
                    │    │  └────┬────┘  └────┬────┘  └────┬────┘ │       │
                    │    │       │            │            │       │       │
                    │    └───────┼────────────┼────────────┼───────┘       │
                    │            │            │            │                │
                    │            ▼            ▼            ▼                │
                    │    ┌──────────────────────────────────────┐          │
                    │    │           ERP ASPEL SAE              │          │
                    │    │     (Firebird / SQL Server)          │          │
                    │    └──────────────────────────────────────┘          │
                    │                                                       │
                    └───────────────────────────────────────────────────────┘
```

---

## 3. Componentes del Sistema

### 3.1 Frontend (Next.js)

**Responsabilidades:**
- Renderizado de UI
- Autenticación de usuarios
- Manejo de estado de cliente
- Comunicación con API

**Tecnologías:**
- Next.js 14+ (App Router)
- React 18
- Tailwind CSS + shadcn/ui
- TanStack Query

**Estructura:**
```
frontend/
├── app/                    # App Router pages
│   ├── (auth)/            # Grupo de rutas auth
│   │   ├── login/
│   │   └── register/
│   ├── (dashboard)/       # Grupo protegido
│   │   ├── page.tsx       # Dashboard home
│   │   ├── cartera/
│   │   └── clientes/
│   └── layout.tsx
├── components/
│   ├── ui/                # shadcn components
│   └── features/          # Domain components
├── lib/
│   ├── api/              # API client
│   └── hooks/            # Custom hooks
└── types/
```

---

### 3.2 Backend (.NET 8)

**Responsabilidades:**
- API REST para frontend
- Autenticación y autorización
- Endpoints de sincronización
- Lógica de negocio
- Acceso a base de datos

**Tecnologías:**
- .NET 8 Minimal API
- Entity Framework Core
- ASP.NET Identity
- MediatR (CQRS ligero)

**Estructura:**
```
backend/
├── CobranzaCloud.Api/           # Entry point
│   ├── Program.cs
│   ├── Endpoints/               # Minimal API endpoints
│   │   ├── AuthEndpoints.cs
│   │   ├── SyncEndpoints.cs
│   │   └── CarteraEndpoints.cs
│   └── Middleware/
├── CobranzaCloud.Core/          # Domain logic
│   ├── Entities/
│   ├── Services/
│   └── Interfaces/
├── CobranzaCloud.Data/          # Data access
│   ├── AppDbContext.cs
│   ├── Migrations/
│   └── Repositories/
└── CobranzaCloud.Tests/
```

---

### 3.3 Base de Datos (PostgreSQL)

**Diseño Multi-Tenant:**
```sql
-- Cada tabla tiene OrganizationId para aislamiento
CREATE TABLE clientes (
    id UUID PRIMARY KEY,
    organization_id UUID NOT NULL REFERENCES organizations(id),
    clave VARCHAR(50),
    nombre VARCHAR(255),
    -- ... más campos
    CONSTRAINT unique_clave_per_org UNIQUE (organization_id, clave)
);

-- Row Level Security para aislamiento
ALTER TABLE clientes ENABLE ROW LEVEL SECURITY;
CREATE POLICY org_isolation ON clientes
    USING (organization_id = current_setting('app.current_org')::uuid);
```

**Esquema Principal:**
```
┌─────────────────┐     ┌─────────────────┐
│  organizations  │     │     users       │
├─────────────────┤     ├─────────────────┤
│ id (PK)         │◄────┤ organization_id │
│ name            │     │ id (PK)         │
│ plan            │     │ email           │
│ created_at      │     │ role            │
└────────┬────────┘     └─────────────────┘
         │
         │ 1:N
         ▼
┌─────────────────┐     ┌─────────────────┐
│   connectors    │     │     clientes    │
├─────────────────┤     ├─────────────────┤
│ id (PK)         │     │ id (PK)         │
│ organization_id │     │ organization_id │
│ name            │     │ clave           │
│ last_sync       │     │ nombre          │
│ status          │     │ saldo           │
└─────────────────┘     └─────────────────┘
                                │
                                │ 1:N
                                ▼
                        ┌─────────────────┐
                        │    facturas     │
                        ├─────────────────┤
                        │ id (PK)         │
                        │ cliente_id      │
                        │ folio           │
                        │ monto           │
                        │ dias_vencido    │
                        └─────────────────┘
```

---

### 3.4 Conector ASPEL (Existente + v2.0)

**Componentes Nuevos (v2.0):**
```
┌──────────────────────────────────────────────────────────┐
│                  CONECTOR v2.0                           │
│                                                          │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────┐   │
│  │ CloudClient │   │ SyncEngine  │   │ChangeDetect │   │
│  │             │   │             │   │             │   │
│  │ • HTTP/S    │◄──┤ • Timer     │◄──┤ • Checksums │   │
│  │ • JWT Auth  │   │ • Orchestr  │   │ • Delta     │   │
│  │ • Retry     │   │ • Batching  │   │ • Triggers  │   │
│  └──────┬──────┘   └─────────────┘   └──────┬──────┘   │
│         │                                    │          │
│         ▼                                    ▼          │
│  ┌─────────────┐                    ┌─────────────────┐│
│  │ StateStore  │                    │   ASPEL SAE     ││
│  │ (JWT, sync) │                    │   (Firebird/SQL)││
│  └─────────────┘                    └─────────────────┘│
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## 4. Flujos de Datos

### 4.1 Registro de Usuario
```
┌────────┐      ┌──────────┐      ┌─────────┐      ┌────────┐
│Frontend│      │  API     │      │  DB     │      │ Email  │
└───┬────┘      └────┬─────┘      └────┬────┘      └───┬────┘
    │                │                 │               │
    │ POST /register │                 │               │
    │───────────────▶│                 │               │
    │                │ Create user     │               │
    │                │────────────────▶│               │
    │                │                 │               │
    │                │ Create org      │               │
    │                │────────────────▶│               │
    │                │                 │               │
    │                │ Send welcome    │               │
    │                │────────────────────────────────▶│
    │                │                 │               │
    │ JWT + Refresh  │                 │               │
    │◀───────────────│                 │               │
    │                │                 │               │
```

### 4.2 Sincronización de Conector
```
┌──────────┐      ┌──────────┐      ┌─────────┐
│ Conector │      │  API     │      │  DB     │
└────┬─────┘      └────┬─────┘      └────┬────┘
     │                 │                 │
     │ (Timer 15 min)  │                 │
     │                 │                 │
     │ POST /sync/cartera               │
     │ [JWT + payload] │                 │
     │────────────────▶│                 │
     │                 │                 │
     │                 │ Validate JWT    │
     │                 │                 │
     │                 │ Upsert cartera  │
     │                 │────────────────▶│
     │                 │                 │
     │ 200 OK + stats  │                 │
     │◀────────────────│                 │
     │                 │                 │
```

### 4.3 OAuth Login
```
┌────────┐      ┌──────────┐      ┌─────────┐      ┌────────┐
│Frontend│      │  API     │      │  DB     │      │ Google │
└───┬────┘      └────┬─────┘      └────┬────┘      └───┬────┘
    │                │                 │               │
    │ Click "Google" │                 │               │
    │────────────────────────────────────────────────▶│
    │                │                 │               │
    │                │         Redirect + code         │
    │◀────────────────────────────────────────────────│
    │                │                 │               │
    │ POST /oauth/google/callback      │               │
    │───────────────▶│                 │               │
    │                │                 │               │
    │                │ Exchange code ────────────────▶│
    │                │◀───────────────────────────────│
    │                │                 │               │
    │                │ Find/Create user│               │
    │                │────────────────▶│               │
    │                │                 │               │
    │ JWT + Refresh  │                 │               │
    │◀───────────────│                 │               │
```

---

## 5. Decisiones de Arquitectura (ADRs)

### ADR-001: Outbound Sync vs Inbound Webhook
**Decisión:** Outbound Sync (Conector → Cloud)

**Contexto:** Necesitamos comunicar datos entre conector local y cloud.

**Alternativas:**
1. Inbound: Cloud llama al conector (requiere IP fija, firewall)
2. Outbound: Conector llama al cloud (solo HTTPS saliente)

**Decisión:** Outbound por:
- No requiere configuración de red en cliente
- Más seguro (no expone puertos)
- Funciona con NAT, proxies corporativos

---

### ADR-002: Multi-Tenant con Row-Level Security
**Decisión:** Base de datos compartida con RLS

**Alternativas:**
1. Database per tenant: Máximo aislamiento, costo alto
2. Schema per tenant: Buen aislamiento, migraciones complejas
3. Shared tables + RLS: Balance costo/aislamiento

**Decisión:** Shared + RLS por:
- Costo operativo menor
- Escalabilidad simple
- PostgreSQL RLS es maduro

---

### ADR-003: Next.js App Router
**Decisión:** Usar App Router en lugar de Pages Router

**Razones:**
- Server Components reducen JS del cliente
- Mejor SEO con RSC
- Streaming y Suspense nativos
- Es la dirección futura de Next.js

---

### ADR-004: .NET Minimal API
**Decisión:** Minimal API en lugar de Controllers

**Razones:**
- Menos boilerplate
- Mejor performance
- Más flexible para vertical slices
- Consistente con el conector existente

---

## 6. Consideraciones de Seguridad

### Perímetro Externo
```
Internet ─────▶ Azure Front Door ─────▶ App
                    │
                    ├── WAF (OWASP rules)
                    ├── DDoS protection
                    ├── Rate limiting
                    └── SSL termination
```

### Comunicación Interna
```
Frontend ◀──HTTPS──▶ Backend ◀──TLS──▶ PostgreSQL
                         │
                         └──TLS──▶ Redis
```

### Autenticación
```
┌────────────────────────────────────────────────┐
│               CAPAS DE AUTH                     │
├────────────────────────────────────────────────┤
│  Usuario → Frontend → JWT → Backend → DB       │
│                                                 │
│  Conector → JWT Connector → Backend → DB       │
└────────────────────────────────────────────────┘
```

---

## 7. Escalabilidad

### Horizontal
```
Azure Container Apps (auto-scale)
├── Frontend: 1-10 instancias
├── Backend: 1-20 instancias
└── Workers: 1-5 instancias
```

### Vertical
```
PostgreSQL: Scale up CPU/RAM según demanda
Redis: Cluster mode si necesario
```

### Por Cliente (Enterprise)
```
Namespace dedicado por cliente
├── Base de datos aislada
├── Secrets separados
└── Configuración personalizada
```

---

## 8. Monitoreo y Observabilidad

### Stack Propuesto
```
┌──────────────────────────────────────────────────┐
│                OBSERVABILIDAD                     │
├──────────────────────────────────────────────────┤
│  Logs    → Azure Log Analytics / Seq             │
│  Metrics → Azure Monitor / Prometheus            │
│  Traces  → Azure Application Insights            │
│  Alerts  → Azure Alerts / PagerDuty              │
└──────────────────────────────────────────────────┘
```

### Métricas Clave
- Request latency (p50, p95, p99)
- Error rate por endpoint
- Sync success rate
- Connectors online count
- Database connection pool

---

*Documento de arquitectura - Actualizar con cada ADR nuevo*
