-- ============================================
-- EventConnect - Base de Datos Completa
-- Sistema de Gestión de Activos y Alquileres
-- Versión: 1.0.0
-- ============================================

DROP DATABASE IF EXISTS db_eventconnect;
CREATE DATABASE db_eventconnect CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE db_eventconnect;

-- ============================================
-- MÓDULO 1: CORE - Gestión de Entidades
-- ============================================

-- Tabla: Empresa (Multi-tenancy)
CREATE TABLE Empresa (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Razon_Social VARCHAR(200) NOT NULL,
    NIT VARCHAR(20) UNIQUE NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Telefono VARCHAR(20),
    Direccion VARCHAR(250),
    Ciudad VARCHAR(100),
    Pais VARCHAR(100) DEFAULT 'Colombia',
    Logo_URL VARCHAR(500),
    Estado ENUM('Activa', 'Inactiva', 'Suspendida') DEFAULT 'Activa',
    Fecha_Registro DATETIME DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_nit (NIT),
    INDEX idx_estado (Estado)
) ENGINE=InnoDB;

-- Tabla: Rol
CREATE TABLE Rol (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Nombre VARCHAR(50) UNIQUE NOT NULL,
    Descripcion VARCHAR(250),
    Nivel_Acceso INT NOT NULL COMMENT '0=SuperAdmin, 1=Admin-Proveedor, 2=Operario, 3=Cliente, 4=Auditor',
    Permisos JSON COMMENT 'Array de permisos específicos',
    Fecha_Creacion DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- Tabla: Usuario
CREATE TABLE Usuario (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NULL COMMENT 'NULL para SuperAdmin',
    Rol_Id INT NOT NULL,
    Usuario VARCHAR(50) UNIQUE NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    Password_Hash VARCHAR(255) NOT NULL,
    Nombre_Completo VARCHAR(150) NOT NULL,
    Telefono VARCHAR(20),
    Avatar_URL VARCHAR(500),
    Estado ENUM('Activo', 'Inactivo', 'Bloqueado') DEFAULT 'Activo',
    Intentos_Fallidos INT DEFAULT 0,
    Ultimo_Acceso DATETIME NULL,
    Fecha_Creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    Requiere_Cambio_Password BOOLEAN DEFAULT FALSE,
    TwoFA_Activo BOOLEAN DEFAULT FALSE,
    TwoFA_Secret VARCHAR(100) NULL,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Rol_Id) REFERENCES Rol(Id),
    INDEX idx_usuario (Usuario),
    INDEX idx_email (Email),
    INDEX idx_empresa (Empresa_Id)
) ENGINE=InnoDB;

-- Tabla: Cliente (Clientes finales de las empresas)
CREATE TABLE Cliente (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NOT NULL COMMENT 'Empresa proveedora que gestiona este cliente',
    Tipo_Cliente ENUM('Persona', 'Empresa') NOT NULL,
    Nombre VARCHAR(150) NOT NULL,
    Documento VARCHAR(50) NOT NULL,
    Tipo_Documento ENUM('CC', 'NIT', 'CE', 'Pasaporte') NOT NULL,
    Email VARCHAR(100),
    Telefono VARCHAR(20),
    Direccion VARCHAR(250),
    Ciudad VARCHAR(100),
    Contacto_Nombre VARCHAR(150) COMMENT 'Persona de contacto si es empresa',
    Contacto_Telefono VARCHAR(20),
    Observaciones TEXT,
    Rating DECIMAL(3,2) DEFAULT 5.00 COMMENT 'Calificación del cliente (1-5)',
    Total_Alquileres INT DEFAULT 0,
    Total_Danos_Reportados INT DEFAULT 0,
    Estado ENUM('Activo', 'Inactivo', 'Bloqueado') DEFAULT 'Activo',
    Fecha_Registro DATETIME DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    UNIQUE KEY uk_empresa_documento (Empresa_Id, Documento),
    INDEX idx_nombre (Nombre),
    INDEX idx_documento (Documento)
) ENGINE=InnoDB;

-- ============================================
-- MÓDULO 2: SUSCRIPCIONES Y PLANES
-- ============================================

-- Tabla: Plan (Definición de planes disponibles)
CREATE TABLE Plan (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion TEXT,
    Precio_Mensual DECIMAL(10,2) NOT NULL,
    Duracion_Prueba_Dias INT DEFAULT 0,
    Modulos_Incluidos JSON COMMENT 'Array de módulos incluidos',
    Limite_Usuarios INT DEFAULT NULL COMMENT 'NULL = ilimitado',
    Limite_Productos INT DEFAULT NULL,
    Activo BOOLEAN DEFAULT TRUE,
    Fecha_Creacion DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- Tabla: Suscripcion
CREATE TABLE Suscripcion (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NOT NULL,
    Plan_Id INT NOT NULL,
    Modulo ENUM('SIGI', 'B2B', 'Analytics_Pro', 'Mobile_Pro') NOT NULL,
    Estado ENUM('Prueba', 'Activa', 'Vencida', 'Cancelada') NOT NULL DEFAULT 'Prueba',
    Fecha_Inicio DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Fecha_Fin_Prueba DATETIME NULL,
    Fecha_Vencimiento DATETIME NULL,
    Auto_Renovar BOOLEAN DEFAULT TRUE,
    Costo_Mensual DECIMAL(10,2) NOT NULL,
    Fecha_Ultimo_Pago DATETIME NULL,
    Metodo_Pago VARCHAR(50),
    Fecha_Creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    Fecha_Cancelacion DATETIME NULL,
    Razon_Cancelacion TEXT NULL,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Plan_Id) REFERENCES Plan(Id),
    INDEX idx_empresa_modulo (Empresa_Id, Modulo),
    INDEX idx_estado (Estado),
    INDEX idx_vencimiento (Fecha_Vencimiento)
) ENGINE=InnoDB;

