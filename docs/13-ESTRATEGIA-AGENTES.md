# Estrategia de Agentes Claude

> **Versión:** 1.0
> **Fecha:** 2025-12-23
> **Propósito:** Definir cuándo, cómo y quién activa los agentes especializados

---

## 1. Modelo de Agentes

### ¿Qué son los "Agentes" en este Proyecto?

Los "agentes" en CobranzaCloud **no son procesos autónomos ni sistemas separados**. Son **sesiones de Claude con contexto especializado**. Cada agente es simplemente una conversación con Claude donde:

1. Se carga el contexto específico del área (CLAUDE-*.md)
2. Se enfoca en tareas de esa área
3. Se aplican las convenciones documentadas

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    MODELO DE AGENTES CLAUDE                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  "Agente" = Sesión de Claude + Contexto Especializado + Tarea Específica   │
│                                                                              │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐  │
│  │   Claude    │    │   Claude    │    │   Claude    │    │   Claude    │  │
│  │      +      │    │      +      │    │      +      │    │      +      │  │
│  │  BACKEND.md │    │ FRONTEND.md │    │  DEVOPS.md  │    │ CLAUDE.md   │  │
│  │      =      │    │      =      │    │      =      │    │      =      │  │
│  │   Claude-   │    │   Claude-   │    │   Claude-   │    │   Sprint-   │  │
│  │   Backend   │    │   Frontend  │    │   DevOps    │    │    Lead     │  │
│  └─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. ¿Quién Crea/Activa los Agentes?

### Respuesta Corta: **TÚ los creas, cuando los necesitas**

Los agentes no se "crean" una vez y quedan activos. Se invocan **por sesión** según la tarea.

### Flujo de Activación

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    FLUJO DE ACTIVACIÓN DE AGENTE                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  1. TÚ identificas la tarea                                                 │
│     └── "Necesito crear un endpoint de clientes"                            │
│                                                                              │
│  2. TÚ determinas el área                                                   │
│     └── Backend → Claude-Backend                                            │
│                                                                              │
│  3. TÚ inicias sesión con Claude                                            │
│     └── Nueva conversación en Claude Code/API                               │
│                                                                              │
│  4. TÚ proporcionas el contexto (prompt de inicio)                          │
│     └── "Lee CLAUDE-BACKEND.md y crea endpoint..."                          │
│                                                                              │
│  5. CLAUDE ejecuta con las convenciones cargadas                            │
│     └── Genera código según el contexto                                     │
│                                                                              │
│  6. TÚ validas y aplicas                                                    │
│     └── Review, ajustes, commit                                             │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Triggers por Tipo de Tarea

### Tabla de Decisión: ¿Qué Agente Necesito?

| Tarea | Agente | Archivo de Contexto | Trigger |
|-------|--------|---------------------|---------|
| Crear/modificar endpoint | Claude-Backend | CLAUDE-BACKEND.md | Cualquier cambio en `/src/backend/` |
| Crear/modificar componente React | Claude-Frontend | CLAUDE-FRONTEND.md | Cualquier cambio en `/src/frontend/` |
| Configurar Docker/CI/CD | Claude-DevOps | CLAUDE-DEVOPS.md | Cualquier cambio en `/docker/`, `.github/` |
| Planificar milestone | Sprint-Lead | CLAUDE.md + 00-PLAN-MAESTRO.md | Inicio de nueva fase |
| Validar feature completa | Sprint-QA | CLAUDE.md + UX Guidelines | Antes de merge a develop |
| Protocolo de sync | Claude-Sync | CLAUDE-BACKEND.md + 07-SYNC-PROTOCOL.md | Trabajo en conectores |

### Señales de Cuándo Cambiar de Agente

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    INDICADORES DE CAMBIO DE AGENTE                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  🔄 CAMBIAR cuando:                                                          │
│                                                                              │
│  Backend → Frontend                                                          │
│  └── "El endpoint está listo, ahora necesito el componente que lo consume"  │
│                                                                              │
│  Frontend → Backend                                                          │
│  └── "Necesito un nuevo campo en la API para este componente"               │
│                                                                              │
│  Cualquiera → DevOps                                                         │
│  └── "Necesito configurar un servicio nuevo en Docker"                      │
│                                                                              │
│  Desarrollo → QA                                                             │
│  └── "Feature completa, necesito validar antes de merge"                    │
│                                                                              │
│  ⚠️ NO CAMBIAR cuando:                                                       │
│  └── Estás en medio de una tarea del área actual                            │
│  └── El cambio es menor y relacionado al contexto actual                    │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Prompt de Inicio por Agente

