# Protocolo de Sincronización Conector ↔ Cloud

> **Versión:** 1.0
> **Fecha:** 2025-12-22
> **Estado:** Definición
> **Referencia:** MEMORIA-TRASLADO-CLOUD.md

---

## 1. Visión General

### 1.1 Modelo de Comunicación

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    OUTBOUND SYNC PATTERN                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────────────┐                      ┌──────────────────┐         │
│  │   CONECTOR       │                      │     CLOUD        │         │
│  │   (Iniciador)    │                      │   (Receptor)     │         │
│  └────────┬─────────┘                      └────────┬─────────┘         │
│           │                                         │                    │
│           │  ──────── HTTPS (443) ────────────────▶│                    │
│           │                                         │                    │
│           │  El conector SIEMPRE inicia la conexión│                    │
│           │  Cloud NUNCA llama al conector         │                    │
│           │                                         │                    │
└───────────┴─────────────────────────────────────────┴────────────────────┘
```

### 1.2 ¿Por qué Outbound Sync?

| Aspecto | Inbound (Cloud→Conector) | Outbound (Conector→Cloud) |
|---------|-------------------------|---------------------------|
| Firewall | Requiere abrir puerto | ✅ Solo HTTPS saliente |
| NAT | Requiere port forwarding | ✅ Funciona sin config |
| IP | Requiere IP fija o DDNS | ✅ IP dinámica OK |
| Seguridad | Superficie de ataque mayor | ✅ Mínima exposición |
| Complejidad | Alta para cliente | ✅ Plug and play |

---

## 2. Componentes del Conector v2.0

### 2.1 Arquitectura Interna

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        CONECTOR v2.0                                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────────┐   ┌─────────────────┐   ┌─────────────────┐       │
│  │   CloudClient   │   │   SyncEngine    │   │ ChangeDetector  │       │
│  │                 │   │                 │   │                 │       │
│  │  • HTTP Client  │◀──│  • Timer        │◀──│  • Checksums    │       │
│  │  • JWT Auth     │   │  • Orchestrator │   │  • Delta detect │       │
│  │  • Retry/Polly  │   │  • Batching     │   │  • Hash compare │       │
│  │  • Compression  │   │  • Error handle │   │                 │       │
│  └────────┬────────┘   └─────────────────┘   └────────┬────────┘       │
│           │                                           │                 │
│           │                                           │                 │
│  ┌────────▼────────┐                        ┌────────▼────────┐        │
│  │   StateStore    │                        │    ASPEL SAE    │        │
│  │                 │                        │   (Firebird/    │        │
│  │  • JWT tokens   │                        │    SQL Server)  │        │
│  │  • Last sync    │                        │                 │        │
│  │  • Checksums    │                        │                 │        │
│  │  • Encrypted    │                        │                 │        │
│  └─────────────────┘                        └─────────────────┘        │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Responsabilidades

| Componente | Responsabilidad |
|------------|-----------------|
| **CloudClient** | Comunicación HTTP con cloud, manejo de auth |
| **SyncEngine** | Orquesta el ciclo de sincronización |
| **ChangeDetector** | Detecta cambios en ASPEL para sync diferencial |
| **StateStore** | Persiste estado local (tokens, checksums) |

---

## 3. Flujo de Registro

### 3.1 Secuencia de Registro

```
┌─────────┐          ┌─────────┐          ┌─────────┐          ┌─────────┐
│ Usuario │          │Dashboard│          │   API   │          │Conector │
└────┬────┘          └────┬────┘          └────┬────┘          └────┬────┘
     │                    │                    │                    │
     │ Clic "Agregar      │                    │                    │
     │ Conector"          │                    │                    │
     │───────────────────▶│                    │                    │
     │                    │                    │                    │
     │                    │ POST /connectors/  │                    │
     │                    │ link-code          │                    │
     │                    │───────────────────▶│                    │
     │                    │                    │                    │
     │                    │  { code: "A1B2C3", │                    │
     │                    │    expiresAt: ... }│                    │
     │                    │◀───────────────────│                    │
     │                    │                    │                    │
     │ Muestra código     │                    │                    │
     │ "A1B2C3"          │                    │                    │
     │◀───────────────────│                    │                    │
     │                    │                    │                    │
     │ Usuario ingresa código                  │                    │
     │ en interfaz del conector               │                    │
     │─────────────────────────────────────────────────────────────▶│
     │                    │                    │                    │
     │                    │                    │  POST /connectors/ │
     │                    │                    │  register          │
     │                    │                    │  { code, machine,  │
     │                    │                    │    version }       │
     │                    │                    │◀───────────────────│
     │                    │                    │                    │
     │                    │                    │  { connectorId,    │
     │                    │                    │    tokens }        │
     │                    │                    │───────────────────▶│
     │                    │                    │                    │
     │                    │   Webhook:         │                    │
     │                    │   connector.registered                  │
     │                    │◀───────────────────│                    │
     │                    │                    │                    │
     │ ✓ Conector         │                    │                    │
     │   vinculado        │                    │                    │
     │◀───────────────────│                    │                    │
     │                    │                    │                    │