-- ============================================
-- MÓDULO 3: INVENTARIO BÁSICO
-- ============================================

-- Tabla: Categoria
CREATE TABLE Categoria (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion TEXT,
    Icono VARCHAR(50),
    Color VARCHAR(7) COMMENT 'Color hexadecimal #RRGGBB',
    Activo BOOLEAN DEFAULT TRUE,
    Fecha_Creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    UNIQUE KEY uk_empresa_nombre (Empresa_Id, Nombre),
    INDEX idx_activo (Activo)
) ENGINE=InnoDB;

-- Tabla: Producto
CREATE TABLE Producto (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NOT NULL,
    Categoria_Id INT NOT NULL,
    SKU VARCHAR(50) NOT NULL COMMENT 'Código único del producto',
    Nombre VARCHAR(150) NOT NULL,
    Descripcion TEXT,
    Unidad_Medida ENUM('Unidad', 'Par', 'Set', 'Metro', 'Metro²', 'Kilo') DEFAULT 'Unidad',
    Precio_Alquiler_Dia DECIMAL(10,2) NOT NULL,
    Cantidad_Stock INT DEFAULT 0 COMMENT 'Para inventario básico',
    Stock_Minimo INT DEFAULT 10,
    Imagen_URL VARCHAR(500),
    Es_Alquilable BOOLEAN DEFAULT TRUE,
    Es_Vendible BOOLEAN DEFAULT FALSE,
    Requiere_Mantenimiento BOOLEAN DEFAULT FALSE,
    Dias_Mantenimiento INT DEFAULT 90 COMMENT 'Cada cuántos días requiere mantenimiento',
    Peso_Kg DECIMAL(8,2),
    Dimensiones VARCHAR(50) COMMENT 'Ej: 1m x 0.5m x 0.8m',
    Observaciones TEXT,
    Activo BOOLEAN DEFAULT TRUE,
    Fecha_Creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Categoria_Id) REFERENCES Categoria(Id),
    UNIQUE KEY uk_empresa_sku (Empresa_Id, SKU),
    INDEX idx_nombre (Nombre),
    INDEX idx_activo (Activo)
) ENGINE=InnoDB;

-- ============================================
-- MÓDULO 4: S.I.G.I. - INVENTARIO AVANZADO
-- ============================================

-- Tabla: Bodega
CREATE TABLE Bodega (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NOT NULL,
    Codigo VARCHAR(20) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Tipo_Ubicacion ENUM('Principal', 'Secundaria', 'Punto_Venta', 'Almacen_Temporal') DEFAULT 'Principal',
    Direccion VARCHAR(250),
    Ciudad VARCHAR(100),
    Responsable_Id INT NULL COMMENT 'Usuario responsable de la bodega',
    Telefono VARCHAR(20),
    Capacidad_M2 DECIMAL(10,2),
    Activa BOOLEAN DEFAULT TRUE,
    Fecha_Creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Responsable_Id) REFERENCES Usuario(Id) ON DELETE SET NULL,
    UNIQUE KEY uk_empresa_codigo (Empresa_Id, Codigo),
    INDEX idx_activa (Activa)
) ENGINE=InnoDB;

-- Tabla: Activo (Ítems individuales con hoja de vida)
CREATE TABLE Activo (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NOT NULL,
    Producto_Id INT NOT NULL,
    Bodega_Id INT NOT NULL,
    Codigo_Activo VARCHAR(50) UNIQUE NOT NULL COMMENT 'Código único QR/RFID',
    Numero_Serie VARCHAR(100) UNIQUE NULL,
    Estado_Fisico ENUM('Nuevo', 'Excelente', 'Bueno', 'Regular', 'Malo') DEFAULT 'Nuevo',
    Estado_Disponibilidad ENUM('Disponible', 'Alquilado', 'En_Mantenimiento', 'Dado_de_Baja') DEFAULT 'Disponible',
    Fecha_Compra DATE,
    Costo_Compra DECIMAL(12,2),
    Proveedor VARCHAR(150),
    Vida_Util_Anos INT DEFAULT 5,
    Valor_Residual DECIMAL(12,2) DEFAULT 0,
    Depreciacion_Acumulada DECIMAL(12,2) DEFAULT 0,
    Foto_Registro_URL VARCHAR(500),
    QR_Code_URL VARCHAR(500) COMMENT 'URL de imagen QR generada',
    Total_Alquileres INT DEFAULT 0,
    Ingresos_Totales DECIMAL(12,2) DEFAULT 0,
    Costos_Mantenimiento DECIMAL(12,2) DEFAULT 0,
    Ultimo_Mantenimiento DATE NULL,
    Proximo_Mantenimiento DATE NULL,
    Observaciones TEXT,
    Activo BOOLEAN DEFAULT TRUE,
    Fecha_Creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Producto_Id) REFERENCES Producto(Id),
    FOREIGN KEY (Bodega_Id) REFERENCES Bodega(Id),
    INDEX idx_codigo (Codigo_Activo),
    INDEX idx_estado (Estado_Disponibilidad),
    INDEX idx_bodega (Bodega_Id)
) ENGINE=InnoDB;

