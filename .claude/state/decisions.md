# Registro de Decisiones Técnicas

> Decisiones arquitectónicas y técnicas importantes tomadas durante el desarrollo.
> Formato: ADR (Architecture Decision Record) simplificado.

---

## DEC-001: Usar .NET 9 en lugar de .NET 8

**Fecha**: 2025-12-23
**Estado**: Aceptada
**Contexto**: La documentación original especificaba .NET 8, pero el SDK disponible en Windows es .NET 9.

**Decisión**: Usar .NET 9.0 para todo el backend.

**Consecuencias**:
- ✅ Mejor rendimiento y nuevas características
- ✅ SDK ya instalado, sin fricción adicional
- ⚠️ Actualizar CLAUDE-BACKEND.md para reflejar versión correcta
- ⚠️ Docker images usan mcr.microsoft.com/dotnet/sdk:9.0-alpine

**Archivos afectados**:
- `src/backend/**/*.csproj` - TargetFramework: net9.0
- `docker/dockerfiles/backend.Dockerfile` - SDK 9.0

---

## DEC-002: Docker Compose V2 (sin guión)

**Fecha**: 2025-12-23
**Estado**: Aceptada
**Contexto**: Ubuntu WSL tenía docker-compose v1.29.2 que es incompatible con Docker Engine moderno.

**Decisión**: Usar `docker compose` (v2) en lugar de `docker-compose` (v1).

**Consecuencias**:
- ✅ Compatibilidad con Docker Engine actual
- ✅ Mejor rendimiento y características
- ⚠️ Documentar en README que se use `docker compose`

---

## DEC-003: next.config.mjs en lugar de .ts

**Fecha**: 2025-12-23
**Estado**: Aceptada
**Contexto**: Next.js 14.2.x no soporta `next.config.ts`, solo `.js` o `.mjs`.

**Decisión**: Usar `next.config.mjs` con JSDoc para tipos.

**Consecuencias**:
- ✅ Compatible con Next.js 14.2
- ⚠️ Perder tipado TypeScript nativo en config
- ⚠️ Usar `/** @type {import('next').NextConfig} */` para tipos

**Archivos afectados**:
- `src/frontend/next.config.mjs` (antes .ts)

---

## DEC-004: Sistema de Alineación con `.claude/`

**Fecha**: 2025-12-23
**Estado**: Aceptada
**Contexto**: Las sesiones de Claude no tienen memoria entre sí, causando pérdida de contexto.

**Decisión**: Crear estructura `.claude/` con archivos de estado y sesión para persistir contexto.

**Consecuencias**:
- ✅ Continuidad entre sesiones
- ✅ Registro de decisiones y progreso
- ✅ Handoff efectivo entre agentes
- ⚠️ Requiere disciplina para mantener actualizado
- ⚠️ Agregar a .gitignore archivos temporales (CURRENT.md)

**Estructura**:
```
.claude/
├── sessions/   # CURRENT.md, HISTORY.md
├── state/      # progress.md, decisions.md, handoff.md
└── templates/  # Plantillas reutilizables
```

---

## DEC-005: Autenticación JWT con Email/Password para MVP

**Fecha**: 2025-12-23
**Estado**: Aceptada
**Contexto**: MVP necesita autenticación funcional. OAuth requiere configuración de proveedores externos.

**Decisión**: Implementar primero autenticación email/password con JWT, OAuth como opcional.

**Consecuencias**:
- ✅ MVP funcional sin dependencias externas
- ✅ Permite registro inmediato de usuarios
- ✅ JWT con refresh tokens (15 min / 7 días)
- ⚠️ OAuth Google/Microsoft queda para después
- ⚠️ Principio FRICTIONLESS: email confirmado automáticamente

**Archivos afectados**:
- `src/backend/src/CobranzaCloud.Api/Endpoints/AuthEndpoints.cs`
- `src/backend/src/CobranzaCloud.Infrastructure/Services/TokenService.cs`
- `src/backend/src/CobranzaCloud.Application/Auth/`

---

## DEC-006: Modelo Multi-tenant con OrganizationId

**Fecha**: 2025-12-23
**Estado**: Aceptada
**Contexto**: Sistema SaaS requiere aislamiento de datos entre clientes.

**Decisión**: Todas las entidades de negocio tienen OrganizationId. Validación obligatoria en cada query.

**Consecuencias**:
- ✅ Aislamiento de datos por organización
- ✅ Índices únicos por organización (ej: Cliente.Clave + OrganizationId)
- ✅ Claims JWT incluyen org_id
- ⚠️ Siempre validar OrganizationId en handlers, nunca confiar solo en el ID

**Archivos afectados**:
- `src/backend/src/CobranzaCloud.Core/Entities/*.cs`
- `src/backend/src/CobranzaCloud.Infrastructure/Data/Configurations/*.cs`

---

## DEC-007: Paquetes JWT en Infrastructure Layer

