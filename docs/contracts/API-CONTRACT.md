# API Contract - Cobranza Agent Connector v1.1.19

> **Documento de contrato para consumidores de la API del Conector ASPEL SAE**
>
> **Fecha:** 2025-12-26
> **Versión API:** 1.1.19
> **Base URL Producción:** `http://bitmovil.ddns.net:5000`

---

## Información General

### Propósito
Este conector expone datos de cartera (cuentas por cobrar) desde ASPEL SAE 9.0 mediante una API REST. Es un componente **local** que se instala en el servidor donde reside la base de datos ASPEL.

### Autenticación
Todas las rutas (excepto `/api/health*`) requieren el header:

```
X-API-Key: {api_key_configurada}
```

**Respuesta sin API Key:**
```json
{
  "success": false,
  "error": {
    "code": "MISSING_API_KEY",
    "message": "Se requiere el header X-API-Key"
  }
}
```

**Respuesta con API Key inválida:**
```json
{
  "success": false,
  "error": {
    "code": "INVALID_API_KEY",
    "message": "La API Key proporcionada no es válida"
  }
}
```

### Formato de Respuestas

#### Respuesta exitosa simple:
```json
{
  "success": true,
  "data": { ... },
  "error": null,
  "message": null
}
```

#### Respuesta exitosa paginada:
```json
{
  "success": true,
  "items": [ ... ],
  "pagination": {
    "total": 100,
    "limit": 50,
    "offset": 0,
    "page": 1,
    "totalPages": 2
  }
}
```

#### Respuesta de error:
```json
{
  "success": false,
  "data": null,
  "error": "Mensaje de error",
  "message": null
}
```

---

## Sistema de Monedas

### Códigos de Moneda

| Código | Descripción | Notas |
|--------|-------------|-------|
| `1` | MXN (Pesos Mexicanos) | Moneda principal |
| `2` | USD (Dólares Americanos) | Moneda secundaria |

### Comportamiento del parámetro `?moneda=`

| Valor | Comportamiento |
|-------|----------------|
| Sin parámetro | Retorna **TODAS las monedas combinadas** (MXN + USD) |
| `?moneda=1` | Solo registros en MXN |
| `?moneda=2` | Solo registros en USD |

**IMPORTANTE:** Los montos NO se convierten entre monedas. Si consultas sin filtro de moneda, obtienes la suma aritmética de MXN + USD (sin conversión).

### Campos de moneda en respuestas

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `moneda` | `int` | Código numérico (1=MXN, 2=USD) |
| `monedaDescripcion` | `string` | Descripción textual ("MXN", "USD") |

---

## Endpoints por Categoría

### Health Check (Públicos - Sin autenticación)

#### `GET /api/health`
Verifica el estado del servicio.

**Respuesta:**
```json
{
  "status": "healthy",
  "timestamp": "2025-12-26T02:17:31Z",
  "version": "1.1.19",
  "service": "CobranzaAgent"
}
```

#### `GET /api/health/ready`
Verifica si el servicio está listo para recibir peticiones.

**Respuesta exitosa (HTTP 200):**
```json
{
  "status": "ready",
  "timestamp": "2025-12-26T02:17:31Z",
  "empresasConfiguradas": 5,
  "apiKeyConfigurada": true
}
```

**Respuesta no lista (HTTP 503):**
```json
{
  "status": "not_ready",
  "timestamp": "2025-12-26T02:17:31Z",
  "empresasConfiguradas": 0,
  "apiKeyConfigurada": true
}
```

---

### Empresas

#### `GET /api/empresas`
Lista todas las empresas ASPEL configuradas.

**Respuesta:**
```json
{
  "success": true,
  "data": [
    {
      "id": "01",
      "rfc": "ABC123456789",
      "motor": "Firebird",
      "activa": true,
      "estadoConexion": "Conectada",
      "rutaBaseDatos": "C:\\...\\SAE90EMPRE01.FDB",
      "mensajeError": null
    }
  ]
}
```

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | `string` | Identificador de empresa (01-99) |
| `rfc` | `string?` | RFC de la empresa |
| `motor` | `string` | "Firebird" o "SqlServer" |
| `activa` | `bool` | Si está habilitada |
| `estadoConexion` | `string` | "Conectada", "Desconectada", "Error" |
| `rutaBaseDatos` | `string?` | Ruta al archivo .FDB o conexión SQL |
| `mensajeError` | `string?` | Mensaje si hay error de conexión |

