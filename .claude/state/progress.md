# Estado del Proyecto

> **Última actualización**: 2025-12-26
> **Versión**: 0.1.0 (Pre-MVP)

---

## Milestone Actual: M3 - Dashboard MVP (En Progreso)

### Roadmap General

| Milestone | Nombre | Estado | Fecha |
|-----------|--------|--------|-------|
| M0 | Foundation | ✅ Completado | 2025-12-23 |
| M1 | Core Auth | ✅ Completado | 2025-12-23 |
| M2 | Sync Infrastructure | ✅ Completado | 2025-12-26 |
| M3 | Dashboard MVP | 🔄 En Progreso | 2025-12-26 |
| M4 | Cobranza Básica | ⏳ Pendiente | - |

---

## M0: Foundation - COMPLETADO ✅

- [x] Proyecto base .NET 9 + Next.js 14
- [x] Docker Compose para desarrollo local
- [x] Base de datos PostgreSQL 16
- [x] Cache Redis 7
- [x] CI/CD básico (GitHub Actions)
- [x] Estructura de carpetas según arquitectura
- [x] Sistema de alineación de sesiones (.claude/)

### Servicios Verificados
| Servicio | Puerto | Estado |
|----------|--------|--------|
| PostgreSQL | 5432 | ✅ Healthy |
| Redis | 6379 | ✅ Healthy |
| Backend API | 5000 | ✅ Running |
| Frontend | 3000 | ✅ Running |

---

## M1: Core Auth - COMPLETADO ✅

### Backend (100% completado)
- [x] Entidades de dominio (User, Organization, RefreshToken)
- [x] Entidades de negocio (Cliente, Factura, Contacto, Connector)
- [x] Servicio JWT (TokenService)
- [x] Endpoints de autenticación
  - [x] POST /api/auth/register
  - [x] POST /api/auth/login
  - [x] POST /api/auth/refresh
  - [x] POST /api/auth/logout
  - [x] GET /api/auth/me
- [x] Configuraciones EF Core
- [x] Middleware de autenticación JWT
- [x] Políticas de autorización
- [x] Migración inicial (InitialCreate aplicada)

### Frontend (100% completado)
- [x] Componentes UI base (shadcn/ui: button, input, label, card)
- [x] Página de login (/login)
- [x] Página de registro (/register)
- [x] Auth Store (Zustand con persistencia)
- [x] Auth Hooks (TanStack Query mutations)
- [x] AuthProvider (auto-refresh tokens)
- [x] ProtectedRoute (redirección automática)
- [x] Integración token en API client

### OAuth (Opcional MVP)
- [ ] OAuth Google
- [ ] OAuth Microsoft 365

---

## M2: Sync Infrastructure - COMPLETADO ✅

### Backend (100% completado)
- [x] Entidad LinkCode (código de 6 dígitos)
- [x] Configuración EF Core LinkCode
- [x] DTOs Connectors (GenerateLinkCode, Register, Heartbeat)
- [x] DTOs Sync (SyncCartera con Clientes, Facturas, Contactos)
- [x] Interface IConnectorService
- [x] Implementación ConnectorService
  - [x] GenerateLinkCodeAsync
  - [x] RegisterConnectorAsync
  - [x] RecordHeartbeatAsync
  - [x] GenerateConnectorAccessToken (JWT especial)
  - [x] RefreshConnectorTokenAsync
- [x] ConnectorsEndpoints
  - [x] POST /api/connectors/link-code (requiere JWT usuario)
  - [x] POST /api/connectors/register (público, usa código)
  - [x] POST /api/connectors/heartbeat (requiere JWT conector)
  - [x] POST /api/connectors/refresh
- [x] SyncEndpoints
  - [x] POST /api/sync/cartera (requiere JWT conector)
- [x] Migración EF Core (AddLinkCodesAndConnectorRefreshTokens)
- [x] RefreshToken modificado para soportar ConnectorId

### Pendiente para integración completa
- [x] Aplicar migración en base de datos (2025-12-26)
- [ ] Tests de integración
- [ ] Probar con conector real

---

## M3: Dashboard MVP - EN PROGRESO 🔄

### Backend (100% completado)
- [x] DTOs Cartera (CarteraResumenResponse, CarteraAntiguedadResponse, etc.)
- [x] CarteraEndpoints
  - [x] GET /api/cartera/resumen (requiere JWT + CarteraRead)
  - [x] GET /api/cartera/antiguedad (requiere JWT + CarteraRead)
- [x] ClientesEndpoints
  - [x] GET /api/clientes (paginado, filtros, ordenamiento)
- [x] Registro de endpoints en EndpointRouteBuilderExtensions

### Frontend (100% completado)
- [x] Componentes UI (table, skeleton)
- [x] Layout del dashboard con navegación
- [x] Página del dashboard
- [x] Hooks de cartera (useCarteraResumen, useCarteraAntiguedad, useClientes)
- [x] Componentes dashboard:
  - [x] KpiCards (4 tarjetas de indicadores)
  - [x] AntiguedadChart (gráfico de barras de antigüedad)
  - [x] ClientesTable (tabla paginada de clientes)
- [x] Tipos TypeScript actualizados en contracts/api-types.ts

### Pendiente
- [ ] Validar con Node.js 20+ (requerido por Next.js 14)
- [ ] Prueba de flujo completo

---

## Blockers Actuales

**Menor**: Node.js en el entorno local es v16.14.0, se requiere v20.9.0+ para Next.js 14.

---

## Métricas

| Métrica | Valor |
|---------|-------|
| Sesiones totales | 3 |
| Commits | ~6 |
| Archivos creados M1 | 20+ |
| Archivos creados M3 | 12 |
| Tests | 0 (pendiente) |

---

*Para actualizar este archivo, editar al completar tareas o cambiar de milestone.*