```

### 3.2 Código de Vinculación

```json
// Generación del código
{
  "code": "A1B2C3",              // 6 caracteres alfanuméricos
  "organizationId": "uuid",      // Org que lo generó
  "createdBy": "user-uuid",      // Usuario que lo creó
  "createdAt": "2025-12-22T15:00:00Z",
  "expiresAt": "2025-12-22T15:15:00Z",  // 15 minutos TTL
  "used": false
}
```

### 3.3 Request de Registro

```json
// POST /connectors/register
{
  "linkCode": "A1B2C3",
  "nombre": "Servidor Principal",
  "machineFingerprint": "sha256:hardware_id_hash",
  "version": "2.0.0",
  "os": "Windows Server 2019",
  "empresas": [
    {
      "id": "01",
      "nombre": "Empresa Principal",
      "baseDatos": "SAE90EMPRE01",
      "tipo": "firebird"
    }
  ]
}
```

### 3.4 Response de Registro

```json
{
  "connectorId": "uuid",
  "organizationId": "uuid",
  "tokens": {
    "accessToken": "jwt...",
    "refreshToken": "refresh...",
    "expiresIn": 86400  // 24 horas
  },
  "config": {
    "syncIntervalMinutes": 15,
    "heartbeatIntervalMinutes": 5,
    "endpoints": {
      "sync": "https://api.cobranzacloud.com/v1/sync",
      "heartbeat": "https://api.cobranzacloud.com/v1/connectors/heartbeat",
      "refresh": "https://api.cobranzacloud.com/v1/auth/connector/refresh"
    },
    "features": {
      "deltaSync": true,
      "compression": true,
      "batchSize": 100
    }
  }
}
```

---

## 4. Flujo de Sincronización

### 4.1 Ciclo Normal

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     CICLO DE SINCRONIZACIÓN                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │  CADA 15 MINUTOS (configurable)                                   │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  1. SyncEngine despierta                                                 │
│     │                                                                    │
│     ▼                                                                    │
│  2. ChangeDetector consulta ASPEL                                        │
│     │  • Calcula checksums de tablas relevantes                         │
│     │  • Compara con checksums almacenados                              │
│     │                                                                    │
│     ├── Sin cambios ──▶ Skip sync, actualiza lastCheck                  │
│     │                                                                    │
│     └── Con cambios ──▶ Continúa                                        │
│                         │                                                │
│                         ▼                                                │
│  3. Extrae datos de ASPEL                                                │
│     │  • Cartera completa o delta                                       │
│     │  • Clientes con saldo                                             │
│     │  • Facturas activas                                               │
│     │                                                                    │
│     ▼                                                                    │
│  4. CloudClient envía a cloud                                            │
│     │  • POST /sync/cartera                                             │
│     │  • Compresión gzip                                                │
│     │  • JWT en header                                                  │
│     │                                                                    │
│     ├── 200 OK ──▶ Actualiza checksums, guarda timestamp                │
│     │                                                                    │
│     ├── 401 ──▶ Refresh token, reintentar                               │
│     │                                                                    │
│     └── Error ──▶ Retry con backoff exponencial                         │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Delta Sync vs Full Sync

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    TIPOS DE SINCRONIZACIÓN                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  FULL SYNC (Primera vez o forzado)                                       │
│  ─────────────────────────────────                                       │
│  • Envía TODOS los clientes con saldo                                   │
│  • Envía TODAS las facturas activas                                     │
│  • Usado en: registro, error recovery, request manual                   │
│  • Payload típico: 50-500 KB                                            │
│                                                                          │
│  DELTA SYNC (Normal)                                                     │
│  ───────────────────                                                     │
│  • Solo registros modificados desde última sync                         │
│  • Incluye: nuevos, modificados, eliminados (soft delete)              │
│  • Payload típico: 5-50 KB                                              │
│  • Más eficiente en red y procesamiento                                 │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 4.3 Payload de Sincronización

```json
// POST /sync/cartera
{
  "connectorId": "uuid",
  "empresaId": "01",
  "syncType": "delta",              // "full" | "delta"
  "timestamp": "2025-12-22T15:30:00Z",
  "previousSyncId": "sync_abc123",  // null si es full
  "checksum": "sha256:...",

  "data": {
    "resumen": {
      "totalCartera": 1375136.97,
      "carteraVigente": 420000.00,
      "carteraVencida": 955136.97,
      "clientesConSaldo": 26,
      "facturasActivas": 55
    },

    "antiguedad": [
      { "rango": "vigente", "monto": 420000.00, "facturas": 15 },
      { "rango": "1-30", "monto": 380000.00, "facturas": 18 },
      { "rango": "31-60", "monto": 290000.00, "facturas": 12 },
      { "rango": "61-90", "monto": 185136.97, "facturas": 8 },
      { "rango": "90+", "monto": 100000.00, "facturas": 2 }
    ],

    "clientes": [
      {
        "operation": "upsert",      // "upsert" | "delete"
        "clave": "C001",
        "nombre": "Cliente Uno S.A.",
        "rfc": "CUN010101000",
        "saldoTotal": 45230.00,
        "saldoVencido": 12500.00,
        "diasMaxVencido": 15,
        "contactos": [...],
        "facturas": [
          {
            "operation": "upsert",
            "folio": "F-1001",
            "fecha": "2025-12-01",
            "vencimiento": "2025-12-15",
            "total": 12500.00,
            "saldo": 12500.00,
            "diasVencido": 7
          }
        ]
      }
    ]
  }
}
```

---

## 5. Heartbeat

### 5.1 Propósito

- Indicar que el conector está online
- Reportar estado de salud
- Recibir comandos del cloud (opcional)

### 5.2 Flujo

```
Conector ────── POST /connectors/heartbeat (cada 5 min) ──────▶ Cloud
         ◀────── { ack: true, commands: [] } ─────────────────
