# CLAUDE.md - Agente de Cobranza Cloud Platform

> **Proyecto:** Plataforma Cloud de Gestión de Cobranza
> **Inicio:** 2025-12-22
> **Versión:** 0.1.0 (Pre-MVP)
> **Tipo:** Fullstack SaaS Multi-tenant

---

## 1. Visión del Proyecto

Sistema de agente de cobranza inteligente que permite a las empresas:
- **Visualizar** indicadores de cartera en un dashboard ejecutivo
- **Automatizar** seguimiento de cobranza mediante correos inteligentes (IA)
- **Sincronizar** datos desde sistemas ERP locales (ASPEL SAE, CONTPAQi futuro)
- **Escalar** mediante despliegue containerizado independiente por cliente
- **Personalizar** comportamiento desde panel de configuración

---

## 2. Filosofía FRICTIONLESS (Principio Central)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         🎯 FRICTIONLESS FIRST                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  "El usuario NO debe pensar en configuración técnica.                       │
│   El sistema debe FUNCIONAR con el mínimo de pasos."                        │
│                                                                              │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                              │
│  REGISTRO                          CONEXIÓN                                  │
│  ─────────                         ────────                                  │
│  • 1 clic con Google/Microsoft     • Código de 6 dígitos para vincular     │
│  • Sin verificación de email       • Auto-detección de empresas            │
│  • Org creada automáticamente      • Sin configurar firewall/puertos       │
│                                                                              │
│  SINCRONIZACIÓN                    COBRANZA                                  │
│  ──────────────                    ────────                                  │
│  • Automática cada 15 min          • Plantillas pre-configuradas           │
│  • Sin intervención del usuario    • IA redacta primer borrador            │
│  • Delta sync (solo cambios)       • 1 clic para enviar                    │
│                                                                              │
│  CONFIGURACIÓN                     ONBOARDING                                │
│  ─────────────                     ──────────                                │
│  • Defaults inteligentes           • Wizard de 3 pasos máximo              │
│  • Panel simple, no abrumador      • Valor visible en < 5 minutos          │
│  • Cambios aplicados al instante   • Skip permitido en todo                │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Principios FRICTIONLESS en Código

| Área | Anti-Pattern ❌ | FRICTIONLESS ✅ |
|------|----------------|-----------------|
| Registro | Formulario de 10 campos | Login con Google, 1 clic |
| Conector | Manual: IP, puerto, API key | Código de 6 dígitos, auto-config |
| Email | Configurar SMTP manualmente | Usar credenciales OAuth existentes |
| Plantillas | Empezar desde cero | Templates pre-hechos + IA |
| Dashboard | Vacío hasta configurar | Datos de demo mientras conecta |

---

## 3. Filosofía de Desarrollo

- **FRICTIONLESS First**: Mínima configuración, máximo valor
- **MVP First**: Entregar valor rápidamente, iterar basado en feedback
- **Security by Design**: La seguridad no es opcional en cobranza
- **Replicabilidad**: Docker-first para escalabilidad horizontal
- **Test-Driven**: Pruebas unitarias y E2E desde el inicio
- **Agentes Especializados**: Claude agents por cada capa del stack

---

## 4. Arquitectura General

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        CLOUD PLATFORM (Azure)                            │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │
│  │  Frontend   │  │   Backend   │  │  Database   │  │    Queue    │    │
│  │   Next.js   │  │   .NET 8    │  │ PostgreSQL  │  │   Redis     │    │
│  │   React     │  │   Minimal   │  │             │  │             │    │
│  └──────┬──────┘  └──────┬──────┘  └─────────────┘  └─────────────┘    │
│         │                │                                              │
│         └────────┬───────┘                                              │
│                  │ API Gateway / Load Balancer                          │
└──────────────────┼──────────────────────────────────────────────────────┘
                   │ HTTPS (443)
                   │
┌──────────────────┼──────────────────────────────────────────────────────┐
│  CLIENTE LOCAL   │ (Outbound Sync - Conector inicia conexión)          │
│                  ▼                                                      │
│  ┌────────────────────────────────┐                                    │
│  │   Conector ASPEL SAE v2.0     │                                     │
│  │   (Windows Service)            │                                     │
│  │   Sync → Cloud cada 15 min    │                                     │
│  └────────────────────────────────┘                                    │
│                  │                                                      │
│                  ▼                                                      │
│  ┌────────────────────────────────┐                                    │
│  │   ERP Local (ASPEL/CONTPAQi)  │                                     │
│  └────────────────────────────────┘                                    │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Stack Tecnológico Definido

