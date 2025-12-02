-- ============================================
-- EventConnect - Datos Iniciales Reales
-- Empresas, Usuarios, Productos con Relaciones
-- ============================================

USE db_eventconnect;

-- ============================================
-- 1. ROLES DEL SISTEMA
-- ============================================

INSERT INTO Rol (Id, Nombre, Descripcion, Nivel_Acceso, Permisos) VALUES
(1, 'SuperAdmin', 'Administrador del sistema EventConnect', 0, '["*"]'),
(2, 'Admin-Proveedor', 'Administrador de empresa proveedora', 1, '["productos.*", "inventario.*", "reservas.*", "clientes.*", "reportes.*"]'),
(3, 'Operario', 'Operario de bodega y entregas', 2, '["inventario.view", "movimientos.create", "entregas.*"]'),
(4, 'Cliente', 'Cliente final que alquila productos', 3, '["catalogo.view", "reservas.create", "reservas.view", "pagos.*"]'),
(5, 'Auditor', 'Auditor financiero y de inventario', 4, '["reportes.view", "auditoria.view"]');

-- ============================================
-- 2. EMPRESAS PROVEEDORAS
-- ============================================

INSERT INTO Empresa (Id, Razon_Social, NIT, Email, Telefono, Direccion, Ciudad, Pais, Logo_URL, Estado) VALUES
(1, 'Eventos Elegantes SAS', '900123456-7', 'contacto@eventoselegantes.com', '+57 310 123 4567', 'Calle 85 #15-30', 'Bogotá', 'Colombia', 'https://ui-avatars.com/api/?name=Eventos+Elegantes&background=6366f1&color=fff&size=200', 'Activa'),
(2, 'Party Time Alquileres SAS', '900234567-8', 'info@partytime.com.co', '+57 311 234 5678', 'Carrera 45 #78-23', 'Medellín', 'Colombia', 'https://ui-avatars.com/api/?name=Party+Time&background=f59e0b&color=fff&size=200', 'Activa'),
(3, 'AudioVisual Pro Colombia SAS', '900345678-9', 'ventas@audiovisualpro.co', '+57 312 345 6789', 'Avenida Circunvalar #12-34', 'Cali', 'Colombia', 'https://ui-avatars.com/api/?name=AudioVisual+Pro&background=10b981&color=fff&size=200', 'Activa');

-- ============================================
-- 3. USUARIOS DEL SISTEMA
-- ============================================

-- Password para todos: EventConnect2024! (hash BCrypt)
-- Hash: $2a$10$KZx3qYlJ5K2x8Z9m3/7r3eE4YJzW5qF8vH9L2kN1pM6sR7tU8vW9y

-- 3.1 SuperAdmin
INSERT INTO Usuario (Id, Empresa_Id, Rol_Id, Usuario, Email, Password_Hash, Nombre_Completo, Telefono, Estado) VALUES
(1, NULL, 1, 'superadmin', 'admin@eventconnect.com', '$2a$10$KZx3qYlJ5K2x8Z9m3/7r3eE4YJzW5qF8vH9L2kN1pM6sR7tU8vW9y', 'Administrador Sistema', '+57 300 000 0000', 'Activo');

-- 3.2 Usuarios Eventos Elegantes SAS
INSERT INTO Usuario (Id, Empresa_Id, Rol_Id, Usuario, Email, Password_Hash, Nombre_Completo, Telefono, Avatar_URL, Estado) VALUES
(2, 1, 2, 'admin.elegantes', 'admin@eventoselegantes.com', '$2a$10$KZx3qYlJ5K2x8Z9m3/7r3eE4YJzW5qF8vH9L2kN1pM6sR7tU8vW9y', 'María Fernanda Rodríguez', '+57 310 123 4567', 'https://ui-avatars.com/api/?name=Maria+Rodriguez&background=6366f1&color=fff', 'Activo'),
(3, 1, 3, 'operario.elegantes', 'bodega@eventoselegantes.com', '$2a$10$KZx3qYlJ5K2x8Z9m3/7r3eE4YJzW5qF8vH9L2kN1pM6sR7tU8vW9y', 'Carlos Andrés Gómez', '+57 310 987 6543', 'https://ui-avatars.com/api/?name=Carlos+Gomez&background=6366f1&color=fff', 'Activo');

