-- Migración: Crear tabla de Daños y Discrepancias
-- Fecha: 2026-02-05
-- Descripción: Implementa el módulo de gestión de daños para el sistema de logística

-- Crear tipo enum para estados de daño
DO $$ BEGIN
    CREATE TYPE danio_estado AS ENUM (
        'Reportado',
        'En_Evaluacion',
        'Confirmado',
        'Rechazado',
        'En_Reparacion',
        'Reparado',
        'Perdida_Total'
    );
EXCEPTION WHEN duplicate_object THEN null;
END $$;

-- Crear tabla de daños
CREATE TABLE IF NOT EXISTS danios (
    id SERIAL PRIMARY KEY,
    reserva_id INTEGER NOT NULL REFERENCES reservas(id) ON DELETE RESTRICT,
    activo_id INTEGER NOT NULL REFERENCES activos(id) ON DELETE RESTRICT,
    cliente_id INTEGER NOT NULL REFERENCES clientes(id) ON DELETE RESTRICT,
    descripcion VARCHAR(1000) NOT NULL,
    estado danio_estado NOT NULL DEFAULT 'Reportado',
    fecha_reporte TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    imagen_url VARCHAR(500),
    monto_estimado DECIMAL(12, 2),
    monto_final DECIMAL(12, 2),
    resolucion VARCHAR(1000),
    fecha_resolucion TIMESTAMP,
    usuario_reportador_id INTEGER NOT NULL REFERENCES usuarios(id) ON DELETE RESTRICT,
    usuario_evaluador_id INTEGER REFERENCES usuarios(id) ON DELETE SET NULL,
    observaciones VARCHAR(500),
    fecha_creacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT chk_montos CHECK (monto_estimado IS NULL OR monto_estimado >= 0),
    CONSTRAINT chk_monto_final CHECK (monto_final IS NULL OR monto_final >= 0),
    CONSTRAINT chk_fecha_resolucion CHECK (
        (estado IN ('Reparado', 'Perdida_Total', 'Rechazado') AND fecha_resolucion IS NOT NULL) OR
        (estado NOT IN ('Reparado', 'Perdida_Total', 'Rechazado'))
    )
);

-- Crear índices para optimizar consultas
CREATE INDEX IF NOT EXISTS idx_danios_reserva ON danios(reserva_id);
CREATE INDEX IF NOT EXISTS idx_danios_activo ON danios(activo_id);
CREATE INDEX IF NOT EXISTS idx_danios_cliente ON danios(cliente_id);
CREATE INDEX IF NOT EXISTS idx_danios_estado ON danios(estado);
CREATE INDEX IF NOT EXISTS idx_danios_fecha_reporte ON danios(fecha_reporte DESC);
CREATE INDEX IF NOT EXISTS idx_danios_usuario_reportador ON danios(usuario_reportador_id);
CREATE INDEX IF NOT EXISTS idx_danios_composite ON danios(activo_id, estado, fecha_reporte DESC);

-- Crear vista para daños recientes (últimos 30 días)
CREATE OR REPLACE VIEW v_danios_recientes AS
SELECT 
    d.id,
    d.reserva_id,
    d.activo_id,
    d.cliente_id,
    d.descripcion,
    d.estado,
    d.fecha_reporte,
    d.monto_estimado,
    d.monto_final,
    ROUND(EXTRACT(DAY FROM CURRENT_TIMESTAMP - d.fecha_reporte)) as dias_desde_reporte,
    a.nombre as nombre_activo,
    u.username as usuario_reportador
FROM danios d
LEFT JOIN activos a ON a.id = d.activo_id
LEFT JOIN usuarios u ON u.id = d.usuario_reportador_id
WHERE d.fecha_reporte >= CURRENT_TIMESTAMP - INTERVAL '30 days'
ORDER BY d.fecha_reporte DESC;

-- Crear vista para estadísticas por activo
CREATE OR REPLACE VIEW v_estadisticas_danios_activo AS
SELECT 
    d.activo_id,
    a.nombre as nombre_activo,
    COUNT(*) as total_danios,
    SUM(CASE WHEN d.estado = 'Confirmado' THEN 1 ELSE 0 END) as confirmados,
    SUM(CASE WHEN d.estado = 'En_Reparacion' THEN 1 ELSE 0 END) as en_reparacion,
    SUM(CASE WHEN d.estado = 'Reparado' THEN 1 ELSE 0 END) as reparados,
    SUM(CASE WHEN d.estado = 'Rechazado' THEN 1 ELSE 0 END) as rechazados,
    SUM(CASE WHEN d.estado = 'Perdida_Total' THEN 1 ELSE 0 END) as perdida_total,
    COALESCE(SUM(d.monto_final), 0) as monto_total_reparaciones