### Backend
| Componente | Tecnología | Justificación |
|------------|------------|---------------|
| Framework | .NET 8 Minimal API | Consistencia con conector, performance, Azure-native |
| ORM | Entity Framework Core | Productividad, migraciones |
| Auth | ASP.NET Identity + OAuth | Estándar, soporta providers externos |
| Cache | Redis | Sesiones, rate limiting |
| Queue | Azure Service Bus / Redis | Emails asíncronos |

### Frontend
| Componente | Tecnología | Justificación |
|------------|------------|---------------|
| Framework | Next.js 14+ (App Router) | SSR, RSC, mejor SEO, excelente DX |
| UI Library | React 18 | Ecosistema maduro |
| Styling | Tailwind CSS + shadcn/ui | Productividad, consistencia |
| State | TanStack Query + Zustand | Server state + client state |
| Forms | React Hook Form + Zod | Validación type-safe |

### Infraestructura
| Componente | Tecnología | Justificación |
|------------|------------|---------------|
| Containers | Docker + Docker Compose | Replicabilidad, escalabilidad |
| Orchestration | Azure Container Apps | Serverless containers, auto-scale |
| Database | PostgreSQL 16 | Costo, JSON support, extensiones |
| Hosting | Azure | Recursos disponibles, .NET native |
| CI/CD | GitHub Actions | Integración natural |

---

## 6. Modelo de Autenticación (MVP)

### Decisión: OAuth Providers + Email/Password

Para el MVP, implementaremos:

```
┌─────────────────────────────────────────────────┐
│           OPCIONES DE AUTENTICACIÓN             │
├─────────────────────────────────────────────────┤
│  ✅ Google OAuth 2.0      (Prioridad 1)        │
│  ✅ Microsoft 365         (Prioridad 2)        │
│  ⏳ Apple ID              (Post-MVP)           │
│  ✅ Email + Password      (Fallback siempre)   │
└─────────────────────────────────────────────────┘
```

### Flujo de Registro
1. Usuario solicita trial en landing page
2. Elige método de autenticación
3. Se crea cuenta en plataforma
4. Dashboard vacío hasta vincular conector

### Seguridad Crítica
- [ ] MFA opcional (recomendado para admins)
- [ ] Rate limiting en login
- [ ] Tokens JWT con refresh tokens
- [ ] Audit log de accesos
- [ ] HTTPS obligatorio

---

## 7. Funcionalidades MVP

### Sprint 1: Foundation
- [ ] Proyecto base .NET 8 + Next.js
- [ ] Docker Compose para desarrollo local
- [ ] Base de datos PostgreSQL
- [ ] Autenticación básica (email/password)
- [ ] CI/CD básico

### Sprint 2: Core Auth
- [ ] OAuth Google
- [ ] OAuth Microsoft 365
- [ ] Gestión de sesiones
- [ ] Registro de organizaciones

### Sprint 3: Sync Infrastructure
- [ ] Endpoints de registro de conector
- [ ] Endpoints de sincronización
- [ ] JWT para conectores
- [ ] Heartbeat y monitoreo

### Sprint 4: Dashboard MVP
- [ ] Panel de indicadores de cartera
- [ ] Resumen de antigüedad
- [ ] Lista de clientes con saldo
- [ ] Detalle de cliente

### Sprint 5: Cobranza Básica
- [ ] Configuración de plantillas de email
- [ ] Envío manual de recordatorios
- [ ] Historial de comunicaciones

---

## 8. Estructura de Repositorio

