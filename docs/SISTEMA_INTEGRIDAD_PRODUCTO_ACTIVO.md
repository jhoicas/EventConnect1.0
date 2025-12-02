# Sistema de Integridad Producto-Activo

## 📋 Descripción General

EventConnect utiliza un **sistema híbrido de inventario** que combina:

- **Producto**: Stock genérico (ej: "50 sillas disponibles")
- **Activo**: Ítems individuales con QR/RFID (ej: "Silla #45 con código QR-12345")

Este sistema garantiza que un activo específico solo se pueda reservar bajo su producto padre correcto, evitando errores como reservar la "Silla #45" clasificándola erróneamente como "Mesa Redonda".

---

## 🎯 Problema que Resuelve

### Sin Validación de Integridad

```sql
-- ❌ ERROR: Reservar activo con producto incorrecto
INSERT INTO detalle_reserva (Reserva_Id, Producto_Id, Activo_Id, Cantidad, Precio_Unitario, Subtotal)
VALUES (100, 5, 123, 1, 50000, 50000);
-- Producto_Id = 5 (Mesa Redonda)
-- Activo_Id = 123 (Silla Napoleón #45 que pertenece a Producto_Id = 2)
-- ❌ Inconsistencia: El activo no pertenece al producto declarado
```

### Con Sistema de Integridad

```sql
-- ✅ Trigger automático detecta y previene el error
INSERT INTO detalle_reserva (Reserva_Id, Producto_Id, Activo_Id, Cantidad, Precio_Unitario, Subtotal)
VALUES (100, 5, 123, 1, 50000, 50000);

-- ERROR 1644 (45000): Integridad violada: El Activo ID 123 pertenece al 
-- Producto ID 2, pero se intentó asociar con Producto ID 5

-- ✅ Auto-completado: Si solo especificas el Activo_Id
INSERT INTO detalle_reserva (Reserva_Id, Activo_Id, Cantidad, Precio_Unitario, Subtotal)
VALUES (100, 123, 1, 50000, 50000);
-- El trigger auto-completa Producto_Id = 2 automáticamente
```

---

## 🏗️ Arquitectura de la Solución

### 1. Triggers MySQL (Base de Datos)

**Archivo**: `database/trigger_integridad_detalle_reserva.sql`

#### Trigger BEFORE INSERT
- Valida que el `Activo_Id` pertenezca al `Producto_Id` especificado
- Auto-completa el `Producto_Id` si solo se especifica `Activo_Id`
- Verifica que el activo esté disponible (`Estado_Disponibilidad = 'Disponible'`)
- Bloquea la inserción si hay inconsistencias

#### Trigger BEFORE UPDATE
- Aplica las mismas validaciones en actualizaciones
- Previene que se cambie un activo a un producto incompatible

#### Stored Procedure: `sp_validar_detalle_reserva`
```sql
CALL sp_validar_detalle_reserva(2, 123, @valido, @mensaje);
SELECT @valido, @mensaje;
-- Resultado: 
-- @valido = 1 (TRUE)
-- @mensaje = 'Validación exitosa'
```

#### Vista: `v_integridad_detalle_reserva`
```sql
SELECT * FROM v_integridad_detalle_reserva WHERE Estado_Integridad != 'OK';
```

Columnas:
- `Detalle_Id`: ID del detalle de reserva
- `Producto_Nombre`: Nombre del producto declarado
- `Activo_Producto_Nombre`: Nombre del producto real del activo
- `Estado_Integridad`: `'OK'`, `'INTEGRIDAD_VIOLADA'`, `'INCOMPLETO'`
- `Descripcion_Error`: Mensaje descriptivo del problema

---

### 2. Servicio de Validación Backend (C#)

**Interface**: `EventConnect.Application/Services/IDetalleReservaValidacionService.cs`

**Implementación**: `EventConnect.Application/Services/Implementation/DetalleReservaValidacionService.cs`

#### Métodos Principales

##### `ValidarProductoActivoAsync(productoId, activoId)`
Valida la integridad Producto-Activo antes de guardar.

```csharp
var (esValido, mensaje, productoIdReal) = await _validacionService
    .ValidarProductoActivoAsync(productoId: 5, activoId: 123);

// Resultado:
// esValido = false
// mensaje = "Integridad violada: El Activo 'QR-12345' pertenece al producto 'Silla Napoleón' (ID 2)..."
// productoIdReal = 2
```

##### `ValidarYNormalizarDetalleAsync(detalle)`
Valida y normaliza un `DetalleReserva` completo antes de guardarlo.

