-- Tabla para contenido editable de landing page
CREATE TABLE IF NOT EXISTS Contenido_Landing (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Seccion VARCHAR(50) NOT NULL COMMENT 'hero, servicios, nosotros, contacto, testimonios',
    Titulo VARCHAR(200) NOT NULL,
    Subtitulo VARCHAR(300),
    Descripcion TEXT,
    Imagen_URL VARCHAR(500),
    Icono_Nombre VARCHAR(50) COMMENT 'Nombre del icono de Lucide React',
    Orden INT DEFAULT 0,
    Activo BOOLEAN DEFAULT TRUE,
    Fecha_Actualizacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_seccion (Seccion),
    INDEX idx_activo (Activo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Contenido por defecto para la landing page
INSERT INTO Contenido_Landing (Seccion, Titulo, Subtitulo, Descripcion, Imagen_URL, Icono_Nombre, Orden, Activo) VALUES
-- Hero Section
('hero', 'Gestiona tu Negocio de Eventos con EventConnect', 'La plataforma todo-en-uno para empresas de alquiler y gestión de eventos', 'Administra inventarios, reservas, activos fijos y mucho más desde un solo lugar. EventConnect te ayuda a crecer y optimizar tu negocio de eventos.', 'https://images.unsplash.com/photo-1492684223066-81342ee5ff30', 'Sparkles', 1, TRUE),

-- Servicios
('servicios', 'Gestión de Reservas', 'Control total de tus eventos', 'Administra reservas, clientes, fechas de eventos, entregas y devoluciones. Sistema completo con estados de workflow y seguimiento de pagos.', 'https://images.unsplash.com/photo-1464366400600-7168b8af9bc3', 'Calendar', 1, TRUE),

('servicios', 'Control de Inventario', 'Nunca pierdas el rastro de tus productos', 'Gestiona productos, categorías, stock en tiempo real, alertas de stock bajo y control de lotes con fechas de vencimiento.', 'https://images.unsplash.com/photo-1553413077-190dd305871c', 'Package', 2, TRUE),

('servicios', 'Sistema SIGI', 'Sistema Integrado de Gestión de Inventarios', 'Módulo avanzado para empresas: gestión de activos fijos, bodegas múltiples, depreciación automática, mantenimientos programados y códigos QR.', 'https://images.unsplash.com/photo-1454165804606-c3d57bc86b40', 'Database', 3, TRUE),

('servicios', 'Gestión de Clientes', 'Base de datos completa de clientes', 'Almacena información de clientes personas y empresas, historial de reservas, calificaciones y datos de contacto.', 'https://images.unsplash.com/photo-1521791136064-7986c2920216', 'Users', 4, TRUE),

('servicios', 'Reportes y Analytics', 'Toma decisiones basadas en datos', 'Visualiza estadísticas, reportes de ventas, productos más rentados, análisis de clientes y mucho más.', 'https://images.unsplash.com/photo-1551288049-bebda4e38f71', 'BarChart3', 5, TRUE),

('servicios', 'Multi-Empresa', 'Gestiona múltiples empresas', 'Ideal para holding o grupos empresariales. Cada empresa con su propia información aislada y segura.', 'https://images.unsplash.com/photo-1556761175-b413da4baf72', 'Building2', 6, TRUE),

-- Nosotros
('nosotros', '¿Por qué EventConnect?', 'Desarrollado por expertos para la industria de eventos', 'EventConnect nace de la necesidad de digitalizar y optimizar empresas de alquiler de equipos para eventos. Nuestra plataforma combina años de experiencia en el sector con tecnología de punta.', 'https://images.unsplash.com/photo-1511578314322-379afb476865', 'Award', 1, TRUE),

-- Planes
('planes', 'Plan Básico', 'Gratis para siempre', 'Gestión de productos, categorías, clientes y reservas básicas. Ideal para emprendedores y pequeñas empresas que inician.', NULL, 'Zap', 1, TRUE),

('planes', 'Plan SIGI', '$99 USD/mes', 'Todo lo del Plan Básico + Sistema Integrado de Gestión de Inventarios: activos fijos, bodegas, lotes, mantenimientos, depreciación y códigos QR.', NULL, 'Rocket', 2, TRUE),

('planes', 'Plan Enterprise', 'Precio personalizado', 'Solución completa para grandes empresas: todo lo de SIGI + soporte prioritario, personalizaciones, integraciones y capacitación.', NULL, 'Crown', 3, TRUE);
