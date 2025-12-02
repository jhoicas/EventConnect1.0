-- =====================================================
-- TRIGGERS PARA INTEGRIDAD PRODUCTO-ACTIVO
-- Garantiza que un Activo solo se reserve bajo su Producto padre
-- =====================================================

USE db_eventconnect;

DELIMITER $$

-- Trigger BEFORE INSERT para validar integridad
DROP TRIGGER IF EXISTS trg_detalle_reserva_before_insert$$
CREATE TRIGGER trg_detalle_reserva_before_insert
BEFORE INSERT ON detalle_reserva
FOR EACH ROW
BEGIN
    DECLARE activo_producto_id INT;
    DECLARE msg VARCHAR(255);
    
    -- Si se especifica un Activo_Id, validar que coincida con el Producto_Id
    IF NEW.Activo_Id IS NOT NULL THEN
        -- Obtener el Producto_Id del Activo
        SELECT Producto_Id INTO activo_producto_id
        FROM activo
        WHERE Id = NEW.Activo_Id AND Activo = 1;
        
        -- Si el activo no existe o está inactivo
        IF activo_producto_id IS NULL THEN
            SET msg = CONCAT('El Activo ID ', NEW.Activo_Id, ' no existe o está inactivo');
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = msg;
        END IF;
        
        -- Validar que el Producto_Id coincida
        IF NEW.Producto_Id IS NOT NULL AND NEW.Producto_Id != activo_producto_id THEN
            SET msg = CONCAT(
                'Integridad violada: El Activo ID ', NEW.Activo_Id, 
                ' pertenece al Producto ID ', activo_producto_id,
                ', pero se intentó asociar con Producto ID ', NEW.Producto_Id
            );
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = msg;
        END IF;
        
        -- Auto-completar el Producto_Id si no se especificó
        IF NEW.Producto_Id IS NULL THEN
            SET NEW.Producto_Id = activo_producto_id;
        END IF;
        
        -- Validar que el activo esté disponible
        IF NOT EXISTS (
            SELECT 1 FROM activo 
            WHERE Id = NEW.Activo_Id 
            AND Estado_Disponibilidad = 'Disponible'
        ) THEN
            SET msg = CONCAT('El Activo ID ', NEW.Activo_Id, ' no está disponible para reserva');
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = msg;
        END IF;
    END IF;
    
    -- Si solo se especifica Producto_Id, validar que exista
    IF NEW.Producto_Id IS NOT NULL AND NEW.Activo_Id IS NULL THEN
        IF NOT EXISTS (SELECT 1 FROM producto WHERE Id = NEW.Producto_Id AND Activo = 1) THEN
            SET msg = CONCAT('El Producto ID ', NEW.Producto_Id, ' no existe o está inactivo');
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = msg;
        END IF;
    END IF;
    
    -- Validar que al menos uno esté presente
    IF NEW.Producto_Id IS NULL AND NEW.Activo_Id IS NULL THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Debe especificar al menos un Producto_Id o Activo_Id';
    END IF;
END$$

-- Trigger BEFORE UPDATE para validar integridad en actualizaciones
DROP TRIGGER IF EXISTS trg_detalle_reserva_before_update$$
CREATE TRIGGER trg_detalle_reserva_before_update
BEFORE UPDATE ON detalle_reserva
FOR EACH ROW
BEGIN
    DECLARE activo_producto_id INT;
    DECLARE msg VARCHAR(255);
    
    -- Si se especifica un Activo_Id, validar que coincida con el Producto_Id
    IF NEW.Activo_Id IS NOT NULL THEN
        -- Obtener el Producto_Id del Activo
        SELECT Producto_Id INTO activo_producto_id
        FROM activo
        WHERE Id = NEW.Activo_Id AND Activo = 1;
        
        -- Si el activo no existe o está inactivo
        IF activo_producto_id IS NULL THEN
            SET msg = CONCAT('El Activo ID ', NEW.Activo_Id, ' no existe o está inactivo');
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = msg;
        END IF;
        
        -- Validar que el Producto_Id coincida
        IF NEW.Producto_Id IS NOT NULL AND NEW.Producto_Id != activo_producto_id THEN
            SET msg = CONCAT(
                'Integridad violada: El Activo ID ', NEW.Activo_Id, 
                ' pertenece al Producto ID ', activo_producto_id,
                ', pero se intentó asociar con Producto ID ', NEW.Producto_Id
            );
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = msg;
        END IF;
        
        -- Auto-completar el Producto_Id si no se especificó
        IF NEW.Producto_Id IS NULL THEN
            SET NEW.Producto_Id = activo_producto_id;
        END IF;
    END IF;
    
    -- Validar que al menos uno esté presente
    IF NEW.Producto_Id IS NULL AND NEW.Activo_Id IS NULL THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Debe especificar al menos un Producto_Id o Activo_Id';
    END IF;