```

### 5.3 Request

```json
// POST /connectors/heartbeat
{
  "connectorId": "uuid",
  "timestamp": "2025-12-22T15:30:00Z",
  "status": "healthy",              // "healthy" | "degraded" | "error"
  "metrics": {
    "uptime": 86400,                // segundos
    "memoryUsageMb": 128,
    "cpuPercent": 5.2,
    "lastSyncStatus": "success",
    "lastSyncDuration": 2.5,        // segundos
    "pendingRetries": 0
  },
  "empresas": [
    {
      "id": "01",
      "status": "online",
      "lastQuery": "2025-12-22T15:28:00Z"
    }
  ]
}
```

### 5.4 Response

```json
{
  "ack": true,
  "serverTime": "2025-12-22T15:30:05Z",
  "commands": [
    // Comandos opcionales del cloud
    { "type": "force_sync", "empresaId": "01" },
    { "type": "update_config", "config": { "syncIntervalMinutes": 10 } }
  ]
}
```

---

## 6. Manejo de Errores

### 6.1 Estrategia de Retry

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    EXPONENTIAL BACKOFF                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  Intento 1: Inmediato                                                    │
│  Intento 2: +2 segundos                                                  │
│  Intento 3: +4 segundos                                                  │
│  Intento 4: +8 segundos                                                  │
│  Intento 5: +16 segundos                                                 │
│  Intento 6: +32 segundos                                                 │
│  Máximo: 5 minutos entre reintentos                                      │
│                                                                          │
│  Después de 5 fallos consecutivos:                                       │
│  • Marcar sync como fallido                                              │
│  • Notificar al dashboard                                                │
│  • Reintentar en próximo ciclo normal                                   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 6.2 Códigos de Error

| Código | Significado | Acción del Conector |
|--------|-------------|---------------------|
| 200 | Éxito | Continuar normal |
| 401 | Token expirado | Refresh token, reintentar |
| 403 | Conector revocado | Mostrar error, detener sync |
| 409 | Conflicto de sync | Forzar full sync |
| 429 | Rate limit | Esperar según Retry-After |
| 500 | Error server | Retry con backoff |
| 503 | Mantenimiento | Esperar y reintentar |

### 6.3 Implementación con Polly (.NET)

```csharp
// CloudClient.cs
services.AddHttpClient<CloudClient>()
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 5,
            sleepDurationProvider: (retryAttempt, response, context) =>
            {
                // Respetar Retry-After si existe
                if (response.Result?.Headers.RetryAfter?.Delta.HasValue == true)
                    return response.Result.Headers.RetryAfter.Delta.Value;

                // Exponential backoff
                return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
            },
            onRetryAsync: async (outcome, timespan, retryAttempt, context) =>
            {
                _logger.LogWarning(
                    "Retry {Attempt} after {Delay}s due to {StatusCode}",
                    retryAttempt,
                    timespan.TotalSeconds,
                    outcome.Result?.StatusCode);
            }));