**Fecha**: 2025-12-23
**Estado**: Aceptada
**Contexto**: TokenService.cs usa clases de JWT pero los paquetes solo estaban en Api project.

**Decisión**: Agregar paquetes JWT directamente a Infrastructure.csproj donde se usa TokenService.

**Consecuencias**:
- ✅ Build de Docker exitoso
- ✅ Clean Architecture respetada (Infrastructure puede usar JWT)
- ⚠️ Versiones alineadas: Microsoft.IdentityModel.Tokens 8.14.0, System.IdentityModel.Tokens.Jwt 8.14.0

**Archivos afectados**:
- `src/backend/src/CobranzaCloud.Infrastructure/CobranzaCloud.Infrastructure.csproj`

---

## DEC-008: Sistema Link Code para Conectores (M2)

**Fecha**: 2025-12-26
**Estado**: Aceptada
**Contexto**: Los conectores locales necesitan vincularse de forma segura sin exponer credenciales.

**Decisión**: Implementar sistema de códigos de 6 caracteres alfanuméricos con las siguientes características:
- Código expira en 15 minutos
- Un solo uso
- Valida machine fingerprint entre generación y registro
- Tokens JWT especiales para conectores con claims `type: connector`

**Consecuencias**:
- ✅ FRICTIONLESS: Usuario solo escribe código de 6 dígitos en conector
- ✅ Seguro: No expone credenciales, tokens separados de usuarios
- ✅ Multi-tenant: OrganizationId vinculado automáticamente
- ⚠️ RefreshToken ahora soporta UserId OR ConnectorId (nullable)

**Archivos afectados**:
- `src/backend/src/CobranzaCloud.Core/Entities/LinkCode.cs` (nuevo)
- `src/backend/src/CobranzaCloud.Core/Entities/RefreshToken.cs` (modificado)
- `src/backend/src/CobranzaCloud.Infrastructure/Services/ConnectorService.cs` (nuevo)
- `src/backend/src/CobranzaCloud.Api/Endpoints/ConnectorsEndpoints.cs` (nuevo)
- `src/backend/src/CobranzaCloud.Api/Endpoints/SyncEndpoints.cs` (nuevo)

---

## DEC-009: Moneda MXN como Estándar por Defecto

**Fecha**: 2025-12-27
**Estado**: Aceptada
**Contexto**: El conector ASPEL soporta múltiples monedas (MXN=1, USD=2). Sin parámetro de moneda, la API suma aritméticamente ambas monedas sin conversión, lo cual puede dar datos incorrectos.

**Decisión**:
- **MUST**: Todas las consultas al conector ASPEL usan `?moneda=1` (MXN) por defecto
- El usuario puede opcionalmente consultar en otra moneda desde el dashboard
- Los datos cacheados se almacenan por moneda separadamente
- La UI muestra MXN a menos que el usuario cambie la preferencia

**Consecuencias**:
- ✅ Datos consistentes y precisos en pesos mexicanos
- ✅ Evita confusión por mezcla de monedas
- ✅ Cache eficiente por moneda
- ⚠️ Para ver USD, usuario debe cambiar filtro explícitamente
- ⚠️ Reportes multi-moneda requieren consultas separadas

**Implementación**:
- Parámetro `moneda=1` obligatorio en todas las llamadas al conector
- Redis keys incluyen sufijo de moneda: `cartera:01:resumen:MXN`
- Frontend muestra selector de moneda (default: MXN)

---

## DEC-010: Redis Cache para Datos del Conector

**Fecha**: 2025-12-27
**Estado**: Aceptada
**Contexto**: Las consultas al conector ASPEL son lentas (lectura directa de Firebird). Necesitamos cache para mejor UX.

**Decisión**:
- Usar Redis (ya configurado en Docker) como cache de datos del conector
- TTL de 15 minutos (alineado con ciclo de sync del conector)
- Cache keys por organización, empresa y moneda
- Invalidación manual disponible vía endpoint

**Consecuencias**:
- ✅ Dashboard carga en < 1 segundo
- ✅ Reduce carga en el conector remoto
- ✅ Resilencia: datos disponibles si conector offline temporalmente
- ⚠️ Datos pueden estar 15 min desactualizados
- ⚠️ Requiere estrategia de invalidación

**Cache Keys**:
```
cobranza:{orgId}:{empresaId}:resumen:{moneda}
cobranza:{orgId}:{empresaId}:antiguedad:{moneda}
cobranza:{orgId}:{empresaId}:clientes:page:{n}
```

---

## [Plantilla para nuevas decisiones]

<!--
## DEC-XXX: Título de la decisión

**Fecha**: YYYY-MM-DD
**Estado**: Propuesta | Aceptada | Deprecada | Reemplazada
**Contexto**: ¿Por qué surgió esta decisión?

**Decisión**: ¿Qué decidimos hacer?

**Consecuencias**:
- ✅ Beneficio 1
- ⚠️ Trade-off o consideración

**Archivos afectados**:
- path/to/file
-->
