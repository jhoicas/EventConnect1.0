-- ============================================
-- MIGRACIÓN: Tabla Evidencia_Entrega para Logística
-- Fecha: 2025-01-XX
-- Descripción: Crea tabla para evidencias de entregas, devoluciones y daños
-- ============================================

-- Versión PostgreSQL
-- ============================================

-- Tabla: Evidencia_Entrega
CREATE TABLE IF NOT EXISTS Evidencia_Entrega (
    Id SERIAL PRIMARY KEY,
    Reserva_Id INTEGER NOT NULL,
    Empresa_Id INTEGER NOT NULL,
    Usuario_Id INTEGER NOT NULL,
    Tipo VARCHAR(20) NOT NULL DEFAULT 'Entrega' CHECK (Tipo IN ('Entrega', 'Devolucion', 'Dano')),
    Url_Imagen VARCHAR(500) NOT NULL,
    Comentario TEXT NULL,
    Latitud DECIMAL(10, 8) NULL COMMENT 'Latitud GPS para geolocalización',
    Longitud DECIMAL(11, 8) NULL COMMENT 'Longitud GPS para geolocalización',
    Nombre_Recibe VARCHAR(150) NULL COMMENT 'Nombre de quien recibe entrega/devolución',
    Url_Firma VARCHAR(500) NULL COMMENT 'URL de imagen de firma digital',
    Fecha_Creacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Reserva_Id) REFERENCES Reserva(Id) ON DELETE CASCADE,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Usuario_Id) REFERENCES Usuario(Id) ON DELETE RESTRICT
);

-- Comentarios de tabla y columnas
COMMENT ON TABLE Evidencia_Entrega IS 'Evidencias fotográficas de entregas, devoluciones y daños en logística';
COMMENT ON COLUMN Evidencia_Entrega.Tipo IS 'Tipo de evidencia: Entrega, Devolucion, Dano';
COMMENT ON COLUMN Evidencia_Entrega.Url_Imagen IS 'URL relativa de la imagen guardada en el servidor';
COMMENT ON COLUMN Evidencia_Entrega.Latitud IS 'Latitud GPS (opcional, para geolocalización)';
COMMENT ON COLUMN Evidencia_Entrega.Longitud IS 'Longitud GPS (opcional, para geolocalización)';
COMMENT ON COLUMN Evidencia_Entrega.Empresa_Id IS 'ID de la empresa (multi-tenancy, se obtiene de la reserva)';

-- Índices para optimización
CREATE INDEX IF NOT EXISTS idx_evidencia_reserva ON Evidencia_Entrega(Reserva_Id);
CREATE INDEX IF NOT EXISTS idx_evidencia_empresa ON Evidencia_Entrega(Empresa_Id);
CREATE INDEX IF NOT EXISTS idx_evidencia_usuario ON Evidencia_Entrega(Usuario_Id);
CREATE INDEX IF NOT EXISTS idx_evidencia_tipo ON Evidencia_Entrega(Tipo);
CREATE INDEX IF NOT EXISTS idx_evidencia_fecha ON Evidencia_Entrega(Fecha_Creacion);

-- Índice compuesto para consultas comunes (reserva + tipo)
CREATE INDEX IF NOT EXISTS idx_evidencia_reserva_tipo ON Evidencia_Entrega(Reserva_Id, Tipo);

-- ============================================
-- Versión MySQL/MariaDB
-- ============================================

/*
-- Tabla: Evidencia_Entrega
CREATE TABLE IF NOT EXISTS Evidencia_Entrega (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Reserva_Id INT NOT NULL,
    Empresa_Id INT NOT NULL,
    Usuario_Id INT NOT NULL,
    Tipo ENUM('Entrega', 'Devolucion', 'Dano') NOT NULL DEFAULT 'Entrega',
    Url_Imagen VARCHAR(500) NOT NULL,
    Comentario TEXT NULL,
    Latitud DECIMAL(10, 8) NULL COMMENT 'Latitud GPS para geolocalización',
    Longitud DECIMAL(11, 8) NULL COMMENT 'Longitud GPS para geolocalización',
    Nombre_Recibe VARCHAR(150) NULL COMMENT 'Nombre de quien recibe entrega/devolución',
    Url_Firma VARCHAR(500) NULL COMMENT 'URL de imagen de firma digital',
    Fecha_Creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Reserva_Id) REFERENCES Reserva(Id) ON DELETE CASCADE,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Usuario_Id) REFERENCES Usuario(Id) ON DELETE RESTRICT,
    INDEX idx_reserva (Reserva_Id),
    INDEX idx_empresa (Empresa_Id),
    INDEX idx_usuario (Usuario_Id),
    INDEX idx_tipo (Tipo),
    INDEX idx_fecha (Fecha_Creacion),
    INDEX idx_reserva_tipo (Reserva_Id, Tipo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
*/

-- ============================================
-- VERIFICACIÓN
-- ============================================

-- Verificar que la tabla fue creada correctamente
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'Evidencia_Entrega'
ORDER BY ordinal_position;

-- Verificar índices
SELECT 
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename = 'Evidencia_Entrega'
ORDER BY indexname;
