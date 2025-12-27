# Plan Maestro - Agente de Cobranza Cloud

> **Versión:** 1.3
> **Fecha:** 2025-12-27
> **Estado:** M0-M3 Completados, M4 Pendiente, M5 Planificado

---

## 1. Resumen Ejecutivo

Este documento define el plan maestro para el desarrollo de la Plataforma Cloud de Gestión de Cobranza, un sistema SaaS que permite a las empresas automatizar y gestionar su proceso de cobranza.

> **📋 Documento Normativo FRICTIONLESS:** [08-FRICTIONLESS-MANIFEST.md](./08-FRICTIONLESS-MANIFEST.md)
>
> Toda decisión en este plan debe alinearse con el principio FRICTIONLESS:
> *"El usuario obtiene valor en < 5 minutos, sin configuración manual."*

> **🔒 Marco de Seguridad:** [03-SEGURIDAD.md](./03-SEGURIDAD.md) | [OWASP Top 10:2025](https://owasp.org/Top10/2025/)
>
> Cada fase incluye checklist de seguridad OWASP. Ver sección "Seguridad OWASP" en cada fase.

### Objetivo Principal
Crear un MVP funcional FRICTIONLESS que permita:
1. Registrar usuarios y organizaciones
2. Visualizar indicadores de cartera
3. Sincronizar datos desde ERP local (ASPEL SAE)
4. Enviar correos de seguimiento configurables

---

## 2. Fases de Desarrollo

```
┌────────────────────────────────────────────────────────────────────────┐
│                        ROADMAP VISUAL                                   │
├────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  FASE 0         FASE 1          FASE 2          FASE 3        FASE 4  │
│  Setup          Cloud Base      Sync            Dashboard     Cobranza│
│  ════════       ══════════      ════════        ═════════     ════════│
│                                                                         │
│  ┌──────┐       ┌──────┐        ┌──────┐        ┌──────┐     ┌──────┐ │
│  │Docker│──────▶│ Auth │───────▶│ API  │───────▶│  UI  │────▶│Email │ │
│  │ CI/CD│       │ JWT  │        │ Sync │        │React │     │Queue │ │
│  └──────┘       └──────┘        └──────┘        └──────┘     └──────┘ │
│                                                                         │
│  ◀─────── MVP (Fases 0-4) ──────────────────────────────────────────▶ │
│                                                                         │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Milestones y Criterios de Avance

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           MILESTONES DEL MVP                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  M0: FOUNDATION READY                          Criterio de Éxito:           │
│  ──────────────────────                        ────────────────────          │
│  • Documentación completa                      docker-compose up            │
│  • Docker Compose funcional                    levanta todo el stack        │
│  • CI/CD básico configurado                    local sin errores            │
│  • Estructura de proyectos                                                  │
│                                                                              │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                              │
│  M1: AUTH COMPLETE                             Criterio de Éxito:           │
│  ─────────────────────                         ────────────────────          │
│  • Registro con email                          Usuario puede registrarse,   │
│  • Login email + Google + Microsoft            hacer login con Google,      │
│  • JWT + Refresh tokens                        y ver su perfil en /me       │
│  • Organización creada al registrar                                         │
│  • Redis para sesiones                                                      │
│                                                                              │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                              │
│  M2: SYNC OPERATIONAL                          Criterio de Éxito:           │
│  ────────────────────────                      ────────────────────          │
│  • Conector puede registrarse                  Datos del conector de        │
│  • Endpoint de sync recibe datos               prueba (bitmovil.ddns.net)   │
│  • PostgreSQL almacena cartera                 visibles en API cloud        │
│  • Heartbeat funcionando                                                    │
│                                                                              │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                              │
│  M3: DASHBOARD LIVE                            Criterio de Éxito:           │
│  ──────────────────────                        ────────────────────          │
│  • UI de login/registro                        Usuario logueado ve          │
│  • Dashboard con KPIs                          dashboard con datos          │
│  • Lista de clientes                           reales de ASPEL              │
│  • Detalle de cliente                                                       │
│                                                                              │
│  ═══════════════════════════════════════════════════════════════════════   │
│                                                                              │
│  M4: MVP COMPLETE                              Criterio de Éxito:           │
│  ────────────────────                          ────────────────────          │
│  • Plantillas de email                         Usuario puede enviar         │
│  • Envío manual de recordatorios               correo de cobranza           │
│  • Historial de comunicaciones                 a un cliente desde           │
│  • Deploy en Azure staging                     la plataforma                │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Validación de Milestones

| Milestone | Validación | Responsable |
|-----------|------------|-------------|
| M0 | `docker-compose up` sin errores, CI pasa | Claude-DevOps |
| M1 | Test E2E de registro + login OAuth | Claude-Backend + QA |
| M2 | Sync con conector de prueba exitoso | Claude-Backend + Sync |
| M3 | Demo de flujo completo en UI | Claude-Frontend + QA |
| M4 | Envío de correo real a casilla de prueba | Sprint-Lead |

---

## 4. Equipo de Agentes Claude

### 4.1 Modelo Híbrido: Stack + Sprint

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     ORGANIZACIÓN DE AGENTES                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  AGENTES PERMANENTES (Por Stack)              AGENTES TEMPORALES (Sprint)   │
│  ═══════════════════════════════              ═══════════════════════════   │
│                                                                              │
│  ┌─────────────────────────────┐              ┌─────────────────────────┐   │
│  │     Claude-Backend          │              │     Sprint-Lead         │   │
│  │     ─────────────────       │              │     ───────────         │   │
│  │     • .NET 8 API            │              │     • Coordina fase     │   │
│  │     • Entity Framework      │              │     • Revisa PRs        │   │
│  │     • PostgreSQL            │              │     • Valida milestone  │   │
│  │     • Autenticación         │              │     • Reporta avance    │   │
│  └─────────────────────────────┘              └─────────────────────────┘   │
│                                                                              │
│  ┌─────────────────────────────┐              ┌─────────────────────────┐   │
│  │     Claude-Frontend         │              │     Sprint-QA           │   │
│  │     ────────────────        │              │     ─────────           │   │
│  │     • Next.js / React       │              │     • Tests E2E         │   │
│  │     • Tailwind / shadcn     │              │     • Validación UX     │   │
│  │     • State management      │              │     • Bug hunting       │   │
│  │     • UX/Accesibilidad      │              │     • Performance       │   │
│  └─────────────────────────────┘              └─────────────────────────┘   │
│                                                                              │
│  ┌─────────────────────────────┐                                            │
│  │     Claude-DevOps           │                                            │
│  │     ─────────────           │                                            │
│  │     • Docker                │                                            │
│  │     • Azure / CI-CD         │                                            │
│  │     • Monitoreo             │                                            │
│  │     • Seguridad infra       │                                            │
│  └─────────────────────────────┘                                            │
│                                                                              │
│  ┌─────────────────────────────┐                                            │
│  │     Claude-Sync             │                                            │
│  │     ───────────             │                                            │
│  │     • Protocolo conector    │                                            │
│  │     • Endpoints sync        │                                            │
│  │     • Integración ASPEL     │                                            │
│  │     • Delta sync            │                                            │
│  └─────────────────────────────┘                                            │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Responsabilidades por Milestone

| Milestone | Agentes Principales | Agentes de Apoyo |
|-----------|--------------------|--------------------|
| M0 | Claude-DevOps | Sprint-Lead |
| M1 | Claude-Backend | Claude-DevOps, Sprint-QA |
| M2 | Claude-Backend, Claude-Sync | Sprint-QA |
| M3 | Claude-Frontend | Claude-Backend, Sprint-QA |
| M4 | Claude-Backend, Claude-Frontend | Claude-DevOps, Sprint-Lead |

### 4.3 Visión FRICTIONLESS por Agente

> **IMPORTANTE:** Cada agente debe aplicar el principio FRICTIONLESS en su área.

| Agente | Responsabilidad FRICTIONLESS |
|--------|------------------------------|
| Claude-Backend | APIs con errores accionables, defaults inteligentes, auto-refresh de tokens |
| Claude-Frontend | 1-clic OAuth, onboarding 3 pasos, ⌘K navigation, skeleton loading |
| Claude-DevOps | `docker-compose up` sin config, auto-migrate en dev, alertas proactivas |
| Claude-Sync | Código de 6 dígitos, auto-detect empresas, sin firewall config |
| Sprint-Lead | Validar checklist FRICTIONLESS de cada milestone |
| Sprint-QA | Medir Time-to-First-Value, clicks para completar tareas |

### 4.4 Contexto de Cada Agente

Cada agente mantiene su propio archivo de contexto en su área de trabajo:

```
src/
├── backend/
│   └── CLAUDE-BACKEND.md      # Contexto específico del backend
├── frontend/
│   └── CLAUDE-FRONTEND.md     # Contexto específico del frontend
└── ...

docker/
└── CLAUDE-DEVOPS.md           # Contexto de infraestructura
```

---

## 5. Infraestructura de Datos

### 5.1 Aclaración: PostgreSQL vs Redis

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    ROLES DE CADA BASE DE DATOS                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  POSTGRESQL (Base de datos principal)                                        │
│  ════════════════════════════════════                                        │
│  • Usuarios y organizaciones                                                 │
│  • Conectores registrados                                                    │
│  • DATOS SINCRONIZADOS DE CARTERA  ◄── Aquí se almacena la cartera         │
│  • Clientes y facturas                                                       │
│  • Plantillas de email                                                       │
│  • Historial de comunicaciones                                              │
│  • Audit logs                                                                │
│                                                                              │
│  REDIS (Caché y sesiones)                                                    │
│  ════════════════════════                                                    │
│  • Sesiones de usuario (JWT refresh tokens)                                 │
│  • Rate limiting por IP/usuario                                              │
│  • Cache de consultas frecuentes (ej: resumen cartera por 5 min)           │
│  • Cola de emails (pub/sub para workers)                                    │
│  • Códigos de vinculación temporales (15 min TTL)                           │
│                                                                              │
│  ─────────────────────────────────────────────────────────────────────────  │
│                                                                              │
│  NOTA: La "DB de cache de cartera" mencionada en la memoria de traslado    │
│  se refiere a POSTGRESQL, donde almacenamos los datos sincronizados.        │
│  Redis es solo para cache temporal y sesiones.                              │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.2 Cuándo se Usa Cada Una

| Operación | Base de Datos | Razón |
|-----------|---------------|-------|
| Guardar cartera sincronizada | PostgreSQL | Persistencia, queries complejas |
| Validar refresh token | Redis | Velocidad, expiración automática |
| Consultar lista de clientes | PostgreSQL | Datos relacionales |
| Cache de KPIs dashboard | Redis | Evitar recalcular cada request |
| Guardar código de vinculación | Redis | TTL de 15 minutos |
| Historial de emails enviados | PostgreSQL | Auditoría, reportes |

---

## 6. Detalle por Fase

### FASE 0: Fundación - COMPLETADO ✅

**Objetivo:** Establecer la base del proyecto

| Entregable | Estado | Descripción |
|------------|--------|-------------|
| CLAUDE.md | ✅ | Memoria general del proyecto |
| docs/ | ✅ | Estructura de documentación |
| Decisiones de stack | ✅ | .NET 9 + Next.js 14 |
| Docker Compose base | ✅ | PostgreSQL 16 + Redis 7 |
| Repositorio configurado | ✅ | Git + GitHub Actions |

**Criterios de Éxito:**
- [x] Documentación completa
- [x] `docker-compose up` levanta el entorno
- [x] CI ejecuta lint/tests básicos

**Seguridad OWASP (Fase 0):**
- [x] A02: `.env.example` sin secretos reales
- [x] A03: Dependabot configurado

---

### FASE 1: Cloud Base - COMPLETADO ✅

**Objetivo:** Backend funcional con autenticación

| Entregable | Estado | Descripción |
|------------|--------|-------------|
| API .NET 9 | ✅ | Proyecto base con estructura clean |
| PostgreSQL | ✅ | Base de datos con migraciones EF Core |
| Auth Email/Password | ✅ | Registro, login, logout |
| OAuth Google | ⏳ | Pendiente post-MVP |
| OAuth Microsoft | ⏳ | Pendiente post-MVP |
| JWT + Refresh Tokens | ✅ | Manejo de sesiones |
| Registro de Org | ✅ | Crear organización/tenant |

**Endpoints Implementados:**
```
POST   /api/auth/register    ✅
POST   /api/auth/login       ✅
POST   /api/auth/logout      ✅
POST   /api/auth/refresh     ✅
GET    /api/auth/me          ✅
POST   /api/auth/oauth/google     ⏳ Post-MVP
POST   /api/auth/oauth/microsoft  ⏳ Post-MVP
```

**Criterios de Éxito:**
- [x] Usuario puede registrarse con email
- [ ] Usuario puede login con Google (post-MVP)
- [x] Tokens JWT funcionan correctamente
- [x] Organización se crea al registrar

**Seguridad OWASP (Fase 1) - CRÍTICO:**
- [x] A01: Validación `OrganizationId` en queries (multi-tenant)
- [x] A05: EF Core con parámetros (nunca concatenar SQL)
- [x] A07: JWT 15min + refresh tokens
- [x] A04: Passwords hasheados, HTTPS obligatorio
- [x] A09: Logging configurado

---

### FASE 2: Infraestructura de Sincronización - COMPLETADO ✅

**Objetivo:** Comunicación segura Cloud ↔ Conector

| Entregable | Estado | Descripción |
|------------|--------|-------------|
| Registro de Conectores | ✅ | Vincular conector con org |
| JWT para Conectores | ✅ | Autenticación de conectores |
| Endpoints de Sync | ✅ | Recibir datos de cartera |
| Heartbeat | ✅ | Monitoreo de conectores online |
| Cache de Cartera | ✅ | Almacenar datos sincronizados |

**Endpoints Implementados:**
```
POST   /api/connectors/link-code   ✅  (genera código 6 dígitos)
POST   /api/connectors/register    ✅
POST   /api/connectors/heartbeat   ✅
POST   /api/connectors/refresh     ✅
POST   /api/sync/cartera           ✅
```

**Flujo de Registro:**
```
1. Usuario en dashboard → "Agregar Conector"
2. Sistema genera código de vinculación (6 dígitos, 15 min TTL)
3. Usuario ingresa código en conector local
4. Conector envía registro con código
5. Cloud valida y retorna JWT permanente
6. Conector guarda JWT en StateStore
```

**Criterios de Éxito:**
- [x] Conector se registra exitosamente
- [x] Datos de cartera se sincronizan
- [ ] Dashboard muestra último sync (pendiente UI)

**Seguridad OWASP (Fase 2):**
- [x] A01: JWT de conector validado en cada sync
- [x] A04: Código de vinculación con TTL 15min, un solo uso
- [x] A10: Manejo de errores de sync sin exponer detalles internos

---

### FASE 3: Dashboard - COMPLETADO ✅

**Objetivo:** UI funcional para visualizar cartera

| Entregable | Estado | Descripción |
|------------|--------|-------------|
| Next.js Project | ✅ | App Router configurado |
| Auth UI | ✅ | Login, registro (OAuth pendiente) |
| Layout Base | ✅ | Sidebar, header, responsive |
| Dashboard Home | ✅ | KPIs principales |
| Vista de Cartera | ✅ | Gráfico de antigüedad |
| Lista de Clientes | ✅ | Con saldo pendiente |
| Detalle de Cliente | ✅ | Facturas, contactos |

**Pantallas Implementadas:**
```
/login                    ✅  Login
/register                 ✅  Registro
/dashboard                ✅  Home con KPIs + Antigüedad + Clientes
/clientes/[id]            ✅  Detalle de cliente
/settings                 ⏳  Pendiente M4
/settings/connectors      ⏳  Pendiente M4
```

**Endpoints Backend M3:**
```
GET  /api/cartera/resumen     ✅  KPIs de cartera
GET  /api/cartera/antiguedad  ✅  Distribución por rangos
GET  /api/clientes            ✅  Lista paginada con filtros
GET  /api/clientes/{id}       ✅  Detalle con contactos y facturas
```

**Criterios de Éxito:**
- [x] UI responsive (Tailwind + shadcn/ui)
- [x] Datos de cartera visibles (KPIs, tabla, gráfico)
- [x] UX intuitiva con skeleton loading

**Seguridad OWASP (Fase 3):**
- [x] A05: React escaping activo
- [x] A06: Validación Zod en formularios

---

### FASE 4: Cobranza Básica

**Objetivo:** Envío de correos de seguimiento

| Entregable | Descripción |
|------------|-------------|
| Plantillas de Email | CRUD de plantillas |
| Configuración de Envío | Reglas por antigüedad |
| Cola de Emails | Procesamiento async |
| Historial | Registro de envíos |
| Envío Manual | Botón "Enviar recordatorio" |

**Funcionalidades:**
```
- Definir plantillas con variables: {cliente}, {monto}, {dias}
- Configurar reglas: "30 días vencido → plantilla_1"
- Ver historial de correos enviados por cliente
- Envío manual desde detalle de cliente
```

**Criterios de Éxito:**
- [ ] Correos se envían correctamente
- [ ] Variables se reemplazan
- [ ] Historial muestra envíos

**Seguridad OWASP (Fase 4) - PRE-PRODUCCIÓN:**
- [ ] A03: `npm audit` y `dotnet list package --vulnerable` en CI
- [ ] A02: WAF configurado en Azure Front Door
- [ ] A09: Azure Monitor alertas activas
- [ ] A10: Error handling global sin exponer stack traces
- [ ] A06: Penetration testing básico antes de producción

---

## 7. Fases Post-MVP

### M5: Multi-Empresa (v1.1) ⭐ PRIORITARIO

**Objetivo:** Suscripciones multi-empresa con selector de empresa en dashboard

```
┌─────────────────────────────────────────────────────────────────┐
│  Header                                          [Usuario ▼]   │
│  ┌──────────────────┐                                          │
│  │ 🏢 Empresa ABC ▼ │  ← Selector de empresa                   │
│  └──────────────────┘                                          │
├─────────────────────────────────────────────────────────────────┤
│   Dashboard filtrado por empresa seleccionada                   │
└─────────────────────────────────────────────────────────────────┘
```

| Entregable | Descripción | Complejidad |
|------------|-------------|-------------|
| Endpoint `/api/empresas` | Lista empresas disponibles del conector | Baja |
| Selector de empresa | Dropdown en header con persistencia | Baja |
| Context empresa activa | Store/Context para empresa seleccionada | Baja |
| Parámetro `?empresaId` | Agregar a todos los endpoints de cartera | Baja |
| Cache por empresa | Ya implementado en CacheKeys | ✅ Listo |

**Modelo de Suscripción Sugerido:**

| Plan | Empresas | Precio Base |
|------|----------|-------------|
| Starter | 1 empresa | $X/mes |
| Business | 3 empresas | +50% |
| Enterprise | Ilimitadas | Custom |

**Criterios de Éxito:**
- [ ] Usuario puede cambiar entre empresas sin logout
- [ ] Dashboard actualiza datos al cambiar empresa
- [ ] Cache funciona independiente por empresa

**Nota:** La arquitectura actual YA soporta multi-empresa (CacheKeys, CobranzaAgentClient). Solo falta UI.

---

### FASE 5: Portal de Clientes (v2.0)
- Acceso para deudores
- Ver su estado de cuenta
- Promesas de pago

### FASE 6: Integraciones (v2.0)
- WhatsApp Business API
- SMS
- Llamadas automatizadas

### FASE 7: Analytics/IA (v2.0+)
- Predicción de pago
- Scoring de clientes
- Recomendaciones de acción

---

## 8. Modelo de Despliegue

### MVP: Single-Region
```
Azure Container Apps
├── Frontend (Next.js)
├── Backend (.NET 8)
├── PostgreSQL (Azure DB)
└── Redis (Azure Cache)
```

### Escalado: Multi-Region
```
Azure Front Door (CDN + LB)
├── Region 1: Container Apps
├── Region 2: Container Apps
└── Database: Read replicas
```

### Por Cliente (Aislamiento Total)
```
Docker Compose stack por cliente
├── Namespace/Resource Group dedicado
├── Base de datos aislada
└── Secrets separados
```

---

## 9. Modelo de Autenticación

### Para Usuarios (Dashboard)

| Provider | Prioridad | Notas |
|----------|-----------|-------|
| Email/Password | MVP | Siempre disponible |
| Google OAuth | MVP | Más común en empresas |
| Microsoft 365 | MVP | Empresarial |
| Apple ID | v2.0 | Menor prioridad |

### Para Conectores (Sync)

| Método | Uso |
|--------|-----|
| JWT Bearer | Autenticación de llamadas |
| Código de vinculación | Registro inicial |
| Heartbeat token | Validación periódica |

---

## 10. Seguridad Crítica

### Día 1 (No negociable)
- ✅ HTTPS everywhere
- ✅ Passwords hasheados (bcrypt/Argon2)
- ✅ SQL Injection prevention (EF Core)
- ✅ XSS prevention (React escaping)
- ✅ CORS configurado

### Sprint 1-2
- Rate limiting por IP y usuario
- Audit log de accesos
- Sesiones con expiración
- Refresh tokens seguros

### Antes de Producción
- Security headers (CSP, HSTS, etc.)
- Penetration testing básico
- Validación de dependencias

---

## 11. Métricas de Éxito

### Técnicas
| Métrica | Objetivo |
|---------|----------|
| Uptime | > 99.5% |
| Response time (p95) | < 500ms |
| Error rate | < 0.1% |
| Sync success rate | > 99% |

### Negocio (Post-Launch)
| Métrica | Objetivo |
|---------|----------|
| Registro → Conector vinculado | < 15 min |
| Tiempo a primer dashboard | < 1 hora |
| Retención 30 días | > 60% |

---

## 12. Riesgos y Mitigaciones

| Riesgo | Impacto | Mitigación |
|--------|---------|------------|
| Conector no conecta | Alto | Logs detallados, modo diagnóstico |
| Datos sensibles expuestos | Crítico | Encriptación, auditoría, acceso mínimo |
| Performance en sync | Medio | Paginación, compresión, delta sync |
| OAuth provider caído | Medio | Fallback a email/password |

---

## 13. Dependencias Externas

| Dependencia | Tipo | Alternativa |
|-------------|------|-------------|
| Azure | Hosting | AWS, GCP |
| SendGrid/Resend | Email | Amazon SES, Mailgun |
| Google OAuth | Auth | Microsoft, manual |
| GitHub | Repos/CI | GitLab, Azure DevOps |

---

## 14. Próximos Pasos

1. **Validar M3 (Pendiente manual):**
   - Rebuild contenedores: `docker compose --profile full up --build -d`
   - Probar flujo completo: login → dashboard → detalle cliente

2. **Iniciar Fase 4 - Cobranza Básica:**
   - Plantillas de email (CRUD)
   - Configuración de reglas de envío
   - Envío manual de recordatorios
   - Historial de comunicaciones

3. **Preparar Infraestructura Producción:**
   - Azure subscription
   - DNS/Dominio
   - SSL certificates
   - Deploy staging

---

## 15. Historial de Versiones

| Versión | Fecha | Cambios |
|---------|-------|---------|
| 1.0 | 2025-12-23 | Documento inicial |
| 1.1 | 2025-12-23 | Completado M0, inicio M1 |
| 1.2 | 2025-12-26 | Completados M1, M2, M3 |
| 1.3 | 2025-12-27 | Integración ASPEL connector, M5 Multi-Empresa planificado |

---

*Documento vivo - Actualizar con cada cambio de fase*
