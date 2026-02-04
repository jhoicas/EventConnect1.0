-- ============================================
-- EventConnect - Base de Datos Completa para PostgreSQL
-- Sistema de Gestión de Activos y Alquileres
-- Versión: 1.0.0
-- Base de datos: PostgreSQL 12+
-- ============================================

-- Eliminar tablas existentes (en orden inverso debido a foreign keys)
-- NOTA: Si prefieres mantener otros objetos, comenta estas líneas y elimina las tablas individuales
-- DROP SCHEMA IF EXISTS public CASCADE;
-- CREATE SCHEMA public;
--
-- En bases de datos en la nube (AWS RDS, Heroku, etc.), generalmente no es necesario
-- hacer DROP SCHEMA. Simplemente ejecuta el script y las tablas se crearán.
-- Si hay tablas existentes que quieres reemplazar, descomenta las líneas de arriba.

-- ============================================
-- EXTENSIONES
-- ============================================

-- Habilitar UUID si es necesario
-- CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ============================================
-- MÓDULO 1: CORE - Gestión de Entidades
-- ============================================

-- Tabla: Empresa (Multi-tenancy)
CREATE TABLE Empresa (
    Id SERIAL PRIMARY KEY,
    Razon_Social VARCHAR(200) NOT NULL,
    NIT VARCHAR(20) UNIQUE NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Telefono VARCHAR(20),
    Direccion VARCHAR(250),
    Ciudad VARCHAR(100),
    Pais VARCHAR(100) DEFAULT 'Colombia',
    Logo_URL VARCHAR(500),
    Estado VARCHAR(20) DEFAULT 'Activa' CHECK (Estado IN ('Activa', 'Inactiva', 'Suspendida')),
    Fecha_Registro TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE Empresa IS 'Empresas del sistema (Multi-tenancy)';
COMMENT ON COLUMN Empresa.Estado IS 'Estado de la empresa: Activa, Inactiva, Suspendida';

CREATE INDEX idx_empresa_nit ON Empresa(NIT);
CREATE INDEX idx_empresa_estado ON Empresa(Estado);

-- Función para actualizar Fecha_Actualizacion automáticamente
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.Fecha_Actualizacion = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Trigger para actualizar Fecha_Actualizacion en Empresa
CREATE TRIGGER update_empresa_updated_at BEFORE UPDATE ON Empresa
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Tabla: Rol
CREATE TABLE Rol (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(50) UNIQUE NOT NULL,
    Descripcion VARCHAR(250),
    Nivel_Acceso INTEGER NOT NULL,
    Permisos JSONB,
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE Rol IS 'Roles de usuarios del sistema';
COMMENT ON COLUMN Rol.Nivel_Acceso IS '0=SuperAdmin, 1=Admin-Proveedor, 2=Operario, 3=Cliente, 4=Auditor';
COMMENT ON COLUMN Rol.Permisos IS 'Array de permisos específicos en formato JSONB';

-- Tabla: Usuario
CREATE TABLE Usuario (
    Id SERIAL PRIMARY KEY,
    Empresa_Id INTEGER NULL,
    Rol_Id INTEGER NOT NULL,
    Usuario VARCHAR(50) UNIQUE NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    Password_Hash VARCHAR(255) NOT NULL,
    Nombre_Completo VARCHAR(150) NOT NULL,
    Telefono VARCHAR(20),
    Avatar_URL VARCHAR(500),
    Estado VARCHAR(20) DEFAULT 'Activo' CHECK (Estado IN ('Activo', 'Inactivo', 'Bloqueado')),
    Intentos_Fallidos INTEGER DEFAULT 0,
    Ultimo_Acceso TIMESTAMP NULL,
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Requiere_Cambio_Password BOOLEAN DEFAULT FALSE,
    TwoFA_Activo BOOLEAN DEFAULT FALSE,
    TwoFA_Secret VARCHAR(100) NULL,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Rol_Id) REFERENCES Rol(Id)
);

COMMENT ON TABLE Usuario IS 'Usuarios del sistema';
COMMENT ON COLUMN Usuario.Empresa_Id IS 'NULL para SuperAdmin';

CREATE INDEX idx_usuario_usuario ON Usuario(Usuario);
CREATE INDEX idx_usuario_email ON Usuario(Email);
CREATE INDEX idx_usuario_empresa ON Usuario(Empresa_Id);

CREATE TRIGGER update_usuario_updated_at BEFORE UPDATE ON Usuario
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Tabla: Cliente (Clientes finales de las empresas)
CREATE TABLE Cliente (
    Id SERIAL PRIMARY KEY,
    Empresa_Id INTEGER NULL,
    Usuario_Id INTEGER NULL,
    Tipo_Cliente VARCHAR(20) NOT NULL CHECK (Tipo_Cliente IN ('Persona', 'Empresa')),
    Nombre VARCHAR(150) NOT NULL,
    Documento VARCHAR(50) NOT NULL,
    Tipo_Documento VARCHAR(20) NOT NULL CHECK (Tipo_Documento IN ('CC', 'NIT', 'CE', 'Pasaporte')),
    Email VARCHAR(100),
    Telefono VARCHAR(20),
    Direccion VARCHAR(250),
    Ciudad VARCHAR(100),
    Contacto_Nombre VARCHAR(150),
    Contacto_Telefono VARCHAR(20),
    Observaciones TEXT,
    Rating DECIMAL(3,2) DEFAULT 5.00,
    Total_Alquileres INTEGER DEFAULT 0,
    Total_Danos_Reportados INTEGER DEFAULT 0,
    Estado VARCHAR(20) DEFAULT 'Activo' CHECK (Estado IN ('Activo', 'Inactivo', 'Bloqueado')),
    Fecha_Registro TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Usuario_Id) REFERENCES Usuario(Id) ON DELETE SET NULL,
    UNIQUE (Empresa_Id, Documento)
);

