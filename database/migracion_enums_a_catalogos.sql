-- ====================================================================
-- SCRIPT DE MIGRACIÓN: ENUMs a Tablas de Catálogo
-- Ejecutar DESPUÉS de tener datos en producción
-- ====================================================================

-- 1. TABLA CATÁLOGO: Estados de Reserva (El más crítico - cambia frecuentemente)
CREATE TABLE IF NOT EXISTS `catalogo_estado_reserva` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Codigo` VARCHAR(50) NOT NULL UNIQUE,
  `Nombre` VARCHAR(100) NOT NULL,
  `Descripcion` TEXT NULL,
  `Color` VARCHAR(20) NULL, -- Para UI: 'blue', 'green', 'red', etc.
  `Orden` INT DEFAULT 0, -- Para ordenar en dropdowns
  `Activo` BOOLEAN DEFAULT TRUE,
  `Sistema` BOOLEAN DEFAULT FALSE, -- TRUE si no se puede eliminar (estados core)
  `Fecha_Creacion` DATETIME DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  INDEX `idx_codigo` (`Codigo`),
  INDEX `idx_activo` (`Activo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insertar estados iniciales de reserva
INSERT INTO `catalogo_estado_reserva` (`Codigo`, `Nombre`, `Descripcion`, `Color`, `Orden`, `Sistema`) VALUES
('Solicitado', 'Solicitado', 'Reserva solicitada, pendiente de aprobación', 'yellow', 1, TRUE),
('Aprobado', 'Aprobado', 'Reserva aprobada y confirmada', 'green', 2, TRUE),
('En_Preparacion', 'En Preparación', 'Productos siendo preparados para entrega', 'blue', 3, TRUE),
('Entregado', 'Entregado', 'Productos entregados al cliente', 'cyan', 4, TRUE),
('En_Evento', 'En Evento', 'Evento en curso', 'purple', 5, TRUE),
('Devuelto', 'Devuelto', 'Productos devueltos por el cliente', 'teal', 6, TRUE),
('Completado', 'Completado', 'Reserva finalizada exitosamente', 'green', 7, TRUE),
('Cancelado', 'Cancelado', 'Reserva cancelada', 'red', 8, TRUE);

-- 2. TABLA CATÁLOGO: Estados de Activo/Producto (Flexible para mantenimiento)
-- NOTA: La tabla Activo usa Estado_Disponibilidad (no Estado)
CREATE TABLE IF NOT EXISTS `catalogo_estado_activo` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Codigo` VARCHAR(50) NOT NULL UNIQUE,
  `Nombre` VARCHAR(100) NOT NULL,
  `Descripcion` TEXT NULL,
  `Color` VARCHAR(20) NULL,
  `Permite_Reserva` BOOLEAN DEFAULT TRUE, -- Si el activo puede ser reservado en este estado
  `Orden` INT DEFAULT 0,
  `Activo` BOOLEAN DEFAULT TRUE,
  `Sistema` BOOLEAN DEFAULT FALSE,
  `Fecha_Creacion` DATETIME DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  INDEX `idx_codigo` (`Codigo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insertar estados iniciales de activo (basados en Estado_Disponibilidad actual)
INSERT INTO `catalogo_estado_activo` (`Codigo`, `Nombre`, `Descripcion`, `Color`, `Permite_Reserva`, `Orden`, `Sistema`) VALUES
('Disponible', 'Disponible', 'Activo disponible para alquilar', 'green', TRUE, 1, TRUE),
('Alquilado', 'Alquilado', 'Activo alquilado para un evento', 'blue', FALSE, 2, TRUE),
('En_Mantenimiento', 'En Mantenimiento', 'Activo en mantenimiento preventivo', 'orange', FALSE, 3, TRUE),
('Dado_de_Baja', 'Dado de Baja', 'Activo dado de baja, no disponible', 'gray', FALSE, 4, TRUE),
-- Nuevos estados que se pueden agregar sin ALTER TABLE:
('Reparacion', 'En Reparación', 'Activo en reparación', 'red', FALSE, 5, FALSE),
('Reparacion_Externa', 'Reparación Externa', 'Activo enviado a reparación externa', 'red', FALSE, 6, FALSE),
('Reservado', 'Reservado', 'Activo reservado pero no entregado', 'purple', FALSE, 7, FALSE),
('Perdido', 'Perdido/Extraviado', 'Activo reportado como perdido', 'red', FALSE, 8, FALSE);

-- 3. TABLA CATÁLOGO: Métodos de Pago (Fácil agregar Nequi, Daviplata, etc.)
CREATE TABLE IF NOT EXISTS `catalogo_metodo_pago` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Codigo` VARCHAR(50) NOT NULL UNIQUE,
  `Nombre` VARCHAR(100) NOT NULL,
  `Descripcion` TEXT NULL,
  `Requiere_Comprobante` BOOLEAN DEFAULT FALSE,
  `Requiere_Referencia` BOOLEAN DEFAULT FALSE,
  `Activo` BOOLEAN DEFAULT TRUE,
  `Orden` INT DEFAULT 0,
  `Fecha_Creacion` DATETIME DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  INDEX `idx_activo` (`Activo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insertar métodos de pago iniciales
