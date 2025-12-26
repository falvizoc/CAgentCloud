# Estado del Proyecto

> **Última actualización**: 2025-12-23
> **Versión**: 0.1.0 (Pre-MVP)

---

## Milestone Actual: M1 - Core Auth

### Roadmap General

| Milestone | Nombre | Estado | Fecha |
|-----------|--------|--------|-------|
| M0 | Foundation | ✅ Completado | 2025-12-23 |
| M1 | Core Auth | ✅ Completado | 2025-12-23 |
| M2 | Sync Infrastructure | ⏳ Pendiente | - |
| M3 | Dashboard MVP | ⏳ Pendiente | - |
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

## Blockers Actuales

Ninguno identificado.

---

## Métricas

| Métrica | Valor |
|---------|-------|
| Sesiones totales | 2 |
| Commits | ~6 |
| Archivos creados M1 | 20+ |
| Tests | 0 (pendiente) |

---

*Para actualizar este archivo, editar al completar tareas o cambiar de milestone.*