COMMENT ON TABLE Cliente IS 'Clientes finales de las empresas';
COMMENT ON COLUMN Cliente.Empresa_Id IS 'Empresa proveedora que gestiona este cliente. NULL para clientes persona sin empresa asociada';
COMMENT ON COLUMN Cliente.Rating IS 'Calificación del cliente (1-5)';
COMMENT ON COLUMN Cliente.Contacto_Nombre IS 'Persona de contacto si es empresa';

CREATE INDEX idx_cliente_nombre ON Cliente(Nombre);
CREATE INDEX idx_cliente_documento ON Cliente(Documento);

CREATE TRIGGER update_cliente_updated_at BEFORE UPDATE ON Cliente
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- ============================================
-- MÓDULO 2: SUSCRIPCIONES Y PLANES
-- ============================================

-- Tabla: Plan (Definición de planes disponibles)
CREATE TABLE Plan (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion TEXT,
    Precio_Mensual DECIMAL(10,2) NOT NULL,
    Duracion_Prueba_Dias INTEGER DEFAULT 0,
    Modulos_Incluidos JSONB,
    Limite_Usuarios INTEGER DEFAULT NULL,
    Limite_Productos INTEGER DEFAULT NULL,
    Activo BOOLEAN DEFAULT TRUE,
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON COLUMN Plan.Modulos_Incluidos IS 'Array de módulos incluidos';
COMMENT ON COLUMN Plan.Limite_Usuarios IS 'NULL = ilimitado';

-- Tabla: Suscripcion
CREATE TABLE Suscripcion (
    Id SERIAL PRIMARY KEY,
    Empresa_Id INTEGER NOT NULL,
    Plan_Id INTEGER NOT NULL,
    Modulo VARCHAR(20) NOT NULL CHECK (Modulo IN ('SIGI', 'B2B', 'Analytics_Pro', 'Mobile_Pro')),
    Estado VARCHAR(20) NOT NULL DEFAULT 'Prueba' CHECK (Estado IN ('Prueba', 'Activa', 'Vencida', 'Cancelada')),
    Fecha_Inicio TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Fecha_Fin_Prueba TIMESTAMP NULL,
    Fecha_Vencimiento TIMESTAMP NULL,
    Auto_Renovar BOOLEAN DEFAULT TRUE,
    Costo_Mensual DECIMAL(10,2) NOT NULL,
    Fecha_Ultimo_Pago TIMESTAMP NULL,
    Metodo_Pago VARCHAR(50),
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Fecha_Cancelacion TIMESTAMP NULL,
    Razon_Cancelacion TEXT NULL,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Plan_Id) REFERENCES Plan(Id)
);

CREATE INDEX idx_suscripcion_empresa_modulo ON Suscripcion(Empresa_Id, Modulo);
CREATE INDEX idx_suscripcion_estado ON Suscripcion(Estado);
CREATE INDEX idx_suscripcion_vencimiento ON Suscripcion(Fecha_Vencimiento);

-- ============================================
-- MÓDULO 3: INVENTARIO BÁSICO
-- ============================================

-- Tabla: Categoria
CREATE TABLE Categoria (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion TEXT,
    Icono VARCHAR(50),
    Color VARCHAR(7),
    Activo BOOLEAN DEFAULT TRUE,
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON COLUMN Categoria.Color IS 'Color hexadecimal #RRGGBB';
COMMENT ON TABLE Categoria IS 'Categorías globales de productos';

CREATE INDEX idx_categoria_activo ON Categoria(Activo);

-- Tabla: Producto
CREATE TABLE Producto (
    Id SERIAL PRIMARY KEY,
    Empresa_Id INTEGER NOT NULL,
    Categoria_Id INTEGER NOT NULL,
    SKU VARCHAR(50) NOT NULL,
    Nombre VARCHAR(150) NOT NULL,
    Descripcion TEXT,
    Unidad_Medida VARCHAR(20) DEFAULT 'Unidad' CHECK (Unidad_Medida IN ('Unidad', 'Par', 'Set', 'Metro', 'Metro²', 'Kilo')),
    Precio_Alquiler_Dia DECIMAL(10,2) NOT NULL,
    Cantidad_Stock INTEGER DEFAULT 0,
    Stock_Minimo INTEGER DEFAULT 10,
    Imagen_URL VARCHAR(500),
    Es_Alquilable BOOLEAN DEFAULT TRUE,
    Es_Vendible BOOLEAN DEFAULT FALSE,
    Requiere_Mantenimiento BOOLEAN DEFAULT FALSE,
    Dias_Mantenimiento INTEGER DEFAULT 90,
    Peso_Kg DECIMAL(8,2),
    Dimensiones VARCHAR(50),
    Observaciones TEXT,
    Activo BOOLEAN DEFAULT TRUE,
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Categoria_Id) REFERENCES Categoria(Id),
    UNIQUE (Empresa_Id, SKU)
);

