using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

/// <summary>
/// Entidad para evidencias de entregas, devoluciones y daños en logística
/// </summary>
[Table("Evidencia_Entrega")]
public class EvidenciaEntrega
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID de la reserva asociada
    /// </summary>
    [Column("Reserva_Id")]
    public int Reserva_Id { get; set; }
    
    /// <summary>
    /// ID de la empresa (para multi-tenancy)
    /// Se obtiene automáticamente de la reserva, pero se guarda para consultas rápidas
    /// </summary>
    [Column("Empresa_Id")]
    public int Empresa_Id { get; set; }
    
    /// <summary>
    /// ID del usuario/operario que subió la evidencia
    /// </summary>
    [Column("Usuario_Id")]
    public int Usuario_Id { get; set; }
    
    /// <summary>
    /// Tipo de evidencia: Entrega, Devolucion, Dano
    /// </summary>
    public string Tipo { get; set; } = "Entrega";
    
    /// <summary>
    /// URL de la imagen guardada
    /// </summary>
    [Column("Url_Imagen")]
    public string Url_Imagen { get; set; } = string.Empty;
    
    /// <summary>
    /// Comentario u observaciones del operario
    /// </summary>
    public string? Comentario { get; set; }
    
    /// <summary>
    /// Latitud GPS (opcional, para geolocalización)
    /// </summary>
    public decimal? Latitud { get; set; }
    
    /// <summary>
    /// Longitud GPS (opcional, para geolocalización)
    /// </summary>
    public decimal? Longitud { get; set; }
    
    /// <summary>
    /// Nombre de quien recibe entrega/devolución (opcional)
    /// </summary>
    [Column("Nombre_Recibe")]
    public string? Nombre_Recibe { get; set; }
    
    /// <summary>
    /// Firma digital o URL de imagen de firma (opcional)
    /// </summary>
    [Column("Url_Firma")]
    public string? Url_Firma { get; set; }
    
    /// <summary>
    /// Fecha de creación de la evidencia
    /// </summary>
    [Column("Fecha_Creacion")]
    public DateTime Fecha_Creacion { get; set; }
}