```csharp
var (esValido, mensaje, detalleNormalizado) = await _validacionService
    .ValidarYNormalizarDetalleAsync(detalle);

if (!esValido) {
    return BadRequest(new { message = mensaje });
}

// Auto-completa Producto_Id si solo se especificó Activo_Id
// Valida subtotales
// Verifica disponibilidad
```

##### `ObtenerActivosDisponiblesAsync(productoId)`
Obtiene lista de activos disponibles de un producto.

```csharp
var activos = await _validacionService.ObtenerActivosDisponiblesAsync(productoId: 2);
// Retorna: List<Activo> con Estado_Disponibilidad = 'Disponible'
```

##### `ContarActivosDisponiblesAsync(productoId)`
Cuenta cuántos activos disponibles tiene un producto.

```csharp
var count = await _validacionService.ContarActivosDisponiblesAsync(productoId: 2);
// Retorna: 15 (sillas disponibles)
```

---

### 3. API REST (Controller)

**Controller**: `EventConnect.API/Controllers/DetalleReservaController.cs`

#### Endpoints

##### POST `/api/DetalleReserva` - Crear Detalle
Crea un detalle de reserva con validación automática.

**Request Body**:
```json
{
  "reserva_Id": 100,
  "activo_Id": 123,
  "cantidad": 1,
  "precio_Unitario": 50000,
  "subtotal": 50000,
  "dias_Alquiler": 1
}
```

**Response** (Success):
```json
{
  "id": 456,
  "reserva_Id": 100,
  "producto_Id": 2,      // ✅ Auto-completado
  "activo_Id": 123,
  "cantidad": 1,
  "precio_Unitario": 50000,
  "subtotal": 50000,
  "dias_Alquiler": 1,
  "estado_Item": "OK",
  "fecha_Creacion": "2025-11-26T13:00:00"
}
```

**Response** (Error):
```json
{
  "message": "Integridad violada: El Activo ID 123 pertenece al Producto ID 2, pero se intentó asociar con Producto ID 5"
}
```

##### POST `/api/DetalleReserva/validar` - Validar sin Guardar
Valida integridad sin crear el registro.

**Request Body**:
```json
{
  "productoId": 5,
  "activoId": 123
}
```

**Response**:
```json
{
  "esValido": false,
  "mensaje": "Integridad violada...",
  "productoIdReal": 2,
  "autoCompletado": false
}
```

##### GET `/api/DetalleReserva/producto/{productoId}/activos-disponibles`
Obtiene activos disponibles de un producto.

**Response**:
```json
{
  "productoId": 2,
  "cantidadDisponible": 15,
  "activos": [
    {
      "id": 123,
      "codigo_Activo": "QR-12345",
      "numero_Serie": "SN-001",
      "estado_Fisico": "Excelente",
      "estado_Disponibilidad": "Disponible"
    },
    // ... más activos
  ]
}
```

##### GET `/api/DetalleReserva/integridad/problemas` [ADMIN]
Lista todos los detalles con problemas de integridad.

**Authorization**: `SuperAdmin`, `Admin-Proveedor`

**Response**:
```json
[
  {
    "detalle_Id": 789,
    "reserva_Id": 100,
    "codigo_Reserva": "RES-20251126-1234",
    "producto_Id": 5,
    "producto_Nombre": "Mesa Redonda",
    "activo_Id": 123,
    "codigo_Activo": "QR-12345",
    "activo_Producto_Real": 2,
    "activo_Producto_Nombre": "Silla Napoleón",
    "estado_Integridad": "INTEGRIDAD_VIOLADA",
    "descripcion_Error": "Producto declarado: Mesa Redonda pero Activo pertenece a: Silla Napoleón"
  }
]
```

##### POST `/api/DetalleReserva/integridad/corregir` [SUPERADMIN]
Corrige automáticamente todos los problemas de integridad.

**Authorization**: `SuperAdmin` only

**Response**:
```json
{
  "message": "Corrección completada",
  "registrosCorregidos": 5
}
```

⚠️ **Advertencia**: Esta operación actualiza masivamente registros. Se recomienda hacer backup antes de ejecutar.

---

## 📊 Casos de Uso

### Caso 1: Reserva Genérica de Producto (Stock)

```json
POST /api/DetalleReserva
{
  "reserva_Id": 100,
  "producto_Id": 2,      // Sillas Napoleón
  "cantidad": 10,        // 10 sillas genéricas
  "precio_Unitario": 5000,
  "subtotal": 50000,
  "dias_Alquiler": 1
}
```

✅ **Resultado**: Reserva 10 sillas del stock general (no activos específicos)

---

### Caso 2: Reserva de Activo Específico