COMMENT ON COLUMN Producto.SKU IS 'Código único del producto';
COMMENT ON COLUMN Producto.Cantidad_Stock IS 'Para inventario básico';
COMMENT ON COLUMN Producto.Dias_Mantenimiento IS 'Cada cuántos días requiere mantenimiento';
COMMENT ON COLUMN Producto.Dimensiones IS 'Ej: 1m x 0.5m x 0.8m';

CREATE INDEX idx_producto_nombre ON Producto(Nombre);
CREATE INDEX idx_producto_activo ON Producto(Activo);

CREATE TRIGGER update_producto_updated_at BEFORE UPDATE ON Producto
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- ============================================
-- MÓDULO 4: S.I.G.I. - INVENTARIO AVANZADO
-- ============================================

-- Tabla: Bodega
CREATE TABLE Bodega (
    Id SERIAL PRIMARY KEY,
    Empresa_Id INTEGER NOT NULL,
    Codigo_Bodega VARCHAR(20) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Direccion VARCHAR(250),
    Ciudad VARCHAR(100),
    Telefono VARCHAR(20),
    Responsable_Id INTEGER NULL,
    Capacidad_M3 DECIMAL(10,2),
    Estado VARCHAR(20) DEFAULT 'Activo' CHECK (Estado IN ('Activo', 'Inactivo')),
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Responsable_Id) REFERENCES Usuario(Id) ON DELETE SET NULL,
    UNIQUE (Empresa_Id, Codigo_Bodega)
);

COMMENT ON COLUMN Bodega.Responsable_Id IS 'Usuario responsable de la bodega';

CREATE INDEX idx_bodega_estado ON Bodega(Estado);

CREATE TRIGGER update_bodega_updated_at BEFORE UPDATE ON Bodega
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Tabla: Activo (Ítems individuales con hoja de vida)
CREATE TABLE Activo (
    Id SERIAL PRIMARY KEY,
    Empresa_Id INTEGER NOT NULL,
    Producto_Id INTEGER NOT NULL,
    Bodega_Id INTEGER NOT NULL,
    Codigo_Activo VARCHAR(50) UNIQUE NOT NULL,
    Numero_Serie VARCHAR(100) UNIQUE NULL,
    Estado_Fisico VARCHAR(20) DEFAULT 'Nuevo' CHECK (Estado_Fisico IN ('Nuevo', 'Excelente', 'Bueno', 'Regular', 'Malo')),
    Estado_Disponibilidad VARCHAR(20) DEFAULT 'Disponible' CHECK (Estado_Disponibilidad IN ('Disponible', 'Alquilado', 'En_Mantenimiento', 'Dado_de_Baja')),
    Fecha_Compra DATE,
    Costo_Compra DECIMAL(12,2),
    Proveedor VARCHAR(150),
    Vida_Util_Anos INTEGER DEFAULT 5,
    Valor_Residual DECIMAL(12,2) DEFAULT 0,
    Depreciacion_Acumulada DECIMAL(12,2) DEFAULT 0,
    Foto_Registro_URL VARCHAR(500),
    QR_Code_URL VARCHAR(500),
    Total_Alquileres INTEGER DEFAULT 0,
    Ingresos_Totales DECIMAL(12,2) DEFAULT 0,
    Costos_Mantenimiento DECIMAL(12,2) DEFAULT 0,
    Ultimo_Mantenimiento DATE NULL,
    Proximo_Mantenimiento DATE NULL,
    Observaciones TEXT,
    Activo BOOLEAN DEFAULT TRUE,
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Producto_Id) REFERENCES Producto(Id),
    FOREIGN KEY (Bodega_Id) REFERENCES Bodega(Id)
);

COMMENT ON COLUMN Activo.Codigo_Activo IS 'Código único QR/RFID';
COMMENT ON COLUMN Activo.QR_Code_URL IS 'URL de imagen QR generada';

CREATE INDEX idx_activo_codigo ON Activo(Codigo_Activo);
CREATE INDEX idx_activo_estado ON Activo(Estado_Disponibilidad);
CREATE INDEX idx_activo_bodega ON Activo(Bodega_Id);

CREATE TRIGGER update_activo_updated_at BEFORE UPDATE ON Activo
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Tabla: Lote (Para productos con fecha de vencimiento)
CREATE TABLE Lote (
    Id SERIAL PRIMARY KEY,
    Producto_Id INTEGER NOT NULL,
    Bodega_Id INTEGER NULL,
    Codigo_Lote VARCHAR(50) NOT NULL,
    Fecha_Fabricacion DATE,
    Fecha_Vencimiento DATE,
    Cantidad_Inicial INTEGER NOT NULL,
    Cantidad_Actual INTEGER NOT NULL,
    Costo_Unitario DECIMAL(10,2) NOT NULL,
    Estado VARCHAR(20) DEFAULT 'Disponible' CHECK (Estado IN ('Disponible', 'Vencido', 'Agotado')),
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Producto_Id) REFERENCES Producto(Id),
    FOREIGN KEY (Bodega_Id) REFERENCES Bodega(Id) ON DELETE SET NULL
);

