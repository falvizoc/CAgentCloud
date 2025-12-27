# Protocolo de Sesiones

> Este archivo define los procedimientos de apertura y cierre de sesión.
> Claude debe leer este archivo cuando el usuario diga "abre sesión" o "cierra sesión".

---

## ABRE SESIÓN

Cuando el usuario diga **"abre sesión"**, **"inicio"**, o **"buenos días"**:

### Pasos Obligatorios
1. `git pull origin main` - Sincronizar cambios
2. Leer `.claude/sessions/CURRENT.md` - Última sesión
3. Leer `.claude/state/progress.md` - Estado de milestones
4. Leer `.claude/state/decisions.md` - Decisiones vigentes
5. `docker compose ps` - Verificar servicios

### Output Esperado
```
## Sesión Iniciada

**Última sesión**: {ID de CURRENT.md}
**Milestone actual**: {de progress.md}
**Servicios**: {estado de docker}

### Pendientes
- {lista de próximos pasos de CURRENT.md}

### Decisiones Vigentes
- {últimas 3 decisiones relevantes}

¿Continuamos con {siguiente tarea}?
```

---

## CIERRA SESIÓN

Cuando el usuario diga **"cierra sesión"**, **"termina"**, o **"hasta mañana"**:

### Pasos Obligatorios
1. Actualizar `.claude/sessions/CURRENT.md` con trabajo realizado
2. Agregar sesión a `.claude/sessions/HISTORY.md` (al inicio)
3. Actualizar `.claude/state/progress.md` si hubo avances
4. Agregar decisiones nuevas a `.claude/state/decisions.md`
5. `git add -A && git diff --staged --stat`
6. Crear commit descriptivo
7. `git push origin main`

### Output Esperado
```
## Sesión Cerrada

**ID**: {YYYYMMDD-agente-NNN}
**Commits**: {lista de commits}
**Archivos**: {+X -Y líneas}

### Completado
- {resumen de trabajo}

### Próxima Sesión
- {pendientes para mañana}

Hasta mañana.
```

---

## Notas

- `CURRENT.md` NO se versiona (está en .gitignore)
- `HISTORY.md` SÍ se versiona
- Siempre hacer push antes de cerrar
- Verificar `git status` limpio antes de confirmar cierre
