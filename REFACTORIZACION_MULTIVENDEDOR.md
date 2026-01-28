# Refactorización del Sistema de Reservas a Multivendedor

## Resumen Ejecutivo

Se ha refactorizado completamente el módulo de reservas para soportar arquitectura **multivendedor**. Ahora una sola reserva puede contener productos de múltiples empresas (proveedores), permitiendo que los clientes centralicen sus compras.

---

## Cambios en la Base de Datos (PostgreSQL)

### Script de Migración
**Archivo:** `database/migrations/20260128_refactor_multivendedor_reservas.sql`

### Cambios principales:

1. **Tabla `Reserva`** - Eliminación de `Empresa_Id`
   - ❌ Removido: `Empresa_Id INTEGER NOT NULL`
   - Razón: Una reserva ahora puede involucrar múltiples empresas

2. **Tabla `Detalle_Reserva`** - Agregación de `Empresa_Id`
   - ✅ Agregado: `Empresa_Id INTEGER NOT NULL`
   - Propósito: Identificar qué empresa proveedor suministra cada línea
   - Restricción: `FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE`

3. **Vistas SQL Creadas:**
   - `vw_reservas_cliente`: Agrupa reservas por cliente con empresas involucradas
   - `vw_reservas_empresa`: Detalles de reserva filtrados por empresa proveedora

### Para aplicar la migración:
```bash
# Conectarse a PostgreSQL y ejecutar:
psql -U usuario -d base_datos -f database/migrations/20260128_refactor_multivendedor_reservas.sql
```

---

## Cambios en Entidades C# (Domain Layer)

### 1. `Reserva.cs` - Entidad Principal
```csharp
// ❌ Eliminado
// public int Empresa_Id { get; set; }

// La empresa ahora se define en DetalleReserva
```

### 2. `DetalleReserva.cs` - Detalles de Reserva
```csharp
// ✅ Agregado
public int Empresa_Id { get; set; } // Empresa proveedora de esta línea
```

---

## Cambios en DTOs (Domain Layer)

### Archivo: `ReservaDTOs.cs`

#### Nuevos DTOs:

1. **`ReservationDetailResponse`**
   - Representa cada línea de reserva con empresa, producto y precios
   - Retorna información desglosada por proveedor

2. **`CreateReservationDetailRequest`**
   ```csharp
   public class CreateReservationDetailRequest
   {
       public int Empresa_Id { get; set; } // Empresa proveedora
       public int? Producto_Id { get; set; }
       public int Cantidad { get; set; }
       public decimal Precio_Unitario { get; set; }
       // ... más campos
   }
   ```

3. **`CreateReservationRequest` (Actualizado)**
   ```csharp
   public class CreateReservationRequest
   {
       public int Cliente_Id { get; set; }
       public DateTime Fecha_Evento { get; set; }
       // ... campos de reserva
       
       // ✨ NUEVO: Lista de detalles de múltiples empresas
       public List<CreateReservationDetailRequest> Detalles { get; set; } = new();
   }
   ```

4. **`ReservationResponse` (Actualizado)**
   ```csharp
   // ❌ Removido
   // public int Empresa_Id { get; set; }
   
   // ✨ Agregado
   public int Cantidad_Empresas { get; set; }
   public List<string>? Empresas_Involucradas { get; set; }
   public List<ReservationDetailResponse>? Detalles { get; set; }
   ```

---

## Cambios en Repositorio (Infrastructure Layer)

### Archivo: `ReservaRepository.cs`

#### Métodos Eliminados:
- ❌ `GetByEmpresaIdAsync()` - Ya no aplica a nivel de Reserva
- ❌ `GetByEstadoAsync(int empresaId, string estado)` - Dependía de Empresa_Id en tabla principal
- ❌ `GenerarCodigoReservaAsync(int empresaId)` - Ahora sin dependencia de empresa

#### Métodos Actualizados:

**`GetReservationsByClienteIdAsync()`**
- Retorna todas las reservas del cliente
- Agrupa empresas involucradas
- Usa `COUNT(DISTINCT dr.Empresa_Id)` para contar proveedores

**`GetReservationsByEmpresaIdAsync()`**
- Filtra por `Detalle_Reserva.Empresa_Id` en lugar de `Reserva.Empresa_Id`
- Ahora muestra solo ítems que pertenecen a esa empresa

#### Métodos Nuevos:

**`GetReservationDetailsAsync(int reservaId)`**
- Retorna detalles desglosados por empresa
- Incluye información de productos y activos

**`GetEmpresasInvolucradasAsync(int reservaId)`**
- Obtiene lista de empresas que participan en la reserva

**`GenerarCodigoReservaAsync()`** (Sin parámetro empresaId)
- Formato: `RES-{AÑO}-{SECUENCIAL:D6}`
- Ej: `RES-26-000001`

---

## Cambios en Controlador (API Layer)

### Archivo: `ReservationsController.cs` (Refactorizado)

#### Nuevo Endpoint Mejorado: `GET /api/reservations/mine`

**Comportamiento Según Rol:**

1. **Si es Cliente:**
   ```json
   GET /api/reservations/mine
   
   Response:
   {
     "success": true,
     "data": [
       {
         "id": 1,
         "codigo_reserva": "RES-26-000001",
         "cantidad_empresas": 3,
         "empresas_involucradas": ["Empresa A", "Empresa B", "Empresa C"],
         "total": 5000000,
         "estado": "Confirmado"
       }
     ]
   }
   ```

2. **Si es Empresa (Proveedor):**
   ```json
   GET /api/reservations/mine
   
   Response:
   {
     "success": true,
     "data": [
       {
         "id": 1,
         "codigo_reserva": "RES-26-000001",
         "cliente_nombre": "Cliente XYZ",
         "detalles": [
           {
             "empresa_nombre": "Mi Empresa",
             "producto_nombre": "Silla",
             "cantidad": 10,
             "subtotal": 500000
           }
         ]
       }
     ]
   }
   ```

#### Nuevo Endpoint: `GET /api/reservations/{id}` (Mejorado)

Retorna detalles completos con desglose por empresa:
```json
{
  "success": true,
  "reserva": { ... },
  "detalles": [
    {
      "id": 101,
      "empresa_nombre": "Empresa A",
      "producto_nombre": "Mesa",
      "cantidad": 5,
      "precio_unitario": 100000,
      "subtotal": 500000
    },
    {
      "id": 102,
      "empresa_nombre": "Empresa B",
      "producto_nombre": "Silla",
      "cantidad": 20,
      "precio_unitario": 50000,
      "subtotal": 1000000
    }
  ],
  "empresas_involucradas": ["Empresa A", "Empresa B"]
}
```

#### Nuevo Endpoint: `POST /api/reservations` (Multivendedor)

**Crear reserva con productos de múltiples empresas:**
```json
{
  "cliente_id": 5,
  "fecha_evento": "2026-02-15",
  "direccion_entrega": "Calle Principal 123",
  "detalles": [
    {
      "empresa_id": 1,
      "producto_id": 10,
      "cantidad": 5,
      "precio_unitario": 100000
    },
    {
      "empresa_id": 2,
      "producto_id": 20,
      "cantidad": 10,
      "precio_unitario": 50000
    },
    {
      "empresa_id": 3,
      "activo_id": 5,
      "cantidad": 2,
      "precio_unitario": 500000
    }
  ]
}
```