CREATE INDEX idx_lote_vencimiento ON Lote(Fecha_Vencimiento);
CREATE INDEX idx_lote_estado ON Lote(Estado);

-- Tabla: Movimiento_Inventario
CREATE TABLE Movimiento_Inventario (
    Id SERIAL PRIMARY KEY,
    Empresa_Id INTEGER NOT NULL,
    Tipo_Movimiento VARCHAR(20) NOT NULL CHECK (Tipo_Movimiento IN ('Entrada', 'Salida', 'Transferencia', 'Ajuste')),
    Producto_Id INTEGER NULL,
    Activo_Id INTEGER NULL,
    Lote_Id INTEGER NULL,
    Bodega_Origen_Id INTEGER NULL,
    Bodega_Destino_Id INTEGER NULL,
    Cantidad INTEGER NOT NULL,
    Costo_Unitario DECIMAL(12,2),
    Motivo TEXT NOT NULL,
    Usuario_Id INTEGER NOT NULL,
    Fecha_Movimiento TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Producto_Id) REFERENCES Producto(Id),
    FOREIGN KEY (Activo_Id) REFERENCES Activo(Id),
    FOREIGN KEY (Lote_Id) REFERENCES Lote(Id),
    FOREIGN KEY (Bodega_Origen_Id) REFERENCES Bodega(Id),
    FOREIGN KEY (Bodega_Destino_Id) REFERENCES Bodega(Id),
    FOREIGN KEY (Usuario_Id) REFERENCES Usuario(Id)
);

COMMENT ON COLUMN Movimiento_Inventario.Activo_Id IS 'Si es un movimiento de activo específico';
COMMENT ON COLUMN Movimiento_Inventario.Motivo IS 'Motivo del movimiento';

CREATE INDEX idx_movimiento_tipo ON Movimiento_Inventario(Tipo_Movimiento);
CREATE INDEX idx_movimiento_fecha ON Movimiento_Inventario(Fecha_Movimiento);
CREATE INDEX idx_movimiento_producto ON Movimiento_Inventario(Producto_Id);
CREATE INDEX idx_movimiento_activo ON Movimiento_Inventario(Activo_Id);

-- Tabla: Mantenimiento
CREATE TABLE Mantenimiento (
    Id SERIAL PRIMARY KEY,
    Activo_Id INTEGER NOT NULL,
    Tipo_Mantenimiento VARCHAR(20) NOT NULL CHECK (Tipo_Mantenimiento IN ('Preventivo', 'Correctivo')),
    Fecha_Programada DATE,
    Fecha_Realizada DATE,
    Descripcion TEXT,
    Responsable_Id INTEGER NULL,
    Proveedor_Servicio VARCHAR(150),
    Costo DECIMAL(10,2),
    Estado VARCHAR(20) DEFAULT 'Pendiente' CHECK (Estado IN ('Pendiente', 'En_Proceso', 'Completado', 'Cancelado')),
    Observaciones TEXT,
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Activo_Id) REFERENCES Activo(Id) ON DELETE CASCADE,
    FOREIGN KEY (Responsable_Id) REFERENCES Usuario(Id) ON DELETE SET NULL
);

CREATE INDEX idx_mantenimiento_activo ON Mantenimiento(Activo_Id);
CREATE INDEX idx_mantenimiento_estado ON Mantenimiento(Estado);

-- Tabla: Depreciacion
CREATE TABLE Depreciacion (
    Id SERIAL PRIMARY KEY,
    Activo_Id INTEGER NOT NULL,
    Periodo INTEGER NOT NULL,
    Fecha_Periodo DATE NOT NULL,
    Valor_Inicial DECIMAL(12,2) NOT NULL,
    Depreciacion_Mensual DECIMAL(12,2) NOT NULL,
    Depreciacion_Acumulada DECIMAL(12,2) NOT NULL,
    Valor_Neto DECIMAL(12,2) NOT NULL,
    Fecha_Calculo TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Activo_Id) REFERENCES Activo(Id) ON DELETE CASCADE
);

COMMENT ON COLUMN Depreciacion.Periodo IS 'Mes desde adquisición';

-- ============================================
-- MÓDULO 5: GESTIÓN DE RESERVAS
-- ============================================