-- Tabla: Lote (Para productos con fecha de vencimiento)
CREATE TABLE Lote (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NOT NULL,
    Producto_Id INT NOT NULL,
    Bodega_Id INT NOT NULL,
    Numero_Lote VARCHAR(50) NOT NULL,
    Fecha_Fabricacion DATE,
    Fecha_Vencimiento DATE,
    Cantidad_Inicial INT NOT NULL,
    Cantidad_Actual INT NOT NULL,
    Costo_Unitario DECIMAL(10,2) NOT NULL,
    Proveedor VARCHAR(150),
    Documento_Soporte VARCHAR(100) COMMENT 'Número de factura o remisión',
    Estado ENUM('Activo', 'Vencido', 'Agotado') DEFAULT 'Activo',
    Fecha_Registro DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Producto_Id) REFERENCES Producto(Id),
    FOREIGN KEY (Bodega_Id) REFERENCES Bodega(Id),
    UNIQUE KEY uk_empresa_lote (Empresa_Id, Numero_Lote),
    INDEX idx_vencimiento (Fecha_Vencimiento),
    INDEX idx_estado (Estado)
) ENGINE=InnoDB;

-- Tabla: Movimiento_Inventario
CREATE TABLE Movimiento_Inventario (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NOT NULL,
    Tipo_Movimiento ENUM('Entrada_Compra', 'Entrada_Devolucion', 'Salida_Alquiler', 'Salida_Venta', 'Salida_Dano', 'Ajuste_Positivo', 'Ajuste_Negativo', 'Transferencia_Entrada', 'Transferencia_Salida') NOT NULL,
    Producto_Id INT NULL,
    Activo_Id INT NULL COMMENT 'Si es un movimiento de activo específico',
    Lote_Id INT NULL,
    Bodega_Origen_Id INT NULL,
    Bodega_Destino_Id INT NULL,
    Cantidad DECIMAL(10,3) NOT NULL,
    Costo_Unitario DECIMAL(12,2),
    Costo_Total DECIMAL(12,2),
    Metodo_Costeo ENUM('PEPS', 'UEPS', 'Promedio') DEFAULT 'Promedio',
    Documento_Tipo VARCHAR(50) COMMENT 'Factura, Remisión, Orden de Compra, etc.',
    Documento_Numero VARCHAR(100),
    Razon TEXT NOT NULL COMMENT 'Motivo del movimiento',
    Usuario_Id INT NOT NULL,
    Reserva_Id INT NULL COMMENT 'Si está asociado a una reserva',
    Requiere_Autorizacion BOOLEAN DEFAULT FALSE,
    Autorizado_Por_Id INT NULL,
    Fecha_Autorizacion DATETIME NULL,
    Evidencia_URL VARCHAR(500) COMMENT 'Foto de soporte',
    Hash_Integridad VARCHAR(64) COMMENT 'SHA-256 para inmutabilidad',
    Fecha_Movimiento DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Producto_Id) REFERENCES Producto(Id),
    FOREIGN KEY (Activo_Id) REFERENCES Activo(Id),
    FOREIGN KEY (Lote_Id) REFERENCES Lote(Id),
    FOREIGN KEY (Bodega_Origen_Id) REFERENCES Bodega(Id),
    FOREIGN KEY (Bodega_Destino_Id) REFERENCES Bodega(Id),
    FOREIGN KEY (Usuario_Id) REFERENCES Usuario(Id),
    FOREIGN KEY (Autorizado_Por_Id) REFERENCES Usuario(Id) ON DELETE SET NULL,
    INDEX idx_tipo (Tipo_Movimiento),
    INDEX idx_fecha (Fecha_Movimiento),
    INDEX idx_producto (Producto_Id),
    INDEX idx_activo (Activo_Id)
) ENGINE=InnoDB;

-- Tabla: Mantenimiento_Activo
CREATE TABLE Mantenimiento_Activo (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Activo_Id INT NOT NULL,
    Tipo_Mantenimiento ENUM('Preventivo', 'Correctivo', 'Limpieza', 'Reparacion') NOT NULL,
    Descripcion TEXT NOT NULL,
    Costo DECIMAL(10,2) DEFAULT 0,
    Responsable_Id INT NOT NULL,
    Proveedor_Externo VARCHAR(150) NULL COMMENT 'Si el mantenimiento es externo',
    Fecha_Inicio DATE NOT NULL,
    Fecha_Fin DATE NULL,
    Estado ENUM('Programado', 'En_Proceso', 'Completado', 'Cancelado') DEFAULT 'Programado',
    Observaciones TEXT,
    Evidencia_URL VARCHAR(500),
    Fecha_Registro DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Activo_Id) REFERENCES Activo(Id) ON DELETE CASCADE,
    FOREIGN KEY (Responsable_Id) REFERENCES Usuario(Id),
    INDEX idx_activo (Activo_Id),
    INDEX idx_fecha_inicio (Fecha_Inicio),
    INDEX idx_estado (Estado)
) ENGINE=InnoDB;

-- Tabla: Clasificacion_ABC (Análisis automático)
CREATE TABLE Clasificacion_ABC (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NOT NULL,
    Producto_Id INT NOT NULL,
    Periodo_Analisis VARCHAR(20) COMMENT 'Ej: 2025-01',
    Clasificacion ENUM('A', 'B', 'C') NOT NULL,
    Valor_Total_Ventas DECIMAL(12,2),
    Cantidad_Ventas INT,
    Porcentaje_Acumulado DECIMAL(5,2),
    Fecha_Calculo DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Producto_Id) REFERENCES Producto(Id) ON DELETE CASCADE,
    UNIQUE KEY uk_periodo_producto (Empresa_Id, Periodo_Analisis, Producto_Id),
    INDEX idx_clasificacion (Clasificacion)
) ENGINE=InnoDB;

