-- ============================================
-- MIGRACIÓN: Campos adicionales para Hoja de Vida de Activos
-- Fecha: 2025-01-XX
-- Descripción: Agrega campos necesarios para trazabilidad total (Codigo_QR, Vida_Util_Meses, Costo_Adquisicion)
-- ============================================

-- Versión PostgreSQL
-- ============================================

-- Agregar columna Codigo_QR (único por empresa)
ALTER TABLE Activo 
ADD COLUMN IF NOT EXISTS Codigo_QR VARCHAR(100) NULL;

-- Crear índice único compuesto (Codigo_QR, Empresa_Id) para garantizar unicidad por empresa
CREATE UNIQUE INDEX IF NOT EXISTS idx_activo_codigo_qr_empresa 
ON Activo (Codigo_QR, Empresa_Id) 
WHERE Codigo_QR IS NOT NULL;

-- Agregar columna Vida_Util_Meses
ALTER TABLE Activo 
ADD COLUMN IF NOT EXISTS Vida_Util_Meses INTEGER NULL;

-- Agregar columna Costo_Adquisicion (si no existe, puede ser alias de Costo_Compra)
ALTER TABLE Activo 
ADD COLUMN IF NOT EXISTS Costo_Adquisicion DECIMAL(12,2) NULL;

-- Migrar datos existentes: Copiar Costo_Compra a Costo_Adquisicion si está vacío
UPDATE Activo 
SET Costo_Adquisicion = Costo_Compra 
WHERE Costo_Adquisicion IS NULL AND Costo_Compra IS NOT NULL;

-- Migrar datos existentes: Convertir Vida_Util_Anos a Vida_Util_Meses
UPDATE Activo 
SET Vida_Util_Meses = Vida_Util_Anos * 12 
WHERE Vida_Util_Meses IS NULL AND Vida_Util_Anos IS NOT NULL;

-- Agregar comentarios descriptivos
COMMENT ON COLUMN Activo.Codigo_QR IS 'Código QR único del activo por empresa';
COMMENT ON COLUMN Activo.Vida_Util_Meses IS 'Vida útil del activo en meses';
COMMENT ON COLUMN Activo.Costo_Adquisicion IS 'Costo de adquisición del activo';

-- ============================================
-- Versión MySQL/MariaDB
-- ============================================

/*
-- Agregar columna Codigo_QR (único por empresa)
ALTER TABLE Activo 
ADD COLUMN Codigo_QR VARCHAR(100) NULL 
AFTER QR_Code_URL;

-- Crear índice único compuesto (Codigo_QR, Empresa_Id) para garantizar unicidad por empresa
CREATE UNIQUE INDEX idx_activo_codigo_qr_empresa 
ON Activo (Codigo_QR, Empresa_Id);

-- Agregar columna Vida_Util_Meses
ALTER TABLE Activo 
ADD COLUMN Vida_Util_Meses INT NULL 
AFTER Vida_Util_Anos;

-- Agregar columna Costo_Adquisicion
ALTER TABLE Activo 
ADD COLUMN Costo_Adquisicion DECIMAL(12,2) NULL 
AFTER Costo_Compra;

-- Migrar datos existentes: Copiar Costo_Compra a Costo_Adquisicion si está vacío
UPDATE Activo 
SET Costo_Adquisicion = Costo_Compra 
WHERE Costo_Adquisicion IS NULL AND Costo_Compra IS NOT NULL;

-- Migrar datos existentes: Convertir Vida_Util_Anos a Vida_Util_Meses
UPDATE Activo 
SET Vida_Util_Meses = Vida_Util_Anos * 12 
WHERE Vida_Util_Meses IS NULL AND Vida_Util_Anos IS NOT NULL;

-- Agregar comentarios descriptivos
ALTER TABLE Activo 
MODIFY COLUMN Codigo_QR VARCHAR(100) NULL COMMENT 'Código QR único del activo por empresa',
MODIFY COLUMN Vida_Util_Meses INT NULL COMMENT 'Vida útil del activo en meses',
MODIFY COLUMN Costo_Adquisicion DECIMAL(12,2) NULL COMMENT 'Costo de adquisición del activo';
*/

-- ============================================
-- VERIFICACIÓN
-- ============================================

-- Verificar que las columnas fueron creadas correctamente
SELECT 
    column_name,
    data_type,
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'Activo' 
AND column_name IN ('Codigo_QR', 'Vida_Util_Meses', 'Costo_Adquisicion')
ORDER BY column_name;
