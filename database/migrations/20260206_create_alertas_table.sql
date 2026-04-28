-- Migración: Crear tabla de Alertas de Mantenimiento y Depreciación
-- Fecha: 2026-02-06
-- Descripción: Implementa sistema preventivo de alertas para activos

-- Crear tipo enum para tipos de alerta
DO $$ BEGIN
    CREATE TYPE alerta_tipo AS ENUM (
        'Mantenimiento',
        'Depreciacion',
        'Vencimiento',
        'Garantia'
    );
EXCEPTION WHEN duplicate_object THEN null;
END $$;

-- Crear tipo enum para severidad
DO $$ BEGIN
    CREATE TYPE alerta_severidad AS ENUM (
        'Critica',
        'Alta',
        'Media',
        'Baja'
    );
EXCEPTION WHEN duplicate_object THEN null;
END $$;

-- Crear tipo enum para estado
DO $$ BEGIN
    CREATE TYPE alerta_estado AS ENUM (
        'Pendiente',
        'Asignada',
        'En_Proceso',
        'Resuelta',
        'Ignorada',
        'Vencida'
    );
EXCEPTION WHEN duplicate_object THEN null;
END $$;

-- Crear tabla de alertas
CREATE TABLE IF NOT EXISTS alertas (
    id SERIAL PRIMARY KEY,
    activo_id INTEGER NOT NULL REFERENCES activos(id) ON DELETE CASCADE,
    tipo alerta_tipo NOT NULL,
    descripcion VARCHAR(500) NOT NULL,
    severidad alerta_severidad NOT NULL DEFAULT 'Media',
    estado alerta_estado NOT NULL DEFAULT 'Pendiente',
    fecha_alerta TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_vencimiento TIMESTAMP,
    fecha_resolucion TIMESTAMP,
    usuario_asignado_id INTEGER REFERENCES usuarios(id) ON DELETE SET NULL,
    detalles_tecnicos TEXT,
    observaciones VARCHAR(500),
    acciones_recomendadas VARCHAR(500),
    notificacion_enviada BOOLEAN DEFAULT FALSE,
    fecha_notificacion TIMESTAMP,
    prioridad INTEGER CHECK (prioridad >= 1 AND prioridad <= 10) DEFAULT 5,
    fecha_creacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT chk_fecha_resolucion CHECK (
        (estado = 'Resuelta' AND fecha_resolucion IS NOT NULL) OR
        (estado != 'Resuelta')
    ),
    CONSTRAINT chk_usuario_asignado CHECK (
        (estado IN ('Asignada', 'En_Proceso') AND usuario_asignado_id IS NOT NULL) OR
        (estado NOT IN ('Asignada', 'En_Proceso'))
    )
);

-- Crear índices para optimizar consultas
CREATE INDEX IF NOT EXISTS idx_alertas_activo ON alertas(activo_id);
CREATE INDEX IF NOT EXISTS idx_alertas_tipo ON alertas(tipo);
CREATE INDEX IF NOT EXISTS idx_alertas_severidad ON alertas(severidad);
CREATE INDEX IF NOT EXISTS idx_alertas_estado ON alertas(estado);
CREATE INDEX IF NOT EXISTS idx_alertas_usuario_asignado ON alertas(usuario_asignado_id);
CREATE INDEX IF NOT EXISTS idx_alertas_fecha_alerta ON alertas(fecha_alerta DESC);
CREATE INDEX IF NOT EXISTS idx_alertas_fecha_vencimiento ON alertas(fecha_vencimiento);
CREATE INDEX IF NOT EXISTS idx_alertas_prioridad ON alertas(prioridad DESC, fecha_alerta ASC);
CREATE INDEX IF NOT EXISTS idx_alertas_composite ON alertas(activo_id, estado, severidad DESC);