-- ============================================
-- MÓDULO 5: GESTIÓN DE RESERVAS
-- ============================================

-- Tabla: Reserva
CREATE TABLE Reserva (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NOT NULL,
    Cliente_Id INT NOT NULL,
    Codigo_Reserva VARCHAR(20) UNIQUE NOT NULL COMMENT 'Código público de la reserva',
    Estado ENUM('Solicitado', 'Confirmado', 'En_Alistamiento', 'En_Transito_Entrega', 'En_Cliente', 'En_Transito_Devolucion', 'En_Verificacion', 'Completado', 'Cancelado') NOT NULL DEFAULT 'Solicitado',
    Fecha_Evento DATE NOT NULL,
    Fecha_Entrega DATETIME,
    Fecha_Devolucion_Programada DATETIME,
    Fecha_Devolucion_Real DATETIME NULL,
    Direccion_Entrega VARCHAR(250),
    Ciudad_Entrega VARCHAR(100),
    Contacto_En_Sitio VARCHAR(150),
    Telefono_Contacto VARCHAR(20),
    Subtotal DECIMAL(12,2) DEFAULT 0,
    Descuento DECIMAL(12,2) DEFAULT 0,
    Total DECIMAL(12,2) NOT NULL,
    Fianza DECIMAL(12,2) DEFAULT 0 COMMENT 'Garantía',
    Fianza_Devuelta BOOLEAN DEFAULT FALSE,
    Metodo_Pago ENUM('Efectivo', 'Transferencia', 'Tarjeta', 'Credito') DEFAULT 'Efectivo',
    Estado_Pago ENUM('Pendiente', 'Parcial', 'Pagado') DEFAULT 'Pendiente',
    Observaciones TEXT,
    Creado_Por_Id INT NOT NULL,
    Aprobado_Por_Id INT NULL,
    Fecha_Aprobacion DATETIME NULL,
    Fecha_Creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    Cancelado_Por_Id INT NULL,
    Razon_Cancelacion TEXT NULL,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    FOREIGN KEY (Cliente_Id) REFERENCES Cliente(Id),
    FOREIGN KEY (Creado_Por_Id) REFERENCES Usuario(Id),
    FOREIGN KEY (Aprobado_Por_Id) REFERENCES Usuario(Id) ON DELETE SET NULL,
    FOREIGN KEY (Cancelado_Por_Id) REFERENCES Usuario(Id) ON DELETE SET NULL,
    INDEX idx_codigo (Codigo_Reserva),
    INDEX idx_estado (Estado),
    INDEX idx_fecha_evento (Fecha_Evento),
    INDEX idx_cliente (Cliente_Id)
) ENGINE=InnoDB;

-- Tabla: Detalle_Reserva
CREATE TABLE Detalle_Reserva (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Reserva_Id INT NOT NULL,
    Producto_Id INT NULL COMMENT 'Para inventario básico',
    Activo_Id INT NULL COMMENT 'Para activos individuales',
    Cantidad INT NOT NULL DEFAULT 1,
    Precio_Unitario DECIMAL(10,2) NOT NULL,
    Subtotal DECIMAL(12,2) NOT NULL,
    Dias_Alquiler INT NOT NULL DEFAULT 1,
    Observaciones TEXT,
    Estado_Item ENUM('OK', 'Dañado', 'Faltante') DEFAULT 'OK' COMMENT 'Estado al devolver',
    Fecha_Creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Reserva_Id) REFERENCES Reserva(Id) ON DELETE CASCADE,
    FOREIGN KEY (Producto_Id) REFERENCES Producto(Id),
    FOREIGN KEY (Activo_Id) REFERENCES Activo(Id),
    INDEX idx_reserva (Reserva_Id),
    INDEX idx_producto (Producto_Id),
    INDEX idx_activo (Activo_Id)
) ENGINE=InnoDB;

-- ============================================
-- MÓDULO 6: LOGÍSTICA Y TRAZABILIDAD
-- ============================================

-- Tabla: Evidencia_Logistica
CREATE TABLE Evidencia_Logistica (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Reserva_Id INT NOT NULL,
    Tipo_Evidencia ENUM('Foto_Carga', 'Foto_Entrega', 'Firma_Entrega', 'Foto_Recogida', 'Firma_Devolucion') NOT NULL,
    Archivo_URL VARCHAR(500) NOT NULL,
    Descripcion TEXT,
    Quien_Recibe VARCHAR(150) COMMENT 'Nombre de quien recibe',
    Latitud DECIMAL(10,8) COMMENT 'Geolocalización',
    Longitud DECIMAL(11,8),
    Usuario_Id INT NOT NULL COMMENT 'Operario que sube la evidencia',
    Fecha_Registro DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Reserva_Id) REFERENCES Reserva(Id) ON DELETE CASCADE,
    FOREIGN KEY (Usuario_Id) REFERENCES Usuario(Id),
    INDEX idx_reserva (Reserva_Id),
    INDEX idx_tipo (Tipo_Evidencia)
) ENGINE=InnoDB;

