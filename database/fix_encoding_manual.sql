-- ============================================
-- Script SIMPLIFICADO para corregir acentos
-- Ejecutar manualmente en MySQL Workbench o phpMyAdmin
-- ============================================

USE db_eventconnect;

-- 1. Corregir Categorías con problemas de encoding
UPDATE Categoria SET 
    Nombre = 'Iluminación', 
    Descripcion = 'Equipos de iluminación y decoración' 
WHERE Id = 2; -- Ajustar el ID según corresponda

UPDATE Categoria SET 
    Nombre = 'Vajilla', 
    Descripcion = 'Platos, vasos, cubiertos' 
WHERE Nombre LIKE '%Vajilla%';

-- 2. Verificar los cambios
SELECT * FROM Categoria;

-- ============================================
-- IMPORTANTE: Si los acentos siguen fallando:
-- ============================================
-- Opción 1: Eliminar y reinsertar los datos
DELETE FROM Categoria WHERE Empresa_Id = 2;

INSERT INTO Categoria (Empresa_Id, Nombre, Descripcion, Icono, Color, Activo) VALUES
(2, 'Mobiliario', 'Sillas, mesas y mobiliario en general', '🪑', '#3B82F6', 1),
(2, 'Iluminación', 'Equipos de iluminación y decoración', '💡', '#F59E0B', 1),
(2, 'Sonido', 'Equipos de sonido y audio', '🔊', '#10B981', 1),
(2, 'Carpas y Toldos', 'Carpas, toldos y estructuras', '⛺', '#EF4444', 1),
(2, 'Vajilla', 'Platos, vasos, cubiertos', '🍽️', '#8B5CF6', 1);

-- 3. Verificar encoding de la conexión
SHOW VARIABLES LIKE 'character_set%';
SHOW VARIABLES LIKE 'collation%';

-- 4. Si es necesario, configurar la conexión UTF-8
SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;