-- Tabla: Reserva
CREATE TABLE Reserva (
    Id SERIAL PRIMARY KEY,
    Empresa_Id INTEGER NOT NULL,
    Cliente_Id INTEGER NOT NULL,
    Codigo_Reserva VARCHAR(20) UNIQUE NOT NULL,
    Estado VARCHAR(20) NOT NULL DEFAULT 'Solicitado' CHECK (Estado IN ('Solicitado', 'Confirmado', 'En_Alistamiento', 'En_Transito_Entrega', 'En_Cliente', 'En_Transito_Devolucion', 'En_Verificacion', 'Completado', 'Cancelado')),
    Fecha_Evento DATE NOT NULL,
    Fecha_Entrega TIMESTAMP,
    Fecha_Devolucion_Programada TIMESTAMP,
    Fecha_Devolucion_Real TIMESTAMP NULL,
    Direccion_Entrega VARCHAR(250),
    Ciudad_Entrega VARCHAR(100),
    Contacto_En_Sitio VARCHAR(150),
    Telefono_Contacto VARCHAR(20),
    Subtotal DECIMAL(12,2) DEFAULT 0,
    Descuento DECIMAL(12,2) DEFAULT 0,
    Total DECIMAL(12,2) NOT NULL,
    Fianza DECIMAL(12,2) DEFAULT 0,
    Fianza_Devuelta BOOLEAN DEFAULT FALSE,
    Metodo_Pago VARCHAR(20) DEFAULT 'Efectivo' CHECK (Metodo_Pago IN ('Efectivo', 'Transferencia', 'Tarjeta', 'Credito')),
    Estado_Pago VARCHAR(20) DEFAULT 'Pendiente' CHECK (Estado_Pago IN ('Pendiente', 'Parcial', 'Pagado')),
    Observaciones TEXT,
    Creado_Por_Id INTEGER NOT NULL,
    Aprobado_Por_Id INTEGER NULL,
    Fecha_Aprobacion TIMESTAMP NULL,
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Fecha_Vencimiento_Cotizacion TIMESTAMP NULL,
    Cancelado_Por_Id INTEGER NULL,
    Razon_Cancelacion TEXT NULL,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Cliente_Id) REFERENCES Cliente(Id),
    FOREIGN KEY (Creado_Por_Id) REFERENCES Usuario(Id),
    FOREIGN KEY (Aprobado_Por_Id) REFERENCES Usuario(Id) ON DELETE SET NULL,
    FOREIGN KEY (Cancelado_Por_Id) REFERENCES Usuario(Id) ON DELETE SET NULL
);

COMMENT ON COLUMN Reserva.Codigo_Reserva IS 'Código público de la reserva';
COMMENT ON COLUMN Reserva.Fianza IS 'Garantía';

CREATE INDEX idx_reserva_codigo ON Reserva(Codigo_Reserva);
CREATE INDEX idx_reserva_estado ON Reserva(Estado);
CREATE INDEX idx_reserva_fecha_evento ON Reserva(Fecha_Evento);
CREATE INDEX idx_reserva_cliente ON Reserva(Cliente_Id);

CREATE TRIGGER update_reserva_updated_at BEFORE UPDATE ON Reserva
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Tabla: Detalle_Reserva
CREATE TABLE Detalle_Reserva (
    Id SERIAL PRIMARY KEY,
    Reserva_Id INTEGER NOT NULL,
    Producto_Id INTEGER NULL,
    Activo_Id INTEGER NULL,
    Cantidad INTEGER NOT NULL DEFAULT 1,
    Precio_Unitario DECIMAL(10,2) NOT NULL,
    Subtotal DECIMAL(12,2) NOT NULL,
    Dias_Alquiler INTEGER NOT NULL DEFAULT 1,
    Observaciones TEXT,
    Estado_Item VARCHAR(20) DEFAULT 'OK' CHECK (Estado_Item IN ('OK', 'Dañado', 'Faltante')),
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Reserva_Id) REFERENCES Reserva(Id) ON DELETE CASCADE,
    FOREIGN KEY (Producto_Id) REFERENCES Producto(Id),
    FOREIGN KEY (Activo_Id) REFERENCES Activo(Id)
);

COMMENT ON COLUMN Detalle_Reserva.Producto_Id IS 'Para inventario básico';
COMMENT ON COLUMN Detalle_Reserva.Activo_Id IS 'Para activos individuales';
COMMENT ON COLUMN Detalle_Reserva.Estado_Item IS 'Estado al devolver';

CREATE INDEX idx_detalle_reserva_reserva ON Detalle_Reserva(Reserva_Id);
CREATE INDEX idx_detalle_reserva_producto ON Detalle_Reserva(Producto_Id);
CREATE INDEX idx_detalle_reserva_activo ON Detalle_Reserva(Activo_Id);

-- Tabla: Pago
CREATE TABLE Pago (
    Id SERIAL PRIMARY KEY,
    Reserva_Id INTEGER NOT NULL,
    Cliente_Id INTEGER NOT NULL,
    Metodo_Pago VARCHAR(20) NOT NULL,
    Monto DECIMAL(12,2) NOT NULL,
    Fecha_Pago TIMESTAMP NOT NULL,
    Numero_Transaccion VARCHAR(100),
    Estado VARCHAR(20) DEFAULT 'Completado' CHECK (Estado IN ('Pendiente', 'Completado', 'Rechazado', 'Reembolsado')),
    Comprobante_URL VARCHAR(500),
    Observaciones TEXT,
    Registrado_Por_Id INTEGER NOT NULL,
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Reserva_Id) REFERENCES Reserva(Id) ON DELETE CASCADE,
    FOREIGN KEY (Cliente_Id) REFERENCES Cliente(Id),
    FOREIGN KEY (Registrado_Por_Id) REFERENCES Usuario(Id)
);

CREATE INDEX idx_pago_reserva ON Pago(Reserva_Id);
CREATE INDEX idx_pago_estado ON Pago(Estado);