```json
POST /api/DetalleReserva
{
  "reserva_Id": 100,
  "activo_Id": 123,      // Silla específica con QR
  "cantidad": 1,         // Siempre 1 para activos específicos
  "precio_Unitario": 5000,
  "subtotal": 5000,
  "dias_Alquiler": 1
}
```

✅ **Resultado**: 
- Sistema auto-completa `producto_Id = 2` (Sillas Napoleón)
- Valida que cantidad sea 1
- Verifica disponibilidad del activo

---

### Caso 3: Reserva con Error de Integridad

```json
POST /api/DetalleReserva
{
  "reserva_Id": 100,
  "producto_Id": 5,      // ❌ Mesa Redonda
  "activo_Id": 123,      // Silla Napoleón (Producto_Id = 2)
  "cantidad": 1,
  "precio_Unitario": 5000,
  "subtotal": 5000
}
```

❌ **Resultado**: HTTP 400 Bad Request
```json
{
  "message": "Integridad violada: El Activo 'QR-12345' pertenece al producto 'Silla Napoleón' (ID 2), pero se intentó asociar con 'Mesa Redonda' (ID 5)"
}
```

---

### Caso 4: Validación Previa (Sin Guardar)

```typescript
// Frontend TypeScript/React
const validarSeleccion = async (productoId: number, activoId: number) => {
  const response = await fetch('/api/DetalleReserva/validar', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ productoId, activoId })
  });
  
  const resultado = await response.json();
  
  if (!resultado.esValido) {
    alert(resultado.mensaje); // Mostrar error al usuario
    return false;
  }
  
  return true;
};
```

---

## 🔧 Instalación y Configuración

### 1. Ejecutar Triggers en MySQL

```bash
mysql -u root -p1234 db_eventconnect < database/trigger_integridad_detalle_reserva.sql
```

### 2. Verificar Instalación

```sql
-- Ver triggers creados
SHOW TRIGGERS WHERE `Table` = 'detalle_reserva';

-- Probar stored procedure
CALL sp_validar_detalle_reserva(2, 123, @valido, @mensaje);
SELECT @valido AS Es_Valido, @mensaje AS Mensaje;

-- Ver vista de integridad
SELECT * FROM v_integridad_detalle_reserva LIMIT 10;
```

### 3. Backend ya está configurado

Los servicios ya están registrados en `Program.cs`:
```csharp
builder.Services.AddScoped(_ => new DetalleReservaRepository(connectionString));
builder.Services.AddScoped<IDetalleReservaValidacionService, DetalleReservaValidacionService>();
```

---

## 🧪 Tests y Validación

### Test 1: Inserción con Producto Incorrecto (Debe Fallar)

```sql
-- Asume: Activo ID 123 pertenece a Producto ID 2
INSERT INTO detalle_reserva 
  (Reserva_Id, Producto_Id, Activo_Id, Cantidad, Precio_Unitario, Subtotal, Dias_Alquiler)
VALUES 
  (1, 999, 123, 1, 100.00, 100.00, 1);

-- Resultado esperado: ERROR 1644 (45000): Integridad violada
```

### Test 2: Auto-completado de Producto_Id

```sql
-- Insertar solo con Activo_Id (sin Producto_Id)
INSERT INTO detalle_reserva 
  (Reserva_Id, Activo_Id, Cantidad, Precio_Unitario, Subtotal, Dias_Alquiler)
VALUES 
  (1, 123, 1, 100.00, 100.00, 1);

-- Verificar auto-completado
SELECT * FROM detalle_reserva WHERE Activo_Id = 123;
-- Producto_Id debería estar auto-completado = 2
```

### Test 3: Verificar Integridad Actual

```sql
SELECT * FROM v_integridad_detalle_reserva 
WHERE Estado_Integridad != 'OK';

-- Si hay registros, significa que hay problemas de integridad
```

### Test 4: Corrección Masiva (Solo si hay problemas)

```sql
-- Ver cuántos registros necesitan corrección
SELECT COUNT(*) FROM detalle_reserva dr
INNER JOIN activo a ON dr.Activo_Id = a.Id
WHERE dr.Activo_Id IS NOT NULL 
  AND (dr.Producto_Id IS NULL OR dr.Producto_Id != a.Producto_Id);

-- Ejecutar corrección
UPDATE detalle_reserva dr
INNER JOIN activo a ON dr.Activo_Id = a.Id
SET dr.Producto_Id = a.Producto_Id
WHERE dr.Activo_Id IS NOT NULL 
  AND (dr.Producto_Id IS NULL OR dr.Producto_Id != a.Producto_Id);
```

---

## 🚀 Integración Frontend

### Ejemplo: Selector de Activos Disponibles

