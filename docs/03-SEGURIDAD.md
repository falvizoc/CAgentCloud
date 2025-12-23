# Políticas de Seguridad

> **Versión:** 1.1
> **Fecha:** 2025-12-23
> **Estado:** Definición
> **Criticidad:** ALTA - Este es un sistema financiero
> **Marco de Referencia:** [OWASP Top 10:2025](https://owasp.org/Top10/2025/)

---

## 0. OWASP Top 10:2025 - Alineación del Proyecto

> **IMPORTANTE:** Este proyecto DEBE cumplir con las recomendaciones OWASP para aplicaciones web.
> Referencia oficial: https://owasp.org/Top10/2025/

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    OWASP TOP 10:2025 - CHECKLIST DE CUMPLIMIENTO             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  #  │ Riesgo                              │ Mitigación en CobranzaCloud     │
│ ════╪═════════════════════════════════════╪═════════════════════════════════│
│ A01 │ Broken Access Control               │ RBAC + RLS PostgreSQL + JWT     │
│ A02 │ Security Misconfiguration           │ IaC, secrets en Key Vault       │
│ A03 │ Software Supply Chain Failures      │ Dependabot, audit npm/nuget     │
│ A04 │ Cryptographic Failures              │ TLS 1.3, AES-256, bcrypt        │
│ A05 │ Injection                           │ EF Core params, Zod validation  │
│ A06 │ Insecure Design                     │ Threat modeling, code review    │
│ A07 │ Authentication Failures             │ Clerk/OAuth, MFA, rate limit    │
│ A08 │ Software/Data Integrity Failures    │ SRI, signed commits, checksums  │
│ A09 │ Security Logging/Alerting Failures  │ Serilog + Azure Monitor         │
│ A10 │ Mishandling Exceptional Conditions  │ Global error handler, no leaks  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Mitigaciones Detalladas por Riesgo OWASP

| # | Riesgo | Implementación en el Proyecto |
|---|--------|-------------------------------|
| **A01** | Broken Access Control | Row Level Security en PostgreSQL, validación de `org_id` en cada query, RBAC con roles granulares |
| **A02** | Security Misconfiguration | Docker hardening, headers seguros (CSP, HSTS), Azure Policy |
| **A03** | Supply Chain Failures | Dependabot activo, `npm audit` y `dotnet list package --vulnerable` en CI |
| **A04** | Cryptographic Failures | TLS 1.3 obligatorio, Argon2id para passwords, secrets en Azure Key Vault |
| **A05** | Injection | Entity Framework con parámetros, Zod para validación frontend, sanitización |
| **A06** | Insecure Design | Threat modeling STRIDE, revisión de diseño en PRs |
| **A07** | Authentication Failures | Clerk con MFA, rate limiting (Polly), bloqueo de cuenta |
| **A08** | Integrity Failures | Subresource Integrity (SRI), commits firmados, checksums en sync |
| **A09** | Logging Failures | Serilog estructurado, Azure Monitor alertas, no loguear PII |
| **A10** | Exceptional Conditions | Middleware global de errores, mensajes genéricos al usuario, logs detallados internos |

---

## 1. Principios de Seguridad

```
┌─────────────────────────────────────────────────────────────┐
│              PRINCIPIOS FUNDAMENTALES                        │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  🔒 Defense in Depth    Múltiples capas de protección       │
│  🔐 Least Privilege     Acceso mínimo necesario             │
│  🔑 Zero Trust          Verificar siempre, nunca confiar    │
│  📝 Audit Everything    Registrar todas las acciones        │
│  🛡️ Secure by Default   Configuración segura inicial        │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Matriz de Amenazas (STRIDE)

| Amenaza | Riesgo | Mitigación |
|---------|--------|------------|
| **S**poofing | Alto | OAuth, JWT, MFA |
| **T**ampering | Alto | HTTPS, integridad de datos, checksums |
| **R**epudiation | Medio | Audit logs, timestamps |
| **I**nformation Disclosure | Crítico | Encriptación, RLS, acceso mínimo |
| **D**enial of Service | Medio | Rate limiting, WAF, auto-scale |
| **E**levation of Privilege | Alto | RBAC, validación de roles |

---

## 3. Autenticación

### 3.1 Métodos Soportados

| Método | Prioridad | Implementación |
|--------|-----------|----------------|
| Email + Password | MVP | ASP.NET Identity |
| Google OAuth | MVP | OAuth 2.0 / OIDC |
| Microsoft 365 | MVP | OAuth 2.0 / OIDC |
| Apple ID | v2.0 | OAuth 2.0 / OIDC |
| MFA (TOTP) | MVP (admins) | Authenticator apps |

### 3.2 Política de Contraseñas

```csharp
services.Configure<IdentityOptions>(options =>
{
    // Requisitos de contraseña
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredUniqueChars = 4;

    // Bloqueo de cuenta
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});
```

### 3.3 JWT Tokens

**Configuración:**
```json
{
  "Jwt": {
    "Issuer": "cobranzacloud.com",
    "Audience": "cobranzacloud-api",
    "AccessTokenExpiration": 15,     // minutos
    "RefreshTokenExpiration": 7,     // días
    "Key": "STORED_IN_AZURE_KEY_VAULT"
  }
}
```

**Estructura del Token:**
```json
{
  "sub": "user-uuid",
  "email": "user@example.com",
  "org_id": "organization-uuid",
  "role": "admin",
  "permissions": ["cartera:read", "cartera:write"],
  "iat": 1703203200,
  "exp": 1703204100
}
```

### 3.4 Refresh Token Rotation

```
1. Access token expira (15 min)
2. Cliente envía refresh token
3. Server valida refresh token
4. Server genera NUEVO access + refresh token
5. Refresh token anterior se invalida
6. Si refresh token reusado → revocar todos los tokens
```

---

## 4. Autorización (RBAC)

### 4.1 Roles del Sistema

| Rol | Permisos | Descripción |
|-----|----------|-------------|
| `owner` | * | Propietario de organización |
| `admin` | Gestión completa excepto billing | Administrador |
| `manager` | CRUD cartera, usuarios read | Gerente de cobranza |
| `collector` | Cartera read/write, clientes read | Cobrador |
| `viewer` | Solo lectura | Consulta |

### 4.2 Matriz de Permisos

```
                   owner  admin  manager  collector  viewer
──────────────────────────────────────────────────────────
users:create         ✓      ✓       -         -        -
users:read           ✓      ✓       ✓         -        -
users:update         ✓      ✓       -         -        -
users:delete         ✓      ✓       -         -        -
──────────────────────────────────────────────────────────
cartera:read         ✓      ✓       ✓         ✓        ✓
cartera:write        ✓      ✓       ✓         ✓        -
cartera:export       ✓      ✓       ✓         -        -
──────────────────────────────────────────────────────────
clientes:read        ✓      ✓       ✓         ✓        ✓
clientes:contact     ✓      ✓       ✓         ✓        -
──────────────────────────────────────────────────────────
connectors:manage    ✓      ✓       -         -        -
settings:manage      ✓      ✓       -         -        -
billing:manage       ✓      -       -         -        -
```

### 4.3 Implementación

```csharp
// Attribute-based authorization
[Authorize(Policy = "CarteraWrite")]
public async Task<IResult> UpdateFactura(...)

// Policy definition
services.AddAuthorizationBuilder()
    .AddPolicy("CarteraWrite", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permissions", "cartera:write") ||
            context.User.IsInRole("admin")));
