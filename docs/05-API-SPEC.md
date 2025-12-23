# Especificación de API

> **Versión:** 1.0
> **Fecha:** 2025-12-22
> **Estado:** Definición
> **Base URL:** `https://api.cobranzacloud.com/v1`

---

## 1. Información General

### 1.1 Convenciones

| Aspecto | Convención |
|---------|------------|
| Formato | JSON (application/json) |
| Encoding | UTF-8 |
| Fechas | ISO 8601 (2025-12-22T15:30:00Z) |
| Moneda | Decimal con 2 decimales |
| IDs | UUID v4 |
| Paginación | Cursor-based o offset-based |
| Versionado | URL path (/v1/) |

### 1.2 Autenticación

```
Authorization: Bearer <jwt_token>
```

Para conectores:
```
Authorization: Bearer <connector_jwt>
X-Connector-Id: <connector_uuid>
```

### 1.3 Rate Limiting

| Endpoint | Límite |
|----------|--------|
| `/auth/login` | 5/15min por IP |
| `/auth/*` | 20/min por IP |
| `/sync/*` | 60/min por conector |
| Otros | 100/min por usuario |

Headers de respuesta:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1703263200
```

### 1.4 Respuestas de Error

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "El campo email es requerido",
    "details": [
      {
        "field": "email",
        "message": "Este campo es requerido"
      }
    ],
    "requestId": "req_abc123"
  }
}
```

Códigos HTTP:
| Código | Uso |
|--------|-----|
| 200 | Éxito |
| 201 | Creado |
| 204 | Sin contenido (delete) |
| 400 | Error de validación |
| 401 | No autenticado |
| 403 | No autorizado |
| 404 | No encontrado |
| 409 | Conflicto |
| 422 | Entidad no procesable |
| 429 | Rate limit excedido |
| 500 | Error interno |

---

## 2. Endpoints de Autenticación

### 2.1 Registro

```http
POST /auth/register
```

**Request:**
```json
{
  "email": "usuario@empresa.com",
  "password": "SecurePass123!",
  "nombre": "Juan García",
  "organizacion": {
    "nombre": "Mi Empresa S.A.",
    "rfc": "XAXX010101000"
  }
}
```

**Response (201):**
```json
{
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "usuario@empresa.com",
    "nombre": "Juan García",
    "role": "owner"
  },
  "organization": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "nombre": "Mi Empresa S.A."
  },
  "tokens": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2...",
    "expiresIn": 900
  }
}
```

---

### 2.2 Login

```http
POST /auth/login
```

**Request:**
```json
{
  "email": "usuario@empresa.com",
  "password": "SecurePass123!"
}
```

**Response (200):**
```json
{
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "usuario@empresa.com",
    "nombre": "Juan García",
    "role": "admin",
    "organizationId": "550e8400-e29b-41d4-a716-446655440001"
  },
  "tokens": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2...",
    "expiresIn": 900
  }
}
```

---

### 2.3 OAuth

```http
POST /auth/oauth/google
POST /auth/oauth/microsoft
```

**Request:**
```json
{
  "code": "oauth_authorization_code",
  "redirectUri": "https://app.cobranzacloud.com/auth/callback"
}
```

**Response:** Igual que login.

---

### 2.4 Refresh Token

```http
POST /auth/refresh
```

**Request:**
```json
{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2..."
}
```

**Response (200):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "bmV3IHJlZnJlc2ggdG9rZW4...",
  "expiresIn": 900
}
```

---

### 2.5 Logout

```http
POST /auth/logout
Authorization: Bearer <token>
```

**Response (204):** Sin contenido.

---

### 2.6 Usuario Actual

```http
GET /auth/me
Authorization: Bearer <token>
```

**Response (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "usuario@empresa.com",
  "nombre": "Juan García",
  "role": "admin",
  "organization": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "nombre": "Mi Empresa S.A.",
    "plan": "professional"
  },
  "permissions": ["cartera:read", "cartera:write", "clientes:read"]
}
```

---

## 3. Endpoints de Organización

### 3.1 Obtener Organización

```http
GET /organizations/current
Authorization: Bearer <token>
```

