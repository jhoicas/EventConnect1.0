# Entidades del Dominio - EventConnect

##  Entidades Implementadas (19 entidades)

### Módulo Core (Gestión Base)
1. **Empresa** - Multi-tenancy, datos empresa
2. **Rol** - Roles de usuarios
3. **Usuario** - Usuarios con autenticación 2FA
4. **Cliente** - Clientes del sistema
5. **Suscripcion** - Planes y suscripciones (módulo SIGI)

### Módulo Inventario Básico
6. **Categoria** - Categorías de productos
7. **Producto** - Productos/mobiliario para eventos
8. **Reserva** - Reservas de productos
9. **DetalleReserva** - Detalle de productos en reserva
10. **Pago** - Pagos de reservas

### Módulo SIGI (Inventario Avanzado)
11. **Activo** - Activos fijos con QR, depreciación
12. **Bodega** - Bodegas/almacenes
13. **Lote** - Lotes de productos con trazabilidad
14. **MovimientoInventario** - Entradas, salidas, transferencias
15. **Mantenimiento** - Mantenimiento preventivo/correctivo
16. **Depreciacion** - Cálculo de depreciación de activos

### Módulo Auditoría y Notificaciones
17. **LogAuditoria** - Trazabilidad inmutable (SHA-256)
18. **Notificacion** - Notificaciones Email/SMS/Push
19. **ConfiguracionSistema** - Configuración global y por empresa

## Características de las Entidades

 **Mapeo con Dapper**: Todas usan [Table] y [Column] attributes
 **Multi-tenancy**: Campo Empresa_Id donde aplica
 **Auditoría**: Campos Fecha_Creacion, Fecha_Actualizacion
 **Estados**: Máquinas de estado para procesos
 **Nullables**: Campos opcionales con ?
 **Valores Default**: Inicialización de campos obligatorios

## Próximos Pasos

1.  Entidades del dominio - **COMPLETADO**
2.  Crear Repositorios especializados (Activo, Bodega, Lote, etc.)
3.  Implementar Controllers para SIGI
4.  Crear DTOs para nuevas entidades
5.  Agregar lógica de negocio en Application layer