```

---

## 5. Seguridad de Datos

### 5.1 Encriptación

| Dato | En Tránsito | En Reposo |
|------|-------------|-----------|
| Contraseñas | HTTPS | Argon2id hash |
| Tokens | HTTPS | AES-256 (DB) |
| Datos financieros | HTTPS/TLS 1.3 | Transparent Data Encryption |
| Backups | - | AES-256 |

### 5.2 Hashing de Contraseñas

```csharp
// Usar Argon2id (más seguro que bcrypt)
services.Configure<PasswordHasherOptions>(options =>
{
    options.IterationCount = 100000;  // PBKDF2 fallback
});

// O implementar Argon2id directamente
public class Argon2PasswordHasher : IPasswordHasher<User>
{
    public string HashPassword(User user, string password)
    {
        return Argon2.Hash(password, new Argon2Config
        {
            Type = Argon2Type.Argon2id,
            MemoryCost = 65536,
            TimeCost = 3,
            Lanes = 4,
            HashLength = 32
        });
    }
}
```

### 5.3 Multi-Tenant Data Isolation

```sql
-- Row Level Security en PostgreSQL
ALTER TABLE clientes ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation ON clientes
    USING (organization_id = current_setting('app.current_org')::uuid);

-- En cada request, setear el contexto
SET app.current_org = 'uuid-de-organizacion';
```

---

## 6. Seguridad de API

### 6.1 Rate Limiting

```csharp
// Por IP + Usuario
services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        context =>
        {
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? context.Connection.RemoteIpAddress?.ToString()
                ?? "anonymous";

            return RateLimitPartition.GetFixedWindowLimiter(userId, _ =>
                new FixedWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(1),
                    PermitLimit = 100,
                    QueueLimit = 10
                });
        });

    // Límite específico para login
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(15),
                PermitLimit = 5
            }));
});
```

### 6.2 CORS

```csharp
services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy
            .WithOrigins(
                "https://app.cobranzacloud.com",
                "https://www.cobranzacloud.com")
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Authorization", "Content-Type", "X-Request-Id")
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});
```

### 6.3 Security Headers

```csharp
app.Use(async (context, next) =>
{
    // Prevenir clickjacking
    context.Response.Headers["X-Frame-Options"] = "DENY";

    // Prevenir MIME sniffing
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";

    // XSS Protection (legacy browsers)
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

    // Referrer Policy
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    // Content Security Policy
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://accounts.google.com; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "connect-src 'self' https://api.cobranzacloud.com;";

    // HSTS (solo en producción con HTTPS)
    if (context.Request.IsHttps)
    {
        context.Response.Headers["Strict-Transport-Security"] =
            "max-age=31536000; includeSubDomains; preload";
    }

    await next();
});
```

---

## 7. Protección contra OWASP Top 10:2025

> **Referencia:** Sección 0 contiene el mapeo completo OWASP 2025 → Mitigaciones.
> Esta sección detalla implementaciones específicas.

### 7.1 A05 - Injection (SQL, NoSQL, Command)

```csharp
// ✅ SIEMPRE usar EF Core / Parameterized queries
var cliente = await _db.Clientes
    .Where(c => c.Clave == clienteId)  // Parameterizado automáticamente
    .FirstOrDefaultAsync();

