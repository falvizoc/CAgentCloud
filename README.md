# CAgentCloud - Plataforma de Gestión de Cobranza

> Sistema inteligente de cobranza con sincronización ERP y automatización por IA

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Next.js](https://img.shields.io/badge/Next.js-14+-000000?logo=nextdotjs)](https://nextjs.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-Proprietary-red)]()

## Descripción

**CAgentCloud** es una plataforma SaaS multi-tenant que permite a las empresas gestionar su cartera de cobranza de manera inteligente. El sistema se conecta con ERPs locales (ASPEL SAE, CONTPAQi) mediante un conector ligero y ofrece:

- **Dashboard ejecutivo** con indicadores de cartera en tiempo real
- **Automatización de cobranza** mediante correos personalizados con IA
- **Sincronización transparente** con sistemas ERP locales
- **Motor de reglas configurable** para seguimiento automático

## Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                    CLOUD (Azure)                             │
│  ┌───────────┐  ┌───────────┐  ┌───────────┐  ┌──────────┐ │
│  │  Next.js  │  │  .NET 8   │  │ PostgreSQL│  │  Redis   │ │
│  │  Frontend │  │  Backend  │  │    DB     │  │  Cache   │ │
│  └─────┬─────┘  └─────┬─────┘  └───────────┘  └──────────┘ │
│        └──────┬───────┘                                     │
└───────────────┼─────────────────────────────────────────────┘
                │ HTTPS
┌───────────────┼─────────────────────────────────────────────┐
│  CLIENTE      │                                             │
│  ┌────────────▼────────────┐    ┌────────────────────────┐ │
│  │  Conector Windows       │───▶│  ERP Local             │ │
│  │  (Sync cada 15 min)     │    │  (ASPEL/CONTPAQi)      │ │
│  └─────────────────────────┘    └────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## Stack Tecnológico

| Capa | Tecnología |
|------|------------|
| **Frontend** | Next.js 14+, React 18, Tailwind CSS, shadcn/ui |
| **Backend** | .NET 8 Minimal API, Entity Framework Core |
| **Base de Datos** | PostgreSQL 16 |
| **Cache/Queue** | Redis |
| **Infraestructura** | Docker, Azure Container Apps |
| **CI/CD** | GitHub Actions |

## Estructura del Proyecto

```
CAgentCloud/
├── docs/                    # Documentación técnica
│   ├── 00-PLAN-MAESTRO.md
│   ├── 01-ARQUITECTURA.md
│   ├── 02-STACK-TECNICO.md
│   ├── 03-SEGURIDAD.md
│   ├── 04-UX-GUIDELINES.md
│   ├── 05-API-SPEC.md
│   ├── 06-DEPLOYMENT.md
│   ├── 07-SYNC-PROTOCOL.md
│   ├── 08-FRICTIONLESS-MANIFEST.md
│   └── contracts/
├── src/
│   ├── backend/             # .NET 8 API
│   └── frontend/            # Next.js App
├── docker/                  # Configuración Docker
└── CLAUDE.md               # Contexto para desarrollo con IA
```

## Inicio Rápido

### Prerrequisitos

- Docker Desktop
- Node.js 20+
- .NET 8 SDK
- Git

### Instalación

```bash
# Clonar repositorio
git clone git@github.com:falvizoc/CAgentCloud.git
cd CAgentCloud

# Iniciar servicios con Docker
docker-compose up -d

# Backend estará en: http://localhost:5000
# Frontend estará en: http://localhost:3000
```

## Filosofía FRICTIONLESS

El principio central del proyecto es **minimizar la fricción** para el usuario:

| Área | Enfoque |
|------|---------|
| **Registro** | 1 clic con Google/Microsoft |
| **Conexión ERP** | Código de 6 dígitos, sin firewall |
| **Sincronización** | Automática cada 15 min |
| **Cobranza** | Plantillas pre-configuradas + IA |

## Características Principales

### MVP (v1.0)
- [ ] Autenticación OAuth (Google, Microsoft)
- [ ] Dashboard de cartera
- [ ] Sincronización con ASPEL SAE
- [ ] Envío de correos de cobranza
- [ ] Motor de reglas básico

### Futuro (v2.0+)
- [ ] Soporte CONTPAQi
- [ ] WhatsApp Business API
- [ ] Predicción de pagos con ML
- [ ] App móvil

## Documentación

| Documento | Descripción |
|-----------|-------------|
| [CLAUDE.md](./CLAUDE.md) | Contexto completo del proyecto |
| [Arquitectura](./docs/01-ARQUITECTURA.md) | Decisiones de arquitectura |
| [Seguridad](./docs/03-SEGURIDAD.md) | Políticas OWASP 2025 |
| [API Spec](./docs/05-API-SPEC.md) | Especificación de endpoints |
| [Deployment](./docs/06-DEPLOYMENT.md) | Guía de despliegue |

## Contribuir

Ver [CONTRIBUTING.md](./CONTRIBUTING.md) para guías de desarrollo y convenciones de código.

## Licencia

Proyecto propietario. Todos los derechos reservados.

---

**Estado:** Pre-MVP | **Versión:** 0.1.0 | **Inicio:** Diciembre 2025
