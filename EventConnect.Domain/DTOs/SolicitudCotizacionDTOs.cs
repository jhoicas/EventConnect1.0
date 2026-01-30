namespace EventConnect.Domain.DTOs;

public class SolicitudCotizacionDTO
{
    public int Id { get; set; }
    public int Cliente_Id { get; set; }
    public int Producto_Id { get; set; }
    public DateTime Fecha_Solicitud { get; set; }
    public int Cantidad_Solicitada { get; set; }
    public decimal Monto_Cotizacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public DateTime? Fecha_Respuesta { get; set; }
    public DateTime Fecha_Creacion { get; set; }
    public DateTime Fecha_Actualizacion { get; set; }
}

public class CreateSolicitudCotizacionRequest
{
    public int? Cliente_Id { get; set; }
    public int? Producto_Id { get; set; }
    public int? Cantidad_Solicitada { get; set; }
    public string? Observaciones { get; set; }
}

public class UpdateSolicitudCotizacionRequest
{
    public decimal? Monto_Cotizacion { get; set; }
    public string? Estado { get; set; }
    public DateTime? Fecha_Respuesta { get; set; }
    public string? Observaciones { get; set; }
}
