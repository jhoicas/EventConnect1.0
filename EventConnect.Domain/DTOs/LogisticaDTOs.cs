namespace EventConnect.Domain.DTOs;

/// <summary>
/// Request para crear una evidencia de entrega/devolución/daño
/// </summary>
public class CrearEvidenciaRequest
{
    /// <summary>
    /// ID de la reserva
    /// </summary>
    public int ReservaId { get; set; }

    /// <summary>
    /// Tipo de evidencia: Entrega, Devolucion, Dano
    /// </summary>
    public string Tipo { get; set; } = "Entrega";

    /// <summary>
    /// Comentario u observaciones
    /// </summary>
    public string? Comentario { get; set; }

    /// <summary>
    /// Latitud GPS (opcional)
    /// </summary>
    public decimal? Latitud { get; set; }

    /// <summary>
    /// Longitud GPS (opcional)
    /// </summary>
    public decimal? Longitud { get; set; }

    /// <summary>
    /// Nombre de quien recibe entrega/devolución (opcional)
    /// </summary>
    public string? NombreRecibe { get; set; }
}

/// <summary>
/// Response para evidencia creada
/// </summary>
public class EvidenciaResponse
{
    public int Id { get; set; }
    public int ReservaId { get; set; }
    public int EmpresaId { get; set; }
    public int UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string UrlImagen { get; set; } = string.Empty;
    public string? Comentario { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public string? NombreRecibe { get; set; }
    public string? UrlFirma { get; set; }
    public DateTime FechaCreacion { get; set; }
}

/// <summary>
/// Request para completar entrega de una reserva
/// </summary>
public class CompletarEntregaRequest
{
    /// <summary>
    /// Comentarios adicionales al completar la entrega
    /// </summary>
    public string? Comentarios { get; set; }
}
