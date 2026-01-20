-- ============================================
-- MIGRACIÓN: Módulo de Facturación para DIAN (Colombia)
-- Fecha: 2025-01-XX
-- Descripción: Crea tablas Factura y Detalle_Factura preparadas para facturación electrónica
-- ============================================

-- Versión PostgreSQL
-- ============================================

-- Tabla: Factura
CREATE TABLE IF NOT EXISTS Factura (
    Id SERIAL PRIMARY KEY,
    Empresa_Id INTEGER NOT NULL,
    Cliente_Id INTEGER NOT NULL,
    Reserva_Id INTEGER NULL,
    Prefijo VARCHAR(10) NOT NULL DEFAULT 'FE',
    Consecutivo INTEGER NOT NULL,
    CUFE VARCHAR(100) NULL,
    Fecha_Emision TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Fecha_Vencimiento TIMESTAMP NULL,
    Subtotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    Impuestos DECIMAL(12,2) NOT NULL DEFAULT 0,
    Total DECIMAL(12,2) NOT NULL DEFAULT 0,
    Estado VARCHAR(20) NOT NULL DEFAULT 'Borrador' CHECK (Estado IN ('Borrador', 'Emitida', 'Anulada')),
    Datos_Cliente_Snapshot JSONB NULL,
    Observaciones TEXT NULL,
    Creado_Por_Id INTEGER NOT NULL,
    Anulado_Por_Id INTEGER NULL,
    Fecha_Anulacion TIMESTAMP NULL,
    Razon_Anulacion TEXT NULL,
    Fecha_Creacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Cliente_Id) REFERENCES Cliente(Id),
    FOREIGN KEY (Reserva_Id) REFERENCES Reserva(Id) ON DELETE SET NULL,
    FOREIGN KEY (Creado_Por_Id) REFERENCES Usuario(Id),
    FOREIGN KEY (Anulado_Por_Id) REFERENCES Usuario(Id) ON DELETE SET NULL,
    CONSTRAINT uk_empresa_prefijo_consecutivo UNIQUE (Empresa_Id, Prefijo, Consecutivo)
);

-- Comentarios de columnas
COMMENT ON TABLE Factura IS 'Tabla de facturas preparadas para DIAN (Colombia)';
COMMENT ON COLUMN Factura.Prefijo IS 'Prefijo de facturación (FE: Factura Electrónica, NC: Nota Crédito, ND: Nota Débito)';
COMMENT ON COLUMN Factura.Consecutivo IS 'Número consecutivo único por prefijo y empresa';
COMMENT ON COLUMN Factura.CUFE IS 'Código Único de Facturación Electrónica generado por el software de facturación electrónica';
COMMENT ON COLUMN Factura.Datos_Cliente_Snapshot IS 'Datos del cliente al momento de facturar en formato JSON (para auditoría)';
COMMENT ON COLUMN Factura.Estado IS 'Estado: Borrador (en edición), Emitida (enviada a DIAN), Anulada';
COMMENT ON COLUMN Factura.Reserva_Id IS 'Reserva asociada (opcional)';

-- Índices para Factura
CREATE INDEX IF NOT EXISTS idx_factura_empresa ON Factura(Empresa_Id);
CREATE INDEX IF NOT EXISTS idx_factura_cliente ON Factura(Cliente_Id);
CREATE INDEX IF NOT EXISTS idx_factura_reserva ON Factura(Reserva_Id);
CREATE INDEX IF NOT EXISTS idx_factura_prefijo_consecutivo ON Factura(Prefijo, Consecutivo);
CREATE INDEX IF NOT EXISTS idx_factura_cufe ON Factura(CUFE) WHERE CUFE IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_factura_estado ON Factura(Estado);
CREATE INDEX IF NOT EXISTS idx_factura_fecha_emision ON Factura(Fecha_Emision);

-- Tabla: Detalle_Factura
CREATE TABLE IF NOT EXISTS Detalle_Factura (
    Id SERIAL PRIMARY KEY,
    Factura_Id INTEGER NOT NULL,
    Producto_Id INTEGER NULL,
    Servicio VARCHAR(250) NOT NULL,
    Cantidad INTEGER NOT NULL DEFAULT 1,
    Precio_Unitario DECIMAL(12,2) NOT NULL,
    Subtotal DECIMAL(12,2) NOT NULL,
    Tasa_Impuesto DECIMAL(5,4) NOT NULL DEFAULT 0.19,
    Impuesto DECIMAL(12,2) NOT NULL DEFAULT 0,
    Total DECIMAL(12,2) NOT NULL,
    Unidad_Medida VARCHAR(20) DEFAULT 'Unidad',
    Observaciones TEXT NULL,
    Fecha_Creacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Factura_Id) REFERENCES Factura(Id) ON DELETE CASCADE,
    FOREIGN KEY (Producto_Id) REFERENCES Producto(Id) ON DELETE SET NULL
);

