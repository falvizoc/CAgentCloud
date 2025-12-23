# Changelog

Todos los cambios notables de este proyecto serán documentados en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es/1.1.0/),
y este proyecto adhiere a [Semantic Versioning](https://semver.org/lang/es/).

---

## [Unreleased]

### Added
- Estructura inicial del proyecto
- Documentación completa en `docs/`
- CLAUDE.md como memoria del proyecto
- Plan maestro con fases y milestones
- Integración OWASP Top 10:2025
- Principios FRICTIONLESS transversales
- Documento de features para marketing

### Security
- Checklist OWASP por fase de desarrollo
- Configuración WAF documentada
- Headers de seguridad definidos

---

## [0.1.0] - 2025-12-23

### Added
- Creación del repositorio
- Documentación inicial:
  - `CLAUDE.md` - Memoria general del proyecto
  - `docs/00-PLAN-MAESTRO.md` - Plan de trabajo
  - `docs/01-ARQUITECTURA.md` - Decisiones de arquitectura
  - `docs/02-STACK-TECNICO.md` - Stack tecnológico
  - `docs/03-SEGURIDAD.md` - Políticas de seguridad (OWASP 2025)
  - `docs/04-UX-GUIDELINES.md` - Guías de UX
  - `docs/05-API-SPEC.md` - Especificación de API
  - `docs/06-DEPLOYMENT.md` - Guía de despliegue
  - `docs/07-SYNC-PROTOCOL.md` - Protocolo de sincronización
  - `docs/08-FRICTIONLESS-MANIFEST.md` - Principios FRICTIONLESS
  - `docs/09-FEATURES-MARKETING.md` - Features para landing

### Decisions
- Stack: .NET 8 + Next.js 14 + PostgreSQL + Redis
- Auth: Clerk (FRICTIONLESS)
- Hosting: Azure Container Apps
- CI/CD: GitHub Actions

---

## Versiones Futuras Planificadas

### [0.2.0] - M0: Foundation
- Docker Compose funcional
- Estructura de código base
- CI/CD básico

### [0.3.0] - M1: Auth
- Autenticación con Clerk
- OAuth Google/Microsoft
- Registro de organizaciones

### [0.4.0] - M2: Sync
- Registro de conectores
- Sincronización de cartera
- Heartbeat de conectores

### [0.5.0] - M3: Dashboard
- UI con Next.js
- Dashboard de indicadores
- Vista de cartera

### [1.0.0] - MVP Complete
- Envío de correos de cobranza
- Motor de reglas configurable
- IA para personalización de emails

### [2.0.0] - Post-MVP
- Portal de clientes
- Predicción de pagos con IA
- Integraciones WhatsApp/SMS

---

## Tipos de Cambios

- `Added` - Nuevas funcionalidades
- `Changed` - Cambios en funcionalidades existentes
- `Deprecated` - Funcionalidades que serán removidas
- `Removed` - Funcionalidades removidas
- `Fixed` - Corrección de bugs
- `Security` - Correcciones de vulnerabilidades
- `Decisions` - Decisiones arquitectónicas importantes

---

*Mantener actualizado con cada release*
