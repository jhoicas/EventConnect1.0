-- Seed de datos para tabla cliente
USE db_eventconnect;

-- Verificar si existen clientes
SELECT COUNT(*) as total_clientes FROM cliente;

-- Insertar clientes de prueba (solo si no existen)
INSERT INTO cliente (Empresa_Id, Nombre, Email, Telefono, Direccion, Ciudad, Tipo, Documento, Observaciones, Estado)
SELECT 1, 'Juan Pérez', 'juan.perez@example.com', '3001234567', 'Calle 123 #45-67', 'Bogotá', 'Persona Natural', '1234567890', 'Cliente regular', 'Activo'
WHERE NOT EXISTS (SELECT 1 FROM cliente WHERE Email = 'juan.perez@example.com');

INSERT INTO cliente (Empresa_Id, Nombre, Email, Telefono, Direccion, Ciudad, Tipo, Documento, Observaciones, Estado)
SELECT 1, 'Eventos Elite SAS', 'contacto@eventselite.com', '3109876543', 'Carrera 50 #100-25', 'Medellín', 'Empresa', '900123456-1', 'Cliente corporativo premium', 'Activo'
WHERE NOT EXISTS (SELECT 1 FROM cliente WHERE Email = 'contacto@eventselite.com');

INSERT INTO cliente (Empresa_Id, Nombre, Email, Telefono, Direccion, Ciudad, Tipo, Documento, Observaciones, Estado)
SELECT 1, 'María González', 'maria.gonzalez@hotmail.com', '3157894561', 'Avenida 68 #25-30', 'Cali', 'Persona Natural', '987654321', 'Organiza bodas', 'Activo'
WHERE NOT EXISTS (SELECT 1 FROM cliente WHERE Email = 'maria.gonzalez@hotmail.com');

INSERT INTO cliente (Empresa_Id, Nombre, Email, Telefono, Direccion, Ciudad, Tipo, Documento, Observaciones, Estado)
SELECT 1, 'Corporación Festiva Ltda', 'info@corpfestiva.co', '3201239876', 'Calle 72 #10-34', 'Barranquilla', 'Empresa', '800456789-2', 'Cliente desde 2020', 'Activo'
WHERE NOT EXISTS (SELECT 1 FROM cliente WHERE Email = 'info@corpfestiva.co');

INSERT INTO cliente (Empresa_Id, Nombre, Email, Telefono, Direccion, Ciudad, Tipo, Documento, Observaciones, Estado)
SELECT 1, 'Carlos Rodríguez', 'carlos.r@gmail.com', '3112345678', 'Transversal 45 #80-12', 'Cartagena', 'Persona Natural', '456789123', NULL, 'Activo'
WHERE NOT EXISTS (SELECT 1 FROM cliente WHERE Email = 'carlos.r@gmail.com');

-- Verificar clientes insertados
SELECT Id, Nombre, Email, Telefono, Ciudad, Tipo, Estado, Fecha_Creacion 
FROM cliente 
ORDER BY Fecha_Creacion DESC;

SELECT CONCAT('Total de clientes: ', COUNT(*)) as resultado FROM cliente;