-- Comentarios de tabla y columnas
COMMENT ON TABLE Detalle_Factura IS 'Detalles de items de una factura';
COMMENT ON COLUMN Detalle_Factura.Producto_Id IS 'Producto asociado (opcional para servicios personalizados)';
COMMENT ON COLUMN Detalle_Factura.Servicio IS 'Descripción del servicio o producto';
COMMENT ON COLUMN Detalle_Factura.Tasa_Impuesto IS 'Tasa de impuesto aplicada (ej: 0.19 para IVA 19% en Colombia)';
COMMENT ON COLUMN Detalle_Factura.Unidad_Medida IS 'Unidad de medida del item (Unidad, Día, Hora, etc.)';

-- Índices para Detalle_Factura
CREATE INDEX IF NOT EXISTS idx_detalle_factura_factura ON Detalle_Factura(Factura_Id);
CREATE INDEX IF NOT EXISTS idx_detalle_factura_producto ON Detalle_Factura(Producto_Id);

-- Trigger para actualizar Fecha_Actualizacion automáticamente
CREATE OR REPLACE FUNCTION update_factura_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.Fecha_Actualizacion = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_factura_timestamp
    BEFORE UPDATE ON Factura
    FOR EACH ROW
    EXECUTE FUNCTION update_factura_timestamp();

-- ============================================
-- Versión MySQL/MariaDB
-- ============================================

/*
-- Tabla: Factura
CREATE TABLE IF NOT EXISTS Factura (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NOT NULL,
    Cliente_Id INT NOT NULL,
    Reserva_Id INT NULL COMMENT 'Reserva asociada (opcional)',
    Prefijo VARCHAR(10) NOT NULL DEFAULT 'FE' COMMENT 'Prefijo de facturación (ej: FE, NC, ND)',
    Consecutivo INT NOT NULL COMMENT 'Número consecutivo de factura',
    CUFE VARCHAR(100) NULL COMMENT 'Código Único de Facturación Electrónica (DIAN)',
    Fecha_Emision DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Fecha_Vencimiento DATETIME NULL,
    Subtotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    Impuestos DECIMAL(12,2) NOT NULL DEFAULT 0,
    Total DECIMAL(12,2) NOT NULL DEFAULT 0,
    Estado ENUM('Borrador', 'Emitida', 'Anulada') NOT NULL DEFAULT 'Borrador',
    Datos_Cliente_Snapshot JSON NULL COMMENT 'Snapshot JSON del cliente al momento de facturar',
    Observaciones TEXT NULL,
    Creado_Por_Id INT NOT NULL,
    Anulado_Por_Id INT NULL,
    Fecha_Anulacion DATETIME NULL,
    Razon_Anulacion TEXT NULL,
    Fecha_Creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Cliente_Id) REFERENCES Cliente(Id),
    FOREIGN KEY (Reserva_Id) REFERENCES Reserva(Id) ON DELETE SET NULL,
    FOREIGN KEY (Creado_Por_Id) REFERENCES Usuario(Id),
    FOREIGN KEY (Anulado_Por_Id) REFERENCES Usuario(Id) ON DELETE SET NULL,
    UNIQUE KEY uk_empresa_prefijo_consecutivo (Empresa_Id, Prefijo, Consecutivo),
    INDEX idx_empresa (Empresa_Id),
    INDEX idx_cliente (Cliente_Id),
    INDEX idx_reserva (Reserva_Id),
    INDEX idx_prefijo_consecutivo (Prefijo, Consecutivo),
    INDEX idx_cufe (CUFE),
    INDEX idx_estado (Estado),
    INDEX idx_fecha_emision (Fecha_Emision)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla: Detalle_Factura
CREATE TABLE IF NOT EXISTS Detalle_Factura (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Factura_Id INT NOT NULL,
    Producto_Id INT NULL COMMENT 'Producto asociado (opcional para servicios personalizados)',
    Servicio VARCHAR(250) NOT NULL COMMENT 'Descripción del servicio o producto',
    Cantidad INT NOT NULL DEFAULT 1,
    Precio_Unitario DECIMAL(12,2) NOT NULL,
    Subtotal DECIMAL(12,2) NOT NULL,
    Tasa_Impuesto DECIMAL(5,4) NOT NULL DEFAULT 0.19 COMMENT 'Tasa de impuesto (ej: 0.19 para IVA 19%)',
    Impuesto DECIMAL(12,2) NOT NULL DEFAULT 0,
    Total DECIMAL(12,2) NOT NULL,
    Unidad_Medida VARCHAR(20) DEFAULT 'Unidad' COMMENT 'Unidad de medida (Unidad, Día, Hora, etc.)',
    Observaciones TEXT NULL,
    Fecha_Creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Factura_Id) REFERENCES Factura(Id) ON DELETE CASCADE,
    FOREIGN KEY (Producto_Id) REFERENCES Producto(Id) ON DELETE SET NULL,
    INDEX idx_factura (Factura_Id),
    INDEX idx_producto (Producto_Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
*/

-- ============================================
-- VERIFICACIÓN
-- ============================================

-- Verificar que las tablas fueron creadas correctamente
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable
FROM information_schema.columns 
WHERE table_name IN ('Factura', 'Detalle_Factura')
ORDER BY table_name, ordinal_position;

-- Verificar índices
SELECT 
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename IN ('Factura', 'Detalle_Factura')
ORDER BY tablename, indexname;