**Response (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "nombre": "Mi Empresa S.A.",
  "rfc": "XAXX010101000",
  "plan": "professional",
  "createdAt": "2025-12-22T10:00:00Z",
  "settings": {
    "timezone": "America/Mexico_City",
    "currency": "MXN",
    "emailDomain": "empresa.com"
  },
  "stats": {
    "usersCount": 5,
    "connectorsCount": 2,
    "lastSyncAt": "2025-12-22T14:30:00Z"
  }
}
```

---

### 3.2 Actualizar Organización

```http
PATCH /organizations/current
Authorization: Bearer <token>
```

**Request:**
```json
{
  "nombre": "Mi Empresa Actualizada S.A.",
  "settings": {
    "timezone": "America/Mexico_City"
  }
}
```

---

## 4. Endpoints de Conectores

### 4.1 Listar Conectores

```http
GET /connectors
Authorization: Bearer <token>
```

**Response (200):**
```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440010",
      "nombre": "Servidor Principal",
      "tipo": "aspel-sae",
      "version": "2.0.0",
      "status": "online",
      "lastHeartbeat": "2025-12-22T15:28:00Z",
      "lastSync": "2025-12-22T15:15:00Z",
      "empresas": [
        { "id": "01", "nombre": "Empresa Principal" },
        { "id": "02", "nombre": "Sucursal Norte" }
      ]
    }
  ],
  "meta": {
    "total": 1
  }
}
```

---

### 4.2 Generar Código de Vinculación

```http
POST /connectors/link-code
Authorization: Bearer <token>
```

**Response (201):**
```json
{
  "code": "A1B2C3",
  "expiresAt": "2025-12-22T15:45:00Z"
}
```

---

### 4.3 Registrar Conector (Llamado por el Conector)

```http
POST /connectors/register
```

**Request:**
```json
{
  "linkCode": "A1B2C3",
  "nombre": "Servidor Principal",
  "machineFingerprint": "hash_del_hardware",
  "version": "2.0.0",
  "empresas": [
    { "id": "01", "nombre": "Empresa Principal", "baseDatos": "SAE90EMPRE01" }
  ]
}
```

**Response (201):**
```json
{
  "connectorId": "550e8400-e29b-41d4-a716-446655440010",
  "tokens": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "cmVmcmVzaCB0b2tlbiBjb25uZWN0b3I...",
    "expiresIn": 86400
  },
  "syncConfig": {
    "intervalMinutes": 15,
    "endpoints": {
      "sync": "https://api.cobranzacloud.com/v1/sync",
      "heartbeat": "https://api.cobranzacloud.com/v1/connectors/heartbeat"
    }
  }
}
```

---

### 4.4 Heartbeat (Llamado por el Conector)

```http
POST /connectors/heartbeat
Authorization: Bearer <connector_token>
X-Connector-Id: <connector_uuid>
```

**Request:**
```json
{
  "status": "healthy",
  "uptime": 3600,
  "memoryUsage": 45.2,
  "lastSyncStatus": "success",
  "empresasOnline": ["01", "02"]
}
```

**Response (200):**
```json
{
  "ack": true,
  "serverTime": "2025-12-22T15:30:00Z",
  "commands": []
}
```

---

## 5. Endpoints de Sincronización

### 5.1 Sync Cartera (Llamado por el Conector)

```http
POST /sync/cartera
Authorization: Bearer <connector_token>
X-Connector-Id: <connector_uuid>
```

**Request:**
```json
{
  "empresaId": "01",
  "timestamp": "2025-12-22T15:30:00Z",
  "checksum": "sha256_de_datos",
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
        "clave": "C001",
        "nombre": "Cliente Uno S.A.",
        "rfc": "CUN010101000",
        "saldoTotal": 45230.00,
        "saldoVencido": 12500.00,
        "diasMaxVencido": 15,
        "facturas": [
          {
            "folio": "F-1001",
            "fecha": "2025-12-01",
            "vencimiento": "2025-12-15",
            "total": 12500.00,
            "saldo": 12500.00,
            "diasVencido": 7
          }
        ],
        "contactos": [
          { "nombre": "Juan Pérez", "email": "juan@clienteuno.com", "telefono": "555-1234" }
        ]
      }
    ]
  }
}
```

**Response (200):**
```json
{
  "success": true,
  "syncId": "sync_abc123",
  "processedAt": "2025-12-22T15:30:05Z",
  "stats": {
    "clientesActualizados": 26,
    "facturasActualizadas": 55,
    "nuevos": 2,
    "modificados": 23,
    "sinCambios": 30
  }
}
```

---

## 6. Endpoints de Cartera (Dashboard)

### 6.1 Resumen de Cartera

```http
GET /cartera/resumen
Authorization: Bearer <token>
```

**Query params:**
- `empresaId` (opcional): Filtrar por empresa
- `desde` (opcional): Fecha inicio
- `hasta` (opcional): Fecha fin

**Response (200):**
```json
{
  "totalCartera": 1375136.97,
  "carteraVigente": 420000.00,
  "carteraVencida": 955136.97,
  "porcentajeVencido": 69.5,
  "clientesConSaldo": 26,
  "facturasActivas": 55,
  "promedioAntiguedad": 28.5,
  "variacion": {
    "cartera": 5.2,
    "vencida": 8.1,
    "periodo": "vs mes anterior"
  },
  "lastSync": "2025-12-22T15:15:00Z"
}
```

---

### 6.2 Antigüedad de Cartera

```http
GET /cartera/antiguedad
Authorization: Bearer <token>
```

**Response (200):**
```json
{
  "rangos": [
    { "rango": "vigente", "label": "Vigente", "monto": 420000.00, "facturas": 15, "porcentaje": 30.5 },
    { "rango": "1-30", "label": "1-30 días", "monto": 380000.00, "facturas": 18, "porcentaje": 27.6 },
    { "rango": "31-60", "label": "31-60 días", "monto": 290000.00, "facturas": 12, "porcentaje": 21.1 },
    { "rango": "61-90", "label": "61-90 días", "monto": 185136.97, "facturas": 8, "porcentaje": 13.5 },
    { "rango": "90+", "label": "+90 días", "monto": 100000.00, "facturas": 2, "porcentaje": 7.3 }
  ],
  "total": 1375136.97
}
```

---

### 6.3 Facturas Vencidas

```http
GET /cartera/vencidas
Authorization: Bearer <token>
```

**Query params:**
- `page` (default: 1)
- `limit` (default: 20, max: 100)
- `sort` (default: diasVencido:desc)
- `rangoAntiguedad` (opcional): 1-30, 31-60, 61-90, 90+

**Response (200):**
```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440100",
      "folio": "F-1001",
      "cliente": {
        "id": "550e8400-e29b-41d4-a716-446655440050",
        "clave": "C001",
        "nombre": "Cliente Uno S.A."
      },
      "fecha": "2025-12-01",
      "vencimiento": "2025-12-15",
      "total": 12500.00,
      "saldo": 12500.00,
      "diasVencido": 7,
      "rangoAntiguedad": "1-30"
    }
  ],
  "meta": {
    "page": 1,
    "limit": 20,
    "total": 40,
    "totalPages": 2
  }
}
```

---

## 7. Endpoints de Clientes

### 7.1 Listar Clientes

```http
GET /clientes
Authorization: Bearer <token>
```

**Query params:**
- `page`, `limit`
- `search` (busca en clave, nombre, RFC)
- `conSaldo` (boolean): Solo con saldo pendiente
- `sort` (default: nombre:asc)

**Response (200):**
```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440050",
      "clave": "C001",
      "nombre": "Cliente Uno S.A.",
      "rfc": "CUN010101000",
      "saldoTotal": 45230.00,
      "saldoVencido": 12500.00,
      "diasMaxVencido": 15,
      "facturasActivas": 3,
      "ultimoPago": "2025-12-10"
    }
  ],
  "meta": {
    "page": 1,
    "limit": 20,
    "total": 26,
    "totalPages": 2
  }
}
```

---

### 7.2 Detalle de Cliente

```http
GET /clientes/{id}
Authorization: Bearer <token>
```

**Response (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440050",
  "clave": "C001",
  "nombre": "Cliente Uno S.A.",
  "rfc": "CUN010101000",
  "direccion": {
    "calle": "Av. Reforma 123",
    "colonia": "Centro",
    "ciudad": "CDMX",
    "cp": "06000"
  },
  "contactos": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440060",
      "nombre": "Juan Pérez",
      "email": "juan@clienteuno.com",
      "telefono": "555-1234",
      "principal": true
    }
  ],
  "resumenCartera": {
    "saldoTotal": 45230.00,
    "saldoVencido": 12500.00,
    "saldoVigente": 32730.00,
    "facturasActivas": 3,
    "diasMaxVencido": 15
  },
  "facturas": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440100",
      "folio": "F-1001",
      "fecha": "2025-12-01",
      "vencimiento": "2025-12-15",
      "total": 12500.00,
      "saldo": 12500.00,
      "diasVencido": 7,
      "status": "vencida"
    }
  ],
  "historialCobranza": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440200",
      "tipo": "email",
      "fecha": "2025-12-18T10:30:00Z",
      "asunto": "Recordatorio de pago",
      "destinatario": "juan@clienteuno.com",
      "status": "delivered"
    }
  ]
}
```