```
CAgentCloud/
├── CLAUDE.md                    # Este archivo - contexto general
│
├── docs/                        # Documentación de desarrollo
│   ├── 00-PLAN-MAESTRO.md      # Plan de trabajo general
│   ├── 01-ARQUITECTURA.md      # Decisiones de arquitectura
│   ├── 02-STACK-TECNICO.md     # Detalles del stack
│   ├── 03-SEGURIDAD.md         # Políticas de seguridad (OWASP 2025)
│   ├── 04-UX-GUIDELINES.md     # Guías de UX
│   ├── 05-API-SPEC.md          # Especificación de API
│   ├── 06-DEPLOYMENT.md        # Guía de despliegue
│   ├── 07-SYNC-PROTOCOL.md     # Protocolo de sincronización
│   ├── 08-FRICTIONLESS-MANIFEST.md  # 🎯 NORMATIVO: Principios FRICTIONLESS
│   ├── 09-FEATURES-MARKETING.md     # Características para landing/marketing
│   ├── 10-FEATURES-ROADMAP.md       # Roadmap de features IA por versión
│   ├── 11-GAPS-ANALISIS.md          # ⚠️ Gaps identificados para desarrollo
│   ├── 12-CLAUDE-PROMPTS-GUIDE.md   # Guía de prompts efectivos
│   ├── 13-ESTRATEGIA-AGENTES.md     # Cuándo y cómo usar agentes
│   └── contracts/
│       └── api-types.ts             # Tipos compartidos frontend/backend
│
├── src/
│   ├── backend/                 # .NET 8 API
│   │   ├── src/
│   │   │   ├── CobranzaCloud.Api/
│   │   │   ├── CobranzaCloud.Core/
│   │   │   └── CobranzaCloud.Data/
│   │   └── tests/
│   │       └── CobranzaCloud.Tests/
│   │
│   └── frontend/                # Next.js App
│       ├── src/
│       │   ├── app/
│       │   ├── components/
│       │   ├── lib/
│       │   ├── hooks/
│       │   └── styles/
│       ├── public/
│       └── messages/            # i18n (next-intl)
│
├── docker/
│   ├── docker-compose.yml
│   ├── docker-compose.prod.yml
│   └── dockerfiles/
│
├── scripts/                     # Scripts de utilidad
│
└── .github/
    └── workflows/
```

---

## 9. Equipos de Agentes Claude

> **IMPORTANTE:** Todo agente debe aplicar el principio FRICTIONLESS en su área.
> Ver [docs/08-FRICTIONLESS-MANIFEST.md](./docs/08-FRICTIONLESS-MANIFEST.md)

### Modelo Híbrido: Stack + Sprint

**Agentes Permanentes (Por Stack):**
| Agente | Responsabilidad | Contexto Principal | Foco FRICTIONLESS |
|--------|-----------------|-------------------|-------------------|
| **Claude-Backend** | .NET API, seguridad, DB, Auth | `/src/backend/`, docs/01-05 | Errores accionables, auto-refresh tokens |
| **Claude-Frontend** | React, Next.js, UX, Accesibilidad | `/src/frontend/`, docs/04 | 1-clic OAuth, ⌘K, skeleton loading |
| **Claude-DevOps** | Docker, Azure, CI/CD, Monitoreo | `/docker/`, docs/06 | `docker-compose up` sin config |
| **Claude-Sync** | Protocolo conector-cloud, ASPEL | docs/07, API Sync | Código 6 dígitos, sin firewall |

**Agentes Temporales (Por Sprint):**
| Agente | Responsabilidad | Cuándo | Foco FRICTIONLESS |
|--------|-----------------|--------|-------------------|
| **Sprint-Lead** | Coordina fase, valida milestones | Por cada fase | Valida checklist FRICTIONLESS |
| **Sprint-QA** | Tests E2E, validación UX, bugs | Por cada fase | Mide Time-to-First-Value |

### Responsabilidades por Milestone

| Milestone | Principales | Apoyo |
|-----------|------------|-------|
| M0: Foundation | Claude-DevOps | Sprint-Lead |
| M1: Auth | Claude-Backend | Claude-DevOps, Sprint-QA |
| M2: Sync | Claude-Backend, Claude-Sync | Sprint-QA |
| M3: Dashboard | Claude-Frontend | Claude-Backend, Sprint-QA |
| M4: MVP | Claude-Backend, Claude-Frontend | Claude-DevOps, Sprint-Lead |

### Contexto por Agente
```
src/backend/CLAUDE-BACKEND.md     # Contexto específico backend
src/frontend/CLAUDE-FRONTEND.md   # Contexto específico frontend
docker/CLAUDE-DEVOPS.md           # Contexto infraestructura
```

### Comunicación entre Agentes
- Cada agente mantiene su CLAUDE.md específico en su área
- Cambios cross-cutting se documentan en docs/
- Conflictos se resuelven en CLAUDE.md raíz (este archivo)

---

## 10. Prioridades de Seguridad

```
🔴 CRÍTICO (Día 1)
├── HTTPS obligatorio
├── Sanitización de inputs
├── Prepared statements (EF Core)
└── Secrets en Azure Key Vault

🟠 ALTO (Sprint 1-2)
├── Rate limiting
├── CORS estricto
├── Validación JWT
└── Audit logging

🟡 MEDIO (Sprint 3+)
├── MFA
├── Penetration testing
└── Compliance check
```

