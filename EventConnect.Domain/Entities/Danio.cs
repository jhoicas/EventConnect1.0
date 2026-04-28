namespace EventConnect.Domain.Entities;

/// <summary>
/// Entidad que registra daños y discrepancias en activos
/// Integrada con el sistema de logística y reservas
/// </summary>
public class Danio
{
    public int Id { get; set; }

    /// <summary>
    /// Referencia a la reserva asociada al daño
    /// </summary>
    public int Reserva_Id { get; set; }

    /// <summary>
    /// Referencia al activo que sufrió el daño
    /// </summary>
    public int Activo_Id { get; set; }

    /// <summary>
    /// Referencia al cliente que reporta o está implicado en el daño
    /// </summary>
    public int Cliente_Id { get; set; }

    /// <summary>
    /// Descripción detallada del daño
    /// </summary>
    public string Descripcion { get; set; } = null!;

    /// <summary>
    /// Estados posibles: Reportado, En_Evaluacion, Confirmado, Rechazado, En_Reparacion, Reparado, Pérdida_Total
    /// </summary>
    public string Estado { get; set; } = "Reportado"; // Default

    /// <summary>
    /// Fecha en que se reportó el daño
    /// </summary>
    public DateTime Fecha_Reporte { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// URL de la imagen del daño para evidencia
    /// </summary>
    public string? Imagen_URL { get; set; }

    /// <summary>
    /// Monto estimado de la reparación en moneda local
    /// </summary>
    public decimal? Monto_Estimado { get; set; }

    /// <summary>
    /// Monto final de la reparación
    /// </summary>
    public decimal? Monto_Final { get; set; }

    /// <summary>
    /// Descripción de cómo se resolvió el daño
    /// </summary>
    public string? Resolucion { get; set; }

    /// <summary>
    /// Fecha en que se resolvió el daño
    /// </summary>
    public DateTime? Fecha_Resolucion { get; set; }

    /// <summary>
    /// Usuario que reportó el daño
    /// </summary>
    public int Usuario_Reportador_Id { get; set; }

    /// <summary>
    /// Usuario que evaluó/aprobó el daño
    /// </summary>
    public int? Usuario_Evaluador_Id { get; set; }

    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Fecha de creación del registro
    /// </summary>
    public DateTime Fecha_Creacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime Fecha_Actualizacion { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Usuario? UsuarioReportador { get; set; }
    public virtual Usuario? UsuarioEvaluador { get; set; }
    public virtual Reserva? Reserva { get; set; }
    public virtual Activo? Activo { get; set; }
    public virtual Cliente? Cliente { get; set; }
}