```typescript
// hooks/useActivosDisponibles.ts
import { useQuery } from '@tanstack/react-query';

export const useActivosDisponibles = (productoId: number) => {
  return useQuery({
    queryKey: ['activos-disponibles', productoId],
    queryFn: async () => {
      const response = await fetch(
        `/api/DetalleReserva/producto/${productoId}/activos-disponibles`
      );
      return response.json();
    }
  });
};

// Componente
const SelectorActivos = ({ productoId }: { productoId: number }) => {
  const { data, isLoading } = useActivosDisponibles(productoId);
  
  if (isLoading) return <Spinner />;
  
  return (
    <div>
      <p>Disponibles: {data.cantidadDisponible}</p>
      <select>
        <option value="">Stock genérico</option>
        {data.activos.map(activo => (
          <option key={activo.id} value={activo.id}>
            {activo.codigo_Activo} - {activo.estado_Fisico}
          </option>
        ))}
      </select>
    </div>
  );
};
```

---

## 📈 Ventajas del Sistema

1. **Integridad Garantizada**: Imposible reservar activo con producto incorrecto
2. **Auto-completado Inteligente**: Reduce errores de usuario
3. **Validación en 3 Capas**: Base de datos, backend y frontend
4. **Auditoría Completa**: Vista para detectar inconsistencias
5. **Corrección Masiva**: Herramienta admin para limpiar datos legacy
6. **Performance Optimizado**: Índices compuestos en columnas críticas

---

## ⚠️ Consideraciones Importantes

### Reglas de Negocio

1. **Cantidad con Activo Específico**: Si se especifica `Activo_Id`, la `Cantidad` debe ser `1`
2. **Disponibilidad**: Solo se pueden reservar activos con `Estado_Disponibilidad = 'Disponible'`
3. **Soft Delete**: Solo activos con `Activo = 1` (no eliminados) son considerados
4. **Auto-completado**: Si se omite `Producto_Id` pero se especifica `Activo_Id`, el sistema lo completa automáticamente

### Migrations y Datos Legacy

Si tienes datos existentes con problemas de integridad:

```sql
-- 1. Identificar problemas
SELECT * FROM v_integridad_detalle_reserva 
WHERE Estado_Integridad = 'INTEGRIDAD_VIOLADA';

-- 2. Corregir automáticamente
UPDATE detalle_reserva dr
INNER JOIN activo a ON dr.Activo_Id = a.Id
SET dr.Producto_Id = a.Producto_Id
WHERE dr.Activo_Id IS NOT NULL 
  AND (dr.Producto_Id IS NULL OR dr.Producto_Id != a.Producto_Id);

-- 3. Verificar corrección
SELECT COUNT(*) FROM v_integridad_detalle_reserva 
WHERE Estado_Integridad != 'OK';
-- Debe retornar 0
```

---

## 🔒 Seguridad y Permisos

### Endpoints Públicos (Autenticados)
- `POST /api/DetalleReserva` - Crear detalle
- `GET /api/DetalleReserva/{id}` - Ver detalle
- `POST /api/DetalleReserva/validar` - Validar sin guardar
- `GET /api/DetalleReserva/producto/{id}/activos-disponibles`

### Endpoints Admin
- `GET /api/DetalleReserva/integridad/problemas` - Requiere: `SuperAdmin` o `Admin-Proveedor`

### Endpoints SuperAdmin
- `POST /api/DetalleReserva/integridad/corregir` - Requiere: `SuperAdmin` únicamente

---

## 📝 Logs y Monitoreo

El sistema registra automáticamente:

```csharp
// Auto-completado
_logger.LogInformation(
    "Auto-completado Producto_Id={ProductoId} para Activo_Id={ActivoId}",
    productoIdReal.Value, 
    detalle.Activo_Id.Value);

// Validación fallida
_logger.LogWarning("Validación fallida: {Mensaje}", mensaje);

// Corrección masiva
_logger.LogWarning(
    "Corrección masiva ejecutada por usuario {UserId}. Registros corregidos: {Count}",
    userId, corregidos);
```

---

## 🎓 Resumen Ejecutivo

Este sistema garantiza la **integridad referencial** entre `Producto` (stock genérico) y `Activo` (ítem individual) mediante:

✅ **Triggers MySQL** - Validación automática en base de datos
✅ **Servicio Backend** - Lógica de negocio y validación programática  
✅ **API REST** - Endpoints para validación y corrección
✅ **Vista de Auditoría** - Monitoreo de inconsistencias
✅ **Corrección Masiva** - Herramienta admin para datos legacy

**Resultado**: Imposible reservar "Silla #45" como "Mesa Redonda" ✨
