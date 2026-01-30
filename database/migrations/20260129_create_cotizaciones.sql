-- Crear tabla Cotizaciones
CREATE TABLE Cotizaciones (
    Id SERIAL PRIMARY KEY,
    Cliente_Id INTEGER NOT NULL,
    Producto_Id INTEGER NOT NULL,
    Fecha_Solicitud TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Cantidad_Solicitada INTEGER NOT NULL,
    Monto_Cotizacion DECIMAL(10, 2) NOT NULL DEFAULT 0,
    Estado VARCHAR(50) DEFAULT 'Solicitada', -- Solicitada, Respondida, Aceptada, Rechazada
    Observaciones TEXT,
    Fecha_Respuesta TIMESTAMP NULL,
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Cliente_Id) REFERENCES Usuario(Id) ON DELETE CASCADE,
    FOREIGN KEY (Producto_Id) REFERENCES Producto(Id) ON DELETE CASCADE
);

-- Índices recomendados
CREATE INDEX idx_cotizaciones_cliente ON Cotizaciones(Cliente_Id);
CREATE INDEX idx_cotizaciones_producto ON Cotizaciones(Producto_Id);
CREATE INDEX idx_cotizaciones_estado ON Cotizaciones(Estado);
CREATE INDEX idx_cotizaciones_fecha_solicitud ON Cotizaciones(Fecha_Solicitud);

-- Trigger para actualizar Fecha_Actualizacion
CREATE TRIGGER update_cotizaciones_updated_at BEFORE UPDATE ON Cotizaciones
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
