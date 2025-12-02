# Script de Instalación de Datos Reales en EventConnect

## Archivo Creado
`database/inserts_data_real.sql` - Script SQL completo con datos reales

## Contenido del Script

### 1. **Roles del Sistema** (5 roles)
- SuperAdmin (nivel 0)
- Admin-Proveedor (nivel 1)
- Operario (nivel 2)
- Cliente (nivel 3)
- Auditor (nivel 4)

### 2. **Empresas Proveedoras** (3 empresas)

#### Eventos Elegantes SAS
- **NIT:** 900123456-7
- **Ciudad:** Bogotá
- **Email:** contacto@eventoselegantes.com
- **Teléfono:** +57 310 123 4567
- **Logo:** Purple (#6366f1)
- **Especialidad:** Mobiliario, vajilla y decoración para bodas y eventos formales

#### Party Time Alquileres SAS
- **NIT:** 900234567-8
- **Ciudad:** Medellín
- **Email:** info@partytime.com.co
- **Teléfono:** +57 311 234 5678
- **Logo:** Orange (#f59e0b)
- **Especialidad:** Fiestas infantiles, inflables y entretenimiento

#### AudioVisual Pro Colombia SAS
- **NIT:** 900345678-9
- **Ciudad:** Cali
- **Email:** ventas@audiovisualpro.co
- **Teléfono:** +57 312 345 6789
- **Logo:** Green (#10b981)
- **Especialidad:** Sonido, iluminación y tecnología para eventos

### 3. **Usuarios del Sistema** (7 usuarios)

#### SuperAdmin
- **Usuario:** superadmin
- **Email:** admin@eventconnect.com
- **Password:** EventConnect2024!

#### Eventos Elegantes SAS
- **Admin:** admin.elegantes / admin@eventoselegantes.com
  - Nombre: María Fernanda Rodríguez
- **Operario:** operario.elegantes / bodega@eventoselegantes.com
  - Nombre: Carlos Andrés Gómez

#### Party Time Alquileres
- **Admin:** admin.partytime / admin@partytime.com.co
  - Nombre: Andrea Paola Martínez
- **Operario:** operario.partytime / logistica@partytime.com.co
  - Nombre: Jorge Luis Ramírez

#### AudioVisual Pro Colombia
- **Admin:** admin.audiovisual / admin@audiovisualpro.co
  - Nombre: Sebastián Andrés Torres
- **Técnico:** tecnico.audiovisual / tecnico@audiovisualpro.co
  - Nombre: Daniel Esteban Vargas

**Nota:** Todos los usuarios tienen la misma contraseña: `EventConnect2024!`

### 4. **Bodegas** (3 bodegas principales)
- BOD-EE-01: Bodega Principal Eventos Elegantes (Bogotá)
- BOD-PT-01: Bodega Principal Party Time (Medellín)
- BOD-AV-01: Bodega AudioVisual Pro (Cali)

### 5. **Categorías** (12 categorías)

#### Eventos Elegantes (3 categorías)
1. Mobiliario (sillas, mesas)
2. Vajilla y Cristalería
3. Decoración

#### Party Time (4 categorías)
4. Mobiliario Infantil
5. Recreación
6. Decoración Temática
7. Alimentos y Bebidas

#### AudioVisual Pro (5 categorías)
8. Sonido
9. Iluminación
10. Proyección
11. DJ y Animación
12. Pantallas LED

### 6. **Productos** (32 productos totales)

#### Eventos Elegantes SAS - 13 productos

**SKU** | **Nombre** | **Precio/Día**
--------|-----------|---------------
EE-SIL-001 | Silla Tiffany Cristal Transparente | $18,000
EE-SIL-002 | Silla Tiffany Dorada Premium | $16,000
EE-SIL-003 | Silla Tiffany Blanca Marfil | $15,000
EE-SIL-004 | Silla Rimax Plástica Blanca | $5,000
EE-MES-001 | Mesa Redonda 10 Personas | $45,000
EE-MES-002 | Mesa Rectangular 8 Personas | $40,000
EE-MES-003 | Mesa Coctel Alta | $25,000
EE-VAJ-001 | Vajilla Porcelana - Set 10 Personas | $55,000
EE-CUB-001 | Cubiertería Plateada - Set 10 Personas | $35,000
EE-CRI-001 | Cristalería Fina - Set 10 Copas | $45,000
EE-DEC-001 | Centro de Mesa Floral | $65,000
EE-MAN-001 | Mantel Redondo Premium | $18,000
EE-CAM-001 | Camino de Mesa Elegante | $12,000

#### Party Time Alquileres - 9 productos

**SKU** | **Nombre** | **Precio/Día**
--------|-----------|---------------
PT-SIL-001 | Silla Infantil Rimax Colores | $4,000
PT-MES-001 | Mesa Infantil Rectangular | $20,000
PT-INF-001 | Castillo Inflable Mediano | $280,000
PT-INF-002 | Castillo Inflable Grande con Piscina | $450,000
PT-DEC-001 | Decoración Temática Princesas | $380,000
PT-DEC-002 | Decoración Temática Superhéroes | $380,000
PT-ALI-001 | Máquina de Algodón de Azúcar | $220,000
PT-ALI-002 | Máquina de Crispetas/Palomitas | $180,000
PT-VAJ-001 | Vajilla Desechable Temática | $45,000

#### AudioVisual Pro Colombia - 10 productos

**SKU** | **Nombre** | **Precio/Día**
--------|-----------|---------------
AV-SON-001 | Sistema de Sonido 2000W | $550,000
AV-SON-002 | Sistema de Sonido Premium 5000W | $1,200,000
AV-ILU-001 | Paquete Iluminación Básica | $350,000
AV-ILU-002 | Paquete Iluminación Profesional | $850,000
AV-PRO-001 | Proyector Full HD + Pantalla 3x2m | $280,000
AV-PRO-002 | Video Beam + Pantalla Gigante 5x3m | $650,000
AV-DJ-001 | Servicio DJ Profesional 4 Horas | $450,000
AV-DJ-002 | Servicio DJ Premium 6 Horas | $750,000
AV-LED-001 | Pantalla LED 2x1m | $800,000
AV-LED-002 | Pantalla LED Gigante 4x3m | $2,500,000

### 7. **Activos Individuales** (12 activos de ejemplo)
- 5 sillas Tiffany cristal (Eventos Elegantes)
- 4 inflables (Party Time)
- 3 sistemas de sonido (AudioVisual Pro)

Cada activo tiene:
- Código único (QR/RFID)
- Estado físico y disponibilidad
- Fecha de compra y costo
- Proveedor y vida útil
- Historial de mantenimiento

### 8. **Clientes** (7 clientes de ejemplo)
- 3 clientes para Eventos Elegantes (2 personas, 1 empresa)
- 2 clientes para Party Time (1 persona, 1 empresa)
- 2 clientes para AudioVisual Pro (1 persona, 1 empresa)

### 9. **Configuración del Sistema**
- Información general (nombre, versión)
- Configuración financiera (IVA 19%, moneda COP)
- Reglas de reservas (anticipación, anticipo 50%)
- Configuración de notificaciones

## Características Especiales

### ✅ Imágenes Corregidas
Todas las imágenes de productos son reales y verificadas:
- URLs de Unsplash con parámetros optimizados
- Imágenes apropiadas para cada producto
- **Silla Tiffany Cristal corregida:** Ahora muestra una silla transparente real

### ✅ Relaciones Completas
- Empresas → Usuarios (relación Empresa_Id)
- Empresas → Bodegas (cada empresa tiene su bodega)
- Empresas → Categorías (categorías específicas por empresa)
- Empresas → Productos (productos asignados a empresas)
- Productos → Activos (ítems individuales con trazabilidad)
- Empresas → Clientes (clientes asignados a cada empresa)

### ✅ Datos Realistas
- NITs válidos con formato colombiano
- Teléfonos con formato +57
- Direcciones reales por ciudad
- Precios acorde al mercado colombiano
- SKUs con nomenclatura profesional
- Stocks realistas por tipo de producto

## Instrucciones de Instalación

### Opción 1: MySQL Workbench (Recomendado)
1. Abrir MySQL Workbench
2. Conectarse a la base de datos
3. Archivo → Open SQL Script
4. Seleccionar: `database/inserts_data_real.sql`
5. Ejecutar el script (⚡ botón Execute)

### Opción 2: Línea de Comandos
```bash
# Si tienes MySQL en el PATH
mysql -u root -p db_eventconnect < database/inserts_data_real.sql

# Si usas XAMPP/WAMP
cd "C:\xampp\mysql\bin"
.\mysql.exe -u root -p db_eventconnect < "C:\Users\yoiner.castillo\source\repos\EventConnect\database\inserts_data_real.sql"
```

### Opción 3: phpMyAdmin
1. Abrir phpMyAdmin
2. Seleccionar base de datos `db_eventconnect`
3. Pestaña "SQL"
4. Copiar y pegar el contenido de `inserts_data_real.sql`
5. Clic en "Go"

## Verificación Post-Instalación

Ejecutar estas consultas para verificar:

```sql
-- Ver empresas insertadas
SELECT Id, Razon_Social, NIT, Ciudad, Estado FROM Empresa;

-- Ver usuarios por empresa
SELECT u.Usuario, u.Email, u.Nombre_Completo, r.Nombre AS Rol, e.Razon_Social AS Empresa
FROM Usuario u
LEFT JOIN Empresa e ON u.Empresa_Id = e.Id
INNER JOIN Rol r ON u.Rol_Id = r.Id
ORDER BY e.Razon_Social, r.Nivel_Acceso;

-- Ver productos por empresa
SELECT e.Razon_Social, c.Nombre AS Categoria, p.SKU, p.Nombre, p.Precio_Alquiler_Dia, p.Cantidad_Stock
FROM Producto p
INNER JOIN Empresa e ON p.Empresa_Id = e.Id
INNER JOIN Categoria c ON p.Categoria_Id = c.Id
ORDER BY e.Razon_Social, c.Nombre;

-- Ver activos disponibles
SELECT a.Codigo_Activo, p.Nombre AS Producto, e.Razon_Social AS Empresa, 
       a.Estado_Fisico, a.Estado_Disponibilidad, b.Nombre AS Bodega
FROM Activo a
INNER JOIN Producto p ON a.Producto_Id = p.Id
INNER JOIN Empresa e ON a.Empresa_Id = e.Id
INNER JOIN Bodega b ON a.Bodega_Id = b.Id
WHERE a.Estado_Disponibilidad = 'Disponible'
ORDER BY e.Razon_Social;
```

## Próximos Pasos

1. **Backend**: Actualizar endpoints para consultar datos reales
2. **Frontend**: Sincronizar mockData.ts con datos de API
3. **Autenticación**: Implementar login con usuarios creados
4. **Pruebas**: Verificar que todas las relaciones funcionen correctamente

## Notas Importantes

- ⚠️ **Contraseñas**: Todos los usuarios usan `EventConnect2024!` (cambiar en producción)
- 📦 **Stock**: Los valores de stock son de ejemplo, ajustar según inventario real
- 💰 **Precios**: Precios en COP (pesos colombianos), validar con precios de mercado
- 🔐 **Hash de Password**: Los hashes BCrypt están pre-generados para facilitar testing
- 🏢 **Multi-tenancy**: Cada empresa es completamente independiente con sus propios datos