-- Tabla: Ticket_Dano
CREATE TABLE Ticket_Dano (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Reserva_Id INT NOT NULL,
    Detalle_Reserva_Id INT NOT NULL,
    Activo_Id INT NULL COMMENT 'Si es un activo específico',
    Tipo_Incidencia ENUM('Daño', 'Pérdida', 'Robo') NOT NULL,
    Descripcion TEXT NOT NULL,
    Costo_Estimado_Reparacion DECIMAL(10,2),
    Responsable ENUM('Cliente', 'Empresa', 'Tercero') DEFAULT 'Cliente',
    Estado ENUM('Reportado', 'En_Evaluacion', 'En_Reparacion', 'Resuelto', 'Cobrado') DEFAULT 'Reportado',
    Evidencia_URL VARCHAR(500),
    Reportado_Por_Id INT NOT NULL,
    Fecha_Reporte DATETIME DEFAULT CURRENT_TIMESTAMP,
    Fecha_Resolucion DATETIME NULL,
    Observaciones TEXT,
    FOREIGN KEY (Reserva_Id) REFERENCES Reserva(Id) ON DELETE CASCADE,
    FOREIGN KEY (Detalle_Reserva_Id) REFERENCES Detalle_Reserva(Id) ON DELETE CASCADE,
    FOREIGN KEY (Activo_Id) REFERENCES Activo(Id) ON DELETE SET NULL,
    FOREIGN KEY (Reportado_Por_Id) REFERENCES Usuario(Id),
    INDEX idx_reserva (Reserva_Id),
    INDEX idx_estado (Estado),
    INDEX idx_activo (Activo_Id)
) ENGINE=InnoDB;

-- ============================================
-- MÓDULO 7: AUDITORÍA Y TRAZABILIDAD
-- ============================================

-- Tabla: Log_Auditoria (Inmutable)
CREATE TABLE Log_Auditoria (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NULL,
    Usuario_Id INT NOT NULL,
    Tabla_Afectada VARCHAR(100) NOT NULL,
    Id_Registro_Afectado INT NOT NULL,
    Accion ENUM('INSERT', 'UPDATE', 'DELETE') NOT NULL,
    Valor_Antes JSON COMMENT 'Estado anterior del registro',
    Valor_Despues JSON COMMENT 'Estado nuevo del registro',
    IP_Address VARCHAR(45),
    User_Agent VARCHAR(500),
    Hash_Integridad VARCHAR(64) NOT NULL COMMENT 'SHA-256 del registro',
    Timestamp_UTC DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE SET NULL,
    FOREIGN KEY (Usuario_Id) REFERENCES Usuario(Id) ON DELETE CASCADE,
    INDEX idx_tabla (Tabla_Afectada),
    INDEX idx_registro (Id_Registro_Afectado),
    INDEX idx_timestamp (Timestamp_UTC),
    INDEX idx_usuario (Usuario_Id)
) ENGINE=InnoDB;

-- Tabla: Log_Acceso
CREATE TABLE Log_Acceso (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    Usuario_Id INT NULL,
    Email_Intento VARCHAR(100),
    Exitoso BOOLEAN NOT NULL,
    IP_Address VARCHAR(45),
    User_Agent VARCHAR(500),
    Motivo_Fallo VARCHAR(100) COMMENT 'Ej: Contraseña incorrecta, Usuario bloqueado',
    Fecha_Hora DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Usuario_Id) REFERENCES Usuario(Id) ON DELETE SET NULL,
    INDEX idx_usuario (Usuario_Id),
    INDEX idx_fecha (Fecha_Hora),
    INDEX idx_exitoso (Exitoso)
) ENGINE=InnoDB;

-- ============================================
-- MÓDULO 8: ANALYTICS Y BI (Tablas auxiliares)
-- ============================================

-- Tabla: Alerta_Sistema
CREATE TABLE Alerta_Sistema (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NOT NULL,
    Tipo_Alerta ENUM('Stock_Bajo', 'Producto_Vencido', 'Mantenimiento_Pendiente', 'Reserva_Sin_Confirmar', 'Pago_Vencido', 'Activo_Depreciado') NOT NULL,
    Severidad ENUM('Baja', 'Media', 'Alta', 'Crítica') DEFAULT 'Media',
    Titulo VARCHAR(200) NOT NULL,
    Mensaje TEXT NOT NULL,
    Referencia_Tabla VARCHAR(100) COMMENT 'Tabla relacionada',
    Referencia_Id INT COMMENT 'ID del registro relacionado',
    Leido BOOLEAN DEFAULT FALSE,
    Fecha_Creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    Fecha_Leido DATETIME NULL,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    INDEX idx_empresa (Empresa_Id),
    INDEX idx_leido (Leido),
    INDEX idx_tipo (Tipo_Alerta)
) ENGINE=InnoDB;

-- ============================================
-- MÓDULO 9: CONTENIDO LANDING (Del sistema original)
-- ============================================

CREATE TABLE Contenido_Landing (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Empresa_Id INT NOT NULL,
    Seccion VARCHAR(50) NOT NULL COMMENT 'hero, servicios, testimonios, etc.',
    Titulo VARCHAR(200),
    Subtitulo VARCHAR(200),
    Contenido TEXT,
    Imagen_URL VARCHAR(500),
    Orden INT DEFAULT 0,
    Activo BOOLEAN DEFAULT TRUE,
    Fecha_Creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    Fecha_Actualizacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (Empresa_Id) REFERENCES Empresa(Id) ON DELETE CASCADE,
    INDEX idx_seccion (Seccion),
    INDEX idx_activo (Activo)
) ENGINE=InnoDB;

-- ============================================
-- VISTAS ÚTILES
-- ============================================

-- Vista: Productos con stock bajo
CREATE OR REPLACE VIEW v_productos_stock_bajo AS
SELECT 
    p.Id,
    p.Empresa_Id,
    e.Razon_Social AS Empresa,
    p.SKU,
    p.Nombre,
    p.Cantidad_Stock AS Stock_Actual,
    p.Stock_Minimo,
    c.Nombre AS Categoria