// ❌ NUNCA concatenar strings
var query = $"SELECT * FROM clientes WHERE clave = '{clienteId}'";  // VULNERABLE
```

### 7.2 A07 - Authentication Failures

- ✅ JWT con expiración corta (15 min)
- ✅ Refresh token rotation
- ✅ Rate limiting en login
- ✅ Lockout después de intentos fallidos
- ✅ MFA para administradores

### 7.3 A04 - Cryptographic Failures

```csharp
// Nunca loggear datos sensibles
_logger.LogInformation("User {Email} logged in", user.Email);  // ✅
_logger.LogInformation("Login with password {Password}", password);  // ❌

// Nunca retornar datos sensibles
public record UserResponse(
    Guid Id,
    string Email,
    string Role
    // NO incluir: PasswordHash, RefreshToken, etc.
);
```

### 7.4 A08 - Software/Data Integrity Failures

```csharp
// Si procesamos XML, deshabilitar DTD
var settings = new XmlReaderSettings
{
    DtdProcessing = DtdProcessing.Prohibit,
    XmlResolver = null
};
```

### 7.5 A01 - Broken Access Control

```csharp
// Siempre verificar ownership
public async Task<IResult> GetCliente(Guid clienteId, ClaimsPrincipal user)
{
    var orgId = user.GetOrganizationId();

    var cliente = await _db.Clientes
        .Where(c => c.Id == clienteId && c.OrganizationId == orgId)  // ✅
        .FirstOrDefaultAsync();

    if (cliente == null) return Results.NotFound();
    return Results.Ok(cliente);
}
```

### 7.6 A02 - Security Misconfiguration

```csharp
// Development vs Production
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();  // Solo en dev
}
else
{
    app.UseExceptionHandler("/error");  // Handler genérico en prod
    app.UseHsts();
}

// Nunca exponer stack traces en producción
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Internal server error" });
        // NO incluir: exception.Message, exception.StackTrace
    });
});
```

### 7.7 A05 - Cross-Site Scripting (XSS)

```typescript
// React escapa automáticamente, pero cuidado con:
// ❌ dangerouslySetInnerHTML
<div dangerouslySetInnerHTML={{ __html: userContent }} />  // PELIGROSO

