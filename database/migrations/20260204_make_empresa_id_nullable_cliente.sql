-- ============================================
-- Migración: Hacer empresa_id nullable en Cliente
-- Descripción: Permite registrar clientes persona sin empresa asociada
-- Fecha: 2026-02-04
-- ============================================

BEGIN;

-- 1. Remover la restricción UNIQUE (Empresa_Id, Documento) porque no funciona con NULL
ALTER TABLE Cliente
DROP CONSTRAINT IF EXISTS cliente_empresa_id_documento_key;

-- 2. Cambiar Empresa_Id a nullable
ALTER TABLE Cliente
ALTER COLUMN Empresa_Id DROP NOT NULL;

-- 3. Añadir nueva restricción UNIQUE que permita NULL
-- En PostgreSQL, NULL no se considera en UNIQUE constraints
ALTER TABLE Cliente
ADD CONSTRAINT cliente_empresa_id_documento_key UNIQUE (Empresa_Id, Documento)
DEFERRABLE INITIALLY DEFERRED;

-- 4. Actualizar comentario
COMMENT ON COLUMN Cliente.Empresa_Id IS 'Empresa proveedora que gestiona este cliente. NULL para clientes persona sin empresa asociada';

COMMIT;
