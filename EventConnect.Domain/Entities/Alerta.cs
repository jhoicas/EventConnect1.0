namespace EventConnect.Domain.Entities;

/// <summary>
/// Entidad que representa alertas de mantenimiento, depreciación y vencimiento de activos
/// Sistema preventivo para mantener activos en óptimas condiciones
/// </summary>
public class Alerta
{
    public int Id { get; set; }

    /// <summary>
    /// Referencia al activo asociado a la alerta
    /// </summary>
    public int Activo_Id { get; set; }

    /// <summary>
    /// Tipo de alerta: Mantenimiento, Depreciacion, Vencimiento, Garantia
    /// </summary>
    public string Tipo { get; set; } = null!; // Mantenimiento, Depreciacion, Vencimiento, Garantia

    /// <summary>
    /// Descripción detallada de la alerta
    /// </summary>
    public string Descripcion { get; set; } = null!;

    /// <summary>
    /// Severidad: Critica, Alta, Media, Baja
    /// </summary>
    public string Severidad { get; set; } = "Media"; // Default

    /// <summary>
    /// Estados posibles: Pendiente, Asignada, En_Proceso, Resuelta, Ignorada, Vencida
    /// </summary>
    public string Estado { get; set; } = "Pendiente"; // Default

    /// <summary>
    /// Fecha en que se generó la alerta
    /// </summary>
    public DateTime Fecha_Alerta { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha en que debe resolvarse la alerta (fecha límite)
    /// </summary>
    public DateTime? Fecha_Vencimiento { get; set; }

    /// <summary>
    /// Fecha en que se resolvió la alerta
    /// </summary>
    public DateTime? Fecha_Resolucion { get; set; }

    /// <summary>
    /// Usuario responsable de resolver la alerta
    /// </summary>
    public int? Usuario_Asignado_Id { get; set; }

    /// <summary>
    /// Detalles técnicos adicionales (JSON)
    /// </summary>
    public string? Detalles_Tecnicos { get; set; }

    /// <summary>
    /// Observaciones o notas sobre la alerta
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Acciones recomendadas para resolver
    /// </summary>
    public string? Acciones_Recomendadas { get; set; }

    /// <summary>
    /// Indica si se envió notificación al usuario
    /// </summary>
    public bool Notificacion_Enviada { get; set; } = false;

    /// <summary>
    /// Fecha en que se envió la notificación
    /// </summary>
    public DateTime? Fecha_Notificacion { get; set; }

    /// <summary>
    /// Prioridad calculada (1-10) para ordenar alertas
    /// </summary>
    public int Prioridad { get; set; } = 5;

    /// <summary>
    /// Fecha de creación del registro
    /// </summary>
    public DateTime Fecha_Creacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime Fecha_Actualizacion { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Activo? Activo { get; set; }
    public virtual Usuario? UsuarioAsignado { get; set; }
}
