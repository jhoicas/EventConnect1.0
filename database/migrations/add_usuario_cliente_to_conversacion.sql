-- Agregar campos de Cliente_Id y Usuario_Id a la tabla Conversacion
-- Para identificar quién participa en la conversación

ALTER TABLE Conversacion 
ADD COLUMN Cliente_Id INT NULL COMMENT 'Usuario cliente que participa en la conversación',
ADD COLUMN Usuario_Id INT NULL COMMENT 'Usuario de la empresa que participa en la conversación';

-- Agregar índices para mejorar el rendimiento
ALTER TABLE Conversacion 
ADD INDEX idx_conversacion_cliente (Cliente_Id),
ADD INDEX idx_conversacion_usuario (Usuario_Id);

-- Agregar llaves foráneas
ALTER TABLE Conversacion 
ADD CONSTRAINT fk_conversacion_cliente 
    FOREIGN KEY (Cliente_Id) REFERENCES Usuario(Id) ON DELETE SET NULL,
ADD CONSTRAINT fk_conversacion_usuario 
    FOREIGN KEY (Usuario_Id) REFERENCES Usuario(Id) ON DELETE SET NULL;