FROM Producto p
INNER JOIN Empresa e ON p.Empresa_Id = e.Id
INNER JOIN Categoria c ON p.Categoria_Id = c.Id
WHERE p.Cantidad_Stock <= p.Stock_Minimo
AND p.Activo = TRUE;

-- Vista: Activos que requieren mantenimiento
CREATE OR REPLACE VIEW v_activos_mantenimiento_pendiente AS
SELECT 
    a.Id,
    a.Empresa_Id,
    e.Razon_Social AS Empresa,
    a.Codigo_Activo,
    p.Nombre AS Producto,
    a.Estado_Disponibilidad,
    a.Ultimo_Mantenimiento,
    a.Proximo_Mantenimiento,
    DATEDIFF(CURDATE(), a.Ultimo_Mantenimiento) AS Dias_Sin_Mantenimiento
FROM Activo a
INNER JOIN Empresa e ON a.Empresa_Id = e.Id
INNER JOIN Producto p ON a.Producto_Id = p.Id
WHERE a.Proximo_Mantenimiento <= DATE_ADD(CURDATE(), INTERVAL 7 DAY)
AND a.Estado_Disponibilidad != 'Dado_de_Baja'
AND a.Activo = TRUE;

-- Vista: Reservas completas con información del cliente
CREATE OR REPLACE VIEW v_reservas_completas AS
SELECT 
    r.Id,
    r.Codigo_Reserva,
    r.Estado,
    r.Fecha_Evento,
    r.Total,
    e.Razon_Social AS Empresa,
    c.Nombre AS Cliente,
    c.Telefono AS Cliente_Telefono,
    c.Email AS Cliente_Email,
    u.Nombre_Completo AS Creado_Por,
    r.Fecha_Creacion
FROM Reserva r
INNER JOIN Empresa e ON r.Empresa_Id = e.Id
INNER JOIN Cliente c ON r.Cliente_Id = c.Id
INNER JOIN Usuario u ON r.Creado_Por_Id = u.Id;

-- Vista: Rentabilidad de activos
CREATE OR REPLACE VIEW v_rentabilidad_activos AS
SELECT 
    a.Id,
    a.Empresa_Id,
    e.Razon_Social AS Empresa,
    a.Codigo_Activo,
    p.Nombre AS Producto,
    a.Costo_Compra,
    a.Ingresos_Totales,
    a.Costos_Mantenimiento,
    a.Depreciacion_Acumulada,
    (a.Ingresos_Totales - a.Costos_Mantenimiento - a.Depreciacion_Acumulada) AS Utilidad_Neta,
    a.Total_Alquileres,
    CASE 
        WHEN a.Costo_Compra > 0 THEN 
            ((a.Ingresos_Totales - a.Costos_Mantenimiento) / a.Costo_Compra) * 100
        ELSE 0 
    END AS ROI_Porcentaje
FROM Activo a
INNER JOIN Empresa e ON a.Empresa_Id = e.Id
INNER JOIN Producto p ON a.Producto_Id = p.Id
WHERE a.Activo = TRUE;

-- ============================================
-- PROCEDIMIENTOS ALMACENADOS
-- ============================================

DELIMITER //

-- Procedimiento: Crear reserva y bloquear inventario
CREATE PROCEDURE sp_crear_reserva(
    IN p_empresa_id INT,
    IN p_cliente_id INT,
    IN p_fecha_evento DATE,
    IN p_creado_por_id INT,
    OUT p_reserva_id INT,
    OUT p_codigo_reserva VARCHAR(20)
)
BEGIN
    DECLARE v_codigo VARCHAR(20);
    
    -- Generar código único de reserva
    SET v_codigo = CONCAT('RES-', DATE_FORMAT(NOW(), '%Y%m%d'), '-', LPAD(FLOOR(RAND() * 10000), 4, '0'));
    
    -- Insertar reserva
    INSERT INTO Reserva (
        Empresa_Id, Cliente_Id, Codigo_Reserva, Estado, 
        Fecha_Evento, Total, Creado_Por_Id
    ) VALUES (
        p_empresa_id, p_cliente_id, v_codigo, 'Solicitado',
        p_fecha_evento, 0, p_creado_por_id
    );
    
    SET p_reserva_id = LAST_INSERT_ID();
    SET p_codigo_reserva = v_codigo;
END //

-- Procedimiento: Calcular depreciación de activo
CREATE PROCEDURE sp_calcular_depreciacion_activo(
    IN p_activo_id INT
)
BEGIN
    DECLARE v_costo_compra DECIMAL(12,2);
    DECLARE v_valor_residual DECIMAL(12,2);
    DECLARE v_vida_util_anos INT;
    DECLARE v_fecha_compra DATE;
    DECLARE v_meses_transcurridos INT;
    DECLARE v_depreciacion_mensual DECIMAL(12,2);
    DECLARE v_depreciacion_acumulada DECIMAL(12,2);
    
    -- Obtener datos del activo
    SELECT Costo_Compra, Valor_Residual, Vida_Util_Anos, Fecha_Compra
    INTO v_costo_compra, v_valor_residual, v_vida_util_anos, v_fecha_compra
    FROM Activo
    WHERE Id = p_activo_id;
    
    -- Calcular meses transcurridos
    SET v_meses_transcurridos = TIMESTAMPDIFF(MONTH, v_fecha_compra, CURDATE());
    
    -- Depreciación mensual = (Costo - Valor Residual) / (Vida Útil en meses)
    SET v_depreciacion_mensual = (v_costo_compra - v_valor_residual) / (v_vida_util_anos * 12);
    
    -- Depreciación acumulada
    SET v_depreciacion_acumulada = v_depreciacion_mensual * v_meses_transcurridos;
    
    -- No puede superar el costo menos valor residual
    IF v_depreciacion_acumulada > (v_costo_compra - v_valor_residual) THEN
        SET v_depreciacion_acumulada = v_costo_compra - v_valor_residual;
    END IF;
    
    -- Actualizar activo
    UPDATE Activo
    SET Depreciacion_Acumulada = v_depreciacion_acumulada
    WHERE Id = p_activo_id;