INSERT INTO `catalogo_metodo_pago` (`Codigo`, `Nombre`, `Descripcion`, `Requiere_Comprobante`, `Requiere_Referencia`, `Orden`) VALUES
('Efectivo', 'Efectivo', 'Pago en efectivo', FALSE, FALSE, 1),
('Transferencia', 'Transferencia Bancaria', 'Transferencia entre cuentas', TRUE, TRUE, 2),
('Tarjeta', 'Tarjeta Débito/Crédito', 'Pago con tarjeta', TRUE, TRUE, 3),
('Nequi', 'Nequi', 'Pago mediante Nequi', TRUE, TRUE, 4),
('Daviplata', 'Daviplata', 'Pago mediante Daviplata', TRUE, TRUE, 5),
('PayU', 'PayU', 'Pasarela de pago PayU', TRUE, TRUE, 6),
('Stripe', 'Stripe', 'Pasarela de pago Stripe', TRUE, TRUE, 7),
('Credito', 'Crédito', 'Pago a crédito con la empresa', FALSE, FALSE, 8);

-- 4. TABLA CATÁLOGO: Tipos de Mantenimiento
CREATE TABLE IF NOT EXISTS `catalogo_tipo_mantenimiento` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Codigo` VARCHAR(50) NOT NULL UNIQUE,
  `Nombre` VARCHAR(100) NOT NULL,
  `Descripcion` TEXT NULL,
  `Es_Preventivo` BOOLEAN DEFAULT FALSE,
  `Activo` BOOLEAN DEFAULT TRUE,
  `Orden` INT DEFAULT 0,
  `Fecha_Creacion` DATETIME DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insertar tipos de mantenimiento
INSERT INTO `catalogo_tipo_mantenimiento` (`Codigo`, `Nombre`, `Descripcion`, `Es_Preventivo`, `Orden`) VALUES
('Preventivo', 'Mantenimiento Preventivo', 'Mantenimiento programado regular', TRUE, 1),
('Correctivo', 'Mantenimiento Correctivo', 'Reparación de fallas', FALSE, 2),
('Limpieza', 'Limpieza Profunda', 'Limpieza y desinfección', TRUE, 3),
('Reparacion', 'Reparación', 'Reparación de daños', FALSE, 4),
('Actualizacion', 'Actualización', 'Actualización o mejora', FALSE, 5);

-- ====================================================================
-- MIGRACIÓN DE DATOS EXISTENTES (Solo si ya tienes datos)
-- ====================================================================

-- NOTA: Las siguientes líneas son EJEMPLOS de cómo migrarías datos existentes
-- Descomenta y ajusta según tus necesidades

-- Migrar estados de reserva (si ya existen reservas)
-- UPDATE reserva SET Estado = 'Solicitado' WHERE Estado IN ('Solicitado', 'Pendiente');
-- UPDATE reserva SET Estado = 'Aprobado' WHERE Estado = 'Confirmado';

-- Migrar estados de activo (si ya existen activos)
-- UPDATE activo SET Estado_Disponibilidad = 'Disponible' WHERE Estado_Disponibilidad = 'Disponible';
-- UPDATE activo SET Estado_Disponibilidad = 'Alquilado' WHERE Estado_Disponibilidad IN ('Alquilado', 'Reservado');

-- ====================================================================
-- FOREIGN KEYS (Aplicar DESPUÉS de verificar que la migración funcionó)
-- ====================================================================

-- ADVERTENCIA: Estas FKs harán que los ENUMs sean obligatorios de la tabla de catálogo
-- Solo descomentar cuando estés 100% seguro de la migración

-- ALTER TABLE `reserva` 
-- ADD CONSTRAINT `fk_reserva_estado` 
-- FOREIGN KEY (`Estado`) REFERENCES `catalogo_estado_reserva` (`Codigo`) 
-- ON UPDATE CASCADE ON DELETE RESTRICT;

-- NOTA: Para Activo, la columna es Estado_Disponibilidad (no Estado)
-- ALTER TABLE `activo` 
-- ADD CONSTRAINT `fk_activo_estado` 
-- FOREIGN KEY (`Estado_Disponibilidad`) REFERENCES `catalogo_estado_activo` (`Codigo`) 
-- ON UPDATE CASCADE ON DELETE RESTRICT;

-- ALTER TABLE `transaccion_pago` 
-- ADD CONSTRAINT `fk_transaccion_metodo` 
-- FOREIGN KEY (`Metodo`) REFERENCES `catalogo_metodo_pago` (`Codigo`) 
-- ON UPDATE CASCADE ON DELETE RESTRICT;

-- ALTER TABLE `mantenimiento` 
-- ADD CONSTRAINT `fk_mantenimiento_tipo` 
-- FOREIGN KEY (`Tipo`) REFERENCES `catalogo_tipo_mantenimiento` (`Codigo`) 
-- ON UPDATE CASCADE ON DELETE RESTRICT;

-- ====================================================================
-- VENTAJAS DE ESTE ENFOQUE:
-- ====================================================================
-- ✅ Agregar nuevos estados sin ALTER TABLE
-- ✅ Soft delete de estados (Activo = FALSE en lugar de DROP)
-- ✅ Descripciones y colores para mejor UX
-- ✅ Control de permisos (¿Quién puede crear estados?)
-- ✅ Auditoría (¿Cuándo se creó cada estado?)
-- ✅ Orden personalizable en dropdowns
-- ✅ Estados protegidos (Sistema = TRUE no se pueden eliminar)

-- ====================================================================
-- DESVENTAJAS:
-- ====================================================================
-- ⚠️ Más JOINs en queries (pero insignificante con índices)
-- ⚠️ Requiere validación adicional en backend
-- ⚠️ Migración compleja si ya tienes datos en producción
