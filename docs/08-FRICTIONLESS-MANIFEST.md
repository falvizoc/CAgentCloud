# Manifiesto FRICTIONLESS

> **Versión:** 1.0
> **Fecha:** 2025-12-23
> **Estado:** Activo - Documento Normativo
> **Alcance:** Todo el proyecto, todos los agentes, todas las fases

---

## 1. Declaración de Principios

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                                                              │
│     "FRICTIONLESS no es una característica, es la ESENCIA del producto."    │
│                                                                              │
│     Cada decisión técnica, de diseño y de negocio debe pasar por           │
│     el filtro: ¿Esto reduce o aumenta la fricción para el usuario?         │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Definición

**FRICTIONLESS** significa que el usuario puede:
1. Comenzar a usar el producto en **menos de 5 minutos**
2. Obtener valor real **sin configuración manual**
3. Realizar tareas complejas con **el mínimo de clics posible**
4. Entender qué hacer **sin leer documentación**

---

## 2. Aplicación por Capa del Sistema

### 2.1 Frontend (Claude-Frontend)

| Área | Fricción ❌ | FRICTIONLESS ✅ | Herramienta |
|------|-------------|-----------------|-------------|
| Login | Formulario largo | 1-clic OAuth | Clerk / BetterAuth |
| Onboarding | Tour de 10 pasos | 3 pasos skip-able | NextStep.js |
| Navegación | Menús anidados | Spotlight search (⌘K) | cmdk |
| Formularios | Validación al submit | Validación en tiempo real | React Hook Form + Zod |
| Feedback | Página de error | Toast + sugerencia de acción | Sonner |
| Carga | Spinner indefinido | Skeleton + optimistic UI | Suspense + TanStack Query |
| Idioma | Solo español | Auto-detect + selector | next-intl |

