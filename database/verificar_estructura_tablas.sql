-- ====================================================================
-- SCRIPT DE CORRECCIÓN: Información sobre estructura de tablas
-- ====================================================================

-- IMPORTANTE: Este script solo te muestra la información actual de tus tablas
-- NO ejecuta cambios, solo consultas SELECT

-- 1. Ver estructura actual de tabla Activo
DESCRIBE Activo;

-- 2. Ver estructura actual de tabla Reserva
DESCRIBE Reserva;

-- 3. Ver valores únicos en Estado_Disponibilidad de Activo
SELECT DISTINCT Estado_Disponibilidad, COUNT(*) as Total 
FROM Activo 
GROUP BY Estado_Disponibilidad;

-- 4. Ver valores únicos en Estado de Reserva
SELECT DISTINCT Estado, COUNT(*) as Total 
FROM Reserva 
GROUP BY Estado;

-- ====================================================================
-- NOTA IMPORTANTE SOBRE LOS ENUMs:
-- ====================================================================

-- Tu tabla Activo usa:
--   - Estado_Fisico (Nuevo, Excelente, Bueno, Regular, Malo)
--   - Estado_Disponibilidad (Disponible, Alquilado, En_Mantenimiento, Dado_de_Baja)

-- Tu tabla Reserva usa:
--   - Estado (valores que definas en tu sistema)

-- ====================================================================
-- PARA AGREGAR UN NUEVO ESTADO SIN CATÁLOGOS (Método ENUM tradicional):
-- ====================================================================

-- Opción 1: Agregar estado a ENUM existente (requiere ALTER TABLE):
-- ALTER TABLE Activo 
-- MODIFY COLUMN Estado_Disponibilidad 
-- ENUM('Disponible', 'Alquilado', 'En_Mantenimiento', 'Dado_de_Baja', 'Reparacion_Externa');

-- ====================================================================
-- PARA USAR EL SISTEMA DE CATÁLOGOS (Recomendado para escalabilidad):
-- ====================================================================

-- Paso 1: Ejecutar el script de creación de tablas de catálogo:
-- SOURCE database/migracion_enums_a_catalogos.sql;

-- Paso 2: Las tablas de catálogo ya tienen los estados base:
--   - catalogo_estado_activo (con Disponible, Alquilado, En_Mantenimiento, etc.)
--   - catalogo_estado_reserva
--   - catalogo_metodo_pago
--   - catalogo_tipo_mantenimiento

-- Paso 3: Para agregar nuevo estado (sin ALTER TABLE):
-- INSERT INTO catalogo_estado_activo (Codigo, Nombre, Descripcion, Color, Permite_Reserva) 
-- VALUES ('Reparacion_Externa', 'Reparación Externa', 'Activo en reparación externa', 'red', FALSE);

-- Paso 4: Usar el nuevo estado en tu aplicación:
-- UPDATE Activo SET Estado_Disponibilidad = 'Reparacion_Externa' WHERE Id = 123;

-- ====================================================================
-- VALIDACIÓN: Verificar que todos los estados en uso existen en catálogo
-- ====================================================================

-- Ver estados de activos que NO están en catálogo:
-- SELECT DISTINCT a.Estado_Disponibilidad 
-- FROM Activo a
-- WHERE NOT EXISTS (
--     SELECT 1 FROM catalogo_estado_activo c 
--     WHERE c.Codigo = a.Estado_Disponibilidad
-- );

-- Si este query devuelve resultados, necesitas:
-- 1. Agregar esos estados al catálogo, O
-- 2. Actualizar los registros al estado correcto

-- ====================================================================
-- RESUMEN DE COMANDOS ÚTILES:
-- ====================================================================

-- Ver todos los estados de activo disponibles en catálogo:
-- SELECT * FROM catalogo_estado_activo WHERE Activo = 1 ORDER BY Orden;

-- Ver todos los métodos de pago disponibles:
-- SELECT * FROM catalogo_metodo_pago WHERE Activo = 1 ORDER BY Orden;

-- Agregar nuevo método de pago (ej: Nequi):
-- INSERT INTO catalogo_metodo_pago (Codigo, Nombre, Descripcion, Requiere_Comprobante, Requiere_Referencia) 
-- VALUES ('Nequi', 'Nequi', 'Pago mediante Nequi', TRUE, TRUE);
