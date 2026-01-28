-- ============================================
-- MIGRACIÓN: Sistema de Reservas Multivendedor
-- Fecha: 2026-01-28
-- Descripción: Refactorización para permitir que una Reserva contenga 
--              productos de múltiples Empresas (Proveedores)
-- ============================================

-- ============================================
-- PASO 1: Agregar Empresa_Id a Detalle_Reserva
-- ============================================
ALTER TABLE Detalle_Reserva
ADD COLUMN Empresa_Id INTEGER NULL;

-- Agregar comentario
COMMENT ON COLUMN Detalle_Reserva.Empresa_Id IS 'ID de la empresa proveedora del producto en este detalle';

-- Agregar restricción de clave foránea
ALTER TABLE Detalle_Reserva
ADD CONSTRAINT fk_detalle_reserva_empresa 
FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE;

-- Crear índice para búsquedas rápidas
CREATE INDEX idx_detalle_reserva_empresa ON Detalle_Reserva(Empresa_Id);

-- ============================================
-- PASO 2: Copiar Empresa_Id a Detalle_Reserva desde Reserva
-- ============================================
-- Esta migración de datos copia la Empresa_Id desde la tabla Reserva
-- a cada detalle de reserva asociado
UPDATE Detalle_Reserva dr
SET Empresa_Id = r.Empresa_Id
FROM Reserva r
WHERE dr.Reserva_Id = r.Id AND dr.Empresa_Id IS NULL;

-- ============================================
-- PASO 3: Cambiar Empresa_Id de NOT NULL a NULL en Reserva
--         para preparar su eliminación posterior
-- ============================================
ALTER TABLE Reserva
ALTER COLUMN Empresa_Id DROP NOT NULL;

-- ============================================
-- PASO 4: Eliminar Foreign Key de Empresa_Id en Reserva
-- ============================================
-- Primero, necesitamos obtener el nombre de la restricción
-- En PostgreSQL, podemos hacerlo así:
ALTER TABLE Reserva
DROP CONSTRAINT IF EXISTS reserva_empresa_id_fkey;

-- ============================================
-- PASO 5: Eliminar la columna Empresa_Id de Reserva
-- ============================================
ALTER TABLE Reserva
DROP COLUMN IF EXISTS Empresa_Id;

-- Actualizar comentario de la tabla Reserva
COMMENT ON TABLE Reserva IS 'Reservas multivendedor - Agrupa servicios de múltiples empresas. Empresa específica se define en Detalle_Reserva';

-- ============================================
-- PASO 6: Crear Índices mejorados en Detalle_Reserva
-- ============================================
-- Índice compuesto para búsquedas de reservas por empresa
CREATE INDEX IF NOT EXISTS idx_detalle_reserva_reserva_empresa 
ON Detalle_Reserva(Reserva_Id, Empresa_Id);

-- ============================================
-- PASO 7: Crear una Vista para agrupar Reservas por Usuario Cliente
-- ============================================
-- Esta vista permite a los clientes ver todas sus reservas agrupadas
-- con información de las empresas involucradas
CREATE OR REPLACE VIEW vw_reservas_cliente AS
SELECT 
    r.Id,
    r.Cliente_Id,
    r.Codigo_Reserva,
    r.Estado,
    r.Fecha_Evento,
    r.Fecha_Entrega,
    r.Fecha_Devolucion_Programada,
    r.Fecha_Devolucion_Real,
    r.Direccion_Entrega,
    r.Ciudad_Entrega,
    r.Total,
    r.Estado_Pago,
    r.Observaciones,
    r.Fecha_Creacion,
    r.Fecha_Actualizacion,
    c.Nombre AS Cliente_Nombre,
    c.Email AS Cliente_Email,
    COUNT(DISTINCT dr.Empresa_Id) AS Cantidad_Empresas,
    STRING_AGG(DISTINCT e.Razon_Social, ', ') AS Empresas_Involucradas,
    COUNT(DISTINCT dr.Id) AS Total_Detalles
FROM Reserva r
INNER JOIN Cliente c ON r.Cliente_Id = c.Id
LEFT JOIN Detalle_Reserva dr ON r.Id = dr.Reserva_Id
LEFT JOIN Empresa e ON dr.Empresa_Id = e.Id
GROUP BY r.Id, c.Nombre, c.Email;

-- ============================================
-- PASO 8: Crear una Vista para Reservas por Empresa
-- ============================================
-- Esta vista permite a cada empresa ver los detalles de reserva que les pertenecen
CREATE OR REPLACE VIEW vw_reservas_empresa AS
SELECT 
    dr.Id AS Detalle_Reserva_Id,
    r.Id AS Reserva_Id,
    r.Cliente_Id,
    r.Codigo_Reserva,
    r.Estado,
    r.Fecha_Evento,
    r.Fecha_Entrega,
    r.Fecha_Devolucion_Programada,
    dr.Empresa_Id,
    dr.Producto_Id,
    dr.Activo_Id,
    dr.Cantidad,
    dr.Precio_Unitario,
    dr.Subtotal,
    dr.Observaciones,
    dr.Estado_Item,
    c.Nombre AS Cliente_Nombre,
    c.Email AS Cliente_Email,
    e.Razon_Social AS Empresa_Nombre,
    r.Fecha_Creacion,
    r.Fecha_Actualizacion
FROM Reserva r
INNER JOIN Cliente c ON r.Cliente_Id = c.Id
INNER JOIN Detalle_Reserva dr ON r.Id = dr.Reserva_Id
INNER JOIN Empresa e ON dr.Empresa_Id = e.Id;

-- ============================================
-- PASO 9: Actualizar índices de la tabla Reserva
-- ============================================
-- Eliminar índice antiguo si existe
DROP INDEX IF EXISTS idx_reserva_empresa;

-- El índice en Cliente_Id ya existe, no es necesario recrearlo

-- ============================================
-- PASO 10: Actualizar Triggers y Constraints
-- ============================================
-- El trigger de actualización de Fecha_Actualizacion sigue funcionando igual

-- Verificación final
COMMENT ON VIEW vw_reservas_cliente IS 'Vista de reservas agrupadas por cliente con info de empresas involucradas';
COMMENT ON VIEW vw_reservas_empresa IS 'Vista de detalles de reserva por empresa proveedora';

-- ============================================
-- ROLLBACK (opcional - para pruebas)
-- ============================================
/*
-- Si necesitas revertir esta migración, ejecuta:
-- ALTER TABLE Detalle_Reserva DROP CONSTRAINT fk_detalle_reserva_empresa;
-- ALTER TABLE Detalle_Reserva DROP COLUMN Empresa_Id;
-- ALTER TABLE Reserva ADD COLUMN Empresa_Id INTEGER NOT NULL;
-- ALTER TABLE Reserva ADD CONSTRAINT fk_reserva_empresa FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id);
-- DROP VIEW IF EXISTS vw_reservas_cliente;
-- DROP VIEW IF EXISTS vw_reservas_empresa;
*/
