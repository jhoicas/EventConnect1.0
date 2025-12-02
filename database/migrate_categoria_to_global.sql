-- ============================================
-- Migration: Make Categoria table global
-- Description: Remove Empresa_Id to make categories shared across all companies
-- Date: 2025-01-25
-- ============================================

USE db_eventconnect;

-- Step 1: Drop foreign key constraint
ALTER TABLE Categoria 
DROP FOREIGN KEY Categoria_ibfk_1;

-- Step 2: Drop unique index that includes Empresa_Id
ALTER TABLE Categoria 
DROP INDEX uk_empresa_nombre;

-- Step 3: Drop Empresa_Id column
ALTER TABLE Categoria 
DROP COLUMN Empresa_Id;

-- Step 4: Add new unique constraint on Nombre only (global uniqueness)
ALTER TABLE Categoria 
ADD UNIQUE KEY uk_nombre (Nombre);

-- Step 5: Make Icono and Color nullable (optional fields)
ALTER TABLE Categoria 
MODIFY COLUMN Icono VARCHAR(50) NULL,
MODIFY COLUMN Color VARCHAR(7) NULL COMMENT 'Color hexadecimal #RRGGBB';

-- Verification: Show updated table structure
DESCRIBE Categoria;

-- Show current categories
SELECT * FROM Categoria;
