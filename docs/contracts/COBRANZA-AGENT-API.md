# Contrato API - Cobranza Agent (Conector ASPEL)

> **Versión:** 1.1.19
> **Fecha:** 2025-12-27
> **Tipo:** API REST - Solo Lectura
> **Modelo:** PULL (Cloud consume del Conector)

---

## Información de Conexión

| Campo | Valor |
|-------|-------|
| **Base URL Producción** | `http://bitmovil.ddns.net:5000` |
| **Versión API** | 1.1.19 |
| **Autenticación** | Header `X-API-Key` |
| **Formato** | JSON |

### Autenticación

Todas las peticiones (excepto `/api/health*`) requieren el header:
```
X-API-Key: {api_key}
```

---

## ENDPOINTS DISPONIBLES

### Health Check (Sin autenticación)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/health` | Estado del servicio |
| GET | `/api/health/ready` | Verifica si está listo (empresas configuradas) |

### Empresas
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/empresas` | Lista empresas configuradas |
| GET | `/api/empresas/{id}` | Detalle de empresa |
| POST | `/api/empresas/{id}/validar` | Valida conexión |
| GET | `/api/empresas/detectar` | Detecta empresas ASPEL |

### Cartera
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/empresas/{id}/cartera/resumen` | Resumen ejecutivo |
| GET | `/api/empresas/{id}/cartera/vencida` | Facturas vencidas (paginado) |
| GET | `/api/empresas/{id}/cartera/pendiente` | Todas las facturas pendientes (paginado) |
| GET | `/api/empresas/{id}/cartera/cliente/{clave}` | Cartera de un cliente |
| GET | `/api/empresas/{id}/cartera/antiguedad` | Análisis de antigüedad (aging) |