-- 3.3 Usuarios Party Time Alquileres
INSERT INTO Usuario (Id, Empresa_Id, Rol_Id, Usuario, Email, Password_Hash, Nombre_Completo, Telefono, Avatar_URL, Estado) VALUES
(4, 2, 2, 'admin.partytime', 'admin@partytime.com.co', '$2a$10$KZx3qYlJ5K2x8Z9m3/7r3eE4YJzW5qF8vH9L2kN1pM6sR7tU8vW9y', 'Andrea Paola Martínez', '+57 311 234 5678', 'https://ui-avatars.com/api/?name=Andrea+Martinez&background=f59e0b&color=fff', 'Activo'),
(5, 2, 3, 'operario.partytime', 'logistica@partytime.com.co', '$2a$10$KZx3qYlJ5K2x8Z9m3/7r3eE4YJzW5qF8vH9L2kN1pM6sR7tU8vW9y', 'Jorge Luis Ramírez', '+57 311 876 5432', 'https://ui-avatars.com/api/?name=Jorge+Ramirez&background=f59e0b&color=fff', 'Activo');

-- 3.4 Usuarios AudioVisual Pro Colombia
INSERT INTO Usuario (Id, Empresa_Id, Rol_Id, Usuario, Email, Password_Hash, Nombre_Completo, Telefono, Avatar_URL, Estado) VALUES
(6, 3, 2, 'admin.audiovisual', 'admin@audiovisualpro.co', '$2a$10$KZx3qYlJ5K2x8Z9m3/7r3eE4YJzW5qF8vH9L2kN1pM6sR7tU8vW9y', 'Sebastián Andrés Torres', '+57 312 345 6789', 'https://ui-avatars.com/api/?name=Sebastian+Torres&background=10b981&color=fff', 'Activo'),
(7, 3, 3, 'tecnico.audiovisual', 'tecnico@audiovisualpro.co', '$2a$10$KZx3qYlJ5K2x8Z9m3/7r3eE4YJzW5qF8vH9L2kN1pM6sR7tU8vW9y', 'Daniel Esteban Vargas', '+57 312 987 6543', 'https://ui-avatars.com/api/?name=Daniel+Vargas&background=10b981&color=fff', 'Activo');

-- ============================================
-- 4. BODEGAS
-- ============================================

INSERT INTO Bodega (Id, Empresa_Id, Codigo, Nombre, Tipo_Ubicacion, Direccion, Ciudad, Responsable_Id, Capacidad_M3, Estado) VALUES
(1, 1, 'BOD-EE-01', 'Bodega Principal Eventos Elegantes', 'Principal', 'Calle 85 #15-30', 'Bogotá', 3, 500.00, 'Activa'),
(2, 2, 'BOD-PT-01', 'Bodega Principal Party Time', 'Principal', 'Carrera 45 #78-23', 'Medellín', 5, 350.00, 'Activa'),
(3, 3, 'BOD-AV-01', 'Bodega AudioVisual Pro', 'Principal', 'Avenida Circunvalar #12-34', 'Cali', 7, 200.00, 'Activa');

-- ============================================
-- 5. CATEGORÍAS GLOBALES (Sin empresa_Id)
-- ============================================

