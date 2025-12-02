# Sistema de Catálogos Dinámicos - Solución al Problema de ENUMs

## 📋 Problema Original

**ENUMs en MySQL** como:
```sql
`Estado` ENUM('Disponible', 'Reservado', 'Mantenimiento', 'Baja')
```

**Desventajas:**
- ❌ Agregar un estado requiere `ALTER TABLE` (downtime en producción)
- ❌ No se puede hacer soft delete de estados
- ❌ Sin metadatos (descripción, color para UI, orden)
- ❌ Sin auditoría (¿quién creó el estado? ¿cuándo?)
- ❌ Difícil controlar permisos (¿quién puede crear estados?)

## ✅ Solución Implementada: Tablas de Catálogo

### Arquitectura Híbrida

**Para estados dinámicos (cambian frecuentemente):**
- ✅ Tablas de catálogo (`catalogo_estado_reserva`, etc.)
- ✅ Se pueden agregar estados sin ALTER TABLE
- ✅ Soft delete (campo `Activo`)
- ✅ Metadatos para UI (color, orden, descripción)

**Para datos estáticos (raramente cambian):**
- ✅ VARCHAR con validación en backend
- ✅ Más rápido para el MVP
- ✅ Ejemplo: `Tipo_Documento` (CC, NIT, Pasaporte)

---

## 🗃️ Tablas de Catálogo Creadas

### 1. **catalogo_estado_reserva**
Estados del flujo de reservas (el más crítico).

**Campos:**
- `Id`: PK auto-increment
- `Codigo`: VARCHAR(50) UNIQUE - Código interno (ej: "En_Preparacion")
- `Nombre`: VARCHAR(100) - Nombre amigable (ej: "En Preparación")
- `Descripcion`: TEXT - Descripción detallada
- `Color`: VARCHAR(20) - Color para UI (blue, green, red)
- `Orden`: INT - Orden en dropdowns
- `Activo`: BOOLEAN - Soft delete
- `Sistema`: BOOLEAN - TRUE si no se puede eliminar (estados core)
- `Fecha_Creacion`: DATETIME

**Estados iniciales:**
- Solicitado (cotización)
- Aprobado
- En_Preparacion
- Entregado
- En_Evento
- Devuelto
- Completado
- Cancelado

**Endpoint:** `GET /api/Catalogo/estados-reserva`

---

### 2. **catalogo_estado_activo**
Estados del ciclo de vida de productos/activos.

**Campos adicionales:**
- `Permite_Reserva`: BOOLEAN - Si el activo puede ser reservado en este estado

**Estados iniciales:**
- Disponible ✅ (permite reserva)
- Reservado 🔒
- En_Uso 🔄
- Mantenimiento 🔧
- Reparacion 🛠️
- **Reparacion_Externa** ⚠️ (nuevo estado sin ALTER TABLE!)
- Baja ❌
- Perdido 🚫

**Endpoint:** `GET /api/Catalogo/estados-activo`

---

### 3. **catalogo_metodo_pago**
Métodos de pago disponibles (fácil agregar Nequi, Daviplata).

**Campos adicionales:**
- `Requiere_Comprobante`: BOOLEAN
- `Requiere_Referencia`: BOOLEAN

**Métodos iniciales:**
- Efectivo
- Transferencia
- Tarjeta
- Nequi 📱
- Daviplata 📱
- PayU 💳
- Stripe 💳
- Credito 💰

**Endpoint:** `GET /api/Catalogo/metodos-pago`

---

### 4. **catalogo_tipo_mantenimiento**
Tipos de mantenimiento para activos.

**Campos adicionales:**
- `Es_Preventivo`: BOOLEAN - Distinguir preventivo vs correctivo

**Tipos iniciales:**
- Preventivo
- Correctivo
- Limpieza
- Reparacion
- Actualizacion

**Endpoint:** `GET /api/Catalogo/tipos-mantenimiento`

---

## 🚀 Cómo Usar

### Backend (.NET)

**1. Obtener catálogo activo:**
```csharp
GET /api/Catalogo/estados-reserva?soloActivos=true
```

**2. Crear nuevo estado (solo SuperAdmin):**
```csharp
POST /api/Catalogo/estados-reserva
{
    "codigo": "Reparacion_Externa",
    "nombre": "Reparación Externa",
    "descripcion": "Activo enviado a proveedor externo",
    "color": "red",
    "orden": 6,
    "activo": true
}
```