---

## 8. Endpoints de Cobranza

### 8.1 Listar Plantillas

```http
GET /cobranza/plantillas
Authorization: Bearer <token>
```

**Response (200):**
```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440300",
      "nombre": "Recordatorio 30 días",
      "asunto": "Recordatorio de pago - {empresa}",
      "cuerpo": "Estimado {cliente}, le recordamos...",
      "variables": ["cliente", "empresa", "monto", "dias", "facturas"],
      "activa": true,
      "usadaEn": 45
    }
  ]
}
```

---

### 8.2 Crear Plantilla

```http
POST /cobranza/plantillas
Authorization: Bearer <token>
```

**Request:**
```json
{
  "nombre": "Recordatorio urgente",
  "asunto": "URGENTE: Pago vencido - {monto}",
  "cuerpo": "Estimado {cliente},\n\nSu cuenta presenta un saldo vencido de {monto}...",
  "activa": true
}
```

---

### 8.3 Enviar Recordatorio

```http
POST /cobranza/recordatorios
Authorization: Bearer <token>
```

**Request:**
```json
{
  "clienteId": "550e8400-e29b-41d4-a716-446655440050",
  "plantillaId": "550e8400-e29b-41d4-a716-446655440300",
  "destinatarios": [
    { "email": "juan@clienteuno.com", "nombre": "Juan Pérez" }
  ],
  "facturasIncluir": ["550e8400-e29b-41d4-a716-446655440100"],
  "programarPara": null,
  "notas": "Primer recordatorio"
}
```