### Claude-Backend

```markdown
## Contexto de Sesión - Backend

Estoy trabajando en el backend de CobranzaCloud.

### Lee estos archivos primero:
1. src/backend/CLAUDE-BACKEND.md (convenciones obligatorias)
2. docs/contracts/api-types.ts (tipos compartidos)

### Fase actual: [M0/M1/M2/etc.]

### Tarea:
[Descripción de la tarea]

### Restricciones:
- Seguir estrictamente las convenciones de CLAUDE-BACKEND.md
- Usar tipos de api-types.ts
- Validar seguridad según docs/03-SEGURIDAD.md
```

### Claude-Frontend

```markdown
## Contexto de Sesión - Frontend

Estoy trabajando en el frontend de CobranzaCloud.

### Lee estos archivos primero:
1. src/frontend/CLAUDE-FRONTEND.md (convenciones obligatorias)
2. docs/contracts/api-types.ts (tipos compartidos)
3. docs/04-UX-GUIDELINES.md (si es trabajo de UI)

### Fase actual: [M0/M1/M2/etc.]

### Tarea:
[Descripción de la tarea]

### Restricciones:
- Seguir estrictamente las convenciones de CLAUDE-FRONTEND.md
- Usar shadcn/ui para componentes
- Skeleton loading obligatorio
- Accesibilidad WCAG 2.1 AA
```

### Claude-DevOps

```markdown
## Contexto de Sesión - DevOps

Estoy trabajando en la infraestructura de CobranzaCloud.

### Lee estos archivos primero:
1. docker/CLAUDE-DEVOPS.md (convenciones obligatorias)
2. docs/06-DEPLOYMENT.md (guía de despliegue)

### Fase actual: [M0/M1/M2/etc.]

### Tarea:
[Descripción de la tarea]

### Restricciones:
- FRICTIONLESS: `docker-compose up` sin configuración manual
- Secrets en variables de entorno, nunca hardcodeados
```

### Sprint-Lead (Planificación)

```markdown
## Contexto de Sesión - Planificación

Necesito planificar el trabajo para [Milestone/Sprint].

### Lee estos archivos primero:
1. CLAUDE.md (contexto general)
2. docs/00-PLAN-MAESTRO.md (plan actual)
3. docs/08-FRICTIONLESS-MANIFEST.md (principios normativos)

### Objetivo:
[Qué queremos lograr]

### Entregables esperados:
[Lista de entregables]

### Restricciones:
- Alinear con principio FRICTIONLESS
- Considerar checklist OWASP de la fase
```

---

## 5. Ciclo de Vida de un Feature

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    CICLO DE VIDA DE FEATURE                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  FASE 1: PLANIFICACIÓN                                                       │
│  ━━━━━━━━━━━━━━━━━━━━                                                       │
│  Agente: Sprint-Lead                                                         │
│  Trigger: Nueva feature en backlog                                           │
│  Output: Tareas definidas, criterios de éxito                               │
│                                                                              │
│  FASE 2: DISEÑO API                                                          │
│  ━━━━━━━━━━━━━━━━━                                                          │
│  Agente: Claude-Backend                                                      │
│  Trigger: Tarea de API asignada                                              │
│  Output: Endpoints, tipos en api-types.ts                                   │
│                                                                              │
│  FASE 3: IMPLEMENTACIÓN BACKEND                                              │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━                                               │
│  Agente: Claude-Backend                                                      │
│  Trigger: API diseñada                                                       │
│  Output: Código, tests, migraciones                                         │
│                                                                              │
│  FASE 4: IMPLEMENTACIÓN FRONTEND                                             │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━                                              │
│  Agente: Claude-Frontend                                                     │
│  Trigger: API disponible                                                     │
│  Output: Componentes, hooks, páginas                                        │
│                                                                              │
│  FASE 5: INTEGRACIÓN                                                         │
│  ━━━━━━━━━━━━━━━━━━                                                         │
│  Agente: Claude-Frontend + Claude-Backend (según necesidad)                 │
│  Trigger: Frontend y Backend listos                                         │
│  Output: Feature funcionando end-to-end                                     │
│                                                                              │
│  FASE 6: QA                                                                  │
│  ━━━━━━━━                                                                   │
│  Agente: Sprint-QA                                                           │
│  Trigger: Feature lista para review                                         │
│  Output: Validación, issues encontrados                                     │
│                                                                              │
│  FASE 7: DEPLOY                                                              │
│  ━━━━━━━━━━━                                                                │
│  Agente: Claude-DevOps                                                       │
│  Trigger: Feature aprobada                                                   │
│  Output: Feature en staging/producción                                      │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Buenas Prácticas

