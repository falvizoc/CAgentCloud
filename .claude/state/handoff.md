# Handoff - Estado de Milestones

> **Última actualización**: 2025-12-23
> **Sesión**: 20251223-fullstack-004

---

## Checklist de Milestones

### M0: Foundation ✅ COMPLETADO
- [x] Proyecto base .NET 9 + Next.js 14
- [x] Docker Compose para desarrollo local
- [x] Base de datos PostgreSQL 16
- [x] Cache Redis 7
- [x] CI/CD básico (GitHub Actions)
- [x] Estructura de carpetas según arquitectura
- [x] Sistema de alineación de sesiones (.claude/)

### M1: Core Auth ✅ COMPLETADO

#### Backend ✅ COMPLETADO
- [x] Entidades de dominio (User, Organization, RefreshToken)
- [x] Entidades de negocio (Cliente, Factura, Contacto, Connector)
- [x] Servicio JWT (TokenService)
- [x] Endpoints de autenticación:
  - [x] POST `/api/auth/register` - Registro de usuario + organización
  - [x] POST `/api/auth/login` - Login con email/password
  - [x] POST `/api/auth/refresh` - Refresh de tokens
  - [x] POST `/api/auth/logout` - Revocación de token
  - [x] GET `/api/auth/me` - Usuario actual
- [x] Configuraciones EF Core para todas las entidades
- [x] Middleware de autenticación JWT
- [x] Políticas de autorización (CarteraRead, CarteraWrite, UsersManage)
- [x] Migración inicial (InitialCreate aplicada)
- [x] Test de registro exitoso

#### Frontend ✅ COMPLETADO
- [x] Componentes UI base (shadcn/ui: button, input, label, card)
- [x] Página de login (`/login`) - React Hook Form + Zod
- [x] Página de registro (`/register`) - Validación completa
- [x] Auth Store (Zustand con persistencia localStorage)
- [x] Auth Hooks (TanStack Query mutations)
- [x] AuthProvider (auto-refresh tokens cada 30s)
- [x] ProtectedRoute (redirección automática a /login)
- [x] Integración token en API client (setTokenGetter)
- [x] Toaster (Sonner) para notificaciones

#### OAuth (Opcional MVP)
- [ ] OAuth Google
- [ ] OAuth Microsoft 365

### M2: Sync Infrastructure ⏳ PENDIENTE
- [ ] POST `/connectors/link-code` - Generar código 6 dígitos
- [ ] POST `/connectors/register` - Registrar conector
- [ ] POST `/connectors/heartbeat` - Heartbeat cada 5 min
- [ ] POST `/sync/cartera` - Recibir datos de cartera
- [ ] POST `/auth/connector/refresh` - Refresh tokens conectores
- [ ] JWT para conectores (claims especiales)
- [ ] Almacenamiento de cartera sincronizada

### M3: Dashboard MVP ⏳ PENDIENTE
- [ ] Panel de indicadores de cartera
- [ ] Resumen de antigüedad
- [ ] Lista de clientes con saldo
- [ ] Detalle de cliente

### M4: Cobranza Básica ⏳ PENDIENTE
- [ ] Configuración de plantillas de email
- [ ] Envío manual de recordatorios
- [ ] Historial de comunicaciones

---

## Commits de esta sesión

| Hash | Mensaje |
|------|---------|
| `1f3f273` | feat(M1): implement backend authentication system |
| `8915cef` | fix(M1): add JWT packages to Infrastructure layer |
| `9f819c5` | feat(M1): add EF Core migrations and complete backend auth |

---

## Contexto crítico

1. **Versiones**: .NET 9, Next.js 14.2, PostgreSQL 16, Redis 7
2. **JWT**: Access token 15 min, Refresh token 7 días
3. **FRICTIONLESS**: Email confirmed = true por defecto
4. **Roles**: Owner, Admin, Manager, Collector, Viewer
5. **Docker**: Usar `docker compose` (v2, no `docker-compose`)
6. **Paquetes JWT**: v8.14.0 (Microsoft.IdentityModel.Tokens, System.IdentityModel.Tokens.Jwt)

---

## Comandos útiles

```bash
# Levantar servicios
cd docker && docker compose --profile full up -d

# Crear migración
docker compose --profile full exec backend dotnet ef migrations add NombreMigracion \
  -p src/CobranzaCloud.Infrastructure \
  -s src/CobranzaCloud.Api \
  -o Data/Migrations

# Ver logs backend
docker compose --profile full logs -f backend

# Test de registro
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test123!","nombre":"Test User","organizacion":{"nombre":"Test Org"}}'

# Test de login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test123!"}'
```

---

## Próximo paso recomendado

**M2: Sync Infrastructure**
- Implementar endpoints para conectores ASPEL SAE
- Código de vinculación de 6 dígitos
- Heartbeat y sync de cartera

---

## Archivos creados en M1 Frontend

```
src/frontend/src/
├── components/
│   ├── auth/
│   │   ├── auth-provider.tsx
│   │   ├── protected-route.tsx
│   │   └── index.ts
│   └── ui/
│       ├── button.tsx
│       ├── input.tsx
│       ├── label.tsx
│       └── card.tsx
├── app/
│   └── (auth)/
│       ├── layout.tsx
│       ├── login/
│       │   └── page.tsx
│       └── register/
│           └── page.tsx
└── lib/
    ├── stores/
    │   └── auth-store.ts
    ├── hooks/
    │   └── use-auth.ts
    └── providers.tsx (modificado)
```

---

*Este archivo se actualiza al final de cada sesión para mantener continuidad.*