// ✅ Usar librerías de sanitización si es necesario
import DOMPurify from 'dompurify';
<div dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(userContent) }} />
```

### 7.8 A03 - Software Supply Chain Failures (NUEVO 2025)

```yaml
# .github/workflows/security.yml
name: Security Scan
on: [push, pull_request]

jobs:
  dependency-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      # Escaneo de dependencias .NET
      - name: .NET Vulnerability Scan
        run: dotnet list package --vulnerable --include-transitive

      # Escaneo de dependencias npm
      - name: npm audit
        run: npm audit --audit-level=high
        working-directory: ./src/frontend

      # Dependabot configurado automáticamente
      # Ver .github/dependabot.yml
```

```yaml
# .github/dependabot.yml
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/src/backend"
    schedule:
      interval: "weekly"
    open-pull-requests-limit: 5

  - package-ecosystem: "npm"
    directory: "/src/frontend"
    schedule:
      interval: "weekly"
    open-pull-requests-limit: 5
```

### 7.9 A10 - Mishandling of Exceptional Conditions (NUEVO 2025)

```csharp
// Middleware global de manejo de errores
public class ErrorHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            // Error de validación: mostrar detalles al usuario
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = 400
            });
        }
        catch (UnauthorizedAccessException)
        {
            // Acceso no autorizado: mensaje genérico
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Access denied" });
        }
        catch (Exception ex)
        {
            // Error interno: NO exponer detalles
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "An error occurred" });
            // NO incluir: ex.Message, ex.StackTrace, etc.
        }
    }
}
```

---

## 8. Seguridad de Conectores

### 8.1 Autenticación de Conectores

```
┌─────────────────────────────────────────────────────────────┐
│              FLUJO DE REGISTRO DE CONECTOR                  │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  1. Admin genera código de vinculación (6 dígitos)          │
│     → Código expira en 15 minutos                           │
│     → Un solo uso                                            │
│                                                              │
│  2. Usuario ingresa código en conector local                │
│                                                              │
│  3. Conector envía:                                          │
│     • Código de vinculación                                  │
│     • Machine fingerprint (hardware ID)                      │
│     • Versión del conector                                   │
│                                                              │
│  4. Cloud valida y retorna:                                  │
│     • JWT de larga duración (connector token)               │
│     • Refresh token                                          │
│     • Connector ID                                           │
│                                                              │
│  5. Conector almacena tokens encriptados localmente         │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### 8.2 Validación en Cada Sync

```csharp
public async Task<IResult> SyncCartera(SyncRequest request, HttpContext context)
{
    var connectorId = context.User.FindFirst("connector_id")?.Value;
    var machineId = request.MachineFingerprint;

    // Validar que el machine ID coincide con el registrado
    var connector = await _db.Connectors.FindAsync(Guid.Parse(connectorId!));
    if (connector?.MachineFingerprint != machineId)
    {
        _logger.LogWarning("Machine fingerprint mismatch for connector {Id}", connectorId);
        return Results.Unauthorized();
    }

    // Procesar sync...
}
```

---

## 9. Audit Logging

### 9.1 Eventos a Registrar

| Categoría | Eventos |
|-----------|---------|
| Auth | Login, logout, failed login, password change, MFA toggle |
| Data | Create, update, delete de entidades críticas |
| Admin | User management, role changes, settings |
| Sync | Connector registration, sync success/failure |
| Security | Rate limit hit, blocked request, suspicious activity |

### 9.2 Estructura del Log

```csharp
public record AuditLog
{
    public Guid Id { get; init; }
    public DateTime Timestamp { get; init; }
    public string Action { get; init; }        // "user.login", "cartera.export"
    public Guid? UserId { get; init; }
    public Guid? OrganizationId { get; init; }
    public string? ConnectorId { get; init; }
    public string IpAddress { get; init; }
    public string UserAgent { get; init; }
    public string? ResourceType { get; init; }  // "Cliente", "Factura"
    public string? ResourceId { get; init; }
    public string? OldValue { get; init; }      // JSON (solo para updates)
    public string? NewValue { get; init; }      // JSON (solo para updates)
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}
```

### 9.3 Retención

- Logs de autenticación: 2 años
- Logs de datos: 5 años (compliance)
- Logs de debug: 30 días

---

## 10. Checklist de Seguridad por Fase

