# Guía de Contribución

> **Proyecto:** CobranzaCloud
> **Versión:** 1.0
> **Fecha:** 2025-12-23

---

## 1. Configuración del Entorno

### Requisitos Previos

| Herramienta | Versión | Propósito |
|-------------|---------|-----------|
| Docker Desktop | 24.x+ | Contenedores |
| Node.js | 20.x LTS | Frontend |
| .NET SDK | 8.0 | Backend |
| Git | 2.x+ | Control de versiones |
| VS Code / Rider | Última | IDE |

### Setup Inicial

```bash
# 1. Clonar repositorio
git clone https://github.com/[org]/cobranzacloud.git
cd cobranzacloud

# 2. Copiar variables de entorno
cp .env.example .env

# 3. Levantar infraestructura
docker-compose up -d postgres redis

# 4. Backend
cd src/backend
dotnet restore
dotnet ef database update -s src/CobranzaCloud.Api
dotnet run --project src/CobranzaCloud.Api

# 5. Frontend (nueva terminal)
cd src/frontend
pnpm install
pnpm dev
```

### Accesos Locales

| Servicio | URL |
|----------|-----|
| Frontend | http://localhost:3000 |
| Backend API | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |
| Adminer (DB) | http://localhost:8080 |

---

## 2. Flujo de Trabajo Git

### Branches

```
main            # Producción - protegida
├── develop     # Integración - base para features
├── feature/*   # Nuevas funcionalidades
├── fix/*       # Corrección de bugs
├── hotfix/*    # Fixes urgentes a producción
└── docs/*      # Cambios de documentación
```

### Crear Feature Branch

```bash
# Desde develop actualizado
git checkout develop
git pull origin develop
git checkout -b feature/nombre-descriptivo
```

### Commits

Usamos **Conventional Commits**:

```
<type>(<scope>): <description>

[body opcional]

[footer opcional]
```

**Types:**
| Type | Uso |
|------|-----|
| `feat` | Nueva funcionalidad |
| `fix` | Corrección de bug |
| `docs` | Documentación |
| `style` | Formato (no afecta código) |
| `refactor` | Refactoring |
| `test` | Tests |
| `chore` | Mantenimiento |

**Ejemplos:**

```bash
git commit -m "feat(clientes): agregar endpoint de búsqueda"
git commit -m "fix(auth): corregir validación de JWT expirado"
git commit -m "docs(api): actualizar spec de cartera"
git commit -m "test(clientes): agregar tests de integración"
```

### Pull Request

1. Push de tu branch:
   ```bash
   git push origin feature/nombre-descriptivo
   ```

2. Crear PR en GitHub hacia `develop`

3. Llenar template de PR:
   ```markdown
   ## Descripción
   [Qué hace este PR]

   ## Tipo de cambio
   - [ ] Nueva funcionalidad
   - [ ] Bug fix
   - [ ] Refactoring
   - [ ] Documentación

   ## Checklist
   - [ ] Tests agregados/actualizados
   - [ ] Documentación actualizada
   - [ ] Código sigue convenciones del proyecto
   - [ ] Sin warnings de lint

   ## Screenshots (si aplica)
   [Capturas de UI]
   ```

4. Esperar review y CI verde

5. Merge (squash preferido)

---

## 3. Convenciones de Código

### Backend (.NET)

Ver: [src/backend/CLAUDE-BACKEND.md](src/backend/CLAUDE-BACKEND.md)

**Resumen:**
- Minimal API, no Controllers
- MediatR para CQRS
- FluentValidation
- Result pattern para errores
- Serilog para logging

**Lint:**
```bash
dotnet format --verify-no-changes
```

### Frontend (Next.js)

Ver: [src/frontend/CLAUDE-FRONTEND.md](src/frontend/CLAUDE-FRONTEND.md)

**Resumen:**
- TypeScript estricto
- shadcn/ui para componentes
- TanStack Query para data fetching
- Zod para validación
- Skeleton loading (no spinners)

**Lint:**
```bash
pnpm lint
pnpm type-check
```

### DevOps

Ver: [docker/CLAUDE-DEVOPS.md](docker/CLAUDE-DEVOPS.md)

---

## 4. Testing

### Backend

```bash
# Unit tests
dotnet test tests/CobranzaCloud.UnitTests

# Integration tests (requiere Docker)
dotnet test tests/CobranzaCloud.IntegrationTests

# Con coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend

```bash
# Unit tests
pnpm test