---

## 11. Convenciones de Código

### Backend (.NET)
- Minimal API con vertical slices
- CQRS ligero (MediatR)
- Result pattern para errores
- Logs estructurados (Serilog)

### Frontend (Next.js)
- App Router + Server Components
- Colocación: componentes junto a páginas
- CSS: Tailwind + CSS Modules cuando necesario
- TypeScript estricto

### Git
- Conventional Commits: `feat:`, `fix:`, `docs:`, etc.
- Branches: `feature/`, `fix/`, `docs/`
- PRs requeridos para `main`

---

## 12. Sistema de Automatización de Cobranza con IA

### Propósito Central
La IA y el motor de reglas trabajan juntos para el **seguimiento continuo automatizado** de cobranza, enviando comunicaciones personalizadas basadas en reglas configurables.

### Tipos de Comunicación Automatizada

| Tipo | Trigger | Tono | Acción |
|------|---------|------|--------|
| **Recordatorio** | Día 0 (vence hoy) | Amigable | Email recordatorio |
| **Primer aviso** | +7 días | Formal | Email + IA personaliza |
| **Segundo aviso** | +15 días | Firme | Email urgente |
| **Aviso suspensión** | +30 días | Serio | Advertencia formal |
| **Suspensión** | +45 días | Legal | Notificación + cambiar estado |
| **Evaluación crédito** | +60 días | Interno | Reducir límite, alertar |

### Motor de Reglas (Microsoft Rules Engine)

Las reglas son **100% configurables** por el usuario desde el panel:
- Definir días de vencimiento para cada acción
- Seleccionar plantilla y tono
- Activar/desactivar IA para personalización
- Habilitar acciones automáticas (cambiar estado, notificar)

```json
{
  "RuleName": "AvisoSuspension30Dias",
  "Expression": "DiasVencido >= 30 AND DiasVencido < 45",
  "Actions": {
    "Tipo": "email",
    "Plantilla": "aviso_suspension",
    "Tono": "serio",
    "UsarIA": true
  }
}
```

### IA para Personalización Multi-idioma

| Función | Descripción |
|---------|-------------|
| **Redacción** | Genera email según contexto del cliente |
| **Tono** | Adapta: amigable → formal → firme → serio → legal |
| **Idioma** | Auto-traduce a ES-MX, EN-US (más en v2.0) |
| **Contexto** | Considera historial de pagos y comunicaciones |

### Configuración
```json
{
  "AI": {
    "Provider": "openai",
    "Model": "gpt-4o-mini",
    "MaxTokens": 500
  },
  "Automation": {
    "Idiomas": ["es-MX", "en-US"],
    "Tonos": ["amigable", "formal", "firme", "serio", "legal"],
    "ReglasPredeterminadas": 5
  }
}
```

### Documentación Completa
→ **[docs/08-FRICTIONLESS-MANIFEST.md](./docs/08-FRICTIONLESS-MANIFEST.md)** - Arquitectura completa del motor de reglas, prompts de IA, UI de configuración

---

## 13. Sistema de Envío de Correos

### Opciones de Configuración (FRICTIONLESS)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    MÉTODOS DE ENVÍO DE CORREO                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  OPCIÓN 1: OAuth del Usuario (Recomendado - FRICTIONLESS)                   │
│  ═══════════════════════════════════════════════════════                    │
│  • Gmail: Usa las mismas credenciales de login                              │
│  • Microsoft 365: Usa las mismas credenciales de login                      │
│  • Correo sale desde la dirección del usuario                               │
│  • Sin configurar nada adicional                                            │
│                                                                              │
│  Flujo:                                                                      │
│  Login Google ──▶ Permisos Gmail API ──▶ Envío directo                      │
│                                                                              │
│  ─────────────────────────────────────────────────────────────────────────  │
│                                                                              │
│  OPCIÓN 2: SMTP Personalizado                                                │
│  ════════════════════════════                                               │
│  • Para empresas con servidor propio                                        │
│  • Configuración manual en panel                                            │
│  • Host, puerto, usuario, contraseña                                        │
│                                                                              │
│  ─────────────────────────────────────────────────────────────────────────  │
│                                                                              │
│  OPCIÓN 3: Servicio Transaccional (Fallback)                                │
│  ═══════════════════════════════════════════                                │
│  • SendGrid, Resend, Amazon SES                                             │
│  • Para envíos masivos o sin OAuth                                          │
│  • Requiere dominio verificado                                              │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Implementación Técnica