-- Crear vista para alertas activas por activo
CREATE OR REPLACE VIEW v_alertas_activas_por_activo AS
SELECT 
    a.activo_id,
    act.nombre as nombre_activo,
    COUNT(*) as total_alertas,
    SUM(CASE WHEN a.severidad = 'Critica' THEN 1 ELSE 0 END) as criticas,
    SUM(CASE WHEN a.estado = 'Pendiente' THEN 1 ELSE 0 END) as pendientes,
    SUM(CASE WHEN a.estado = 'Asignada' THEN 1 ELSE 0 END) as asignadas,
    SUM(CASE WHEN a.tipo = 'Mantenimiento' THEN 1 ELSE 0 END) as mantenimiento,
    SUM(CASE WHEN a.tipo = 'Depreciacion' THEN 1 ELSE 0 END) as depreciacion,
    MAX(a.prioridad) as prioridad_maxima,
    MIN(a.fecha_vencimiento) as proximo_vencimiento
FROM alertas a
LEFT JOIN activos act ON act.id = a.activo_id
WHERE a.estado != 'Resuelta'
GROUP BY a.activo_id, act.nombre;

-- Crear vista para alertas críticas o vencidas
CREATE OR REPLACE VIEW v_alertas_criticas AS
SELECT 
    a.id,
    a.activo_id,
    a.tipo,
    a.descripcion,
    a.severidad,
    a.estado,
    a.fecha_alerta,
    a.fecha_vencimiento,
    a.prioridad,
    act.nombre as nombre_activo,
    CAST(EXTRACT(DAY FROM a.fecha_vencimiento - NOW()) AS INTEGER) as dias_para_vencer
FROM alertas a
LEFT JOIN activos act ON act.id = a.activo_id
WHERE (a.severidad = 'Critica' OR a.estado = 'Vencida') 
  AND a.estado != 'Resuelta'
ORDER BY a.prioridad DESC, a.fecha_alerta ASC;

-- Crear vista para alertas próximas a vencer
CREATE OR REPLACE VIEW v_alertas_proximamente_vencidas AS
SELECT 
    a.id,
    a.activo_id,
    a.tipo,
    a.descripcion,
    a.severidad,
    a.estado,
    a.fecha_alerta,
    a.fecha_vencimiento,
    CAST(EXTRACT(DAY FROM a.fecha_vencimiento - NOW()) AS INTEGER) as horas_para_vencer,
    act.nombre as nombre_activo
FROM alertas a
LEFT JOIN activos act ON act.id = a.activo_id
WHERE a.fecha_vencimiento BETWEEN NOW() AND NOW() + INTERVAL '2 days'
  AND a.estado != 'Resuelta'
ORDER BY a.fecha_vencimiento ASC;

-- Crear función para actualizar fecha_actualizacion
CREATE OR REPLACE FUNCTION actualizar_fecha_actualizacion_alerta()
RETURNS TRIGGER AS $$
BEGIN
    NEW.fecha_actualizacion = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Crear trigger para actualizar fecha_actualizacion
DROP TRIGGER IF EXISTS trigger_actualizar_alerta ON alertas;
CREATE TRIGGER trigger_actualizar_alerta
BEFORE UPDATE ON alertas
FOR EACH ROW
EXECUTE FUNCTION actualizar_fecha_actualizacion_alerta();

-- Crear función para marcar alertas como vencidas
CREATE OR REPLACE FUNCTION marcar_alertas_vencidas()
RETURNS void AS $$
BEGIN
    UPDATE alertas 
    SET estado = 'Vencida', fecha_actualizacion = CURRENT_TIMESTAMP
    WHERE fecha_vencimiento < CURRENT_TIMESTAMP 
      AND estado IN ('Pendiente', 'Asignada', 'En_Proceso');
END;
$$ LANGUAGE plpgsql;

-- Crear función para generar alerta de mantenimiento
CREATE OR REPLACE FUNCTION generar_alerta_mantenimiento(
    p_activo_id INTEGER,
    p_dias_desde_mantenimiento INTEGER
)
RETURNS INTEGER AS $$
DECLARE
    v_alerta_id INTEGER;
