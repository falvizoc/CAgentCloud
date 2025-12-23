# Guía de Prompts para Claude

> **Versión:** 1.0
> **Fecha:** 2025-12-23
> **Propósito:** Estandarizar la interacción con Claude para máxima eficiencia

---

## 1. Estructura de Prompt Efectivo

### Template General

```markdown
## Contexto
[Qué estoy haciendo, en qué parte del proyecto]

## Archivos Relevantes
- [Lista de archivos que Claude debe conocer]

## Tarea
[Descripción clara y específica]

## Requisitos
- [Lista de requisitos técnicos]

## Restricciones
- [Qué NO debe hacer]

## Output Esperado
[Código, análisis, plan, etc.]
```

---

## 2. Prompts por Tipo de Tarea

### 2.1 Crear Endpoint Backend

```markdown
## Contexto
Estoy trabajando en el backend de CobranzaCloud.
Lee el archivo `src/backend/CLAUDE-BACKEND.md` para conocer las convenciones.

## Tarea
Crear un endpoint POST /api/clientes que permita crear un nuevo cliente.

## Requisitos
- Seguir convenciones de CLAUDE-BACKEND.md
- Usar Minimal API (no Controllers)
- Validación con FluentValidation
- Result pattern para errores
- Incluir handler MediatR
- Multi-tenant: validar OrganizationId del token

## Tipos
Referencia: docs/contracts/api-types.ts
- Request: CreateClienteRequest
- Response: Cliente

## Output Esperado
1. Endpoint en ClientesEndpoints.cs
2. Command en Commands/Clientes/CreateClienteCommand.cs
3. Handler en Commands/Clientes/CreateClienteHandler.cs
4. Validator en Validators/CreateClienteValidator.cs
5. Tests unitarios
```

### 2.2 Crear Componente React

```markdown
## Contexto
Estoy trabajando en el frontend de CobranzaCloud.
Lee `src/frontend/CLAUDE-FRONTEND.md` para las convenciones.

## Tarea
Crear un componente ClienteCard que muestre resumen de un cliente.

## Requisitos
- TypeScript estricto
- Usar shadcn/ui (Card, Badge)
- Props tipadas con interface
- Skeleton para loading state
- Accesibilidad WCAG 2.1 AA

## Props
```typescript
interface ClienteCardProps {
  cliente: Cliente; // de docs/contracts/api-types.ts
  onSelect?: (id: string) => void;
  isSelected?: boolean;
}
```

## Output Esperado
1. Componente en components/features/clientes/cliente-card.tsx
2. Skeleton en cliente-card-skeleton.tsx
3. Test básico
```

### 2.3 Crear Hook de Data Fetching

```markdown
## Contexto
Frontend de CobranzaCloud, usando TanStack Query.

## Tarea
Crear hook useClientes para obtener lista paginada de clientes.

## Requisitos
- TanStack Query v5
- Tipado con types de docs/contracts/api-types.ts
- Soporte para search, paginación
- Cache de 5 minutos
- Invalidación al crear/actualizar

## Output Esperado
```typescript
// Uso esperado
const { data, isLoading, error } = useClientes({ search: 'term', page: 1 });
const { mutate: createCliente } = useCreateCliente();
```
```

### 2.4 Crear Migración de Base de Datos

```markdown
## Contexto
Backend CobranzaCloud, Entity Framework Core 8.

## Tarea
Crear migración para agregar tabla `email_templates`.

## Schema
```sql
email_templates:
- id: UUID (PK)
- organization_id: UUID (FK)
- nombre: VARCHAR(100)
- asunto: VARCHAR(255)
- cuerpo: TEXT
- variables: JSONB
- activa: BOOLEAN
- created_at: TIMESTAMP
- updated_at: TIMESTAMP
```

## Requisitos
- Entity Configuration separada
- Índice único (organization_id, nombre)
- Query filter multi-tenant
- Seeder con 3 plantillas default

## Output
1. Entity en Entities/EmailTemplate.cs
2. Configuration en Configurations/EmailTemplateConfiguration.cs
3. Comando de migración
```

