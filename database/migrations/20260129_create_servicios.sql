-- Crear tabla Servicios
CREATE TABLE Servicios (
    Id_Servicio SERIAL PRIMARY KEY,
    Titulo VARCHAR(100) NOT NULL,
    Descripcion TEXT NOT NULL,
    Icono VARCHAR(50),
    Imagen_Url VARCHAR(500) NOT NULL,
    Orden INTEGER DEFAULT 0,
    Activo BOOLEAN DEFAULT TRUE,
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Índices recomendados
CREATE INDEX idx_servicios_activo_orden ON Servicios(Activo, Orden);
CREATE INDEX idx_servicios_fecha_creacion ON Servicios(Fecha_Creacion);

-- Trigger para actualizar Fecha_Actualizacion
CREATE TRIGGER update_servicios_updated_at BEFORE UPDATE ON Servicios
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