END$$

DELIMITER ;

-- =====================================================
-- STORED PROCEDURE PARA VALIDACIÓN MANUAL
-- =====================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_validar_detalle_reserva$$
CREATE PROCEDURE sp_validar_detalle_reserva(
    IN p_producto_id INT,
    IN p_activo_id INT,
    OUT p_valido BOOLEAN,
    OUT p_mensaje VARCHAR(255)
)
sp_label: BEGIN
    DECLARE v_activo_producto_id INT;
    
    SET p_valido = FALSE;
    
    -- Validar que al menos uno esté presente
    IF p_producto_id IS NULL AND p_activo_id IS NULL THEN
        SET p_mensaje = 'Debe especificar al menos un Producto_Id o Activo_Id';
        LEAVE sp_label;
    END IF;
    
    -- Si se especifica Activo_Id
    IF p_activo_id IS NOT NULL THEN
        -- Obtener el Producto_Id del Activo
        SELECT Producto_Id INTO v_activo_producto_id
        FROM activo
        WHERE Id = p_activo_id AND Activo = 1;
        
        -- Si el activo no existe
        IF v_activo_producto_id IS NULL THEN
            SET p_mensaje = CONCAT('El Activo ID ', p_activo_id, ' no existe o está inactivo');
            LEAVE sp_label;
        END IF;
        
        -- Validar que el Producto_Id coincida
        IF p_producto_id IS NOT NULL AND p_producto_id != v_activo_producto_id THEN
            SET p_mensaje = CONCAT(
                'Integridad violada: El Activo ID ', p_activo_id, 
                ' pertenece al Producto ID ', v_activo_producto_id,
                ', pero se intentó asociar con Producto ID ', p_producto_id
            );
            LEAVE sp_label;
        END IF;
        
        -- Validar disponibilidad
        IF NOT EXISTS (
            SELECT 1 FROM activo 
            WHERE Id = p_activo_id 
            AND Estado_Disponibilidad = 'Disponible'
        ) THEN
            SET p_mensaje = CONCAT('El Activo ID ', p_activo_id, ' no está disponible para reserva');
            LEAVE sp_label;
        END IF;
    END IF;
    
    -- Si solo se especifica Producto_Id
    IF p_producto_id IS NOT NULL AND p_activo_id IS NULL THEN
        IF NOT EXISTS (SELECT 1 FROM producto WHERE Id = p_producto_id AND Activo = 1) THEN
            SET p_mensaje = CONCAT('El Producto ID ', p_producto_id, ' no existe o está inactivo');
            LEAVE sp_label;
        END IF;
    END IF;
    
    -- Si llega aquí, todo está bien
    SET p_valido = TRUE;
    SET p_mensaje = 'Validación exitosa';
END$$

DELIMITER ;

-- =====================================================
-- VISTA PARA VERIFICAR INTEGRIDAD ACTUAL
-- =====================================================

DROP VIEW IF EXISTS v_integridad_detalle_reserva;
CREATE VIEW v_integridad_detalle_reserva AS
SELECT 
    dr.Id AS Detalle_Id,
    dr.Reserva_Id,
    r.Codigo_Reserva,
    dr.Producto_Id,
    p.Nombre AS Producto_Nombre,
    dr.Activo_Id,
    a.Codigo_Activo,
    a.Producto_Id AS Activo_Producto_Real,
    pa.Nombre AS Activo_Producto_Nombre,
    CASE 
        WHEN dr.Activo_Id IS NOT NULL 
             AND dr.Producto_Id IS NOT NULL 
             AND dr.Producto_Id != a.Producto_Id 
        THEN 'INTEGRIDAD VIOLADA'
        WHEN dr.Producto_Id IS NULL AND dr.Activo_Id IS NULL 
        THEN 'INCOMPLETO'
        ELSE 'OK'
    END AS Estado_Integridad,
    CASE 
        WHEN dr.Activo_Id IS NOT NULL 
             AND dr.Producto_Id IS NOT NULL 
             AND dr.Producto_Id != a.Producto_Id 
        THEN CONCAT('Producto declarado: ', p.Nombre, ' pero Activo pertenece a: ', pa.Nombre)
        ELSE NULL
    END AS Descripcion_Error