#### `GET /api/empresas/{empresaId}`
Obtiene detalle de una empresa específica.

**Parámetros de ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `empresaId` | `string` | ID de empresa (ej: "01", "02") |

#### `POST /api/empresas/{empresaId}/validar`
Valida la conexión a la base de datos de una empresa.

**Respuesta:**
```json
{
  "success": true,
  "data": {
    "empresaId": "01",
    "exitoso": true,
    "estado": "Conectada",
    "mensaje": "Conexión exitosa",
    "rfc": "ABC123456789"
  }
}
```

#### `GET /api/empresas/detectar`
Detecta automáticamente empresas ASPEL instaladas en el servidor.

---

### Cartera

#### `GET /api/empresas/{empresaId}/cartera/resumen`
Obtiene resumen ejecutivo de la cartera.

**Parámetros:**
| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `empresaId` | `string` | - | **Requerido.** ID de empresa |
| `moneda` | `int?` | null | Filtrar por moneda (1=MXN, 2=USD, null=todas) |

**Respuesta:**
```json
{
  "success": true,
  "data": {
    "totalCartera": 1327905.74,
    "carteraVencida": 536064.49,
    "carteraPorVencer": 791841.25,
    "totalFacturas": 46,
    "facturasVencidas": 40,
    "clientesConSaldo": 24,
    "moneda": "MXN"
  }
}
```

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `totalCartera` | `decimal` | Suma total de saldos pendientes |
| `carteraVencida` | `decimal` | Suma de saldos vencidos |
| `carteraPorVencer` | `decimal` | Suma de saldos no vencidos |
| `totalFacturas` | `int` | Cantidad de facturas con saldo |
| `facturasVencidas` | `int` | Cantidad de facturas vencidas |
| `clientesConSaldo` | `int` | Cantidad de clientes únicos con saldo |
| `moneda` | `string` | "MXN", "USD" o "MIXED" si no se filtró |

---

#### `GET /api/empresas/{empresaId}/cartera/vencida`
Lista facturas vencidas (fecha vencimiento < hoy).

**Parámetros:**
| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `empresaId` | `string` | - | **Requerido.** ID de empresa |
| `moneda` | `int?` | null | Filtrar por moneda |
| `diasMinimo` | `int?` | null | Solo facturas con >= N días vencidas |
| `limite` | `int` | 50 | Máximo de registros (1-1000) |
| `offset` | `int` | 0 | Registros a saltar (paginación) |
| `ordenarPor` | `string?` | null | Campo para ordenar |
| `direccion` | `string?` | null | "asc" o "desc" |

**Valores válidos para `ordenarPor`:**
- `diasVencido` - Por días de vencimiento
- `saldo` - Por monto de saldo
- `fechaVencimiento` - Por fecha de vencimiento
- `claveCliente` - Por clave de cliente

**Respuesta:**
```json
{
  "success": true,
  "items": [
    {
      "claveCliente": "9",
      "noFactura": "14",
      "cargo": 1392.00,
      "pagado": 0.00,
      "saldo": 1392.00,
      "fechaFactura": "2015-06-30T00:00:00",
      "fechaVencimiento": "2015-07-01T00:00:00",
      "diasVencido": 3831,
      "estaVencida": true,
      "moneda": 1,
      "monedaDescripcion": "MXN",
      "referencia": "14"
    }
  ],
  "pagination": {
    "total": 40,
    "limit": 50,
    "offset": 0,
    "page": 1,
    "totalPages": 1
  }
}
```

---

#### `GET /api/empresas/{empresaId}/cartera/pendiente`
Lista todas las facturas pendientes (vencidas y por vencer).

**Parámetros:**
| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `empresaId` | `string` | - | **Requerido.** ID de empresa |
| `moneda` | `int?` | null | Filtrar por moneda |
| `claveCliente` | `string?` | null | Filtrar por cliente específico |
| `limite` | `int` | 50 | Máximo de registros (1-1000) |
| `offset` | `int` | 0 | Registros a saltar |
| `ordenarPor` | `string?` | null | Campo para ordenar |
| `direccion` | `string?` | null | "asc" o "desc" |

**Respuesta:** Misma estructura que `/cartera/vencida`

---

#### `GET /api/empresas/{empresaId}/cartera/cliente/{claveCliente}`
Obtiene cartera detallada de un cliente específico.