-- Categorías base del sistema (IDs 6-17 para pruebas de registro)
INSERT INTO Categoria (Id, Nombre, Descripcion, Icono, Color, Activo) VALUES
(6, 'Decoración', 'Centros de mesa, manteles y elementos decorativos', 'sparkles', '#F59E0B', TRUE),
(7, 'Recreación', 'Inflables, juegos y entretenimiento', 'balloon', '#14B8A6', TRUE),
(8, 'Vajilla y Cristalería', 'Sets completos de vajilla, copas y cubiertos finos', 'utensils', '#EC4899', TRUE),
(9, 'Proyección', 'Proyectores, pantallas y equipos audiovisuales', 'tv', '#6366F1', TRUE),
(10, 'DJ y Animación', 'Servicios de DJ y animación profesional', 'music', '#A855F7', TRUE),
(11, 'Pantallas LED', 'Pantallas LED modulares para eventos', 'monitor', '#06B6D4', TRUE),
(12, 'Alimentos y Bebidas', 'Máquinas de snacks, dulces y bebidas', 'coffee', '#F97316', TRUE),
(13, 'Fotografía y Video', 'Equipos y servicios de fotografía profesional', 'camera', '#EC4899', TRUE),
(14, 'Escenarios y Tarimas', 'Estructuras para escenarios y presentaciones', 'stage', '#8B5CF6', TRUE),
(15, 'Climatización', 'Equipos de calefacción y ventilación', 'fan', '#06B6D4', TRUE),
(16, 'Seguridad', 'Equipos y servicios de seguridad para eventos', 'shield', '#EF4444', TRUE),
(17, 'Transporte', 'Servicios de transporte y logística', 'truck', '#F59E0B', TRUE);

-- ============================================
-- 6. PRODUCTOS - EVENTOS ELEGANTES SAS
-- ============================================