FROM detalle_reserva dr
INNER JOIN reserva r ON dr.Reserva_Id = r.Id
LEFT JOIN producto p ON dr.Producto_Id = p.Id
LEFT JOIN activo a ON dr.Activo_Id = a.Id
LEFT JOIN producto pa ON a.Producto_Id = pa.Id;

-- =====================================================
-- FUNCIÓN PARA OBTENER ACTIVOS DISPONIBLES DE UN PRODUCTO
-- =====================================================

DELIMITER $$

DROP FUNCTION IF EXISTS fn_contar_activos_disponibles$$
CREATE FUNCTION fn_contar_activos_disponibles(p_producto_id INT)
RETURNS INT
DETERMINISTIC
READS SQL DATA
BEGIN
    DECLARE v_count INT;
    
    SELECT COUNT(*) INTO v_count
    FROM activo
    WHERE Producto_Id = p_producto_id
    AND Estado_Disponibilidad = 'Disponible'
    AND Activo = 1;
    
    RETURN v_count;
END$$

DELIMITER ;

-- =====================================================
-- ÍNDICES PARA MEJORAR RENDIMIENTO DE VALIDACIONES
-- =====================================================

-- Índice compuesto para validación rápida de producto-activo
-- Nota: MySQL 5.7/8.0 no soporta IF NOT EXISTS en CREATE INDEX
-- Usar procedimiento o ignorar error si ya existe

-- Verificar y crear índice si no existe
SET @index_exists = (
    SELECT COUNT(*) 
    FROM information_schema.statistics 
    WHERE table_schema = DATABASE() 
    AND table_name = 'activo' 
    AND index_name = 'idx_activo_producto_disponibilidad'
);

SET @sql_create_index1 = IF(
    @index_exists = 0,
    'CREATE INDEX idx_activo_producto_disponibilidad ON activo(Producto_Id, Estado_Disponibilidad, Activo)',
    'SELECT "Index idx_activo_producto_disponibilidad already exists" AS message'
);

PREPARE stmt FROM @sql_create_index1;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Índice para detalle_reserva
SET @index_exists2 = (
    SELECT COUNT(*) 
    FROM information_schema.statistics 
    WHERE table_schema = DATABASE() 
    AND table_name = 'detalle_reserva' 
    AND index_name = 'idx_detalle_producto_activo'
);

SET @sql_create_index2 = IF(
    @index_exists2 = 0,
    'CREATE INDEX idx_detalle_producto_activo ON detalle_reserva(Producto_Id, Activo_Id)',
    'SELECT "Index idx_detalle_producto_activo already exists" AS message'
);

PREPARE stmt2 FROM @sql_create_index2;
EXECUTE stmt2;
DEALLOCATE PREPARE stmt2;

-- =====================================================
-- SCRIPT DE CORRECCIÓN PARA DATOS EXISTENTES
-- =====================================================

-- Corregir detalles donde el Producto_Id no coincide con el Activo
UPDATE detalle_reserva dr
INNER JOIN activo a ON dr.Activo_Id = a.Id
SET dr.Producto_Id = a.Producto_Id
WHERE dr.Activo_Id IS NOT NULL 
AND (dr.Producto_Id IS NULL OR dr.Producto_Id != a.Producto_Id);

-- Reporte de correcciones realizadas
SELECT 
    'Detalles corregidos' AS Tipo,
    COUNT(*) AS Cantidad
FROM detalle_reserva dr
INNER JOIN activo a ON dr.Activo_Id = a.Id
WHERE dr.Producto_Id = a.Producto_Id;

-- =====================================================
-- TESTS DE VALIDACIÓN
-- =====================================================

-- Test 1: Intentar insertar activo con producto incorrecto (debe fallar)
-- INSERT INTO detalle_reserva (Reserva_Id, Producto_Id, Activo_Id, Cantidad, Precio_Unitario, Subtotal, Dias_Alquiler)
-- VALUES (1, 999, 1, 1, 100.00, 100.00, 1);
-- Resultado esperado: ERROR 1644 (45000): Integridad violada

-- Test 2: Insertar solo con Activo_Id (debe auto-completar Producto_Id)
-- INSERT INTO detalle_reserva (Reserva_Id, Activo_Id, Cantidad, Precio_Unitario, Subtotal, Dias_Alquiler)
-- VALUES (1, 1, 1, 100.00, 100.00, 1);
-- Resultado esperado: Success con Producto_Id auto-completado

-- Test 3: Consultar integridad actual
SELECT * FROM v_integridad_detalle_reserva 
WHERE Estado_Integridad != 'OK';

-- Test 4: Validación mediante stored procedure
-- CALL sp_validar_detalle_reserva(1, 1, @valido, @mensaje);
-- SELECT @valido, @mensaje;
