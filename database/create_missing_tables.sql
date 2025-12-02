-- ============================================
-- Crear tablas faltantes: Conversacion y Mantenimiento
-- ============================================

USE db_eventconnect;

-- Tabla: Conversacion
CREATE TABLE IF NOT EXISTS Conversacion (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NOT NULL,
    Asunto VARCHAR(250),
    Reserva_Id INT NULL,
    Fecha_Creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    Estado ENUM('Abierta', 'Cerrada', 'Archivada') DEFAULT 'Abierta',
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Reserva_Id) REFERENCES Reserva(Id) ON DELETE SET NULL,
    INDEX idx_empresa (Empresa_Id),
    INDEX idx_estado (Estado),
    INDEX idx_fecha (Fecha_Creacion)
) ENGINE=InnoDB;

-- Tabla: Mensaje (para el chat)
CREATE TABLE IF NOT EXISTS Mensaje (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Conversacion_Id INT NOT NULL,
    Usuario_Id INT NOT NULL,
    Mensaje TEXT NOT NULL,
    Fecha_Envio DATETIME DEFAULT CURRENT_TIMESTAMP,
    Leido BOOLEAN DEFAULT FALSE,
    FOREIGN KEY (Conversacion_Id) REFERENCES Conversacion(Id) ON DELETE CASCADE,
    FOREIGN KEY (Usuario_Id) REFERENCES Usuario(Id) ON DELETE CASCADE,
    INDEX idx_conversacion (Conversacion_Id),
    INDEX idx_fecha (Fecha_Envio)
) ENGINE=InnoDB;

-- Tabla: Mantenimiento
CREATE TABLE IF NOT EXISTS Mantenimiento (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Activo_Id INT NOT NULL,
    Tipo_Mantenimiento ENUM('Preventivo', 'Correctivo') NOT NULL,
    Fecha_Programada DATETIME NULL,
    Fecha_Realizada DATETIME NULL,
    Descripcion TEXT,
    Responsable_Id INT NULL,
    Proveedor_Servicio VARCHAR(200),
    Costo DECIMAL(10,2),
    Estado ENUM('Pendiente', 'En Proceso', 'Completado', 'Cancelado') DEFAULT 'Pendiente',
    Observaciones TEXT,
    Fecha_Creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Activo_Id) REFERENCES Activo(Id) ON DELETE CASCADE,
    FOREIGN KEY (Responsable_Id) REFERENCES Usuario(Id) ON DELETE SET NULL,
    INDEX idx_activo (Activo_Id),
    INDEX idx_estado (Estado),
    INDEX idx_tipo (Tipo_Mantenimiento),
    INDEX idx_fecha_programada (Fecha_Programada)
) ENGINE=InnoDB;

-- Tabla: Configuracion_Sistema
CREATE TABLE IF NOT EXISTS Configuracion_Sistema (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NULL COMMENT 'NULL para configuración global',
    Clave VARCHAR(100) NOT NULL,
    Valor TEXT,
    Descripcion VARCHAR(500),
    Tipo_Dato ENUM('string', 'int', 'bool', 'json') DEFAULT 'string',
    Es_Global BOOLEAN DEFAULT FALSE,
    Fecha_Actualizacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    UNIQUE KEY unique_config (Empresa_Id, Clave),
    INDEX idx_clave (Clave),
    INDEX idx_empresa (Empresa_Id)
) ENGINE=InnoDB;

SELECT 'Tablas creadas exitosamente' as Resultado;