**Respuesta:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "codigo_reserva": "RES-26-000001",
    "estado": "Solicitado",
    "total": 1600000,
    "cantidad_empresas": 3,
    "empresas_involucradas": ["Empresa 1", "Empresa 2", "Empresa 3"]
  },
  "message": "Reserva multivendedor creada exitosamente"
}
```

#### Validaciones de Seguridad:

1. **Cliente:**
   - ✅ Puede ver TODAS sus reservas (sin límite de empresas)
   - ✅ Puede crear reservas con múltiples proveedores
   - ❌ No puede ver reservas de otros clientes

2. **Empresa (Proveedor):**
   - ✅ Solo ve las reservas donde aparece como proveedor (en Detalle_Reserva)
   - ✅ Solo ve los detalles que le pertenecen
   - ❌ No ve detalles de otros proveedores en la misma reserva (privacidad)

3. **SuperAdmin:**
   - ✅ Acceso a todo

---

## Flujo de Creación de Reserva (Multivendedor)

```
1. Cliente inicia creación de reserva
2. Selecciona múltiples proveedores y productos
3. POST /api/reservations con lista de detalles
4. Sistema:
   ├─ Valida cliente existe
   ├─ Genera código único (RES-26-000001)
   ├─ Crea Reserva base (Cliente_Id, Fecha_Evento, etc.)
   ├─ Para cada detalle:
   │  ├─ Valida empresa existe y está activa
   │  ├─ Crea DetalleReserva (con Empresa_Id)
   │  └─ Suma subtotales
   ├─ Actualiza Reserva.Total = suma de todos los detalles
   └─ Retorna Reserva con detalles
```

---

## Impacto en Consultas

### Consultas Antiguas (Monoproveedor):
```sql
SELECT * FROM Reserva 
WHERE Empresa_Id = 1 AND Cliente_Id = 5
```

### Nuevas Consultas (Multivendedor):
```sql
-- Reservas de un cliente (todas, sin importar empresa)
SELECT DISTINCT r.* 
FROM Reserva r
LEFT JOIN Detalle_Reserva dr ON r.Id = dr.Reserva_Id
WHERE r.Cliente_Id = 5

-- Detalles que pertenecen a una empresa
SELECT * FROM Detalle_Reserva
WHERE Empresa_Id = 1 AND Reserva_Id = 5
```

---

## Migraciones de Datos

Para clientes existentes, los datos se migran automáticamente con el script SQL:
```sql
UPDATE Detalle_Reserva dr
SET Empresa_Id = r.Empresa_Id
FROM Reserva r
WHERE dr.Reserva_Id = r.Id AND dr.Empresa_Id IS NULL;
```

---

## Testing Recomendado

1. **Test: Crear Reserva Multivendedor**
   - Crear reserva con 3 empresas diferentes
   - Verificar que se creen 3 detalles asociados

2. **Test: Cliente Ve Todas sus Reservas**
   - Login como cliente
   - Verificar que GET /mine retorna todas las reservas

3. **Test: Empresa Solo Ve sus Detalles**
   - Login como empresa A
   - Crear reserva multivendedor (A, B, C)
   - Verificar que empresa A solo ve su detalle

4. **Test: Estadísticas por Empresa**
   - GET /stats como empresa
   - Debe sumar solo sus detalles

---

## Compatibilidad hacia Atrás

- ✅ Las reservas antiguas siguen funcionando (Empresa_Id migrado a Detalle_Reserva)
- ✅ Los endpoints antiguos siguen disponibles
- ⚠️ Aplicaciones cliente deben adaptarse a nueva estructura de detalles

---

## Pasos para Aplicar en Producción

```bash
# 1. Backup
pg_dump -U usuario -d eventconnect > backup_before_migration.sql

# 2. Aplicar migración
psql -U usuario -d eventconnect -f database/migrations/20260128_refactor_multivendedor_reservas.sql

# 3. Compilar proyecto .NET
cd EventConnect.API
dotnet build

# 4. Ejecutar pruebas
dotnet test

# 5. Desplegar
dotnet publish -c Release
```

---

## Documentación API Completa

Consulta los comentarios en `ReservationsController.cs` para documentación de endpoints con ejemplos curl y respuestas detalladas.