**Gmail API (OAuth):**
```csharp
// Scopes requeridos al login
var scopes = new[] {
    "openid",
    "email",
    "profile",
    "https://www.googleapis.com/auth/gmail.send"  // Enviar correos
};
```

**Microsoft Graph (OAuth):**
```csharp
var scopes = new[] {
    "openid",
    "email",
    "profile",
    "Mail.Send"  // Enviar correos desde Outlook
};
```

### Prioridad de Envío
1. Si usuario logueó con Google → Usar Gmail API
2. Si usuario logueó con Microsoft → Usar Graph API
3. Si configuró SMTP → Usar SMTP
4. Fallback → Servicio transaccional de la plataforma

---

## 14. Panel de Configuración de Usuario

### Estructura del Panel

```
/settings
├── /profile          # Datos personales
├── /organization     # Datos de la empresa
├── /email            # Configuración de correo
├── /templates        # Plantillas de cobranza
├── /notifications    # Alertas y notificaciones
├── /connectors       # Gestión de conectores
└── /billing          # Facturación (futuro)
```

### Configuraciones por Sección

**Perfil (/settings/profile)**
- Nombre, avatar
- Cambiar contraseña (si no es OAuth)
- Zona horaria
- Idioma

**Organización (/settings/organization)**
- Nombre de empresa, RFC
- Logo (para correos)
- Dirección fiscal
- Usuarios y roles

**Email (/settings/email)**
```
┌─────────────────────────────────────────────────────────────────┐
│  CONFIGURACIÓN DE CORREO                                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Método de envío:                                                │
│  ○ Usar mi cuenta de Google (recomendado)                       │
│  ○ Usar mi cuenta de Microsoft 365                              │
│  ○ Configurar SMTP manualmente                                  │
│                                                                  │
│  ─────────────────────────────────────────────────────────────  │
│                                                                  │
│  Firma de correo:                                                │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ Atentamente,                                             │   │
│  │ {nombre_usuario}                                         │   │
│  │ {nombre_empresa}                                         │   │
│  │ Tel: {telefono}                                          │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                  │
│  [Guardar cambios]                                              │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

**Plantillas (/settings/templates)**
- CRUD de plantillas de correo
- Variables disponibles: `{cliente}`, `{monto}`, `{dias}`, `{facturas}`
- Preview antes de guardar
- Plantillas por defecto pre-cargadas

**Notificaciones (/settings/notifications)**
- Alertas de facturas próximas a vencer
- Resumen diario/semanal por email
- Notificaciones push (futuro)

---

## 15. Estrategia de Testing

### Pirámide de Tests

```
                    ┌─────────┐
                    │   E2E   │  ← Playwright/Cypress
                   ─┴─────────┴─     (pocos, críticos)
                  ┌─────────────┐
                  │ Integration │  ← TestContainers
                 ─┴─────────────┴─   (API + DB reales)
                ┌─────────────────┐
                │      Unit       │  ← xUnit/Jest
               ─┴─────────────────┴─  (muchos, rápidos)
```

### Backend (.NET)

**Estructura de Tests:**
```
CobranzaCloud.Tests/
├── Unit/
│   ├── Services/
│   │   ├── CarteraServiceTests.cs
│   │   └── EmailServiceTests.cs
│   └── Validators/
│       └── LoginRequestValidatorTests.cs
├── Integration/
│   ├── Endpoints/
│   │   ├── AuthEndpointsTests.cs
│   │   └── SyncEndpointsTests.cs
│   └── Fixtures/
│       └── DatabaseFixture.cs
└── E2E/
    └── Flows/
        └── RegistroConectorFlowTests.cs
```

**Ejemplo Unit Test:**
```csharp
public class CarteraServiceTests
{
    [Fact]
    public void CalcularAntiguedad_FacturaVencida30Dias_RetornaRango1a30()
    {
        // Arrange
        var factura = new Factura { Vencimiento = DateTime.Today.AddDays(-30) };

        // Act
        var rango = CarteraService.CalcularRangoAntiguedad(factura);

        // Assert
        Assert.Equal("1-30", rango);
    }
}
```

**Integration con TestContainers:**
```csharp
public class AuthEndpointsTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgres;
    private HttpClient _client;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder().Build();
        await _postgres.StartAsync();
        // Setup WebApplicationFactory con connection string real
    }
}
```

### Frontend (Next.js)

**Estructura:**
```
src/frontend/
├── __tests__/
│   ├── components/
│   │   └── LoginForm.test.tsx
│   └── hooks/
│       └── useCartera.test.ts
├── e2e/
│   └── login.spec.ts
└── vitest.config.ts
```

**Ejemplo Component Test:**
```typescript
import { render, screen } from '@testing-library/react';
import { LoginForm } from '@/components/LoginForm';

