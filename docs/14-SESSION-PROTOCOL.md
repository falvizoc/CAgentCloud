# Protocolo de Sesiones Claude

> **Versión**: 1.0
> **Fecha**: 2025-12-23
> **Propósito**: Mantener continuidad y contexto entre sesiones de Claude

---

## Resumen

Este protocolo define cómo iniciar, mantener y cerrar sesiones de trabajo con Claude para preservar el contexto del proyecto y asegurar continuidad efectiva.

---

## Estructura de Archivos

```
.claude/
├── sessions/
│   ├── CURRENT.md      # Sesión activa (no commitear)
│   └── HISTORY.md      # Log de sesiones pasadas
├── state/
│   ├── progress.md     # Estado del proyecto y milestones
│   ├── decisions.md    # Decisiones técnicas (ADRs)
│   └── handoff.md      # Notas para próxima sesión
└── templates/          # Plantillas reutilizables
```

---

## Flujo de Sesión

### 1. Al Iniciar Sesión

```
┌─────────────────────────────────────────────────────────────┐
│                    INICIO DE SESIÓN                         │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  1. Leer .claude/state/handoff.md                           │
│     → Contexto inmediato de sesión anterior                 │
│                                                              │
│  2. Revisar .claude/state/progress.md                       │
│     → Estado actual del proyecto y milestone                │
│                                                              │
│  3. Cargar documentación según tarea:                       │
│     • CLAUDE.md (siempre)                                   │
│     • CLAUDE-BACKEND.md (si backend)                        │
│     • CLAUDE-FRONTEND.md (si frontend)                      │
│     • CLAUDE-DEVOPS.md (si infraestructura)                 │
│                                                              │
│  4. Actualizar .claude/sessions/CURRENT.md                  │
│     → Registrar inicio de nueva sesión                      │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

**Prompt sugerido para iniciar:**
```
Lee .claude/state/handoff.md y .claude/state/progress.md
para entender el contexto actual del proyecto.
Luego actualiza CURRENT.md con esta nueva sesión.
```

### 2. Durante la Sesión

```
┌─────────────────────────────────────────────────────────────┐
│                   DURANTE LA SESIÓN                          │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Al completar tareas:                                        │
│  → Actualizar progress.md (marcar completado)               │
│                                                              │
│  Al tomar decisiones importantes:                           │
│  → Agregar entrada en decisions.md                          │
│                                                              │
│  Al modificar archivos:                                      │
│  → Listar en CURRENT.md                                     │
│                                                              │
│  Usar TodoWrite tool:                                        │
│  → Mantener lista de tareas visible                         │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### 3. Al Cerrar Sesión

```
┌─────────────────────────────────────────────────────────────┐
│                    CIERRE DE SESIÓN                          │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  1. Actualizar .claude/state/handoff.md                     │
│     → Resumen de lo hecho                                   │
│     → Qué quedó pendiente                                   │
│     → Contexto crítico para próxima sesión                  │
│                                                              │
│  2. Mover contenido de CURRENT.md a HISTORY.md              │
│     → Preservar registro histórico                          │
│                                                              │
│  3. Actualizar progress.md                                   │
│     → Estado final de tareas                                │
│                                                              │
│  4. Commit de archivos .claude/ (opcional)                  │
│     → git add .claude/state/ .claude/sessions/HISTORY.md    │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

**Prompt sugerido para cerrar:**
```
Antes de terminar, actualiza handoff.md con un resumen
de lo que hicimos, qué quedó pendiente, y contexto
importante para la próxima sesión.
```

---

## Handoff Entre Agentes

Cuando se cambia de un agente a otro (ej: Backend → Frontend):

```
┌─────────────────────────────────────────────────────────────┐
│                  HANDOFF ENTRE AGENTES                       │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Agente Saliente (ej: Backend):                             │
│  1. Documentar APIs/interfaces creadas                      │
│  2. Listar archivos relevantes para siguiente agente        │
│  3. Escribir notas específicas en handoff.md                │
│                                                              │
│  Agente Entrante (ej: Frontend):                            │
│  1. Leer handoff.md primero                                 │
│  2. Revisar archivos mencionados                            │
│  3. Verificar api-types.ts para contratos                   │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Archivos por Tipo de Agente

| Agente | Archivos a leer | Archivos a actualizar |
|--------|-----------------|----------------------|
| **Fullstack** | CLAUDE.md, handoff.md, progress.md | Todos |
| **Backend** | CLAUDE.md, CLAUDE-BACKEND.md, handoff.md | decisions.md, progress.md |
| **Frontend** | CLAUDE.md, CLAUDE-FRONTEND.md, handoff.md | progress.md |
| **DevOps** | CLAUDE.md, CLAUDE-DEVOPS.md, handoff.md | decisions.md |

---

## Qué NO Commitear

Agregar a `.gitignore`:
```
# Session files (temporary)
.claude/sessions/CURRENT.md
```

**SÍ commitear:**
- `.claude/sessions/HISTORY.md` - Historial de sesiones
- `.claude/state/progress.md` - Estado del proyecto
- `.claude/state/decisions.md` - Decisiones técnicas
- `.claude/state/handoff.md` - Notas de handoff

---

## Checklist de Alineación

### Inicio de sesión
- [ ] Leí `handoff.md`
- [ ] Revisé `progress.md`
- [ ] Cargué CLAUDE.md y CLAUDE-{AGENT}.md relevante
- [ ] Actualicé `CURRENT.md`

### Fin de sesión
- [ ] Actualicé `handoff.md` con resumen y próximos pasos
- [ ] Actualicé `progress.md` con tareas completadas
- [ ] Documenté decisiones importantes en `decisions.md`
- [ ] Moví `CURRENT.md` a `HISTORY.md`

---

## Recuperación de Contexto (Post-Compactación)

Si Claude pierde contexto por compactación automática:

1. **Lectura rápida**:
   ```
   Lee .claude/state/handoff.md para recuperar
   contexto de la sesión actual.
   ```

2. **Si necesitas más contexto**:
   ```
   Lee también progress.md y decisions.md para
   entender el estado completo del proyecto.
   ```

3. **Para contexto técnico específico**:
   - Backend: Lee `CLAUDE-BACKEND.md`
   - Frontend: Lee `CLAUDE-FRONTEND.md`
   - DevOps: Lee `CLAUDE-DEVOPS.md`

---

## Ejemplo de Handoff Efectivo

```markdown
# Handoff - Notas para Próxima Sesión

## Resumen
Completé los endpoints de autenticación OAuth en el backend.
Frontend necesita implementar las páginas de login.

## Dónde lo dejamos
- ✅ /auth/google endpoint funcional
- ✅ /auth/microsoft endpoint funcional
- ✅ JWT generation implementado
- ⏳ Frontend sin páginas de auth

## Qué necesita hacerse
1. Crear página /login con botones OAuth
2. Implementar manejo de tokens en cliente
3. Crear contexto de autenticación (React Context)

## Archivos clave
- `src/backend/src/.../AuthEndpoints.cs` - Endpoints creados
- `docs/contracts/api-types.ts` - Tipos de auth actualizados
- `.env.example` - Variables OAuth documentadas

## Contexto crítico
- Redirect URI configurado: http://localhost:3000/auth/callback
- Tokens expiran en 1 hora, refresh en 7 días
```

---

*Este protocolo debe seguirse en cada sesión para mantener continuidad efectiva.*