### Clientes
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/empresas/{id}/clientes` | Clientes con saldo (paginado) |
| GET | `/api/empresas/{id}/clientes/{clave}` | Detalle de cliente |
| GET | `/api/empresas/{id}/clientes/buscar?q={term}` | Buscar clientes |

---

## SISTEMA DE MONEDAS

### Códigos
| Código | Moneda |
|--------|--------|
| `1` | MXN (Pesos Mexicanos) |
| `2` | USD (Dólares Americanos) |

### Comportamiento del parámetro `?moneda=`

| Valor | Resultado |
|-------|-----------|
| Sin parámetro | **TODAS las monedas combinadas** (MXN + USD sumados sin conversión) |
| `?moneda=1` | Solo registros en MXN |
| `?moneda=2` | Solo registros en USD |

> **CRÍTICO:** Sin filtro de moneda, los montos son la suma aritmética de MXN + USD. NO hay conversión de tipo de cambio. Si necesitas montos precisos por moneda, SIEMPRE usa el parámetro `?moneda=`.

### Campos en respuestas
- `moneda`: `int` - Código (1 o 2)
- `monedaDescripcion`: `string` - "MXN" o "USD"

---

## PAGINACIÓN

Los endpoints que retornan listas soportan:

| Parámetro | Default | Descripción |
|-----------|---------|-------------|
| `limite` | 50 | Máximo 1000 |
| `offset` | 0 | Registros a saltar |
| `ordenarPor` | null | Campo para ordenar |
| `direccion` | null | "asc" o "desc" |

**Respuesta paginada:**
```json
{
  "success": true,
  "items": [...],
  "pagination": {
    "total": 100,
    "limit": 50,
    "offset": 0,
    "page": 1,
    "totalPages": 2
  }
}
```

---

## ESTRUCTURAS DE DATOS

### Factura
```json
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
```

### Cliente con Saldo
```json
{
  "clave": "226",
  "nombre": "CLIENTE EJEMPLO SA",
  "rfc": "CEJ123456789",
  "email": "contacto@ejemplo.com",
  "saldoTotal": 740783.70,
  "saldoVencido": 0.00,
  "facturasPendientes": 2
}
```

### Cliente Detalle
```json
{
  "clave": "20",
  "nombre": "CLIENTE EJEMPLO SA",
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
```

### Resumen Cartera
```json
{
  "totalCartera": 1327905.74,
  "carteraVencida": 536064.49,
  "carteraPorVencer": 791841.25,
  "totalFacturas": 46,
  "facturasVencidas": 40,
  "clientesConSaldo": 24,
  "moneda": "MXN"
}
```

### Análisis Antigüedad
```json
{
  "corriente": 791841.25,
  "rango1a30": 50000.00,
  "rango31a60": 75000.00,
  "rango61a90": 100000.00,
  "rangoMas90": 311064.49,
  "total": 1327905.74,
  "moneda": "MXN",
  "rangos": [
    {"nombre": "Corriente", "diasDesde": 0, "diasHasta": 0, "monto": 791841.25},
    {"nombre": "1-30 días", "diasDesde": 1, "diasHasta": 30, "monto": 50000.00},
    {"nombre": "31-60 días", "diasDesde": 31, "diasHasta": 60, "monto": 75000.00},
    {"nombre": "61-90 días", "diasDesde": 61, "diasHasta": 90, "monto": 100000.00},
    {"nombre": "Más de 90 días", "diasDesde": 91, "diasHasta": null, "monto": 311064.49}
  ]
}
```

---

## CÓDIGOS HTTP

| Código | Significado |
|--------|-------------|
| 200 | Éxito |
| 400 | Parámetros inválidos |
| 401 | API Key faltante/inválida |
| 404 | Recurso no encontrado |
| 500 | Error del servidor |
| 503 | Servicio no listo |

---

## EJEMPLOS DE CONSUMO

### Obtener resumen de cartera en MXN
```bash
curl -H "X-API-Key: {api_key}" \
  "http://bitmovil.ddns.net:5000/api/empresas/01/cartera/resumen?moneda=1"
```

### Obtener facturas vencidas ordenadas por monto
```bash
curl -H "X-API-Key: {api_key}" \
  "http://bitmovil.ddns.net:5000/api/empresas/01/cartera/vencida?moneda=1&limite=20&ordenarPor=saldo&direccion=desc"
```

### Obtener cartera de cliente específico
```bash
curl -H "X-API-Key: {api_key}" \
  "http://bitmovil.ddns.net:5000/api/empresas/01/cartera/cliente/20?moneda=1"
```

### Buscar clientes
```bash
curl -H "X-API-Key: {api_key}" \
  "http://bitmovil.ddns.net:5000/api/empresas/01/clientes/buscar?q=ejemplo&limite=10"
```

---

## LIMITACIONES

1. **Sin filtro de moneda en `/clientes`**: El endpoint de lista de clientes NO soporta filtro por moneda. Los saldos mostrados son la combinación de MXN + USD.

2. **Sin conversión de tipo de cambio**: La API NO convierte entre monedas. Si un cliente tiene facturas en MXN y USD, debes consultarlas por separado.

3. **Solo lectura**: La API es de SOLO LECTURA. No puedes crear, modificar ni eliminar datos.

4. **Empresa requerida**: Todos los endpoints de cartera y clientes requieren `{empresaId}` en la ruta. Generalmente es "01" para la empresa principal.

5. **Paginación máxima**: El límite máximo por consulta es 1000 registros.

---

## FLUJO RECOMENDADO DE INTEGRACIÓN

```
1. GET /api/health/ready
   └── Verificar disponibilidad

2. GET /api/empresas
   └── Obtener lista de empresas

3. Para cada empresa activa:
   ├── GET /api/empresas/{id}/cartera/resumen?moneda=1
   ├── GET /api/empresas/{id}/cartera/resumen?moneda=2
   ├── GET /api/empresas/{id}/cartera/antiguedad?moneda=1
   └── GET /api/empresas/{id}/clientes
       └── Para cada cliente con saldo:
           └── GET /api/empresas/{id}/cartera/cliente/{clave}?moneda=1
```

---

## CONSIDERACIONES DE RENDIMIENTO

- Las consultas a `/cartera/vencida` y `/cartera/pendiente` pueden ser lentas si hay muchos registros
- Usa paginación con límites razonables (50-100)
- Cachea el resumen de cartera si no necesitas datos en tiempo real
- Los endpoints de health son rápidos y no requieren autenticación

---

*Documento generado desde Cobranza Agent v1.1.19*