-- Tabla: Transaccion_Pago
CREATE TABLE Transaccion_Pago (
    Id SERIAL PRIMARY KEY,
    Reserva_Id INTEGER NOT NULL,
    Monto DECIMAL(12,2) NOT NULL,
    Tipo VARCHAR(20) NOT NULL CHECK (Tipo IN ('Pago', 'Devolucion_Fianza', 'Reembolso')),
    Metodo VARCHAR(20) NOT NULL,
    Referencia_Externa VARCHAR(100),
    Comprobante_URL VARCHAR(500),
    Fecha_Transaccion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Registrado_Por_Usuario_Id INTEGER NOT NULL,
    FOREIGN KEY (Reserva_Id) REFERENCES Reserva(Id) ON DELETE CASCADE,
    FOREIGN KEY (Registrado_Por_Usuario_Id) REFERENCES Usuario(Id)
);

COMMENT ON COLUMN Transaccion_Pago.Referencia_Externa IS 'ID de transacción de Stripe/PayU/Banco';

CREATE INDEX idx_transaccion_reserva ON Transaccion_Pago(Reserva_Id);

-- ============================================
-- MÓDULO 6: COMUNICACIÓN
-- ============================================

-- Tabla: Conversacion
CREATE TABLE Conversacion (
    Id SERIAL PRIMARY KEY,
    Empresa_Id INTEGER NOT NULL,
    Cliente_Id INTEGER NULL,
    Usuario_Id INTEGER NULL,
    Asunto VARCHAR(200),
    Reserva_Id INTEGER NULL,
    Estado VARCHAR(20) DEFAULT 'Abierta' CHECK (Estado IN ('Abierta', 'Cerrada', 'Archivada')),
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Cliente_Id) REFERENCES Cliente(Id) ON DELETE SET NULL,
    FOREIGN KEY (Usuario_Id) REFERENCES Usuario(Id) ON DELETE SET NULL,
    FOREIGN KEY (Reserva_Id) REFERENCES Reserva(Id) ON DELETE SET NULL
);

CREATE INDEX idx_conversacion_empresa ON Conversacion(Empresa_Id);
CREATE INDEX idx_conversacion_cliente ON Conversacion(Cliente_Id);
CREATE INDEX idx_conversacion_estado ON Conversacion(Estado);

-- Tabla: Mensaje
CREATE TABLE Mensaje (
    Id BIGSERIAL PRIMARY KEY,
    Conversacion_Id INTEGER NOT NULL,
    Emisor_Usuario_Id INTEGER NOT NULL,
    Contenido TEXT NOT NULL,
    Leido BOOLEAN DEFAULT FALSE,
    Fecha_Envio TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Conversacion_Id) REFERENCES Conversacion(Id) ON DELETE CASCADE,
    FOREIGN KEY (Emisor_Usuario_Id) REFERENCES Usuario(Id)
);

CREATE INDEX idx_mensaje_conversacion ON Mensaje(Conversacion_Id);
CREATE INDEX idx_mensaje_fecha ON Mensaje(Fecha_Envio);

-- Tabla: Notificacion
CREATE TABLE Notificacion (
    Id SERIAL PRIMARY KEY,
    Empresa_Id INTEGER NULL,
    Usuario_Id INTEGER NULL,
    Tipo VARCHAR(20) NOT NULL CHECK (Tipo IN ('Email', 'SMS', 'Push', 'Sistema')),
    Titulo VARCHAR(200) NOT NULL,
    Mensaje TEXT NOT NULL,
    Destinatario_Email VARCHAR(100),
    Destinatario_Telefono VARCHAR(20),
    Estado VARCHAR(20) DEFAULT 'Pendiente' CHECK (Estado IN ('Pendiente', 'Enviado', 'Error')),
    Leido BOOLEAN DEFAULT FALSE,
    Fecha_Envio TIMESTAMP NULL,
    Fecha_Lectura TIMESTAMP NULL,
    Error_Mensaje TEXT,
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Usuario_Id) REFERENCES Usuario(Id) ON DELETE SET NULL
);

CREATE INDEX idx_notificacion_empresa ON Notificacion(Empresa_Id);
CREATE INDEX idx_notificacion_usuario ON Notificacion(Usuario_Id);
CREATE INDEX idx_notificacion_estado ON Notificacion(Estado);

-- ============================================
-- MÓDULO 7: AUDITORÍA Y TRAZABILIDAD
-- ============================================

-- Tabla: Log_Auditoria (Inmutable)
CREATE TABLE Log_Auditoria (
    Id BIGSERIAL PRIMARY KEY,
    Empresa_Id INTEGER NULL,
    Usuario_Id INTEGER NOT NULL,
    Nombre_Usuario VARCHAR(150),
    Tipo_Operacion VARCHAR(20) NOT NULL CHECK (Tipo_Operacion IN ('INSERT', 'UPDATE', 'DELETE', 'LOGIN')),
    Tabla_Afectada VARCHAR(100),
    Registro_Id INTEGER,
    Valores_Anteriores JSONB,
    Valores_Nuevos JSONB,
    IP_Address VARCHAR(45),
    User_Agent VARCHAR(500),
    Hash_Integridad VARCHAR(64),
    Fecha_Operacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE SET NULL,
    FOREIGN KEY (Usuario_Id) REFERENCES Usuario(Id) ON DELETE CASCADE
);

COMMENT ON COLUMN Log_Auditoria.Hash_Integridad IS 'SHA-256 del registro';

CREATE INDEX idx_log_auditoria_tabla ON Log_Auditoria(Tabla_Afectada);
CREATE INDEX idx_log_auditoria_registro ON Log_Auditoria(Registro_Id);
CREATE INDEX idx_log_auditoria_fecha ON Log_Auditoria(Fecha_Operacion);
CREATE INDEX idx_log_auditoria_usuario ON Log_Auditoria(Usuario_Id);