BEGIN
    -- Verificar que no exista alerta pendiente
    SELECT id INTO v_alerta_id
    FROM alertas
    WHERE activo_id = p_activo_id 
      AND tipo = 'Mantenimiento'
      AND estado != 'Resuelta'
    LIMIT 1;

    IF v_alerta_id IS NOT NULL THEN
        RETURN v_alerta_id;
    END IF;

    -- Crear nueva alerta
    INSERT INTO alertas (
        activo_id, tipo, descripcion, severidad, estado,
        fecha_alerta, fecha_vencimiento, acciones_recomendadas,
        prioridad, fecha_creacion, fecha_actualizacion
    ) VALUES (
        p_activo_id,
        'Mantenimiento'::alerta_tipo,
        'Mantenimiento requerido - últimas revisión hace ' || p_dias_desde_mantenimiento || ' días',
        'Alta'::alerta_severidad,
        'Pendiente'::alerta_estado,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP + INTERVAL '7 days',
        'Programar revisión técnica completa del equipo',
        8,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    ) RETURNING id INTO v_alerta_id;

    RETURN v_alerta_id;
END;
$$ LANGUAGE plpgsql;

-- Crear función para generar alerta de depreciación
CREATE OR REPLACE FUNCTION generar_alerta_depreciacion(
    p_activo_id INTEGER,
    p_dias_para_fin INTEGER
)
RETURNS INTEGER AS $$
DECLARE
    v_alerta_id INTEGER;
    v_severidad alerta_severidad;
BEGIN
    -- Verificar que no exista alerta pendiente
    SELECT id INTO v_alerta_id
    FROM alertas
    WHERE activo_id = p_activo_id 
      AND tipo = 'Depreciacion'
      AND estado != 'Resuelta'
    LIMIT 1;

    IF v_alerta_id IS NOT NULL THEN
        RETURN v_alerta_id;
    END IF;

    -- Determinar severidad según días restantes
    v_severidad := CASE 
        WHEN p_dias_para_fin <= 7 THEN 'Critica'::alerta_severidad
        WHEN p_dias_para_fin <= 14 THEN 'Alta'::alerta_severidad
        ELSE 'Media'::alerta_severidad
    END;

    -- Crear nueva alerta
    INSERT INTO alertas (
        activo_id, tipo, descripcion, severidad, estado,
        fecha_alerta, fecha_vencimiento, acciones_recomendadas,
        prioridad, fecha_creacion, fecha_actualizacion
    ) VALUES (
        p_activo_id,
        'Depreciacion'::alerta_tipo,
        'Fin de vida útil próximo en ' || p_dias_para_fin || ' días',
        v_severidad,
        'Pendiente'::alerta_estado,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP + INTERVAL '30 days',
        'Evaluar reemplazo del activo. Contactar al proveedor para cotización.',
        9,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    ) RETURNING id INTO v_alerta_id;

    RETURN v_alerta_id;
END;
$$ LANGUAGE plpgsql;

-- Comentarios de documentación
COMMENT ON TABLE alertas IS 'Tabla que registra alertas de mantenimiento, depreciación y vencimiento de activos';
COMMENT ON COLUMN alertas.tipo IS 'Tipo de alerta: Mantenimiento (revisión técnica), Depreciacion (fin de vida útil), Vencimiento (fecha límite), Garantia (cobertura)';
COMMENT ON COLUMN alertas.severidad IS 'Nivel de urgencia: Critica (inmediata), Alta (días), Media (semanas), Baja (informativo)';
COMMENT ON COLUMN alertas.estado IS 'Ciclo de vida: Pendiente → Asignada → En_Proceso → Resuelta/Ignorada/Vencida';
COMMENT ON COLUMN alertas.prioridad IS 'Escala 1-10 para ordenar por importancia (10=máxima urgencia)';

-- Crear índice full-text para búsqueda en descripción
CREATE INDEX IF NOT EXISTS idx_alertas_descripcion_fts ON alertas USING gin(
    to_tsvector('spanish', descripcion || ' ' || COALESCE(observaciones, ''))
);