describe('LoginForm', () => {
  it('muestra botón de Google OAuth', () => {
    render(<LoginForm />);
    expect(screen.getByText(/continuar con google/i)).toBeInTheDocument();
  });
});
```

### Cobertura Mínima

| Área | Cobertura Objetivo |
|------|-------------------|
| Core Services | > 80% |
| API Endpoints | > 70% |
| UI Components | > 60% |
| E2E Flows | 100% de flujos críticos |

---

## 16. Migraciones y Seeders

### Entity Framework Core Migrations

**Comandos:**
```bash
# Crear migración
dotnet ef migrations add NombreMigracion -p CobranzaCloud.Data -s CobranzaCloud.Api

# Aplicar migraciones
dotnet ef database update -s CobranzaCloud.Api

# Revertir última migración
dotnet ef migrations remove -p CobranzaCloud.Data -s CobranzaCloud.Api

# Script SQL (para producción)
dotnet ef migrations script -o migrations.sql -s CobranzaCloud.Api
```

### Seeders (Datos Iniciales)

**¿Qué se siembra?**

| Entidad | Datos | Cuándo |
|---------|-------|--------|
| Roles | admin, manager, collector, viewer | Siempre |
| Plantillas Default | 3-4 plantillas de cobranza | Siempre |
| Config Default | Zona horaria, moneda MXN | Siempre |
| Demo Data | Org demo, clientes fake | Solo desarrollo |

**Implementación:**
```csharp
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Role { Name = "admin", Permissions = ["*"] },
                new Role { Name = "manager", Permissions = ["cartera:*", "clientes:*"] },
                new Role { Name = "collector", Permissions = ["cartera:read", "clientes:read", "email:send"] },
                new Role { Name = "viewer", Permissions = ["cartera:read", "clientes:read"] }
            );
        }

        if (!await db.EmailTemplates.AnyAsync())
        {
            db.EmailTemplates.AddRange(DefaultTemplates.GetAll());
        }

        await db.SaveChangesAsync();
    }
}
```

**Ejecución en Startup:**
```csharp
// Program.cs
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}
```

---

## 17. Recursos y Referencias

### Documentación
- [docs/](./docs/) - Documentación completa del proyecto
- [08-FRICTIONLESS-MANIFEST.md](./docs/08-FRICTIONLESS-MANIFEST.md) - Principios normativos
- [03-SEGURIDAD.md](./docs/03-SEGURIDAD.md) - OWASP Top 10:2025

### Infraestructura Disponible
- **Azure**: Recursos para producción y staging
- **Dominio**: Por definir
- **Conector de pruebas**: `bitmovil.ddns.net:5000` (API v1.1.17)

---

## 18. Protocolo de Sesiones

> **IMPORTANTE**: Al iniciar o cerrar sesión, leer `.claude/PROTOCOL.md`

| Comando Usuario | Acción Claude |
|-----------------|---------------|
| "abre sesión" / "inicio" / "buenos días" | Ejecutar protocolo de apertura |
| "cierra sesión" / "termina" / "hasta mañana" | Ejecutar protocolo de cierre |

El protocolo garantiza:
- Sincronización con git antes de trabajar
- Documentación del trabajo realizado
- Commits y push antes de cerrar
- Continuidad entre sesiones

---

## 19. Próximos Pasos Inmediatos

1. ✅ Revisar memoria de traslado
2. ✅ Crear CLAUDE.md (este documento)
3. ✅ Crear estructura docs/
4. ✅ Definir milestones y agentes
5. ✅ Documentar FRICTIONLESS, IA, Email, Testing
6. ✅ Configurar proyecto base (Docker Compose)
7. ✅ Crear estructura de código
8. ✅ Integración ASPEL connector (M3)

---

*Documento creado: 2025-12-22*
*Última actualización: 2025-12-27*
*Próxima revisión: Al completar Milestone M4*