**Parámetros:**
| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `empresaId` | `string` | - | **Requerido.** ID de empresa |
| `claveCliente` | `string` | - | **Requerido.** Clave del cliente |
| `moneda` | `int?` | null | Filtrar por moneda |

**Respuesta:**
```json
{
  "success": true,
  "data": {
    "cliente": {
      "clave": "20",
      "nombre": "CLIENTE EJEMPLO SA DE CV",
      "rfc": "CEJ123456789",
      "email": "contacto@ejemplo.com",
      "telefono": "555-1234567",
      "celular": "555-9876543",
      "contacto": "Juan Pérez",
      "limiteCredito": 500000.00,
      "diasCredito": 30,
      "status": "Activo",
      "emails": ["contacto@ejemplo.com", "pagos@ejemplo.com"]
    },
    "saldoTotal": 125000.50,
    "saldoVencido": 45000.00,
    "facturas": [
      {
        "claveCliente": "20",
        "noFactura": "1234",
        "cargo": 50000.00,
        "pagado": 5000.00,
        "saldo": 45000.00,
        "fechaFactura": "2025-10-15T00:00:00",
        "fechaVencimiento": "2025-11-14T00:00:00",
        "diasVencido": 42,
        "estaVencida": true,
        "moneda": 1,
        "monedaDescripcion": "MXN",
        "referencia": "1234"
      }
    ]
  }
}
```

---

#### `GET /api/empresas/{empresaId}/cartera/antiguedad`
Obtiene análisis de antigüedad de cartera (aging report).

**Parámetros:**
| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `empresaId` | `string` | - | **Requerido.** ID de empresa |
| `moneda` | `int?` | null | Filtrar por moneda |

**Respuesta:**
```json
{
  "success": true,
  "data": {
    "corriente": 791841.25,
    "rango1a30": 50000.00,
    "rango31a60": 75000.00,
    "rango61a90": 100000.00,
    "rangoMas90": 311064.49,
    "total": 1327905.74,
    "moneda": "MXN",
    "rangos": [
      { "nombre": "Corriente", "diasDesde": 0, "diasHasta": 0, "monto": 791841.25 },
      { "nombre": "1-30 días", "diasDesde": 1, "diasHasta": 30, "monto": 50000.00 },
      { "nombre": "31-60 días", "diasDesde": 31, "diasHasta": 60, "monto": 75000.00 },
      { "nombre": "61-90 días", "diasDesde": 61, "diasHasta": 90, "monto": 100000.00 },
      { "nombre": "Más de 90 días", "diasDesde": 91, "diasHasta": null, "monto": 311064.49 }
    ]
  }
}
```

---

### Clientes

#### `GET /api/empresas/{empresaId}/clientes`
Lista clientes con saldo pendiente.

**Parámetros:**
| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `empresaId` | `string` | - | **Requerido.** ID de empresa |
| `limite` | `int` | 50 | Máximo de registros |
| `offset` | `int` | 0 | Registros a saltar |
| `ordenarPor` | `string?` | null | Campo para ordenar |
| `direccion` | `string?` | null | "asc" o "desc" |

**Valores válidos para `ordenarPor`:**
- `saldoTotal` - Por saldo total
- `saldoVencido` - Por saldo vencido
- `nombre` - Por nombre de cliente
- `clave` - Por clave de cliente

**Respuesta:**
```json
{
  "success": true,
  "items": [
    {
      "clave": "226",
      "nombre": "CREACIONES IGUAZU",
      "rfc": "CIG041222UI7",
      "email": "cfdipagos@iguazu.com.mx",
      "saldoTotal": 740783.70,
      "saldoVencido": 0.00,
      "facturasPendientes": 2
    }
  ],
  "pagination": {
    "total": 21,
    "limit": 50,
    "offset": 0,
    "page": 1,
    "totalPages": 1
  }
}
```

**NOTA:** Este endpoint retorna clientes con saldo > 0 considerando TODAS las monedas combinadas. El saldo mostrado es la suma de MXN + USD sin conversión.

---

#### `GET /api/empresas/{empresaId}/clientes/{claveCliente}`
Obtiene detalle completo de un cliente.