**3. Desactivar estado (soft delete):**
```csharp
DELETE /api/Catalogo/estados-reserva/8
// No elimina, solo marca Activo = false
```

**4. Validar estado en controladores:**
```csharp
// Antes (hardcoded):
if (activo.Estado != "Disponible") { ... }

// Ahora (dinámico):
var estadoValido = await _estadoActivoRepo.GetByCodigoAsync(request.Estado);
if (estadoValido == null || !estadoValido.Activo)
    return BadRequest("Estado no válido");

if (!estadoValido.Permite_Reserva)
    return BadRequest("Este activo no puede ser reservado en su estado actual");
```

---

### Frontend (React)

**1. Hook RTK Query:**
```typescript
const { data: estadosReserva } = useGetEstadosReservaQuery();
const { data: metodosP ago } = useGetMetodosPagoQuery();
```

**2. Select dinámico:**
```tsx
<Select placeholder="Seleccionar estado">
  {estadosReserva?.map(estado => (
    <option key={estado.id} value={estado.codigo}>
      {estado.nombre}
    </option>
  ))}
</Select>
```

**3. Badge con color dinámico:**
```tsx
<Badge colorScheme={estado.color}>
  {estado.nombre}
</Badge>
```

---

## 📊 Ventajas vs Desventajas

### ✅ VENTAJAS

1. **Flexibilidad Extrema**
   - Agregar "Reparación Externa" sin downtime
   - Agregar "Pago con Criptomonedas" sin ALTER TABLE

2. **Mejor UX**
   - Colores configurables para estados
   - Orden personalizable en dropdowns
   - Descripciones para ayuda contextual

3. **Control de Permisos**
   - Solo SuperAdmin puede crear estados de reserva
   - Admin puede crear métodos de pago

4. **Auditoría**
   - Fecha de creación de cada estado
   - Rastrear quién creó el estado (agregar `Creado_Por_Id` si necesario)

5. **Soft Delete**
   - No se pierden datos al "eliminar" un estado
   - Se puede reactivar después

6. **Metadatos para UI**
   - Color para badges
   - Descripción para tooltips
   - Orden para mejor experiencia

### ⚠️ DESVENTAJAS

1. **Más JOINs**
   - Necesitas JOIN con tabla de catálogo
   - Solución: Índices en `Codigo` y `Activo`
   - Impacto: Mínimo con índices correctos

2. **Validación más Compleja**
   - Antes: MySQL rechaza valores no-ENUM automáticamente
   - Ahora: Necesitas validar en backend
   - Solución: Middleware de validación

3. **Migración Compleja**
   - Si ya tienes datos en producción
   - Necesitas script de migración cuidadoso
   - Ver `migracion_enums_a_catalogos.sql`

---

## 🔄 Estrategia de Migración

### Opción 1: Nuevo Proyecto (Sin Datos)
```sql
-- Simplemente ejecutar el script
source database/migracion_enums_a_catalogos.sql;
```

### Opción 2: Proyecto con Datos Existentes

**Paso 1:** Crear tablas de catálogo
```sql
-- Ejecutar SOLO las CREATE TABLE
```

**Paso 2:** Insertar datos iniciales
```sql
-- Ejecutar SOLO los INSERT INTO
```

**Paso 3:** Verificar correspondencia
```sql
-- Verificar que todos los valores en Reserva.Estado existen en el catálogo
SELECT DISTINCT Estado 
FROM Reserva 
WHERE Estado NOT IN (SELECT Codigo FROM catalogo_estado_reserva);
```

**Paso 4:** Migrar datos huérfanos (si hay)
```sql
-- Insertar estados faltantes
INSERT INTO catalogo_estado_reserva (Codigo, Nombre, ...) 
VALUES ('Estado_Huerfano', 'Estado Huérfano', ...);
```

**Paso 5:** Agregar Foreign Keys (OPCIONAL - solo cuando estés 100% seguro)
```sql
ALTER TABLE reserva 
ADD CONSTRAINT fk_reserva_estado 
FOREIGN KEY (Estado) REFERENCES catalogo_estado_reserva(Codigo) 
ON UPDATE CASCADE ON DELETE RESTRICT;
```

---

## 🎯 Recomendaciones

### Para MVP / Startup (Tu Caso Actual)
✅ **USAR CATÁLOGOS EN:**
- Estados de reserva (cambia frecuentemente con nuevas features)
- Métodos de pago (agregarás más integraciones)
- Estados de activo (mantenimiento evoluciona)

