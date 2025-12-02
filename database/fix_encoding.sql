-- ============================================
-- Script para corregir codificación de acentos
-- EventConnect - Fix UTF-8 Encoding
-- ============================================

USE db_eventconnect;

-- Corregir Categorías
UPDATE Categoria SET Nombre = 'Iluminación', Descripcion = 'Equipos de iluminación y decoración' 
WHERE Nombre LIKE '%Iluminaci%n%';

UPDATE Categoria SET Nombre = 'Carpas y Toldos', Descripcion = 'Carpas, toldos y estructuras' 
WHERE Nombre LIKE '%Carpas%';

-- Corregir Roles
UPDATE Rol SET Descripcion = 'Administrador de empresa proveedora' 
WHERE Nombre = 'Admin-Proveedor' AND Descripcion LIKE '%Administrador%';

UPDATE Rol SET Descripcion = 'Operador de campo para entregas' 
WHERE Nombre = 'Operario-Logística' OR Nombre LIKE '%Operario%';

UPDATE Rol SET Descripcion = 'Cliente con portal de autogestión' 
WHERE Nombre = 'Cliente-Final' AND Descripcion LIKE '%Cliente%';

-- Corregir Planes
UPDATE Plan SET Descripcion = 'Funcionalidades básicas de gestión' 
WHERE Nombre = 'Plan Básico' OR Nombre LIKE '%Plan B_sico%';

UPDATE Plan SET Nombre = 'Plan Básico', Descripcion = 'Funcionalidades básicas de gestión' 
WHERE Nombre LIKE '%Plan B%sico%';

-- Corregir Empresas
UPDATE Empresa SET Ciudad = 'Bogotá' 
WHERE Ciudad LIKE '%Bogot%' AND Ciudad NOT LIKE 'Bogotá';

-- Corregir Usuarios
UPDATE Usuario SET Nombre_Completo = 'Admin Empresa Demo' 
WHERE Nombre_Completo LIKE '%Admin Empresa%' AND Nombre_Completo NOT LIKE 'Admin Empresa Demo';

-- Verificar correcciones
SELECT 'Categorías corregidas:' AS Resultado;
SELECT Id, Nombre, Descripcion FROM Categoria;

SELECT 'Roles corregidos:' AS Resultado;
SELECT Id, Nombre, Descripcion FROM Rol;

SELECT 'Planes corregidos:' AS Resultado;
SELECT Id, Nombre, Descripcion FROM Plan;

SELECT 'Empresas corregidas:' AS Resultado;
SELECT Id, Razon_Social, Ciudad FROM Empresa;

SELECT 'Script completado exitosamente' AS Resultado;
