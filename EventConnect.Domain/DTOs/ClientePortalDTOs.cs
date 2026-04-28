using System.ComponentModel.DataAnnotations;

namespace EventConnect.Domain.DTOs;

/// <summary>
/// DTO para que un cliente vea sus propias reservas
/// </summary>
public class MiReservaResponse
{
    public int Id { get; set; }
    public DateTime Fecha_Reserva { get; set; }
    public DateTime Fecha_Inicio { get; set; }
    public DateTime Fecha_Fin { get; set; }
    public string Estado { get; set; } = null!;
    public decimal Monto_Total { get; set; }
    public decimal? Monto_Pagado { get; set; }
    public decimal Saldo_Pendiente => Monto_Total - (Monto_Pagado ?? 0);
    public int Cantidad_Activos { get; set; }
    public List<string> NombresActivos { get; set; } = new();
    public bool Requiere_Entrega { get; set; }
    public bool Entrega_Completada { get; set; }
    public bool Devolucion_Completada { get; set; }
}

/// <summary>
/// DTO para crear una nueva reserva desde el portal del cliente
/// </summary>
public class CrearReservaClienteRequest
{
    [Required(ErrorMessage = "La fecha de inicio es requerida")]
    public DateTime Fecha_Inicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es requerida")]
    public DateTime Fecha_Fin { get; set; }

    [Required(ErrorMessage = "Debe incluir al menos un activo")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un activo")]
    public List<DetalleReservaClienteRequest> Activos { get; set; } = new();

    [StringLength(500)]
    public string? Observaciones { get; set; }

    [StringLength(200)]
    public string? Direccion_Entrega { get; set; }
}

/// <summary>
/// DTO para especificar cada activo en la reserva del cliente
/// </summary>
public class DetalleReservaClienteRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ID de activo inválido")]
    public int Activo_Id { get; set; }

    [Required]
    [Range(1, 1000, ErrorMessage = "Cantidad debe ser entre 1 y 1000")]
    public int Cantidad { get; set; }

    [Range(0, 999999.99, ErrorMessage = "Precio unitario debe ser válido")]
    public decimal? Precio_Unitario_Sugerido { get; set; }
}

/// <summary>
/// DTO para el seguimiento detallado de una reserva
/// </summary>
public class SeguimientoReservaResponse
{
    public int Reserva_Id { get; set; }
    public string Estado_Actual { get; set; } = null!;
    public DateTime Fecha_Reserva { get; set; }
    public DateTime Fecha_Inicio { get; set; }
    public DateTime Fecha_Fin { get; set; }
    public decimal Monto_Total { get; set; }
    public decimal Monto_Pagado { get; set; }
    public decimal Saldo_Pendiente { get; set; }
    
    // Detalles de activos
    public List<ActivoReservadoResponse> Activos { get; set; } = new();
    
    // Logística
    public LogisticaReservaResponse? Logistica { get; set; }
    
    // Pagos
    public List<PagoReservaResponse> Pagos { get; set; } = new();
    
    // Historial de estados
    public List<CambioEstadoReservaResponse> Historial_Estados { get; set; } = new();
}

/// <summary>
/// DTO para activo dentro de la reserva
/// </summary>
public class ActivoReservadoResponse
{
    public int Activo_Id { get; set; }
    public string Nombre { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal Precio_Unitario { get; set; }
    public decimal Subtotal { get; set; }
    public string? Estado_Activo { get; set; }
}

/// <summary>
/// DTO para información de logística de la reserva
/// </summary>
public class LogisticaReservaResponse
{
    public bool Entrega_Programada { get; set; }
    public DateTime? Fecha_Entrega_Programada { get; set; }
    public DateTime? Fecha_Entrega_Real { get; set; }
    public string? Estado_Entrega { get; set; }
    public bool Devolucion_Programada { get; set; }
    public DateTime? Fecha_Devolucion_Programada { get; set; }
    public DateTime? Fecha_Devolucion_Real { get; set; }
    public string? Estado_Devolucion { get; set; }
    public string? Direccion_Entrega { get; set; }
}

/// <summary>
/// DTO para cada pago realizado
/// </summary>
public class PagoReservaResponse
{
    public int Id { get; set; }
    public DateTime Fecha_Pago { get; set; }
    public decimal Monto { get; set; }
    public string Metodo_Pago { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public string? Referencia { get; set; }
}

/// <summary>
/// DTO para cambios de estado en la reserva
/// </summary>
public class CambioEstadoReservaResponse
{
    public string Estado { get; set; } = null!;
    public DateTime Fecha_Cambio { get; set; }
    public string? Usuario { get; set; }
    public string? Comentario { get; set; }
}

/// <summary>
/// DTO para cotizaciones del cliente
/// </summary>
public class MiCotizacionResponse
{
    public int Id { get; set; }
    public DateTime Fecha_Solicitud { get; set; }
    public string Estado { get; set; } = null!;
    public string? Descripcion_Servicio { get; set; }
    public DateTime Fecha_Evento { get; set; }
    public string? Ubicacion_Evento { get; set; }
    public decimal? Monto_Estimado { get; set; }
    public bool Tiene_Respuesta { get; set; }
    public DateTime? Fecha_Respuesta { get; set; }
}

/// <summary>
/// DTO para solicitar una cotización
/// </summary>
public class SolicitarCotizacionClienteRequest
{
    [Required(ErrorMessage = "El servicio es requerido")]
    [Range(1, int.MaxValue)]
    public int Servicio_Id { get; set; }

    [Required(ErrorMessage = "La descripción es requerida")]
    [StringLength(1000, MinimumLength = 20)]
    public string Descripcion { get; set; } = null!;

    [Required(ErrorMessage = "La fecha del evento es requerida")]
    public DateTime Fecha_Evento { get; set; }

    [Required(ErrorMessage = "La ubicación es requerida")]
    [StringLength(200)]
    public string Ubicacion_Evento { get; set; } = null!;

    [Range(1, 10000)]
    public int? Cantidad_Personas_Estimada { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }
}

/// <summary>
/// Estadísticas del cliente
/// </summary>
public class EstadisticasClienteResponse
{
    public int Total_Reservas { get; set; }
    public int Reservas_Activas { get; set; }
    public int Reservas_Completadas { get; set; }
    public int Reservas_Canceladas { get; set; }
    public decimal Total_Gastado { get; set; }
    public decimal Saldo_Pendiente { get; set; }
    public int Total_Cotizaciones { get; set; }
    public int Cotizaciones_Pendientes { get; set; }
    public DateTime? Ultima_Reserva { get; set; }
    public DateTime Fecha_Registro { get; set; }
}

/// <summary>
/// Respuesta de disponibilidad para fechas
/// </summary>
public class DisponibilidadResponse
{
    public int Activo_Id { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Disponible { get; set; }
    public int Cantidad_Disponible { get; set; }
    public string? Motivo_No_Disponible { get; set; }
    public DateTime? Proxima_Disponibilidad { get; set; }
}

/// <summary>
/// Respuesta de verificación de disponibilidad
/// </summary>
public class VerificarDisponibilidadResponse
{
    public DateTime Fecha_Inicio { get; set; }
    public DateTime Fecha_Fin { get; set; }
    public bool Todos_Disponibles { get; set; }
    public List<DisponibilidadResponse> Activos { get; set; } = new();
}
