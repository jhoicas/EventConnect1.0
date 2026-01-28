-- ============================================
-- Migración: Agregar índice para Cliente_Id en Conversacion
-- Fecha: 2026-01-28
-- Descripción: Optimiza la consulta de conversaciones por cliente
-- ============================================

-- Verificar si el índice ya existe antes de crearlo
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes 
        WHERE schemaname = 'public' 
        AND tablename = 'conversacion' 
        AND indexname = 'idx_conversacion_cliente'
    ) THEN
        CREATE INDEX idx_conversacion_cliente ON Conversacion(Cliente_Id);
        RAISE NOTICE 'Índice idx_conversacion_cliente creado exitosamente';
    ELSE
        RAISE NOTICE 'Índice idx_conversacion_cliente ya existe';
    END IF;
END $$;

-- Verificar el resultado
SELECT 
    schemaname,
    tablename,
    indexname,
    indexdef
FROM pg_indexes 
WHERE tablename = 'conversacion'
ORDER BY indexname;