END //

-- Procedimiento: Clasificación ABC
CREATE PROCEDURE sp_calcular_clasificacion_abc(
    IN p_empresa_id INT,
    IN p_periodo VARCHAR(20)
)
BEGIN
    DECLARE v_total_ventas DECIMAL(12,2);
    DECLARE v_acumulado DECIMAL(12,2) DEFAULT 0;
    DECLARE v_porcentaje DECIMAL(5,2);
    DECLARE done INT DEFAULT FALSE;
    
    DECLARE v_producto_id INT;
    DECLARE v_valor_ventas DECIMAL(12,2);
    
    DECLARE cur CURSOR FOR
        SELECT 
            dr.Producto_Id,
            SUM(dr.Subtotal) AS Valor_Ventas
        FROM Detalle_Reserva dr
        INNER JOIN Reserva r ON dr.Reserva_Id = r.Id
        WHERE r.Empresa_Id = p_empresa_id
        AND r.Estado = 'Completado'
        AND DATE_FORMAT(r.Fecha_Creacion, '%Y-%m') = p_periodo
        AND dr.Producto_Id IS NOT NULL
        GROUP BY dr.Producto_Id
        ORDER BY Valor_Ventas DESC;
    
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;
    
    -- Calcular total de ventas del periodo
    SELECT SUM(Subtotal) INTO v_total_ventas
    FROM Detalle_Reserva dr
    INNER JOIN Reserva r ON dr.Reserva_Id = r.Id
    WHERE r.Empresa_Id = p_empresa_id
    AND r.Estado = 'Completado'
    AND DATE_FORMAT(r.Fecha_Creacion, '%Y-%m') = p_periodo
    AND dr.Producto_Id IS NOT NULL;
    
    -- Limpiar clasificaciones anteriores del periodo
    DELETE FROM Clasificacion_ABC 
    WHERE Empresa_Id = p_empresa_id 
    AND Periodo_Analisis = p_periodo;
    
    -- Abrir cursor
    OPEN cur;
    
    read_loop: LOOP
        FETCH cur INTO v_producto_id, v_valor_ventas;
        IF done THEN
            LEAVE read_loop;
        END IF;
        
        SET v_acumulado = v_acumulado + v_valor_ventas;
        SET v_porcentaje = (v_acumulado / v_total_ventas) * 100;
        
        -- Insertar clasificación
        INSERT INTO Clasificacion_ABC (
            Empresa_Id, Producto_Id, Periodo_Analisis, 
            Clasificacion, Valor_Total_Ventas, Porcentaje_Acumulado
        ) VALUES (
            p_empresa_id, v_producto_id, p_periodo,
            CASE 
                WHEN v_porcentaje <= 80 THEN 'A'
                WHEN v_porcentaje <= 95 THEN 'B'
                ELSE 'C'
            END,
            v_valor_ventas, v_porcentaje
        );
    END LOOP;
    
    CLOSE cur;
END //

DELIMITER ;

-- ============================================
-- DATOS INICIALES
-- ============================================

-- Insertar Roles
INSERT INTO Rol (Nombre, Descripcion, Nivel_Acceso, Permisos) VALUES
('SuperAdmin', 'Control total del sistema', 0, '["*"]'),
('Admin-Proveedor', 'Administrador de empresa proveedora', 1, '["empresa.*", "inventario.*", "reservas.*", "clientes.*", "usuarios.read"]'),
('Operario-Logística', 'Operador de campo para entregas', 2, '["reservas.read", "logistica.*", "evidencias.create"]'),
('Cliente-Final', 'Cliente con portal de autogestión', 3, '["reservas.create", "reservas.read_own"]'),
('Contador-Auditor', 'Auditor con acceso solo lectura', 4, '["inventario.read", "reportes.read", "logs.read"]');

-- Insertar Planes
INSERT INTO Plan (Nombre, Descripcion, Precio_Mensual, Duracion_Prueba_Dias, Modulos_Incluidos) VALUES
('Plan Básico', 'Funcionalidades básicas de gestión', 0.00, 0, '["core", "inventario_basico", "reservas", "logistica"]'),
('Plan Premium S.I.G.I.', 'Sistema de Gestión Integral de Inventarios', 150000.00, 3, '["core", "inventario_basico", "SIGI", "reservas", "logistica", "analytics"]'),
('Plan Enterprise', 'Funcionalidades completas + B2B', 300000.00, 7, '["*"]');

-- Insertar SuperAdmin
INSERT INTO Empresa (Razon_Social, NIT, Email, Telefono, Ciudad, Pais, Estado) VALUES
('EventConnect System', '900000000-0', 'admin@eventconnect.com', '+57 300 000 0000', 'Bogotá', 'Colombia', 'Activa');