INSERT INTO Producto (Id, Empresa_Id, Categoria_Id, SKU, Nombre, Descripcion, Unidad_Medida, Precio_Alquiler_Dia, Cantidad_Stock, Stock_Minimo, Imagen_URL, Es_Alquilable, Requiere_Mantenimiento, Peso_Kg, Dimensiones, Activo) VALUES
-- Sillas (usa categoría 1: Mobiliario)
(1, 1, 1, 'EE-SIL-001', 'Silla Tiffany Cristal Transparente', 'Silla Tiffany en acrílico cristal, elegante y moderna. Perfecta para bodas y eventos sofisticados.', 'Unidad', 18000.00, 150, 50, 'https://images.unsplash.com/photo-1505843513577-22bb7d21e455?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 3.5, '40cm x 40cm x 92cm', TRUE),
(2, 1, 1, 'EE-SIL-002', 'Silla Tiffany Dorada Premium', 'Clásica silla Tiffany con acabado dorado brillante. La más solicitada para bodas elegantes.', 'Unidad', 16000.00, 200, 50, 'https://images.unsplash.com/photo-1519710164239-da123dc03ef4?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 3.8, '40cm x 40cm x 92cm', TRUE),
(3, 1, 1, 'EE-SIL-003', 'Silla Tiffany Blanca Marfil', 'Silla Tiffany en tono marfil elegante, perfecta para combinar con cualquier decoración.', 'Unidad', 15000.00, 180, 50, 'https://images.unsplash.com/photo-1592078615290-033ee584e267?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 3.7, '40cm x 40cm x 92cm', TRUE),
(4, 1, 1, 'EE-SIL-004', 'Silla Rimax Plástica Blanca', 'Silla rimax económica para eventos masivos. Resistente y práctica.', 'Unidad', 5000.00, 500, 100, 'https://images.unsplash.com/photo-1503602642458-232111445657?w=600&h=400&fit=crop&q=80', TRUE, FALSE, 2.5, '45cm x 45cm x 85cm', TRUE),

-- Mesas (usa categoría 1: Mobiliario)
(5, 1, 1, 'EE-MES-001', 'Mesa Redonda 10 Personas - Mantel Incluido', 'Mesa redonda de 1.80m de diámetro con base metálica y tablero en madera. Mantel blanco incluido.', 'Unidad', 45000.00, 50, 10, 'https://images.unsplash.com/photo-1511795409834-ef04bbd61622?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 35.0, '180cm diámetro x 75cm alto', TRUE),
(6, 1, 1, 'EE-MES-002', 'Mesa Rectangular 8 Personas', 'Mesa rectangular 2.40m x 90cm. Ideal para eventos formales y cenas elegantes.', 'Unidad', 40000.00, 40, 10, 'https://images.unsplash.com/photo-1617806118233-18e1de247200?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 28.0, '240cm x 90cm x 75cm', TRUE),
(7, 1, 1, 'EE-MES-003', 'Mesa Coctel Alta con Mantel', 'Mesa alta tipo coctel de 1.10m de altura. Perfecta para eventos de pie y recepciones.', 'Unidad', 25000.00, 60, 15, 'https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=600&h=400&fit=crop&q=80', TRUE, FALSE, 15.0, '80cm diámetro x 110cm alto', TRUE),

-- Vajilla (usa categoría 8: Vajilla y Cristalería)
(8, 1, 8, 'EE-VAJ-001', 'Vajilla Porcelana Blanca - Set 10 Personas', 'Set completo: 10 platos base, 10 platos hondos, 10 platos postre. Porcelana de primera calidad.', 'Set', 55000.00, 30, 10, 'https://images.unsplash.com/photo-1610701596007-11502861dcfa?w=600&h=400&fit=crop&q=80', TRUE, FALSE, 12.0, 'Caja: 50cm x 40cm x 30cm', TRUE),
(9, 1, 8, 'EE-CUB-001', 'Cubiertería Plateada Elegante - Set 10 Personas', 'Set de 40 piezas: tenedores, cuchillos, cucharas y cucharitas. Acabado plateado brillante.', 'Set', 35000.00, 40, 10, 'https://images.unsplash.com/photo-1578500494198-246f612d3b3d?w=600&h=400&fit=crop&q=80', TRUE, FALSE, 5.0, 'Caja: 40cm x 30cm x 10cm', TRUE),
(10, 1, 8, 'EE-CRI-001', 'Cristalería Fina - Set 10 Copas', 'Incluye: 10 copas vino, 10 copas agua, 10 copas champagne. Cristal de alta calidad.', 'Set', 45000.00, 35, 10, 'https://images.unsplash.com/photo-1547595628-c61a29f496f0?w=600&h=400&fit=crop&q=80', TRUE, FALSE, 8.0, 'Caja: 45cm x 35cm x 25cm', TRUE),

-- Decoración (usa categoría 6: Decoración)
(11, 1, 6, 'EE-DEC-001', 'Centro de Mesa Floral Elegante', 'Arreglo floral premium con rosas, hortensias y follaje. Base en cristal o cerámica.', 'Unidad', 65000.00, 40, 10, 'https://images.unsplash.com/photo-1490750967868-88aa4486c946?w=600&h=400&fit=crop&q=80', TRUE, FALSE, 2.5, '40cm x 40cm x 35cm', TRUE),
(12, 1, 6, 'EE-MAN-001', 'Mantel Redondo Premium - Diversas Telas', 'Manteles en lino, algodón o satín. Disponibles en 20+ colores. Para mesas de 1.80m.', 'Unidad', 18000.00, 100, 20, 'https://images.unsplash.com/photo-1519225421980-715cb0215aed?w=600&h=400&fit=crop&q=80', TRUE, FALSE, 1.5, '2.5m diámetro', TRUE),
(13, 1, 6, 'EE-CAM-001', 'Camino de Mesa Elegante', 'Camino de mesa en yute, lino o encaje. 3m de largo. Perfecto para mesas rectangulares.', 'Unidad', 12000.00, 80, 20, 'https://images.unsplash.com/photo-1464207687429-7505649dae38?w=600&h=400&fit=crop&q=80', TRUE, FALSE, 0.8, '300cm x 40cm', TRUE);

-- ============================================
-- 7. PRODUCTOS - PARTY TIME ALQUILERES
-- ============================================

INSERT INTO Producto (Id, Empresa_Id, Categoria_Id, SKU, Nombre, Descripcion, Unidad_Medida, Precio_Alquiler_Dia, Cantidad_Stock, Stock_Minimo, Imagen_URL, Es_Alquilable, Requiere_Mantenimiento, Peso_Kg, Dimensiones, Activo) VALUES
-- Mobiliario Infantil (usa categoría 1: Mobiliario)
(14, 2, 1, 'PT-SIL-001', 'Silla Infantil Rimax Colores', 'Silla plástica para niños en colores vivos: rojo, azul, amarillo, verde. Resistente y segura.', 'Unidad', 4000.00, 200, 50, 'https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=600&h=400&fit=crop&q=80', TRUE, FALSE, 1.5, '35cm x 35cm x 55cm', TRUE),
(15, 2, 1, 'PT-MES-001', 'Mesa Infantil Rectangular - 6 Niños', 'Mesa plástica resistente para 6 niños. Altura ideal para edades 3-10 años.', 'Unidad', 20000.00, 50, 10, 'https://images.unsplash.com/photo-1503602642458-232111445657?w=600&h=400&fit=crop&q=80', TRUE, FALSE, 12.0, '120cm x 60cm x 55cm', TRUE),

-- Recreación (usa categoría 7: Recreación)
(16, 2, 7, 'PT-INF-001', 'Castillo Inflable Mediano', 'Castillo inflable 4x4m con tobogán. Capacidad: 8 niños. Incluye motor y operador 4 horas.', 'Unidad', 280000.00, 8, 2, 'https://images.unsplash.com/photo-1513151233558-d860c5398176?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 45.0, '4m x 4m x 3m', TRUE),
(17, 2, 7, 'PT-INF-002', 'Castillo Inflable Grande con Piscina de Pelotas', 'Inflable 6x5m con tobogán doble y piscina de pelotas. El más solicitado! Operador 6 horas.', 'Unidad', 450000.00, 5, 1, 'https://images.unsplash.com/photo-1464820453369-31d2c0b651af?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 75.0, '6m x 5m x 4m', TRUE),

-- Decoración Temática (usa categoría 6: Decoración)
(18, 2, 6, 'PT-DEC-001', 'Decoración Temática Princesas - Paquete Completo', 'Incluye: arco de globos, banner, centro de mesa, platos/vasos temáticos para 20 niños.', 'Set', 380000.00, 12, 3, 'https://images.unsplash.com/photo-1530103862676-de8c9debad1d?w=600&h=400&fit=crop&q=80', TRUE, FALSE, 15.0, 'Caja: 100cm x 60cm x 40cm', TRUE),
(19, 2, 6, 'PT-DEC-002', 'Decoración Temática Superhéroes - Paquete Completo', 'Incluye: arco de globos, banner, centro de mesa, platos/vasos temáticos para 20 niños.', 'Set', 380000.00, 12, 3, 'https://images.unsplash.com/photo-1608889476518-738c9b1dcae6?w=600&h=400&fit=crop&q=80', TRUE, FALSE, 15.0, 'Caja: 100cm x 60cm x 40cm', TRUE),

-- Alimentos (usa categoría 12: Alimentos y Bebidas)
(20, 2, 12, 'PT-ALI-001', 'Máquina de Algodón de Azúcar', 'Máquina profesional con operador y 100 conos. Incluye azúcar y palitos de colores.', 'Unidad', 220000.00, 6, 2, 'https://images.unsplash.com/photo-1563559004-4f4a5800a6ed?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 18.0, '50cm x 50cm x 70cm', TRUE),
(21, 2, 12, 'PT-ALI-002', 'Máquina de Crispetas/Palomitas', 'Máquina estilo cine con carrito. Incluye maíz para 100 porciones y bolsas.', 'Unidad', 180000.00, 8, 2, 'https://images.unsplash.com/photo-1585647347483-22b66260dfff?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 25.0, '60cm x 50cm x 150cm', TRUE),
(22, 2, 8, 'PT-VAJ-001', 'Vajilla Desechable Temática - 20 Personas', 'Platos, vasos, servilletas con diseños infantiles. Resistente y biodegradable.', 'Set', 45000.00, 50, 15, 'https://images.unsplash.com/photo-1565299543923-37dd37887442?w=600&h=400&fit=crop&q=80', TRUE, FALSE, 3.0, 'Caja: 40cm x 30cm x 20cm', TRUE);

-- ============================================
-- 8. PRODUCTOS - AUDIOVISUAL PRO COLOMBIA
-- ============================================

INSERT INTO Producto (Id, Empresa_Id, Categoria_Id, SKU, Nombre, Descripcion, Unidad_Medida, Precio_Alquiler_Dia, Cantidad_Stock, Stock_Minimo, Imagen_URL, Es_Alquilable, Requiere_Mantenimiento, Peso_Kg, Dimensiones, Activo) VALUES
-- Sonido (usa categoría 3: Sonido)
(23, 3, 3, 'AV-SON-001', 'Sistema de Sonido Profesional 2000W', 'Dos bafles activos JBL, consola digital, 2 micrófonos inalámbricos. Técnico incluido 6 horas.', 'Set', 550000.00, 15, 3, 'https://images.unsplash.com/photo-1519892300165-cb5542fb47c7?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 85.0, 'Bafles: 50cm x 40cm x 90cm c/u', TRUE),
(24, 3, 3, 'AV-SON-002', 'Sistema de Sonido Premium 5000W', 'Sistema line array, consola digital avanzada, 4 micrófonos. Para eventos grandes. Técnico 8 horas.', 'Set', 1200000.00, 8, 2, 'https://images.unsplash.com/photo-1514320291840-2e0a9bf2a9ae?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 180.0, 'Sistema completo en road cases', TRUE),

-- Iluminación (usa categoría 2: Iluminación)
(25, 3, 2, 'AV-ILU-001', 'Paquete Iluminación Básica', '4 luces LED RGBW, controlador DMX, técnico. Perfecto para fiestas y eventos pequeños.', 'Set', 350000.00, 12, 3, 'https://images.unsplash.com/photo-1470225620780-dba8ba36b745?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 35.0, 'Case: 80cm x 60cm x 40cm', TRUE),
(26, 3, 2, 'AV-ILU-002', 'Paquete Iluminación Profesional Bodas', '12 luces LED, moving heads, luces arquitectónicas, máquina humo. Técnico especializado.', 'Set', 850000.00, 6, 2, 'https://images.unsplash.com/photo-1492684223066-81342ee5ff30?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 120.0, 'Sistema completo en road cases', TRUE),

-- Proyección (usa categoría 9: Proyección)
(27, 3, 9, 'AV-PRO-001', 'Proyector Full HD + Pantalla 3x2m', 'Proyector 5000 lúmenes, pantalla trípode 3x2m, cables HDMI. Ideal para presentaciones.', 'Set', 280000.00, 10, 2, 'https://images.unsplash.com/photo-1615986201152-7686a4867f30?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 22.0, 'Proyector + pantalla en maletas', TRUE),
(28, 3, 9, 'AV-PRO-002', 'Video Beam Profesional + Pantalla Gigante 5x3m', 'Proyector 10,000 lúmenes, pantalla gigante con estructura. Para eventos masivos.', 'Set', 650000.00, 5, 1, 'https://images.unsplash.com/photo-1561342293-e600240d17ca?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 95.0, 'Proyector + estructura modular', TRUE),

-- DJ (usa categoría 10: DJ y Animación)
(29, 3, 10, 'AV-DJ-001', 'Servicio DJ Profesional 4 Horas', 'DJ con experiencia, equipo completo, luces básicas, música variada. 4 horas continuas.', 'Unidad', 450000.00, 8, 2, 'https://images.unsplash.com/photo-1571266028243-d220b17c48c4?w=600&h=400&fit=crop&q=80', TRUE, FALSE, 45.0, 'Equipo completo portátil', TRUE),
(30, 3, 10, 'AV-DJ-002', 'Servicio DJ Premium 6 Horas + Efectos', 'DJ profesional, equipo premium, iluminación avanzada, máquina humo, láser. 6 horas.', 'Unidad', 750000.00, 5, 1, 'https://images.unsplash.com/photo-1516450360452-9312f5e86fc7?w=600&h=400&fit=crop&q=80', TRUE, FALSE, 85.0, 'Equipo completo en road cases', TRUE),

-- Pantallas LED (usa categoría 11: Pantallas LED)
(31, 3, 11, 'AV-LED-001', 'Pantalla LED 2x1m - Eventos Pequeños', 'Pantalla LED modular 2x1m, ideal para presentaciones y videos en eventos pequeños.', 'Unidad', 800000.00, 6, 1, 'https://images.unsplash.com/photo-1587825140708-dfaf72ae4b04?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 45.0, '2m x 1m (módulos)', TRUE),
(32, 3, 11, 'AV-LED-002', 'Pantalla LED Gigante 4x3m - Eventos Masivos', 'Pantalla LED modular 4x3m con estructura reforzada. Para conciertos y eventos grandes.', 'Unidad', 2500000.00, 3, 1, 'https://images.unsplash.com/photo-1563770660941-20978e870e26?w=600&h=400&fit=crop&q=80', TRUE, TRUE, 250.0, '4m x 3m (estructura completa)', TRUE);

-- ============================================
-- 9. ACTIVOS INDIVIDUALES (EJEMPLOS)
-- ============================================

-- Activos para Eventos Elegantes (Sillas Tiffany Cristal)
INSERT INTO Activo (Empresa_Id, Producto_Id, Bodega_Id, Codigo_Activo, Estado_Fisico, Estado_Disponibilidad, Fecha_Compra, Costo_Compra, Proveedor, Vida_Util_Anos) VALUES
(1, 1, 1, 'EE-SIL-001-A001', 'Nuevo', 'Disponible', '2024-01-15', 45000.00, 'Muebles y Eventos SAS', 5),
(1, 1, 1, 'EE-SIL-001-A002', 'Nuevo', 'Disponible', '2024-01-15', 45000.00, 'Muebles y Eventos SAS', 5),
(1, 1, 1, 'EE-SIL-001-A003', 'Excelente', 'Disponible', '2023-06-10', 45000.00, 'Muebles y Eventos SAS', 5),
(1, 1, 1, 'EE-SIL-001-A004', 'Excelente', 'Disponible', '2023-06-10', 45000.00, 'Muebles y Eventos SAS', 5),
(1, 1, 1, 'EE-SIL-001-A005', 'Bueno', 'Disponible', '2022-11-20', 42000.00, 'Muebles y Eventos SAS', 5);

-- Activos para Party Time (Inflables)
INSERT INTO Activo (Empresa_Id, Producto_Id, Bodega_Id, Codigo_Activo, Numero_Serie, Estado_Fisico, Estado_Disponibilidad, Fecha_Compra, Costo_Compra, Proveedor, Vida_Util_Anos) VALUES
(2, 16, 2, 'PT-INF-001-A001', 'INF-2024-001', 'Nuevo', 'Disponible', '2024-03-01', 3500000.00, 'Inflables Colombia', 3),
(2, 16, 2, 'PT-INF-001-A002', 'INF-2024-002', 'Nuevo', 'Disponible', '2024-03-01', 3500000.00, 'Inflables Colombia', 3),
(2, 17, 2, 'PT-INF-002-A001', 'INF-2024-003', 'Nuevo', 'Disponible', '2024-02-15', 5800000.00, 'Inflables Colombia', 3),
(2, 17, 2, 'PT-INF-002-A002', 'INF-2023-004', 'Excelente', 'Disponible', '2023-12-10', 5500000.00, 'Inflables Colombia', 3);

-- Activos para AudioVisual Pro (Sistemas de Sonido)
INSERT INTO Activo (Empresa_Id, Producto_Id, Bodega_Id, Codigo_Activo, Numero_Serie, Estado_Fisico, Estado_Disponibilidad, Fecha_Compra, Costo_Compra, Proveedor, Vida_Util_Anos) VALUES
(3, 23, 3, 'AV-SON-001-A001', 'JBL-2024-001', 'Nuevo', 'Disponible', '2024-04-10', 8500000.00, 'Audio Imports SAS', 7),
(3, 23, 3, 'AV-SON-001-A002', 'JBL-2024-002', 'Nuevo', 'Disponible', '2024-04-10', 8500000.00, 'Audio Imports SAS', 7),
(3, 24, 3, 'AV-SON-002-A001', 'LINE-2024-001', 'Nuevo', 'Disponible', '2024-05-20', 18500000.00, 'Pro Audio Colombia', 7);

-- ============================================
-- 10. CLIENTES DE EJEMPLO
-- ============================================

INSERT INTO Cliente (Empresa_Id, Tipo_Cliente, Nombre, Documento, Tipo_Documento, Email, Telefono, Direccion, Ciudad, Estado) VALUES
-- Clientes de Eventos Elegantes
(1, 'Persona', 'Juan Carlos Pérez Hernández', '1234567890', 'CC', 'jcperez@email.com', '+57 300 111 2222', 'Calle 100 #20-30', 'Bogotá', 'Activo'),
(1, 'Empresa', 'Hotel Gran Eventos SAS', '900555666-1', 'NIT', 'eventos@hotelgraneventos.com', '+57 301 222 3333', 'Carrera 7 #50-80', 'Bogotá', 'Activo'),
(1, 'Persona', 'María Camila Rodríguez López', '9876543210', 'CC', 'mcrodriguez@email.com', '+57 302 333 4444', 'Avenida 19 #120-45', 'Bogotá', 'Activo'),

-- Clientes de Party Time
(2, 'Persona', 'Laura Sofía Gómez Martínez', '1122334455', 'CC', 'lsgomez@email.com', '+57 311 444 5555', 'Calle 30 #25-15', 'Medellín', 'Activo'),
(2, 'Empresa', 'Colegio Los Ángeles', '800111222-3', 'NIT', 'eventos@colegiolosangeles.edu.co', '+57 312 555 6666', 'Carrera 65 #40-20', 'Medellín', 'Activo'),

-- Clientes de AudioVisual Pro
(3, 'Empresa', 'Centro de Convenciones Valle del Cauca', '800333444-5', 'NIT', 'audiovisual@centroconvenciones.com', '+57 320 666 7777', 'Calle 5 #70-30', 'Cali', 'Activo'),
(3, 'Persona', 'Andrés Felipe Vargas Torres', '5544332211', 'CC', 'afvargas@email.com', '+57 321 777 8888', 'Avenida 6N #30-15', 'Cali', 'Activo');

-- ============================================
-- 11. CONFIGURACIÓN DEL SISTEMA
-- ============================================

INSERT INTO Configuracion_Sistema (Clave, Valor, Tipo_Dato, Descripcion, Categoria) VALUES
('sistema.nombre', 'EventConnect', 'string', 'Nombre del sistema', 'General'),
('sistema.version', '1.0.0', 'string', 'Versión actual del sistema', 'General'),
('moneda.simbolo', '$', 'string', 'Símbolo de la moneda', 'Financiero'),
('moneda.codigo', 'COP', 'string', 'Código ISO de la moneda', 'Financiero'),
('iva.porcentaje', '19', 'number', 'Porcentaje de IVA', 'Financiero'),
('reservas.dias_minimos_anticipo', '2', 'number', 'Días mínimos de anticipación para reservar', 'Reservas'),
('reservas.porcentaje_anticipo', '50', 'number', 'Porcentaje de anticipo requerido', 'Reservas'),
('email.smtp_host', 'smtp.gmail.com', 'string', 'Servidor SMTP', 'Email'),
('email.smtp_port', '587', 'number', 'Puerto SMTP', 'Email'),
('notificaciones.email_activo', 'true', 'boolean', 'Activar notificaciones por email', 'Notificaciones'),
('notificaciones.sms_activo', 'false', 'boolean', 'Activar notificaciones por SMS', 'Notificaciones');

-- ============================================
-- FIN DEL SCRIPT
-- ============================================

-- Verificar datos insertados
SELECT 'RESUMEN DE DATOS INSERTADOS:' AS Info;
SELECT 'Empresas:', COUNT(*) AS Total FROM Empresa;
SELECT 'Usuarios:', COUNT(*) AS Total FROM Usuario;
SELECT 'Bodegas:', COUNT(*) AS Total FROM Bodega;
SELECT 'Categorías:', COUNT(*) AS Total FROM Categoria;
SELECT 'Productos:', COUNT(*) AS Total FROM Producto;
SELECT 'Activos:', COUNT(*) AS Total FROM Activo;
SELECT 'Clientes:', COUNT(*) AS Total FROM Cliente;