FROM danios d
LEFT JOIN activos a ON a.id = d.activo_id
GROUP BY d.activo_id, a.nombre;

-- Crear función para actualizar fecha_actualizacion
CREATE OR REPLACE FUNCTION actualizar_fecha_actualizacion_danio()
RETURNS TRIGGER AS $$
BEGIN
    NEW.fecha_actualizacion = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Crear trigger para actualizar fecha_actualizacion
DROP TRIGGER IF EXISTS trigger_actualizar_danio ON danios;
CREATE TRIGGER trigger_actualizar_danio
BEFORE UPDATE ON danios
FOR EACH ROW
EXECUTE FUNCTION actualizar_fecha_actualizacion_danio();

-- Crear función para validar transiciones de estado
CREATE OR REPLACE FUNCTION validar_transicion_estado_danio()
RETURNS TRIGGER AS $$
BEGIN
    -- Transiciones permitidas
    IF NEW.estado = OLD.estado THEN
        RETURN NEW;
    END IF;
    
    -- Desde Reportado
    IF OLD.estado = 'Reportado' AND NEW.estado NOT IN ('En_Evaluacion', 'Rechazado') THEN
        RAISE EXCEPTION 'Transición inválida: de Reportado solo a En_Evaluacion o Rechazado';
    END IF;
    
    -- Desde En_Evaluacion
    IF OLD.estado = 'En_Evaluacion' AND NEW.estado NOT IN ('Confirmado', 'Rechazado') THEN
        RAISE EXCEPTION 'Transición inválida: de En_Evaluacion solo a Confirmado o Rechazado';
    END IF;
    
    -- Desde Confirmado
    IF OLD.estado = 'Confirmado' AND NEW.estado NOT IN ('En_Reparacion', 'Rechazado') THEN
        RAISE EXCEPTION 'Transición inválida: de Confirmado solo a En_Reparacion o Rechazado';
    END IF;
    
    -- Desde En_Reparacion
    IF OLD.estado = 'En_Reparacion' AND NEW.estado NOT IN ('Reparado', 'Perdida_Total') THEN
        RAISE EXCEPTION 'Transición inválida: de En_Reparacion solo a Reparado o Perdida_Total';
    END IF;
    
    -- Estados finales no pueden cambiar
    IF OLD.estado IN ('Reparado', 'Perdida_Total') THEN
        RAISE EXCEPTION 'El estado % es final y no puede cambiar', OLD.estado;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Crear trigger para validar transiciones
DROP TRIGGER IF EXISTS trigger_validar_transicion_danio ON danios;
CREATE TRIGGER trigger_validar_transicion_danio
BEFORE UPDATE ON danios
FOR EACH ROW
EXECUTE FUNCTION validar_transicion_estado_danio();

-- Crear función para actualizar estado del activo cuando hay daño confirmado
CREATE OR REPLACE FUNCTION actualizar_estado_activo_por_danio()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.estado = 'Confirmado' THEN
        UPDATE activos 
        SET estado = 'En_Mantenimiento', fecha_actualizacion = CURRENT_TIMESTAMP
        WHERE id = NEW.activo_id;
    ELSIF NEW.estado = 'Reparado' THEN
        UPDATE activos 
        SET estado = 'Disponible', fecha_actualizacion = CURRENT_TIMESTAMP
        WHERE id = NEW.activo_id;
    ELSIF NEW.estado = 'Perdida_Total' THEN
        UPDATE activos 
        SET estado = 'No_Disponible', fecha_actualizacion = CURRENT_TIMESTAMP
        WHERE id = NEW.activo_id;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Crear trigger para actualizar estado del activo
DROP TRIGGER IF EXISTS trigger_actualizar_estado_activo ON danios;
CREATE TRIGGER trigger_actualizar_estado_activo
AFTER UPDATE ON danios
FOR EACH ROW
EXECUTE FUNCTION actualizar_estado_activo_por_danio();

-- Comentarios de documentación
COMMENT ON TABLE danios IS 'Tabla que registra daños y discrepancias en activos durante el proceso de logística';
COMMENT ON COLUMN danios.estado IS 'Estado del daño: Reportado → En_Evaluacion → (Confirmado/Rechazado) → (En_Reparacion/Perdida_Total) → (Reparado/Perdida_Total)';
COMMENT ON COLUMN danios.monto_estimado IS 'Estimación inicial del costo de reparación';
COMMENT ON COLUMN danios.monto_final IS 'Costo final de la reparación';
COMMENT ON COLUMN danios.resolucion IS 'Descripción de cómo se resolvió el daño';