INSERT INTO Usuario (Empresa_Id, Rol_Id, Usuario, Email, Password_Hash, Nombre_Completo, Estado) VALUES
(NULL, 1, 'superadmin', 'admin@eventconnect.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIbHc3lW5G', 'Super Administrador', 'Activo');
-- Password: SuperAdmin123$

-- Insertar Empresa de Ejemplo
INSERT INTO Empresa (Razon_Social, NIT, Email, Telefono, Direccion, Ciudad, Pais) VALUES
('Eventos y Mobiliario Premium SAS', '900123456-7', 'contacto@eventospremium.com', '+57 310 123 4567', 'Calle 100 #15-20', 'Bogotá', 'Colombia');

-- Insertar Usuario Admin de la empresa
INSERT INTO Usuario (Empresa_Id, Rol_Id, Usuario, Email, Password_Hash, Nombre_Completo, Telefono, Estado) VALUES
(2, 2, 'admin_empresa', 'admin@eventospremium.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIbHc3lW5G', 'Admin Empresa Demo', '+57 310 123 4567', 'Activo');
-- Password: Admin123$

-- Insertar categorías de ejemplo
INSERT INTO Categoria (Empresa_Id, Nombre, Descripcion, Icono, Color) VALUES
(2, 'Mobiliario', 'Sillas, mesas y mobiliario en general', '🪑', '#3B82F6'),
(2, 'Iluminación', 'Equipos de iluminación y decoración', '💡', '#F59E0B'),
(2, 'Sonido', 'Equipos de sonido y audio', '🔊', '#10B981'),
(2, 'Carpas y Toldos', 'Carpas, toldos y estructuras', '⛺', '#EF4444'),
(2, 'Vajilla', 'Platos, vasos, cubiertos', '🍽️', '#8B5CF6');

-- ============================================
-- TRIGGERS PARA AUDITORÍA AUTOMÁTICA
-- ============================================

DELIMITER //

-- Trigger para auditar cambios en Producto
CREATE TRIGGER trg_producto_audit_update
AFTER UPDATE ON Producto
FOR EACH ROW
BEGIN
    INSERT INTO Log_Auditoria (
        Empresa_Id, Usuario_Id, Tabla_Afectada, Id_Registro_Afectado,
        Accion, Valor_Antes, Valor_Despues, Hash_Integridad
    ) VALUES (
        NEW.Empresa_Id, 
        @current_user_id,
        'Producto',
        NEW.Id,
        'UPDATE',
        JSON_OBJECT(
            'Nombre', OLD.Nombre,
            'Precio_Alquiler_Dia', OLD.Precio_Alquiler_Dia,
            'Cantidad_Stock', OLD.Cantidad_Stock,
            'Activo', OLD.Activo
        ),
        JSON_OBJECT(
            'Nombre', NEW.Nombre,
            'Precio_Alquiler_Dia', NEW.Precio_Alquiler_Dia,
            'Cantidad_Stock', NEW.Cantidad_Stock,
            'Activo', NEW.Activo
        ),
        SHA2(CONCAT(NEW.Id, NEW.Empresa_Id, NOW(6)), 256)
    );
END //

-- Trigger para auditar cambios en Reserva
CREATE TRIGGER trg_reserva_audit_update
AFTER UPDATE ON Reserva
FOR EACH ROW
BEGIN
    INSERT INTO Log_Auditoria (
        Empresa_Id, Usuario_Id, Tabla_Afectada, Id_Registro_Afectado,
        Accion, Valor_Antes, Valor_Despues, Hash_Integridad
    ) VALUES (
        NEW.Empresa_Id,
        @current_user_id,
        'Reserva',
        NEW.Id,
        'UPDATE',
        JSON_OBJECT(
            'Estado', OLD.Estado,
            'Total', OLD.Total,
            'Estado_Pago', OLD.Estado_Pago
        ),
        JSON_OBJECT(
            'Estado', NEW.Estado,
            'Total', NEW.Total,
            'Estado_Pago', NEW.Estado_Pago
        ),
        SHA2(CONCAT(NEW.Id, NEW.Empresa_Id, NOW(6)), 256)
    );
END //

DELIMITER ;

-- ============================================
-- ÍNDICES ADICIONALES PARA OPTIMIZACIÓN
-- ============================================

-- Índices compuestos para consultas frecuentes
CREATE INDEX idx_reserva_empresa_estado_fecha ON Reserva(Empresa_Id, Estado, Fecha_Evento);
CREATE INDEX idx_activo_empresa_disponibilidad ON Activo(Empresa_Id, Estado_Disponibilidad);
CREATE INDEX idx_movimiento_empresa_fecha ON Movimiento_Inventario(Empresa_Id, Fecha_Movimiento);
CREATE INDEX idx_log_empresa_fecha ON Log_Auditoria(Empresa_Id, Timestamp_UTC);

-- ============================================
-- CONFIGURACIONES DE SEGURIDAD
-- ============================================

-- Deshabilitar LOAD DATA LOCAL INFILE para prevenir ataques
SET GLOBAL local_infile = 0;

-- Configurar timeouts apropiados
SET GLOBAL max_connect_errors = 10;
SET GLOBAL connect_timeout = 10;

-- ============================================
-- FIN DEL SCRIPT
-- ============================================

SELECT 'Base de datos EventConnect creada exitosamente!' AS Mensaje;
SELECT 'SuperAdmin usuario: superadmin' AS Usuario;
SELECT 'SuperAdmin password: SuperAdmin123$' AS Password;
SELECT 'Empresa Demo usuario: admin_empresa' AS Usuario_Demo;
SELECT 'Empresa Demo password: Admin123$' AS Password_Demo;
