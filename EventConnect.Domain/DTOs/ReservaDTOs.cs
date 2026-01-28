namespace EventConnect.Domain.DTOs;

/// <summary>
/// DTO para la respuesta de una reserva con información del cliente y las empresas
/// </summary>
public class ReservationResponse
{
    public int Id { get; set; }
    public int Cliente_Id { get; set; }
    public string Codigo_Reserva { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime Fecha_Evento { get; set; }
    public DateTime? Fecha_Entrega { get; set; }
    public DateTime? Fecha_Devolucion_Programada { get; set; }
    public DateTime? Fecha_Devolucion_Real { get; set; }
    public string? Direccion_Entrega { get; set; }
    public string? Ciudad_Entrega { get; set; }
    public string? Contacto_En_Sitio { get; set; }
    public string? Telefono_Contacto { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public decimal Fianza { get; set; }
    public bool Fianza_Devuelta { get; set; }
    public string Metodo_Pago { get; set; } = string.Empty;
    public string Estado_Pago { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public DateTime Fecha_Creacion { get; set; }
    public DateTime? Fecha_Vencimiento_Cotizacion { get; set; }
    public DateTime Fecha_Actualizacion { get; set; }
    
    // Información del Cliente
    public string? Cliente_Nombre { get; set; }
    public string? Cliente_Email { get; set; }
    public string? Cliente_Telefono { get; set; }
    public string? Cliente_Documento { get; set; }
    
    // Información de empresas involucradas (multivendedor)
    public int Cantidad_Empresas { get; set; }
    public List<string>? Empresas_Involucradas { get; set; }
    
    // Detalles de la reserva
    public List<ReservationDetailResponse>? Detalles { get; set; }
    
    // Información adicional
    public string? Creado_Por_Nombre { get; set; }
    public string? Aprobado_Por_Nombre { get; set; }
}

/// <summary>
/// DTO para los detalles de una reserva (multivendedor)
/// </summary>
public class ReservationDetailResponse
{
    public int Id { get; set; }
    public int Reserva_Id { get; set; }
    public int Empresa_Id { get; set; }
    public string? Empresa_Nombre { get; set; }
    public int? Producto_Id { get; set; }
    public string? Producto_Nombre { get; set; }
    public int? Activo_Id { get; set; }
    public string? Activo_Codigo { get; set; }
    public int Cantidad { get; set; }
    public decimal Precio_Unitario { get; set; }
    public decimal Subtotal { get; set; }
    public int Dias_Alquiler { get; set; }
    public string? Observaciones { get; set; }
    public string Estado_Item { get; set; } = "OK";
    public DateTime Fecha_Creacion { get; set; }
}

/// <summary>
/// DTO para crear una nueva reserva con múltiples detalles de diferentes empresas
/// </summary>
public class CreateReservationRequest
{
    public int Cliente_Id { get; set; }
    public DateTime Fecha_Evento { get; set; }
    public DateTime? Fecha_Entrega { get; set; }
    public DateTime? Fecha_Devolucion_Programada { get; set; }
    public string? Direccion_Entrega { get; set; }
    public string? Ciudad_Entrega { get; set; }
    public string? Contacto_En_Sitio { get; set; }
    public string? Telefono_Contacto { get; set; }
    public string Metodo_Pago { get; set; } = "Efectivo";
    public string? Observaciones { get; set; }
    public DateTime? Fecha_Vencimiento_Cotizacion { get; set; }
    
    // Detalles multivendedor
    public List<CreateReservationDetailRequest> Detalles { get; set; } = new();
}

/// <summary>
/// DTO para crear detalles de reserva (ítems individuales de diferentes empresas)
/// </summary>
public class CreateReservationDetailRequest
{
    public int Empresa_Id { get; set; } // Empresa proveedora del producto
    public int? Producto_Id { get; set; }
    public int? Activo_Id { get; set; }
    public int Cantidad { get; set; } = 1;
    public decimal Precio_Unitario { get; set; }
    public int Dias_Alquiler { get; set; } = 1;
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para actualizar el estado de una reserva
/// </summary>
public class UpdateReservationStatusRequest
{
    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string? Razon_Cancelacion { get; set; }
}

/// <summary>
/// DTO para obtener reservas con filtros
/// </summary>
public class ReservationFilterRequest
{
    public string? Estado { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? ClienteId { get; set; }
    public int? EmpresaId { get; set; } // Para filtrar por empresa proveedora
}

/// <summary>
/// DTO para estadísticas de reservas
/// </summary>
public class ReservationStatsDTO
{
    public int Total_Reservas { get; set; }
    public int Reservas_Pendientes { get; set; }
    public int Reservas_Confirmadas { get; set; }
    public int Reservas_Canceladas { get; set; }
    public int Reservas_Completadas { get; set; }
    public decimal Total_Ingresos { get; set; }
    public decimal Total_Pendiente_Pago { get; set; }
}

/// <summary>
/// DTO para resumen de una reserva (vista cliente)
/// </summary>
public class ReservationSummaryDTO
{
    public int Id { get; set; }
    public string Codigo_Reserva { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime Fecha_Evento { get; set; }
    public DateTime Fecha_Creacion { get; set; }
    public decimal Total { get; set; }
    public int Cantidad_Empresas { get; set; }
    public List<string>? Empresas { get; set; }
}
