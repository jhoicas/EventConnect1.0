namespace EventConnect.Domain.DTOs;

/// <summary>
/// DTO para el historial de movimientos de inventario de un activo
/// </summary>
public class HistorialMovimientoDto
{
    public int Id { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public string? UbicacionOrigen { get; set; }
    public string? UbicacionDestino { get; set; }
    public string? Motivo { get; set; }
    public int? UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
}

/// <summary>
/// DTO para el historial de mantenimientos de un activo
/// </summary>
public class HistorialMantenimientoDto
{
    public int Id { get; set; }
    public DateTime? FechaProgramada { get; set; }
    public DateTime? FechaRealizada { get; set; }
    public string TipoMantenimiento { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal? Costo { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string? ProveedorServicio { get; set; }
}

/// <summary>
/// DTO para el historial de reservas de un activo
/// </summary>
public class HistorialReservaDto
{
    public int Id { get; set; }
    public int ReservaId { get; set; }
    public string CodigoReserva { get; set; } = string.Empty;
    public DateTime FechaEvento { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string? ClienteEmail { get; set; }
    public string EstadoReserva { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int DiasAlquiler { get; set; }
}

/// <summary>
/// DTO completo para la Hoja de Vida de un Activo
/// </summary>
public class HojaVidaActivoDto
{
    // Información del Activo
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int ProductoId { get; set; }
    public string? ProductoNombre { get; set; }
    public int BodegaId { get; set; }
    public string? BodegaNombre { get; set; }
    public string CodigoActivo { get; set; } = string.Empty;
    public string? CodigoQR { get; set; }
    public string? NumeroSerie { get; set; }
    public string EstadoFisico { get; set; } = string.Empty;
    public string EstadoDisponibilidad { get; set; } = string.Empty;
    public DateTime? FechaCompra { get; set; }
    public decimal? CostoAdquisicion { get; set; }
    public decimal? ValorResidual { get; set; }
    public int? VidaUtilMeses { get; set; }
    public string? Proveedor { get; set; }
    public DateTime FechaCreacion { get; set; }
    
    // Historiales
    public List<HistorialMovimientoDto> HistorialMovimientos { get; set; } = new();
    public List<HistorialMantenimientoDto> HistorialMantenimientos { get; set; } = new();
    public List<HistorialReservaDto> HistorialReservas { get; set; } = new();
}
