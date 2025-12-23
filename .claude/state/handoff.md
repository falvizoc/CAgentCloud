# Handoff - Notas para Próxima Sesión

> **Última actualización**: 2025-12-23
> **Sesión anterior**: 20251223-fullstack-002
> **Próximo agente sugerido**: Claude-Frontend

---

## Resumen Ejecutivo

**M0: Foundation COMPLETADO.**
**M1: Core Auth EN PROGRESO** - Backend 90% completado, falta frontend.

---

## Dónde lo dejamos

### Backend M1 - COMPLETADO
- [x] Entidades: Organization, User, RefreshToken, Connector, Cliente, Factura, Contacto
- [x] Servicio de tokens JWT (TokenService)
- [x] Endpoints de autenticación:
  - POST `/api/auth/register` - Registro de usuario + organización
  - POST `/api/auth/login` - Login con email/password
  - POST `/api/auth/refresh` - Refresh de tokens
  - POST `/api/auth/logout` - Revocación de token
  - GET `/api/auth/me` - Usuario actual
- [x] Configuraciones EF Core para todas las entidades
- [x] Middleware de autenticación JWT
- [x] Políticas de autorización (CarteraRead, CarteraWrite, UsersManage)

### Pendiente
- [ ] Crear migración inicial y probar
- [ ] Frontend: páginas de login/registro
- [ ] OAuth Google (opcional para MVP)
- [ ] OAuth Microsoft (opcional para MVP)

---

## Qué necesita hacerse (Frontend Auth)

### Prioridad Alta
1. **Crear páginas de autenticación**
   - `/login` - Formulario de login
   - `/register` - Formulario de registro
   - `/` (home) - Redirección según estado de auth

2. **Implementar manejo de tokens**
   - Guardar tokens en localStorage o httpOnly cookies
   - Interceptor para agregar Authorization header
   - Auto-refresh de tokens

3. **Crear contexto de autenticación**
   - AuthContext con user, isAuthenticated, login, logout
   - Proteger rutas privadas

### Documentación relevante
- [CLAUDE-FRONTEND.md](src/frontend/CLAUDE-FRONTEND.md) - Patrones de React/Next.js
- [api-types.ts](docs/contracts/api-types.ts) - Tipos de auth

---

## Archivos clave creados en M1

### Backend - Auth
| Archivo | Propósito |
|---------|-----------|
| `src/backend/src/CobranzaCloud.Core/Entities/User.cs` | Entidad de usuario |
| `src/backend/src/CobranzaCloud.Core/Entities/Organization.cs` | Entidad de organización |
| `src/backend/src/CobranzaCloud.Core/Entities/RefreshToken.cs` | Tokens de refresh |
| `src/backend/src/CobranzaCloud.Application/Auth/` | DTOs y interfaces de auth |
| `src/backend/src/CobranzaCloud.Infrastructure/Services/TokenService.cs` | Generación de JWT |
| `src/backend/src/CobranzaCloud.Api/Endpoints/AuthEndpoints.cs` | Endpoints de auth |
| `src/backend/src/CobranzaCloud.Infrastructure/Data/Configurations/` | Configuraciones EF |

---

## Contexto crítico a recordar

1. **Versiones**: .NET 9, Next.js 14.2, PostgreSQL 16, Redis 7
2. **JWT**: Access token 15 min, Refresh token 7 días
3. **FRICTIONLESS**: Email confirmed = true por defecto (sin verificación)
4. **Roles**: Owner, Admin, Manager, Collector, Viewer
5. **Docker**: Usar `docker compose` (v2)

---

## Comandos útiles

```bash
# Levantar servicios
cd docker && docker compose --profile full up -d

# Crear migración (cuando Docker esté arriba)
cd src/backend
dotnet ef migrations add InitialCreate -p src/CobranzaCloud.Infrastructure -s src/CobranzaCloud.Api

# Ver logs
docker compose --profile full logs -f backend

# Probar registro (cuando esté arriba)
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test123!","nombre":"Test User","organizacion":{"nombre":"Test Org"}}'
```

---

## Siguiente paso recomendado

1. Reconstruir backend con Docker para crear migración
2. Probar endpoints de auth
3. Iniciar frontend con páginas de login/registro

---

*Este archivo debe actualizarse al final de cada sesión.*