**Respuesta:**
```json
{
  "success": true,
  "data": {
    "clave": "20",
    "nombre": "CLIENTE EJEMPLO SA DE CV",
    "rfc": "CEJ123456789",
    "email": "contacto@ejemplo.com",
    "telefono": "555-1234567",
    "celular": "555-9876543",
    "contacto": "Juan Pérez",
    "limiteCredito": 500000.00,
    "diasCredito": 30,
    "status": "Activo",
    "emails": ["contacto@ejemplo.com", "pagos@ejemplo.com"]
  }
}
```

---

#### `GET /api/empresas/{empresaId}/clientes/buscar`
Busca clientes por nombre o RFC.

**Parámetros:**
| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `empresaId` | `string` | - | **Requerido.** ID de empresa |
| `q` | `string` | - | **Requerido.** Término de búsqueda (mín 2 caracteres) |
| `limite` | `int` | 20 | Máximo de resultados |

**Respuesta:**
```json
{
  "success": true,
  "data": [
    {
      "clave": "226",
      "nombre": "CREACIONES IGUAZU",
      "rfc": "CIG041222UI7",
      "email": "cfdipagos@iguazu.com.mx",
      "saldoTotal": 740783.70,
      "saldoVencido": 0.00,
      "facturasPendientes": 2
    }
  ]
}
```

---

## Códigos HTTP

| Código | Significado | Cuándo ocurre |
|--------|-------------|---------------|
| 200 | OK | Petición exitosa |
| 400 | Bad Request | Parámetros inválidos |
| 401 | Unauthorized | API Key faltante o inválida |
| 404 | Not Found | Empresa o cliente no encontrado |
| 500 | Internal Server Error | Error del servidor |
| 503 | Service Unavailable | Servicio no listo (sin empresas configuradas) |

---

## Ejemplos de Uso con cURL

```bash
# Health check (sin autenticación)
curl "http://bitmovil.ddns.net:5000/api/health"

# Listar empresas
curl -H "X-API-Key: CobranzaAgent2025!" \
  "http://bitmovil.ddns.net:5000/api/empresas"

# Resumen de cartera (todas las monedas)
curl -H "X-API-Key: CobranzaAgent2025!" \
  "http://bitmovil.ddns.net:5000/api/empresas/01/cartera/resumen"

# Resumen de cartera (solo MXN)
curl -H "X-API-Key: CobranzaAgent2025!" \
  "http://bitmovil.ddns.net:5000/api/empresas/01/cartera/resumen?moneda=1"

# Facturas vencidas con paginación
curl -H "X-API-Key: CobranzaAgent2025!" \
  "http://bitmovil.ddns.net:5000/api/empresas/01/cartera/vencida?limite=10&offset=0&ordenarPor=diasVencido&direccion=desc"

# Cartera de un cliente específico
curl -H "X-API-Key: CobranzaAgent2025!" \
  "http://bitmovil.ddns.net:5000/api/empresas/01/cartera/cliente/20"

# Buscar clientes
curl -H "X-API-Key: CobranzaAgent2025!" \
  "http://bitmovil.ddns.net:5000/api/empresas/01/clientes/buscar?q=iguazu"
```

---

## Notas Técnicas

### Cálculo de Saldos
```
SALDO = CARGO_FACTURA - SUM(PAGOS_APLICADOS)
```

Los pagos se identifican por la **referencia** de la factura, no por el número de factura visible.

### Fechas
Todas las fechas se devuelven en formato ISO 8601: `YYYY-MM-DDTHH:mm:ss`

### Decimales
Los montos se devuelven con precisión de 2 decimales, redondeados.

### Paginación
- El límite máximo por consulta es 1000 registros
- El offset comienza en 0
- `page` se calcula automáticamente: `(offset / limit) + 1`

---

## Limitaciones Conocidas

1. **Sin filtro de moneda en `/clientes`**: El endpoint de clientes no soporta filtro por moneda, siempre retorna saldos combinados
2. **Sin conversión de moneda**: Los montos en USD y MXN se suman aritméticamente sin conversión
3. **Solo lectura**: La API es de solo lectura, no modifica datos en ASPEL

---

## Changelog

| Versión | Fecha | Cambios |
|---------|-------|---------|
| 1.1.19 | 2025-12-23 | Fix bug crítico en paginación Firebird |
| 1.1.18 | 2025-12-23 | Corrección JOIN CUEN_M/CUEN_DET |
| 1.1.17 | 2025-12-22 | Kestrel escucha en 0.0.0.0 |