### ✅ DO (Hacer)

1. **Iniciar cada sesión con contexto**
   - Siempre pedir a Claude que lea el archivo CLAUDE-*.md correspondiente
   - Proporcionar la fase/milestone actual

2. **Una tarea, un agente**
   - No mezclar tareas de backend y frontend en la misma sesión
   - Cambiar de agente cuando cambies de área

3. **Validar output contra convenciones**
   - Verificar que el código sigue las convenciones documentadas
   - Si no las sigue, pedir corrección referenciando el documento

4. **Mantener contexto actualizado**
   - Actualizar archivos CLAUDE-*.md cuando haya nuevos patrones
   - Documentar decisiones importantes

### ❌ DON'T (No Hacer)

1. **No asumir que Claude recuerda**
   - Cada sesión es nueva, proporcionar contexto completo

2. **No mezclar áreas en una sesión**
   - ❌ "Crea el endpoint Y el componente que lo usa"
   - ✅ Sesión 1: Endpoint (Backend) → Sesión 2: Componente (Frontend)

3. **No ignorar las convenciones**
   - Si Claude genera código que no sigue convenciones, corregir antes de usar

4. **No olvidar los tipos**
   - Siempre referenciar `api-types.ts` para consistencia

---

## 7. Escenarios Comunes

### Escenario 1: "Necesito una feature nueva"

```
1. Sprint-Lead: Planificar tareas
2. Claude-Backend: Diseñar e implementar API
3. Actualizar api-types.ts
4. Claude-Frontend: Implementar UI
5. Sprint-QA: Validar
6. Claude-DevOps: Deploy (si hay cambios de infra)
```

### Escenario 2: "Hay un bug en producción"

```
1. Identificar área del bug
2. Activar agente correspondiente (Backend/Frontend)
3. Proporcionar: error, logs, contexto
4. Implementar fix
5. Claude-DevOps: Hotfix deploy
```

### Escenario 3: "Necesito refactorizar"

```
1. Activar agente del área a refactorizar
2. Proporcionar: código actual, objetivo del refactor
3. Asegurar que tests existentes pasan
4. Implementar refactor incrementalmente
```

### Escenario 4: "Nuevo milestone"

```
1. Sprint-Lead: Revisar plan maestro, definir scope
2. Actualizar 00-PLAN-MAESTRO.md
3. Crear tareas en backlog
4. Proceder con ciclo normal de features
```

---

## 8. Métricas de Efectividad

| Métrica | Cómo Medir | Target |
|---------|------------|--------|
| Iteraciones por feature | Sesiones de Claude necesarias | < 3 |
| Código aceptado sin cambios | % de código que no requiere refactor | > 70% |
| Bugs por convención ignorada | Bugs causados por no seguir convenciones | 0 |
| Tiempo de contexto | Tiempo para que Claude "entienda" la tarea | < 5 min |

---

## 9. Resumen

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    RESUMEN: AGENTES EN COBRANZACLOUD                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ¿QUÉ ES UN AGENTE?                                                          │
│  → Sesión de Claude + Contexto especializado                                │
│                                                                              │
│  ¿QUIÉN LOS CREA?                                                            │
│  → TÚ, cuando inicias una sesión de Claude                                  │
│                                                                              │
│  ¿CUÁNDO ACTIVARLOS?                                                         │
│  → Cuando tienes una tarea específica de un área                            │
│                                                                              │
│  ¿CÓMO ACTIVARLOS?                                                           │
│  → Prompt de inicio + pedir que lea CLAUDE-*.md correspondiente             │
│                                                                              │
│  ¿TRIGGER PARA CAMBIAR?                                                      │
│  → Cuando la tarea cambia de área (backend→frontend, etc.)                  │
│                                                                              │
│  ¿DEBEN TENER DIRECTIVA?                                                     │
│  → SÍ, cada uno tiene su archivo CLAUDE-*.md con convenciones               │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

*Documento de estrategia de agentes - Actualizar según evolucione el flujo de trabajo*