### Pre-MVP (Obligatorio)
- [ ] HTTPS configurado
- [ ] Passwords hasheados con Argon2id
- [ ] JWT con expiración
- [ ] SQL injection prevention (EF Core)
- [ ] XSS prevention (React escaping)
- [ ] CORS configurado
- [ ] Rate limiting básico
- [ ] Security headers

### MVP
- [ ] OAuth Google implementado
- [ ] OAuth Microsoft implementado
- [ ] Refresh token rotation
- [ ] Account lockout
- [ ] Audit logging básico
- [ ] Multi-tenant isolation (RLS)

### Pre-Producción
- [ ] MFA para admins
- [ ] Penetration testing básico
- [ ] Dependency vulnerability scan
- [ ] WAF configurado (Azure Front Door)
- [ ] Backup encryption
- [ ] Incident response plan

### Producción
- [ ] Monitoreo de seguridad activo
- [ ] Alertas de actividad sospechosa
- [ ] Rotación de secretos automatizada
- [ ] Compliance review (si aplica)

---

## 11. Gestión de Secretos

### Azure Key Vault

```csharp
// En Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{vaultName}.vault.azure.net/"),
    new DefaultAzureCredential());

// Los secretos se acceden como configuración normal
var jwtKey = builder.Configuration["Jwt:Key"];
```

### Secretos Requeridos

| Secreto | Descripción |
|---------|-------------|
| `Jwt-Key` | Clave para firmar JWT |
| `Db-ConnectionString` | Connection string de PostgreSQL |
| `Redis-ConnectionString` | Connection string de Redis |
| `OAuth-Google-ClientSecret` | Secreto de Google OAuth |
| `OAuth-Microsoft-ClientSecret` | Secreto de Microsoft OAuth |
| `SendGrid-ApiKey` | API key para emails |

---

## 12. Seguridad Web y WAF (Azure Front Door)

> **IMPORTANTE:** El servicio estará online y requiere protección WAF.

### 12.1 Arquitectura de Protección

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     CAPAS DE PROTECCIÓN WEB                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  INTERNET                                                                    │
│      │                                                                       │
│      ▼                                                                       │
│  ┌───────────────────────────────────────┐                                  │
│  │        Azure Front Door (CDN)          │                                  │
│  │  • DDoS Protection (L3/L4)             │                                  │
│  │  • Geo-blocking (si necesario)         │                                  │
│  │  • SSL/TLS termination                 │                                  │
│  └───────────────────┬───────────────────┘                                  │
│                      ▼                                                       │
│  ┌───────────────────────────────────────┐                                  │
│  │        Azure WAF (Web App Firewall)    │                                  │
│  │  • OWASP Core Rule Set 3.2+           │                                  │
│  │  • Custom rules                        │                                  │
│  │  • Rate limiting                       │                                  │
│  │  • Bot protection                      │                                  │
│  └───────────────────┬───────────────────┘                                  │
│                      ▼                                                       │
│  ┌───────────────────────────────────────┐                                  │
│  │        Application (Container Apps)    │                                  │
│  │  • Application-level security          │                                  │
│  │  • Input validation                    │                                  │
│  │  • Business logic protection           │                                  │
│  └───────────────────────────────────────┘                                  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 12.2 Configuración Azure WAF

```bicep
// infrastructure/waf-policy.bicep
resource wafPolicy 'Microsoft.Network/FrontDoorWebApplicationFirewallPolicies@2022-05-01' = {
  name: 'cobranzacloud-waf'
  location: 'Global'
  properties: {
    policySettings: {
      enabledState: 'Enabled'
      mode: 'Prevention'  // Bloquear, no solo detectar
      requestBodyCheck: 'Enabled'
    }
    managedRules: {
      managedRuleSets: [
        {
          ruleSetType: 'Microsoft_DefaultRuleSet'
          ruleSetVersion: '2.1'
        }
        {
          ruleSetType: 'Microsoft_BotManagerRuleSet'
          ruleSetVersion: '1.0'
        }
      ]
    }
    customRules: {
      rules: [
        {
          name: 'RateLimitRule'
          priority: 1
          ruleType: 'RateLimitRule'
          rateLimitDurationInMinutes: 1
          rateLimitThreshold: 100
          matchConditions: [
            {
              matchVariable: 'RequestUri'
              operator: 'Contains'
              matchValue: ['/api/']
            }
          ]
          action: 'Block'
        }
        {
          name: 'BlockSuspiciousCountries'
          priority: 2
          ruleType: 'MatchRule'
          matchConditions: [
            {
              matchVariable: 'RemoteAddr'
              operator: 'GeoMatch'
              matchValue: ['RU', 'CN', 'KP']  // Ajustar según necesidad
            }
          ]
          action: 'Block'
        }
      ]
    }
  }
}
```