✅ **DEJAR VARCHAR EN:**
- Tipo de documento (CC, NIT, Pasaporte - estable)
- Unidades de medida (unidad, kg, m² - estable)
- Tipos de evento (boda, cumpleaños - estable)

### Para Producción Grande
✅ **TODAS las "listas" deberían ser catálogos**
✅ **Agregar auditoría completa** (Creado_Por, Modificado_Por)
✅ **Versionado de catálogos** (si necesitas histórico)
✅ **Caché en backend** (Redis) para reducir JOINs

---

## 📚 Endpoints Disponibles

### Estados de Reserva
```
GET    /api/Catalogo/estados-reserva
POST   /api/Catalogo/estados-reserva        [SuperAdmin]
PUT    /api/Catalogo/estados-reserva/{id}   [SuperAdmin]
DELETE /api/Catalogo/estados-reserva/{id}   [SuperAdmin] (soft delete)
```

### Estados de Activo
```
GET    /api/Catalogo/estados-activo?soloPermiteReserva=true
POST   /api/Catalogo/estados-activo         [SuperAdmin]
```

### Métodos de Pago
```
GET    /api/Catalogo/metodos-pago
POST   /api/Catalogo/metodos-pago           [SuperAdmin,Admin]
PUT    /api/Catalogo/metodos-pago/{id}      [SuperAdmin,Admin]
```

### Tipos de Mantenimiento
```
GET    /api/Catalogo/tipos-mantenimiento?soloPreventivos=true
POST   /api/Catalogo/tipos-mantenimiento    [SuperAdmin,Admin]
```

---

## 🔐 Seguridad

**Permisos por Endpoint:**
- `GET` (lectura): Todos los usuarios autenticados
- `POST/PUT/DELETE`: Solo **SuperAdmin** para estados críticos
- `POST/PUT/DELETE`: **SuperAdmin + Admin** para métodos de pago

**Estados Protegidos:**
- Estados con `Sistema = TRUE` no se pueden eliminar
- Solo se pueden desactivar con `Activo = FALSE`
- El código de estados del sistema no se puede cambiar

---

## 🎨 UI con Catálogos

**Badge con color dinámico:**
```tsx
{/* Antes (hardcoded) */}
<Badge colorScheme={estado === 'Aprobado' ? 'green' : 'yellow'}>
  {estado}
</Badge>

{/* Ahora (dinámico) */}
<Badge colorScheme={estadoCatalogo?.color}>
  {estadoCatalogo?.nombre}
</Badge>
```

**Tooltip con descripción:**
```tsx
<Tooltip label={estadoCatalogo?.descripcion}>
  <Badge>{estadoCatalogo?.nombre}</Badge>
</Tooltip>
```

**Select ordenado:**
```tsx
{/* Los estados vienen ya ordenados por campo Orden */}
<Select>
  {estados.map(e => (
    <option key={e.id} value={e.codigo}>
      {e.nombre}
    </option>
  ))}
</Select>
```

---

## 📈 Métricas y Monitoreo

**¿Cuántos estados se usan?**
```sql
SELECT e.Nombre, COUNT(r.Id) as Total_Reservas
FROM catalogo_estado_reserva e
LEFT JOIN Reserva r ON r.Estado = e.Codigo
GROUP BY e.Id
ORDER BY Total_Reservas DESC;
```

**Estados nunca usados (candidatos a eliminar):**
```sql
SELECT * FROM catalogo_estado_reserva e
WHERE NOT EXISTS (
    SELECT 1 FROM Reserva WHERE Estado = e.Codigo
)
AND Sistema = FALSE;
```

---

## ✨ Conclusión

**Para EventConnect (tu proyecto):**
- ✅ Implementa catálogos para **estados de reserva** y **métodos de pago**
- ✅ Mantén VARCHAR para datos estáticos (tipo documento, etc.)
- ✅ Usa soft delete (`Activo = FALSE`) en lugar de DELETE
- ✅ Protege estados core con `Sistema = TRUE`
- ✅ Agrega Foreign Keys solo cuando estés en producción estable

**Esta arquitectura te permite:**
- Agregar "Pago con Nequi" en 30 segundos
- Crear estado "En Reparación Externa" sin downtime
- Personalizar colores de UI sin tocar código
- Escalar a miles de reservas sin problemas de performance

🚀 **Backend listo en http://localhost:5555**
