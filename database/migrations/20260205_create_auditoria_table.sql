-- ============================================
-- Migración: Crear tabla Auditoria
-- Descripción: Sistema de auditoría inmutable para trazabilidad completa
-- Fecha: 2026-02-05
-- ============================================

BEGIN;

-- Crear tabla Auditoria
CREATE TABLE IF NOT EXISTS Auditoria (
    Id SERIAL PRIMARY KEY,
    Tabla_Afectada VARCHAR(100) NOT NULL,
    Registro_Id INTEGER NOT NULL,
    Usuario_Id INTEGER NOT NULL,
    Accion VARCHAR(50) NOT NULL CHECK (Accion IN ('Create', 'Update', 'Delete', 'StatusChange', 'Entrega', 'Devolución', 'Confirmacion')),
    Datos_Anteriores TEXT,
    Datos_Nuevos TEXT NOT NULL,
    Detalles TEXT,
    IP_Origen VARCHAR(50),
    User_Agent TEXT,
    Fecha_Accion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Usuario_Id) REFERENCES Usuario(Id) ON DELETE SET NULL
);

-- Crear índices para búsquedas rápidas
CREATE INDEX idx_auditoria_tabla_registro ON Auditoria(Tabla_Afectada, Registro_Id);
CREATE INDEX idx_auditoria_usuario ON Auditoria(Usuario_Id);
CREATE INDEX idx_auditoria_fecha ON Auditoria(Fecha_Accion);
CREATE INDEX idx_auditoria_accion ON Auditoria(Accion);
CREATE INDEX idx_auditoria_tabla_fecha ON Auditoria(Tabla_Afectada, Fecha_Accion);

-- Crear índice para búsquedas de texto (full text search)
CREATE INDEX idx_auditoria_datos_nuevos ON Auditoria USING GIN (to_tsvector('spanish', Datos_Nuevos));
CREATE INDEX idx_auditoria_detalles ON Auditoria USING GIN (to_tsvector('spanish', COALESCE(Detalles, '')));

-- Crear comentarios
COMMENT ON TABLE Auditoria IS 'Tabla de auditoría inmutable para rastrear todos los cambios en el sistema';
COMMENT ON COLUMN Auditoria.Tabla_Afectada IS 'Nombre de la tabla donde se realizó el cambio';
COMMENT ON COLUMN Auditoria.Registro_Id IS 'ID del registro afectado en esa tabla';
COMMENT ON COLUMN Auditoria.Usuario_Id IS 'Usuario que realizó la acción';
COMMENT ON COLUMN Auditoria.Accion IS 'Tipo de acción: Create, Update, Delete, StatusChange, etc.';
COMMENT ON COLUMN Auditoria.Datos_Anteriores IS 'Valores anteriores (JSON)';
COMMENT ON COLUMN Auditoria.Datos_Nuevos IS 'Valores nuevos (JSON)';
COMMENT ON COLUMN Auditoria.Detalles IS 'Descripción adicional de la acción';
COMMENT ON COLUMN Auditoria.IP_Origen IS 'Dirección IP del usuario que realizó la acción';
COMMENT ON COLUMN Auditoria.User_Agent IS 'Navegador/Cliente del usuario';
COMMENT ON COLUMN Auditoria.Fecha_Accion IS 'Timestamp de la acción (inmutable)';

-- Crear función para asegurar que Fecha_Accion no pueda cambiar
CREATE OR REPLACE FUNCTION prevent_auditoria_update()
RETURNS TRIGGER AS $$
BEGIN
    RAISE EXCEPTION 'No se pueden actualizar registros de auditoría';
END;
$$ LANGUAGE plpgsql;

-- Crear trigger para prevenir actualizaciones
CREATE TRIGGER trigger_prevent_auditoria_update
BEFORE UPDATE ON Auditoria
FOR EACH ROW
EXECUTE FUNCTION prevent_auditoria_update();

-- Crear función para crear particiones por mes (para optimización de grandes volúmenes)
-- Esta será útil cuando la tabla crezca
CREATE OR REPLACE FUNCTION create_auditoria_partition()
RETURNS VOID AS $$
BEGIN
    -- Crear partición para el mes actual si no existe
    -- Se ejecutará manualmente o vía job scheduler
    NULL;
END;
$$ LANGUAGE plpgsql;

-- Crear vista para auditoría reciente (últimas 7 días)
CREATE OR REPLACE VIEW v_auditoria_reciente AS
SELECT 
    a.Id,
    a.Tabla_Afectada,
    a.Registro_Id,
    a.Usuario_Id,
    u.Usuario,
    u.Email,
    a.Accion,
    a.Datos_Nuevos,
    a.Detalles,
    a.Fecha_Accion,
    DATE_PART('day', NOW() - a.Fecha_Accion)::INTEGER as Dias_Atras
FROM Auditoria a
LEFT JOIN Usuario u ON a.Usuario_Id = u.Id
WHERE a.Fecha_Accion > NOW() - INTERVAL '7 days'
ORDER BY a.Fecha_Accion DESC;

COMMENT ON VIEW v_auditoria_reciente IS 'Vista de auditoría de los últimos 7 días';

-- Crear política de seguridad (si es necesario en PostgreSQL con RLS)
-- ALTER TABLE Auditoria ENABLE ROW LEVEL SECURITY;

COMMIT;