**Response (202):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440400",
  "status": "queued",
  "scheduledFor": null,
  "estimatedDelivery": "2025-12-22T15:35:00Z"
}
```

---

### 8.4 Historial de Envíos

```http
GET /cobranza/historial
Authorization: Bearer <token>
```

**Query params:**
- `clienteId` (opcional)
- `desde`, `hasta`
- `status`: queued, sent, delivered, failed

**Response (200):**
```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440400",
      "tipo": "email",
      "cliente": {
        "id": "550e8400-e29b-41d4-a716-446655440050",
        "nombre": "Cliente Uno S.A."
      },
      "destinatario": "juan@clienteuno.com",
      "asunto": "Recordatorio de pago - Mi Empresa",
      "plantilla": "Recordatorio 30 días",
      "enviadoPor": {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "nombre": "Juan García"
      },
      "fechaEnvio": "2025-12-22T15:35:00Z",
      "status": "delivered",
      "abierto": true,
      "fechaApertura": "2025-12-22T16:00:00Z"
    }
  ],
  "meta": {
    "page": 1,
    "total": 45
  }
}
```

---

## 9. Endpoints de Usuarios

### 9.1 Listar Usuarios

```http
GET /users
Authorization: Bearer <token>
```

**Response (200):**
```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "admin@empresa.com",
      "nombre": "Juan García",
      "role": "admin",
      "status": "active",
      "lastLogin": "2025-12-22T14:00:00Z",
      "createdAt": "2025-12-01T10:00:00Z"
    }
  ]
}
```

---

### 9.2 Invitar Usuario

```http
POST /users/invite
Authorization: Bearer <token>
```

**Request:**
```json
{
  "email": "nuevo@empresa.com",
  "nombre": "María López",
  "role": "collector"
}
```

**Response (201):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440500",
  "email": "nuevo@empresa.com",
  "status": "invited",
  "invitedBy": "Juan García",
  "expiresAt": "2025-12-29T15:30:00Z"
}
```

---

## 10. Webhooks (Futuro)

### 10.1 Configurar Webhook

```http
POST /webhooks
Authorization: Bearer <token>
```

**Request:**
```json
{
  "url": "https://mi-sistema.com/webhook",
  "eventos": ["cliente.vencido", "sync.completado", "pago.registrado"],
  "secret": "mi_secreto_webhook"
}
```

### 10.2 Eventos Disponibles

| Evento | Descripción |
|--------|-------------|
| `sync.completado` | Sincronización exitosa |
| `sync.fallido` | Error en sincronización |
| `cliente.vencido` | Cliente pasó a vencido |
| `factura.vencida` | Factura venció |
| `recordatorio.enviado` | Email de cobranza enviado |

---

## 11. SDK Ejemplo (TypeScript)

```typescript
// lib/api-client.ts
import { ApiClient } from '@cobranzacloud/sdk';

const api = new ApiClient({
  baseUrl: 'https://api.cobranzacloud.com/v1',
  onTokenRefresh: async (refreshToken) => {
    const tokens = await api.auth.refresh(refreshToken);
    localStorage.setItem('tokens', JSON.stringify(tokens));
    return tokens.accessToken;
  },
});

// Uso
const cartera = await api.cartera.getResumen();
const clientes = await api.clientes.list({ conSaldo: true });
await api.cobranza.enviarRecordatorio({
  clienteId: 'uuid',
  plantillaId: 'uuid',
});
```

---

*Especificación de API - Actualizar con cada endpoint nuevo*