-- ============================================
-- MÓDULO 8: CONFIGURACIÓN
-- ============================================

-- Tabla: Configuracion_Sistema
CREATE TABLE Configuracion_Sistema (
    Id SERIAL PRIMARY KEY,
    Empresa_Id INTEGER NULL,
    Clave VARCHAR(100) NOT NULL,
    Valor TEXT,
    Descripcion TEXT,
    Tipo_Dato VARCHAR(20) DEFAULT 'string' CHECK (Tipo_Dato IN ('string', 'int', 'bool', 'json')),
    Es_Global BOOLEAN DEFAULT FALSE,
    Fecha_Actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    UNIQUE (Empresa_Id, Clave)
);

CREATE INDEX idx_configuracion_empresa ON Configuracion_Sistema(Empresa_Id);

CREATE TRIGGER update_configuracion_updated_at BEFORE UPDATE ON Configuracion_Sistema
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Tabla: Contenido_Landing
CREATE TABLE Contenido_Landing (
    Id SERIAL PRIMARY KEY,
    Seccion VARCHAR(50) NOT NULL,
    Titulo VARCHAR(200),
    Subtitulo VARCHAR(200),
    Descripcion TEXT,
    Imagen_URL VARCHAR(500),
    Icono_Nombre VARCHAR(50),
    Orden INTEGER DEFAULT 0,
    Activo BOOLEAN DEFAULT TRUE,
    Fecha_Actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON COLUMN Contenido_Landing.Seccion IS 'hero, servicios, nosotros, contacto';
COMMENT ON COLUMN Contenido_Landing.Icono_Nombre IS 'Nombre del icono';

CREATE INDEX idx_contenido_seccion ON Contenido_Landing(Seccion);
CREATE INDEX idx_contenido_activo ON Contenido_Landing(Activo);

CREATE TRIGGER update_contenido_landing_updated_at BEFORE UPDATE ON Contenido_Landing
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- ============================================
-- CATÁLOGOS
-- ============================================

-- Tabla: catalogo_estado_reserva
CREATE TABLE catalogo_estado_reserva (
    Id SERIAL PRIMARY KEY,
    Codigo VARCHAR(50) UNIQUE NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion TEXT,
    Color VARCHAR(7),
    Orden INTEGER DEFAULT 0,
    Activo BOOLEAN DEFAULT TRUE,
    Sistema BOOLEAN DEFAULT FALSE,
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON COLUMN catalogo_estado_reserva.Sistema IS 'Estados del sistema no se pueden eliminar';

-- Tabla: catalogo_estado_activo
CREATE TABLE catalogo_estado_activo (
    Id SERIAL PRIMARY KEY,
    Codigo VARCHAR(50) UNIQUE NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion TEXT,
    Color VARCHAR(7),
    Permite_Reserva BOOLEAN DEFAULT TRUE,
    Orden INTEGER DEFAULT 0,
    Activo BOOLEAN DEFAULT TRUE,
    Sistema BOOLEAN DEFAULT FALSE,
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabla: catalogo_metodo_pago
CREATE TABLE catalogo_metodo_pago (
    Id SERIAL PRIMARY KEY,
    Codigo VARCHAR(50) UNIQUE NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion TEXT,
    Requiere_Comprobante BOOLEAN DEFAULT FALSE,
    Requiere_Referencia BOOLEAN DEFAULT FALSE,
    Activo BOOLEAN DEFAULT TRUE,
    Orden INTEGER DEFAULT 0,
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabla: catalogo_tipo_mantenimiento
CREATE TABLE catalogo_tipo_mantenimiento (
    Id SERIAL PRIMARY KEY,
    Codigo VARCHAR(50) UNIQUE NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion TEXT,
    Es_Preventivo BOOLEAN DEFAULT FALSE,
    Activo BOOLEAN DEFAULT TRUE,
    Orden INTEGER DEFAULT 0,
    Fecha_Creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ============================================
-- ÍNDICES ADICIONALES PARA OPTIMIZACIÓN
-- ============================================

CREATE INDEX idx_reserva_empresa_estado_fecha ON Reserva(Empresa_Id, Estado, Fecha_Evento);
CREATE INDEX idx_activo_empresa_disponibilidad ON Activo(Empresa_Id, Estado_Disponibilidad);
CREATE INDEX idx_movimiento_empresa_fecha ON Movimiento_Inventario(Empresa_Id, Fecha_Movimiento);
CREATE INDEX idx_log_empresa_fecha ON Log_Auditoria(Empresa_Id, Fecha_Operacion);

-- ============================================
-- DATOS INICIALES
-- ============================================

-- Insertar Roles
INSERT INTO Rol (Nombre, Descripcion, Nivel_Acceso, Permisos) VALUES
('SuperAdmin', 'Control total del sistema', 0, '["*"]'::jsonb),
('Admin-Proveedor', 'Administrador de empresa proveedora', 1, '["empresa.*", "inventario.*", "reservas.*", "clientes.*", "usuarios.read"]'::jsonb),
('Operario-Logística', 'Operador de campo para entregas', 2, '["reservas.read", "logistica.*", "evidencias.create"]'::jsonb),
('Cliente-Final', 'Cliente con portal de autogestión', 3, '["reservas.create", "reservas.read_own"]'::jsonb),
('Contador-Auditor', 'Auditor con acceso solo lectura', 4, '["inventario.read", "reportes.read", "logs.read"]'::jsonb);

-- Insertar Planes
INSERT INTO Plan (Nombre, Descripcion, Precio_Mensual, Duracion_Prueba_Dias, Modulos_Incluidos) VALUES
('Plan Básico', 'Funcionalidades básicas de gestión', 0.00, 0, '["core", "inventario_basico", "reservas", "logistica"]'::jsonb),
('Plan Premium S.I.G.I.', 'Sistema de Gestión Integral de Inventarios', 150000.00, 3, '["core", "inventario_basico", "SIGI", "reservas", "logistica", "analytics"]'::jsonb),
('Plan Enterprise', 'Funcionalidades completas + B2B', 300000.00, 7, '["*"]'::jsonb);

-- Insertar Empresa SuperAdmin
INSERT INTO Empresa (Razon_Social, NIT, Email, Telefono, Ciudad, Pais, Estado) VALUES
('EventConnect System', '900000000-0', 'admin@eventconnect.com', '+57 300 000 0000', 'Bogotá', 'Colombia', 'Activa');

-- Insertar Usuario SuperAdmin
-- Password: SuperAdmin123$
INSERT INTO Usuario (Empresa_Id, Rol_Id, Usuario, Email, Password_Hash, Nombre_Completo, Estado) VALUES
(NULL, (SELECT Id FROM Rol WHERE Nombre = 'SuperAdmin'), 'superadmin', 'admin@eventconnect.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIbHc3lW5G', 'Super Administrador', 'Activo');

-- Insertar Empresa de Ejemplo
INSERT INTO Empresa (Razon_Social, NIT, Email, Telefono, Direccion, Ciudad, Pais, Estado) VALUES
('Eventos y Mobiliario Premium SAS', '900123456-7', 'contacto@eventospremium.com', '+57 310 123 4567', 'Calle 100 #15-20', 'Bogotá', 'Colombia', 'Activa');

-- Insertar Usuario Admin de la empresa
-- Usamos el ID de la empresa recién creada (será 2 si es la segunda empresa)
INSERT INTO Usuario (Empresa_Id, Rol_Id, Usuario, Email, Password_Hash, Nombre_Completo, Telefono, Estado) VALUES
((SELECT Id FROM Empresa WHERE NIT = '900123456-7'), (SELECT Id FROM Rol WHERE Nombre = 'Admin-Proveedor'), 'admin_empresa', 'admin@eventospremium.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIbHc3lW5G', 'Admin Empresa Demo', '+57 310 123 4567', 'Activo');
-- Password: Admin123$

-- Insertar categorías de ejemplo (Categoria es global, no tiene Empresa_Id)
INSERT INTO Categoria (Nombre, Descripcion, Icono, Color) VALUES
('Mobiliario', 'Sillas, mesas y mobiliario en general', '🪑', '#3B82F6'),
('Iluminación', 'Equipos de iluminación y decoración', '💡', '#F59E0B'),
('Sonido', 'Equipos de sonido y audio', '🔊', '#10B981'),
('Carpas y Toldos', 'Carpas, toldos y estructuras', '⛺', '#EF4444'),
('Vajilla', 'Platos, vasos, cubiertos', '🍽️', '#8B5CF6');

-- ============================================
-- FIN DEL SCRIPT
-- ============================================

-- Mensaje de confirmación
DO $$
DECLARE
    total_tablas INTEGER;
    total_roles INTEGER;
    total_planes INTEGER;
    total_empresas INTEGER;
    total_usuarios INTEGER;
    total_categorias INTEGER;
BEGIN
    -- Contar tablas
    SELECT COUNT(*) INTO total_tablas
    FROM information_schema.tables 
    WHERE table_schema = 'public' 
    AND table_type = 'BASE TABLE';
    
    -- Contar datos insertados
    SELECT COUNT(*) INTO total_roles FROM Rol;
    SELECT COUNT(*) INTO total_planes FROM Plan;
    SELECT COUNT(*) INTO total_empresas FROM Empresa;
    SELECT COUNT(*) INTO total_usuarios FROM Usuario;
    SELECT COUNT(*) INTO total_categorias FROM Categoria;
    
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Base de datos EventConnect creada exitosamente para PostgreSQL!';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Total de tablas creadas: %', total_tablas;
    RAISE NOTICE 'Datos insertados:';
    RAISE NOTICE '  - Roles: %', total_roles;
    RAISE NOTICE '  - Planes: %', total_planes;
    RAISE NOTICE '  - Empresas: %', total_empresas;
    RAISE NOTICE '  - Usuarios: %', total_usuarios;
    RAISE NOTICE '  - Categorías: %', total_categorias;
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Credenciales de acceso:';
    RAISE NOTICE '  SuperAdmin - Usuario: superadmin';
    RAISE NOTICE '  SuperAdmin - Password: SuperAdmin123$';
    RAISE NOTICE '  Empresa Demo - Usuario: admin_empresa';
    RAISE NOTICE '  Empresa Demo - Password: Admin123$';
    RAISE NOTICE '========================================';
END $$;