### 2.5 Debugging / Investigación

```markdown
## Contexto
Tengo un error en el endpoint de login.

## Error
```
System.InvalidOperationException: No authenticationScheme was specified
at Microsoft.AspNetCore.Authentication...
```

## Lo que intenté
1. Verifiqué que AddAuthentication() está en Program.cs
2. El JWT está configurado en appsettings.json

## Archivos Relevantes
- src/backend/src/CobranzaCloud.Api/Program.cs

## Pregunta
¿Por qué ocurre este error y cómo lo soluciono?
```

### 2.6 Code Review

```markdown
## Contexto
Necesito revisar este código antes de hacer merge.

## Código a Revisar
```csharp
[código aquí]
```

## Criterios de Review
1. Seguridad (OWASP según docs/03-SEGURIDAD.md)
2. Convenciones (según CLAUDE-BACKEND.md)
3. Performance
4. Mantenibilidad
5. Tests

## Output Esperado
Lista de issues con severidad (crítico/alto/medio/bajo) y sugerencias.
```

### 2.7 Refactoring

```markdown
## Contexto
Este código funciona pero necesita mejorarse.

## Código Actual
```typescript
[código aquí]
```

## Problemas Identificados
1. [Problema 1]
2. [Problema 2]

## Restricciones
- No cambiar la API pública
- Mantener compatibilidad con X

## Output Esperado
Código refactorizado con explicación de cambios.
```

### 2.8 Crear Docker Configuration

```markdown
## Contexto
Necesito configurar Docker para desarrollo local.
Ver docker/CLAUDE-DEVOPS.md para convenciones.

## Tarea
Crear docker-compose para levantar PostgreSQL y Redis.

## Requisitos
- PostgreSQL 16
- Redis 7
- Healthchecks
- Volúmenes persistentes
- Red compartida

## Objetivo FRICTIONLESS
`docker-compose up` debe funcionar sin configuración adicional.

## Output
docker-compose.yml completo y funcional.
```

---

## 3. Anti-Patterns (Qué NO Hacer)

### 3.1 Prompts Demasiado Vagos

```markdown
❌ INCORRECTO
"Haz un CRUD de clientes"

✅ CORRECTO
"Crea el endpoint GET /api/clientes con paginación,
siguiendo CLAUDE-BACKEND.md, con tipos de api-types.ts"
```

### 3.2 Sin Contexto

```markdown
❌ INCORRECTO
"Arregla este bug"
[código sin contexto]

✅ CORRECTO
"Bug en el endpoint de login.
Error: [mensaje]
Contexto: Ocurre cuando [condición]
Archivos: [lista]"
```

### 3.3 Múltiples Tareas No Relacionadas

```markdown
❌ INCORRECTO
"Crea el endpoint de clientes,
arregla el bug de login,
y configura el Docker"

✅ CORRECTO
Una tarea por prompt, o tareas relacionadas:
"Crea los endpoints CRUD de clientes:
1. GET /api/clientes (lista)
2. GET /api/clientes/{id} (detalle)
3. POST /api/clientes (crear)
4. PUT /api/clientes/{id} (actualizar)"
```

### 3.4 Ignorar Convenciones Existentes

```markdown
❌ INCORRECTO
"Crea un Controller de clientes"

✅ CORRECTO
"Crea endpoints de clientes siguiendo CLAUDE-BACKEND.md
(usa Minimal API, no Controllers)"
```

---

## 4. Prompts de Inicio de Sesión

### 4.1 Inicio de Sesión Backend

```markdown
## Contexto de Sesión - Backend

### Proyecto
- Nombre: CobranzaCloud
- Área: Backend API
- Stack: .NET 8 Minimal API + PostgreSQL + Redis

### Archivos de Contexto
1. Lee: CLAUDE.md (contexto general)
2. Lee: src/backend/CLAUDE-BACKEND.md (convenciones)
3. Lee: docs/contracts/api-types.ts (tipos)

### Fase Actual
[M0/M1/M2/etc.] - [Descripción]

### Tarea de Esta Sesión
[Describir tarea]
```