```

---

## 7. Seguridad

### 7.1 Autenticación del Conector

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    JWT PARA CONECTORES                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  Claims incluidos:                                                       │
│  {                                                                       │
│    "sub": "connector_uuid",                                              │
│    "org_id": "organization_uuid",                                        │
│    "type": "connector",                                                  │
│    "machine_id": "sha256:hardware_fingerprint",                         │
│    "version": "2.0.0",                                                   │
│    "iat": 1703263200,                                                    │
│    "exp": 1703349600                                                     │
│  }                                                                       │
│                                                                          │
│  Validaciones en cada request:                                           │
│  • Token no expirado                                                     │
│  • Conector no revocado en DB                                            │
│  • Machine fingerprint coincide                                          │
│  • Org activa y con plan válido                                          │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Machine Fingerprint

```csharp
// Generación de fingerprint único por máquina
public class MachineFingerprint
{
    public static string Generate()
    {
        var components = new[]
        {
            GetProcessorId(),
            GetMotherboardId(),
            GetMacAddress(),
            GetVolumeSerial(),
        };

        var combined = string.Join("|", components);
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
        return $"sha256:{Convert.ToHexString(hash).ToLower()}";
    }
}
```

### 7.3 Transporte Seguro

- **HTTPS obligatorio** (TLS 1.2+)
- **Certificate pinning** opcional para clientes enterprise
- **Compresión gzip** del payload
- **Timeout configurable** (default 30s)

---

## 8. Configuración del Conector

### 8.1 appsettings.json (v2.0)

```json
{
  "Agent": {
    "Puerto": 5000,
    "ApiKey": "local-api-key"
  },

  "Cloud": {
    "Enabled": true,
    "Endpoint": "https://api.cobranzacloud.com/v1",
    "SyncIntervalMinutes": 15,
    "HeartbeatIntervalMinutes": 5,
    "TimeoutSeconds": 30,
    "RetryCount": 5,
    "Compression": true
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "CloudSync": "Debug"
    }
  }
}
```

### 8.2 Modos de Operación

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    MODOS DE OPERACIÓN                                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  MODO            Cloud.Enabled   Puerto 5000   Caso de Uso              │
│  ──────────────────────────────────────────────────────────────────     │
│  Local Only      false           Activo        Desarrollo, testing      │
│  Híbrido         true            Activo        Producción normal        │
│  Cloud Only      true            Inactivo      Máxima seguridad         │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 9. Monitoreo y Diagnóstico

### 9.1 Dashboard de Conectores

```
┌─────────────────────────────────────────────────────────────────────────┐
│  CONECTORES                                                    [Refresh] │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ● Servidor Principal                                                    │
│    Status: Online                     Último heartbeat: hace 2 min      │
│    Versión: 2.0.0                    Última sync: hace 12 min          │
│    Empresas: 01 (Online), 02 (Online)                                   │
│                                                                          │
│  ○ Sucursal Norte (Offline hace 2 horas)                                │
│    Status: Offline                    Último heartbeat: hace 2h        │
│    Versión: 1.1.17                   Última sync: hace 2h 15min        │
│    [⚠️ Requiere actualización]                                          │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 9.2 Logs del Conector

```
2025-12-22 15:30:00 [INF] SyncEngine: Starting sync cycle
2025-12-22 15:30:00 [DBG] ChangeDetector: Checking for changes in empresa 01
2025-12-22 15:30:01 [DBG] ChangeDetector: Checksum changed (old: abc123, new: def456)
2025-12-22 15:30:01 [INF] SyncEngine: Delta sync required for empresa 01
2025-12-22 15:30:02 [INF] CloudClient: Sending sync payload (12.5 KB compressed)
2025-12-22 15:30:03 [INF] CloudClient: Sync successful (syncId: sync_xyz789)
2025-12-22 15:30:03 [INF] SyncEngine: Sync cycle completed in 3.2s
```

---

## 10. Checklist de Implementación

### Fase 1: Cloud MVP
- [ ] Endpoint POST /connectors/link-code
- [ ] Endpoint POST /connectors/register
- [ ] Endpoint POST /connectors/heartbeat
- [ ] Endpoint POST /sync/cartera
- [ ] Endpoint POST /auth/connector/refresh
- [ ] JWT para conectores
- [ ] Almacenamiento de cartera sincronizada

### Fase 2: Conector v2.0
- [ ] CloudClient con Polly
- [ ] SyncEngine con timer
- [ ] ChangeDetector con checksums
- [ ] StateStore encriptado
- [ ] UI para código de vinculación
- [ ] Toggle Cloud.Enabled

### Fase 3: Integración
- [ ] Tests E2E del flujo completo
- [ ] Validación de datos cloud vs ASPEL
- [ ] Pruebas de reconexión
- [ ] Pruebas de error recovery

---

*Protocolo de sincronización - Base para implementación*