### 12.3 Reglas WAF Personalizadas

| Regla | Propósito | Acción |
|-------|-----------|--------|
| Rate Limit API | Prevenir abuso de API | Block si > 100 req/min |
| Rate Limit Login | Prevenir brute force | Block si > 10 req/min |
| Geo-blocking | Reducir superficie de ataque | Block países de alto riesgo |
| Bot Protection | Bloquear bots maliciosos | Block bots conocidos |
| SQL Injection | Detectar patrones SQLi | Block + Log |
| XSS | Detectar patrones XSS | Block + Log |

### 12.4 Headers de Seguridad (Next.js)

```typescript
// next.config.js
const securityHeaders = [
  {
    key: 'X-DNS-Prefetch-Control',
    value: 'on'
  },
  {
    key: 'Strict-Transport-Security',
    value: 'max-age=63072000; includeSubDomains; preload'
  },
  {
    key: 'X-XSS-Protection',
    value: '1; mode=block'
  },
  {
    key: 'X-Frame-Options',
    value: 'SAMEORIGIN'
  },
  {
    key: 'X-Content-Type-Options',
    value: 'nosniff'
  },
  {
    key: 'Referrer-Policy',
    value: 'strict-origin-when-cross-origin'
  },
  {
    key: 'Permissions-Policy',
    value: 'camera=(), microphone=(), geolocation=()'
  },
  {
    key: 'Content-Security-Policy',
    value: `
      default-src 'self';
      script-src 'self' 'unsafe-eval' 'unsafe-inline' https://accounts.google.com https://js.clerk.dev;
      style-src 'self' 'unsafe-inline';
      img-src 'self' blob: data: https:;
      font-src 'self';
      connect-src 'self' https://api.clerk.dev https://api.openai.com;
      frame-src 'self' https://accounts.google.com;
    `.replace(/\s{2,}/g, ' ').trim()
  }
];

module.exports = {
  async headers() {
    return [
      {
        source: '/:path*',
        headers: securityHeaders,
      },
    ];
  },
};
```

### 12.5 Monitoreo de Seguridad

```csharp
// Alertas de seguridad en Azure Monitor
// infrastructure/alerts.bicep
resource securityAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: 'waf-block-spike'
  properties: {
    description: 'Alert when WAF blocks spike'
    severity: 2
    enabled: true
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'WAFBlockedRequests'
          metricName: 'WebApplicationFirewallRequestCount'
          dimensions: [
            {
              name: 'Action'
              operator: 'Include'
              values: ['Block']
            }
          ]
          operator: 'GreaterThan'
          threshold: 100
          timeAggregation: 'Total'
        }
      ]
    }
    actions: [
      {
        actionGroupId: alertActionGroup.id
      }
    ]
  }
}
```

---

## 13. Respuesta a Incidentes

### Niveles de Severidad

| Nivel | Descripción | Response Time |
|-------|-------------|---------------|
| P1 | Breach confirmado, datos expuestos | Inmediato |
| P2 | Intento de breach, vulnerabilidad explotable | < 1 hora |
| P3 | Vulnerabilidad detectada, no explotada | < 24 horas |
| P4 | Mejora de seguridad recomendada | Sprint siguiente |

### Playbook Básico

```
1. DETECTAR
   └── Alertas de monitoreo, reporte de usuario

2. CONTENER
   ├── Revocar tokens comprometidos
   ├── Bloquear IPs sospechosas
   └── Aislar sistemas afectados

3. INVESTIGAR
   ├── Revisar audit logs
   ├── Identificar alcance
   └── Determinar vector de ataque

4. REMEDIAR
   ├── Parchear vulnerabilidad
   ├── Rotar secretos si necesario
   └── Restaurar desde backup si necesario

5. COMUNICAR
   ├── Notificar a usuarios afectados
   └── Documentar incidente

6. MEJORAR
   ├── Post-mortem
   └── Actualizar controles
```

---

*Documento de seguridad - Revisar trimestralmente*