### 4.2 Inicio de Sesión Frontend

```markdown
## Contexto de Sesión - Frontend

### Proyecto
- Nombre: CobranzaCloud
- Área: Frontend Web
- Stack: Next.js 14 + React 18 + TanStack Query

### Archivos de Contexto
1. Lee: CLAUDE.md (contexto general)
2. Lee: src/frontend/CLAUDE-FRONTEND.md (convenciones)
3. Lee: docs/contracts/api-types.ts (tipos)
4. Lee: docs/04-UX-GUIDELINES.md (si es UI)

### Tarea de Esta Sesión
[Describir tarea]
```

### 4.3 Inicio de Sesión DevOps

```markdown
## Contexto de Sesión - DevOps

### Proyecto
- Nombre: CobranzaCloud
- Área: Infraestructura
- Stack: Docker + GitHub Actions + Azure

### Archivos de Contexto
1. Lee: CLAUDE.md (contexto general)
2. Lee: docker/CLAUDE-DEVOPS.md (convenciones)
3. Lee: docs/06-DEPLOYMENT.md (si es deploy)

### Tarea de Esta Sesión
[Describir tarea]
```

---

## 5. Formato de Respuesta Esperada

### 5.1 Para Código

```markdown
## Output Esperado

1. **Explicación breve** (2-3 líneas máximo)
2. **Código completo y funcional** (no snippets incompletos)
3. **Tests** si la tarea los requiere
4. **Notas de implementación** (consideraciones, trade-offs)
```

### 5.2 Para Análisis

```markdown
## Output Esperado

1. **Resumen ejecutivo** (1 párrafo)
2. **Opciones** con pros/cons
3. **Recomendación** con justificación
4. **Próximos pasos** concretos
```

### 5.3 Para Debugging

```markdown
## Output Esperado

1. **Causa del error**
2. **Solución** con código
3. **Prevención** (cómo evitarlo en el futuro)
```

---

## 6. Ejemplos de Prompts Reales

### Ejemplo 1: Feature Completa

```markdown
## Contexto
Milestone M2: Infraestructura de Sincronización
Backend CobranzaCloud

## Tarea
Implementar el flujo completo de vinculación de conector:
1. Endpoint para generar código de 6 dígitos
2. Endpoint para registrar conector con el código
3. Validación de código (TTL 15 min, un solo uso)

## Archivos de Referencia
- CLAUDE-BACKEND.md (convenciones)
- docs/07-SYNC-PROTOCOL.md (protocolo)
- docs/contracts/api-types.ts (tipos)

## Requisitos de Seguridad (OWASP)
- A04: Código con TTL, un solo uso
- A07: Rate limiting en generación de código

## Output
1. ConnectorsEndpoints.cs con ambos endpoints
2. Commands y handlers
3. Tests
```

### Ejemplo 2: Debugging con Contexto

```markdown
## Contexto
Frontend, página de clientes

## Error
La lista de clientes no carga, muestra error en consola:
```
TypeError: Cannot read properties of undefined (reading 'data')
```

## Código Relevante
```typescript
const { data } = useClientes();
return <ClienteList clientes={data.data} />;
```

## Lo que sé
- El hook funciona en otras páginas
- La API responde correctamente (verificado en Network tab)

## Pregunta
¿Cuál es el bug y cómo lo soluciono siguiendo las convenciones de CLAUDE-FRONTEND.md?
```

---

## 7. Checklist Pre-Prompt

Antes de enviar un prompt, verifica:

- [ ] ¿Incluí el contexto del proyecto/área?
- [ ] ¿Referencié los archivos CLAUDE-*.md relevantes?
- [ ] ¿La tarea es específica y acotada?
- [ ] ¿Incluí los tipos de api-types.ts si aplica?
- [ ] ¿Especifiqué el output esperado?
- [ ] ¿Mencioné restricciones importantes?
- [ ] ¿Es una sola tarea o tareas relacionadas?

---

*Guía de prompts - Actualizar con patrones que funcionen bien*