**Herramientas Recomendadas:**
- **[Clerk](https://clerk.com)**: Auth components pre-built, OAuth en 1 clic
- **[NextStep.js](https://nextstepjs.com)**: Onboarding ligero para Next.js
- **[cmdk](https://cmdk.paco.me)**: Command palette (⌘K) para navegación rápida
- **[Sonner](https://sonner.emilkowal.ski)**: Toasts elegantes y accesibles
- **[next-intl](https://next-intl.dev)**: i18n nativo para App Router

---

### 2.2 Backend (Claude-Backend)

| Área | Fricción ❌ | FRICTIONLESS ✅ | Implementación |
|------|-------------|-----------------|----------------|
| API Responses | Errores genéricos | Mensajes accionables i18n | Problem Details + códigos |
| Auth tokens | Expiración sin aviso | Auto-refresh transparente | Refresh token rotation |
| Validación | Rechazar silencioso | Sugerir corrección | FluentValidation + hints |
| Rate limit | Bloqueo inmediato | Degradación graceful | Polly + Redis |
| Defaults | Todo en null | Valores inteligentes | Seeders + config |

**Principios de API FRICTIONLESS:**
```csharp
// ❌ ANTI-PATTERN: Error genérico
return BadRequest("Error de validación");

// ✅ FRICTIONLESS: Error accionable
return Problem(
    title: "Email inválido",
    detail: "El formato debe ser usuario@dominio.com",
    instance: "/api/auth/register",
    extensions: new Dictionary<string, object?>
    {
        ["field"] = "email",
        ["suggestion"] = "¿Quisiste decir usuario@gmail.com?",
        ["code"] = "INVALID_EMAIL_FORMAT"
    }
);
```

---

### 2.3 Sincronización (Claude-Sync)

| Área | Fricción ❌ | FRICTIONLESS ✅ |
|------|-------------|-----------------|
| Vinculación | API key + IP + puerto | Código de 6 dígitos |
| Configuración | Manual en JSON | Auto-detectado del cloud |
| Firewall | Abrir puertos | Outbound-only (443) |
| Empresas | Seleccionar manualmente | Auto-detección de todas |
| Errores de sync | Log en archivo | Alerta en dashboard + sugerencia |
| Actualización | Reinstalar manual | Auto-update silencioso |

---

### 2.4 DevOps (Claude-DevOps)

| Área | Fricción ❌ | FRICTIONLESS ✅ |
|------|-------------|-----------------|
| Setup local | 20 comandos | `docker-compose up` |
| Variables | Copiar .env manual | .env.example completo |
| Migraciones | Ejecutar manualmente | Auto-migrate en dev |
| Secrets | Archivos locales | Azure Key Vault |
| Deploy | SSH + scripts | GitHub Actions auto |
| Monitoreo | Revisar logs | Alertas proactivas |

---

## 3. Herramientas FRICTIONLESS Aprobadas

### 3.1 Stack de Autenticación

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    OPCIONES DE AUTH (Orden de preferencia)                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  OPCIÓN 1: Clerk (Recomendado para MVP)                                     │
│  ═══════════════════════════════════════                                    │
│  • UI pre-construida, personalizable                                        │
│  • OAuth Google/Microsoft/Apple en minutos                                  │
│  • MFA, session management, webhooks incluidos                              │
│  • Pricing: Free tier generoso, $25/mo para pro                            │
│                                                                              │
│  OPCIÓN 2: BetterAuth (Si queremos control total)                          │
│  ═════════════════════════════════════════════════                          │
│  • Open source, self-hosted                                                 │
│  • Más trabajo inicial, máxima flexibilidad                                 │
│  • TypeScript-first, hooks de React                                         │
│                                                                              │
│  OPCIÓN 3: ASP.NET Identity + OAuth Manual                                  │
│  ═════════════════════════════════════════                                  │
│  • Control total, más código                                                │
│  • Solo si hay requisitos muy específicos                                   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Stack de Onboarding

| Librería | Uso | FRICTIONLESS Score |
|----------|-----|-------------------|
| **NextStep.js** | Tour de producto | ⭐⭐⭐⭐⭐ |
| OnboardJS | Flows de activación | ⭐⭐⭐⭐ |
| React Joyride | Tours complejos | ⭐⭐⭐ |
| Driver.js | Highlights simples | ⭐⭐⭐⭐ |

### 3.3 Stack de Internacionalización

```typescript
// next-intl: Configuración recomendada
// src/i18n/request.ts
import { getRequestConfig } from 'next-intl/server';

export default getRequestConfig(async ({ locale }) => ({
  messages: (await import(`../messages/${locale}.json`)).default,
  timeZone: 'America/Mexico_City',
  now: new Date(),
  formats: {
    dateTime: {
      short: { day: 'numeric', month: 'short', year: 'numeric' },
    },
    number: {
      currency: { style: 'currency', currency: 'MXN' },
    },
  },
}));
```

**Idiomas MVP:**
- 🇲🇽 Español (México) - Default
- 🇺🇸 English (US)

**Post-MVP:**
- 🇧🇷 Português (Brasil)
- 🇫🇷 Français

---

## 4. Motor de Reglas de Automatización

### 4.1 Arquitectura del Sistema de Cobranza Automatizada

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                 MOTOR DE AUTOMATIZACIÓN DE COBRANZA                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                        REGLAS CONFIGURABLES                          │   │
│  │                   (Definidas por el usuario en UI)                   │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                    │                                         │
│                                    ▼                                         │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                         MOTOR DE REGLAS                              │   │
│  │                    (Microsoft Rules Engine)                          │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                    │                                         │
│         ┌──────────────────────────┼──────────────────────────┐             │
│         ▼                          ▼                          ▼             │
│  ┌─────────────┐           ┌─────────────┐           ┌─────────────┐       │
│  │  ACCIÓN:    │           │  ACCIÓN:    │           │  ACCIÓN:    │       │
│  │  Enviar     │           │  Cambiar    │           │  Notificar  │       │
│  │  Email      │           │  Estado     │           │  Usuario    │       │
│  └─────────────┘           └─────────────┘           └─────────────┘       │
│         │                          │                          │             │
│         ▼                          ▼                          ▼             │
│  ┌─────────────┐           ┌─────────────┐           ┌─────────────┐       │
│  │  IA genera  │           │  Suspender  │           │  Push/Email │       │
│  │  contenido  │           │  crédito    │           │  al cobrador│       │
│  └─────────────┘           └─────────────┘           └─────────────┘       │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Tipos de Acciones Automatizadas

| Tipo | Trigger (Días vencido) | Acción | Tono |
|------|------------------------|--------|------|
| **Recordatorio amable** | 0 (día de vencimiento) | Email recordatorio | Amigable |
| **Primer aviso** | +7 días | Email + IA personaliza | Formal |
| **Segundo aviso** | +15 días | Email urgente | Firme |
| **Aviso de suspensión** | +30 días | Email advertencia | Serio |
| **Suspensión de servicio** | +45 días | Email + cambiar estado | Legal |
| **Evaluación de crédito** | +60 días | Notificar + sugerir reducir límite | Interno |

### 4.3 UI de Configuración de Reglas (FRICTIONLESS)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  CONFIGURACIÓN DE AUTOMATIZACIÓN                                 [+ Nueva]  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  Reglas activas:                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ ✓  Recordatorio amable              Día 0        [Editar] [···]    │   │
│  │ ✓  Primer aviso de pago             +7 días      [Editar] [···]    │   │
│  │ ✓  Segundo aviso urgente            +15 días     [Editar] [···]    │   │
│  │ ✓  Aviso de suspensión              +30 días     [Editar] [···]    │   │
│  │ ○  Suspender servicio               +45 días     [Editar] [···]    │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│  ─────────────────────────────────────────────────────────────────────────  │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  CREAR NUEVA REGLA                                                   │   │
│  ├─────────────────────────────────────────────────────────────────────┤   │
│  │                                                                      │   │
│  │  Cuando la factura tenga [___] días vencida:                        │   │
│  │                                                                      │   │
│  │  Acción:                                                             │   │
│  │  ○ Enviar email de recordatorio                                     │   │
│  │  ○ Enviar email de aviso                                            │   │
│  │  ○ Enviar email de suspensión                                       │   │
│  │  ○ Cambiar estado de cliente                                        │   │
│  │  ○ Notificar al cobrador                                            │   │
│  │                                                                      │   │
│  │  Plantilla: [Seleccionar plantilla ▼]                               │   │
│  │                                                                      │   │
│  │  Tono del mensaje:                                                   │   │
│  │  [Amigable] [Formal] [Firme] [Serio] [Legal]                        │   │
│  │                                                                      │   │
│  │  ☑ Usar IA para personalizar el mensaje                             │   │
│  │                                                                      │   │
│  │  [Cancelar]                                    [Guardar regla]      │   │
│  │                                                                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.4 Implementación con Microsoft Rules Engine

```csharp
// Definición de regla en JSON (almacenado en PostgreSQL)
{
  "WorkflowName": "CobranzaAutomatizada",
  "Rules": [
    {
      "RuleName": "PrimerAviso7Dias",
      "SuccessEvent": "EnviarEmailPrimerAviso",
      "Expression": "input.DiasVencido >= 7 AND input.DiasVencido < 15 AND input.EmailEnviado == false",
      "Actions": {
        "OnSuccess": {
          "Name": "OutputExpression",
          "Context": {
            "Accion": "email",
            "Plantilla": "primer_aviso",
            "Tono": "formal",
            "UsarIA": true
          }
        }
      }
    }
  ]
}
```

```csharp
// Servicio de evaluación de reglas
public class CobranzaAutomationService
{
    private readonly RulesEngine _rulesEngine;
    private readonly IEmailService _emailService;
    private readonly IAIService _aiService;

    public async Task EvaluarFacturasVencidasAsync()
    {
        var facturas = await _db.Facturas
            .Where(f => f.DiasVencido > 0)
            .ToListAsync();

        foreach (var factura in facturas)
        {
            var input = new {
                DiasVencido = factura.DiasVencido,
                EmailEnviado = factura.UltimoEmailDias < factura.DiasVencido
            };

            var results = await _rulesEngine.ExecuteAllRulesAsync("CobranzaAutomatizada", input);

            foreach (var result in results.Where(r => r.IsSuccess))
            {
                await EjecutarAccionAsync(result, factura);
            }
        }
    }
}
```

---

## 5. Sistema de IA para Cobranza

### 5.1 Funcionalidades de IA

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         IA EN COBRANZA CLOUD                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  GENERACIÓN DE CONTENIDO                                                     │
│  ═══════════════════════                                                     │
│  • Redacción de emails personalizados según contexto                        │
│  • Adaptación de tono (amigable → legal)                                    │
│  • Traducción automática al idioma del cliente                              │
│  • Sugerencia de asunto efectivo                                            │
│                                                                              │
│  PERSONALIZACIÓN INTELIGENTE                                                 │
│  ═══════════════════════════                                                │
│  • Analiza historial de pagos del cliente                                   │
│  • Considera comunicaciones previas                                         │
│  • Adapta mensaje según monto y antigüedad                                  │
│  • Sugiere mejor momento para enviar                                        │
│                                                                              │
│  ANÁLISIS (Futuro v2.0)                                                      │
│  ═════════════════════                                                       │
│  • Predicción de probabilidad de pago                                       │
│  • Scoring de clientes                                                       │
│  • Recomendación de estrategia de cobranza                                  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.2 Prompt System para Emails

```json
{
  "system_prompt": "Eres un asistente de cobranza profesional para {empresa}.
    Tu objetivo es redactar mensajes efectivos pero respetuosos que motiven
    el pago sin dañar la relación comercial.

    Reglas:
    - Siempre menciona el nombre del cliente
    - Incluye el monto exacto y fecha de vencimiento
    - Adapta el tono según el parámetro 'tono'
    - No amenaces, sé profesional
    - Incluye instrucciones claras de pago
    - Ofrece opciones si es posible
    - Idioma: {idioma}",

  "tonos": {
    "amigable": "Sé cordial y empático, como si hablaras con un amigo",
    "formal": "Mantén un tono profesional y respetuoso",
    "firme": "Sé directo sobre la importancia del pago",
    "serio": "Comunica urgencia y posibles consecuencias",
    "legal": "Incluye lenguaje formal sobre términos y condiciones"
  }
}
```

### 5.3 Ejemplo de Generación

**Input:**
```json
{
  "cliente": "Distribuidora Pérez S.A.",
  "contacto": "Juan Pérez",
  "monto": 45230.00,
  "moneda": "MXN",
  "dias_vencido": 15,
  "historial_pagos": "generalmente paga a tiempo, primer retraso",
  "tono": "formal",
  "idioma": "es-MX",
  "comunicaciones_previas": 1
}
```

**Output (IA):**
```
Asunto: Recordatorio de pago - Factura por $45,230.00 MXN

Estimado Juan Pérez,

Esperamos que se encuentre bien. Le escribimos respecto a la factura
F-2024-001 por un monto de $45,230.00 MXN, la cual presenta 15 días
de vencimiento.

Entendemos que pueden surgir situaciones imprevistas. Si existe algún
inconveniente o necesita establecer un plan de pago, con gusto podemos
conversarlo.

Para su comodidad, puede realizar el pago mediante:
• Transferencia bancaria: [datos]
• Portal de pagos: [link]

Quedamos atentos a sus comentarios.

Atentamente,
Departamento de Cobranza
{empresa}
```

---

## 6. Checklist FRICTIONLESS por Milestone

### M0: Foundation Ready
- [ ] `docker-compose up` funciona sin configuración manual
- [ ] README con setup de 3 pasos máximo
- [ ] Variables de entorno con defaults funcionales
- [ ] Hot-reload funcionando

### M1: Auth Complete
- [ ] Login con Google en 1 clic
- [ ] Login con Microsoft en 1 clic
- [ ] Sin verificación de email obligatoria
- [ ] Organización creada automáticamente
- [ ] Onboarding de 3 pasos (skip-able)

### M2: Sync Operational
- [ ] Código de 6 dígitos para vincular
- [ ] Auto-detección de empresas ASPEL
- [ ] Sin configurar firewall
- [ ] Estado de sync visible en dashboard

### M3: Dashboard Live
- [ ] Datos visibles al primer login
- [ ] Skeleton loading (no spinners)
- [ ] Navegación con ⌘K
- [ ] Responsive sin configurar
- [ ] Idioma auto-detectado

### M4: MVP Complete
- [ ] Plantillas pre-cargadas listas para usar
- [ ] IA genera primer borrador automático
- [ ] Envío con OAuth del usuario (sin SMTP)
- [ ] Reglas de automatización pre-configuradas
- [ ] 1 clic para enviar recordatorio

---

## 7. Métricas de Éxito FRICTIONLESS

| Métrica | Objetivo | Cómo Medir |
|---------|----------|------------|
| Time to First Value | < 5 minutos | Analytics: registro → primer dashboard |
| Clicks to Send Email | ≤ 3 clics | UX testing |
| Connector Setup Time | < 2 minutos | Tiempo desde código hasta sync |
| Support Tickets (Setup) | < 5% de usuarios | Zendesk/Intercom |
| Drop-off en Onboarding | < 20% | PostHog/Mixpanel |

---

## 8. Evaluación de Complejidad MVP

### Features por Prioridad

| Feature | Complejidad | MVP | Justificación |
|---------|-------------|-----|---------------|
| OAuth Google/Microsoft | Media | ✅ | Core FRICTIONLESS |
| Clerk Auth | Baja | ✅ | Reduce código, mejor UX |
| Código 6 dígitos | Baja | ✅ | Core FRICTIONLESS |
| Dashboard con KPIs | Media | ✅ | Valor inmediato |
| Envío email OAuth | Alta | ✅ | FRICTIONLESS > SMTP |
| IA redacción | Media | ✅ | Diferenciador clave |
| Motor de reglas básico | Media | ✅ | 5 reglas pre-config |
| i18n (ES/EN) | Baja | ✅ | next-intl es simple |
| Reglas personalizables UI | Alta | ⚠️ Simplificado | Solo editar existentes |
| Predicción IA | Alta | ❌ | v2.0 |
| WhatsApp | Alta | ❌ | v2.0 |

### Stack Final MVP (FRICTIONLESS-Optimizado)

```
Frontend:
├── Next.js 14 (App Router)
├── Clerk (Auth) ← Reduce semanas de desarrollo
├── next-intl (i18n)
├── NextStep.js (Onboarding)
├── cmdk (Command palette)
├── Sonner (Toasts)
└── shadcn/ui + Tailwind

Backend:
├── .NET 8 Minimal API
├── Entity Framework Core
├── Microsoft Rules Engine (reglas)
├── OpenAI API (redacción IA)
├── PostgreSQL
└── Redis

Servicios Externos:
├── Clerk (Auth management)
├── OpenAI (GPT-4o-mini)
├── Gmail API / Microsoft Graph (envío email)
└── Azure (hosting)
```

---

## 9. Referencias

### Herramientas Investigadas
- [Clerk - Auth Components](https://clerk.com)
- [BetterAuth - Open Source Auth](https://www.devtoolsacademy.com/blog/betterauth-vs-nextauth/)
- [NextStep.js - Onboarding](https://nextstepjs.com)
- [next-intl - i18n](https://next-intl.dev)
- [Microsoft Rules Engine](https://github.com/microsoft/RulesEngine)
- [OnboardJS](https://onboardjs.com)

### Mejores Prácticas
- [Best React Onboarding Libraries 2025](https://onboardjs.com/blog/5-best-react-onboarding-libraries-in-2025-compared)
- [Next.js Auth Libraries 2025](https://dev.to/joodi/best-authentication-libraries-for-nextjs-in-2025-5eca)
- [i18n Best Practices Next.js](https://next-intl.dev/docs/getting-started/app-router)
- [Collections Process Automation](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/collections-process-automate)

---

*Este documento es NORMATIVO. Toda decisión técnica debe alinearse con estos principios.*
*Actualizar cuando se identifiquen nuevas oportunidades de reducir fricción.*