# Watch mode
pnpm test:watch

# Coverage
pnpm test --coverage
```

### E2E (Playwright)

```bash
pnpm e2e
pnpm e2e:ui  # Con UI
```

### Cobertura Mínima

| Área | Mínimo |
|------|--------|
| Core Services | 80% |
| API Endpoints | 70% |
| UI Components | 60% |

---

## 5. Desarrollo con Claude

### Archivos de Contexto

Antes de usar Claude, asegúrate de que lea:

| Área | Archivo |
|------|---------|
| General | `CLAUDE.md` |
| Backend | `src/backend/CLAUDE-BACKEND.md` |
| Frontend | `src/frontend/CLAUDE-FRONTEND.md` |
| DevOps | `docker/CLAUDE-DEVOPS.md` |
| Tipos | `docs/contracts/api-types.ts` |

### Guía de Prompts

Ver: [docs/12-CLAUDE-PROMPTS-GUIDE.md](docs/12-CLAUDE-PROMPTS-GUIDE.md)

---

## 6. Estructura de Archivos

### Nuevo Endpoint Backend

```
src/backend/
├── src/CobranzaCloud.Api/
│   └── Endpoints/
│       └── [Nombre]Endpoints.cs        # Agregar aquí
├── src/CobranzaCloud.Application/
│   ├── Commands/[Nombre]/
│   │   ├── [Nombre]Command.cs
│   │   └── [Nombre]Handler.cs
│   ├── Queries/[Nombre]/
│   │   ├── [Nombre]Query.cs
│   │   └── [Nombre]Handler.cs
│   └── Validators/
│       └── [Nombre]Validator.cs
└── tests/
    └── CobranzaCloud.UnitTests/
        └── [Nombre]/
            └── [Nombre]HandlerTests.cs
```

### Nuevo Componente Frontend

```
src/frontend/
├── components/
│   └── features/
│       └── [nombre]/
│           ├── [nombre].tsx            # Componente
│           ├── [nombre]-skeleton.tsx   # Skeleton
│           └── [nombre].test.tsx       # Test
├── lib/
│   └── hooks/
│       └── use-[nombre].ts             # Hook si necesario
└── __tests__/
    └── components/
        └── [nombre].test.tsx
```

---

## 7. Checklist de PR

Antes de crear PR, verifica:

### Código
- [ ] Lint sin errores (`dotnet format` / `pnpm lint`)
- [ ] Types sin errores (`pnpm type-check`)
- [ ] Tests pasan (`dotnet test` / `pnpm test`)
- [ ] Cobertura no disminuye

### Seguridad
- [ ] No hay secrets hardcodeados
- [ ] Inputs validados
- [ ] Multi-tenant verificado (OrganizationId)
- [ ] Sin `console.log` / logs con PII

### Documentación
- [ ] Tipos actualizados en `api-types.ts` si hay cambios de API
- [ ] Swagger actualizado (automático con Minimal API)
- [ ] README actualizado si cambia setup

### UX (Frontend)
- [ ] Loading states con Skeleton
- [ ] Error states manejados
- [ ] Accesibilidad verificada
- [ ] Mobile responsive

---

## 8. Releases

### Versionado

Usamos **Semantic Versioning**:
- `MAJOR.MINOR.PATCH` (ej: 1.2.3)
- `MAJOR`: Breaking changes
- `MINOR`: Nuevas features backward-compatible
- `PATCH`: Bug fixes

### Changelog

Actualizar `CHANGELOG.md` con cada release siguiendo formato Keep a Changelog.

---

## 9. Recursos

### Documentación del Proyecto
- [CLAUDE.md](CLAUDE.md) - Contexto general
- [docs/](docs/) - Documentación completa

### Stack
- [Next.js Docs](https://nextjs.org/docs)
- [.NET Minimal API](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
- [shadcn/ui](https://ui.shadcn.com/)
- [TanStack Query](https://tanstack.com/query)

### Herramientas
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Keep a Changelog](https://keepachangelog.com/)

---

## 10. Soporte

- **Dudas de código:** Abrir Discussion en GitHub
- **Bugs:** Crear Issue con template de bug
- **Features:** Crear Issue con template de feature request

---

*Guía de contribución - Actualizar según evolucione el proyecto*
