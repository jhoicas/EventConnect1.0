using System.ComponentModel.DataAnnotations;

namespace EventConnect.Domain.DTOs;

/// <summary>
/// DTO para crear una nueva alerta
/// </summary>
public class CrearAlertaRequest
{
    [Required(ErrorMessage = "El activo es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "ID de activo inválido")]
    public int Activo_Id { get; set; }

    [Required(ErrorMessage = "El tipo de alerta es requerido")]
    [StringLength(50)]
    public string Tipo { get; set; } = null!; // Mantenimiento, Depreciacion, Vencimiento, Garantia

    [Required(ErrorMessage = "La descripción es requerida")]
    [StringLength(500, MinimumLength = 5)]
    public string Descripcion { get; set; } = null!;

    [StringLength(20)]
    public string? Severidad { get; set; } = "Media";

    [StringLength(500)]
    public string? Acciones_Recomendadas { get; set; }

    public DateTime? Fecha_Vencimiento { get; set; }

    [Range(1, 10)]
    public int? Prioridad { get; set; } = 5;
}

/// <summary>
/// DTO para actualizar el estado de una alerta
/// </summary>
public class ActualizarAlertaRequest
{
    [StringLength(50)]
    public string? Estado { get; set; }

    [Range(1, int.MaxValue)]
    public int? Usuario_Asignado_Id { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }

    [Range(1, 10)]
    public int? Prioridad { get; set; }

    public bool? Marcar_Como_Resuelta { get; set; }
}

/// <summary>
/// DTO para filtrar alertas con criterios avanzados
/// </summary>
public class FiltrarAlertasRequest
{
    public int? Activo_Id { get; set; }
    public string? Tipo { get; set; }
    public string? Severidad { get; set; }
    public string? Estado { get; set; }
    public int? Usuario_Asignado_Id { get; set; }
    public DateTime? Fecha_Desde { get; set; }
    public DateTime? Fecha_Hasta { get; set; }
    public bool? Solo_Pendientes { get; set; } = false;

    [Range(1, 100)]
    public int Pagina { get; set; } = 1;

    [Range(5, 100)]
    public int Cantidad_Por_Pagina { get; set; } = 20;
}

/// <summary>
/// DTO básico de alerta para listar
/// </summary>
public class AlertaResponse
{
    public int Id { get; set; }
    public int Activo_Id { get; set; }
    public string Tipo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public string Severidad { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public DateTime Fecha_Alerta { get; set; }
    public DateTime? Fecha_Vencimiento { get; set; }
    public string? NombreActivo { get; set; }
    public int Prioridad { get; set; }
    public int? Dias_Para_Vencer { get; set; }
}

/// <summary>
/// DTO detallado de alerta con toda la información
/// </summary>
public class AlertaDetalladoResponse
{
    public int Id { get; set; }
    public int Activo_Id { get; set; }
    public string Tipo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public string Severidad { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public DateTime Fecha_Alerta { get; set; }
    public DateTime? Fecha_Vencimiento { get; set; }
    public DateTime? Fecha_Resolucion { get; set; }
    public int? Usuario_Asignado_Id { get; set; }
    public string? NombreActivo { get; set; }
    public string? NombreUsuarioAsignado { get; set; }
    public string? Detalles_Tecnicos { get; set; }
    public string? Observaciones { get; set; }
    public string? Acciones_Recomendadas { get; set; }
    public bool Notificacion_Enviada { get; set; }
    public DateTime? Fecha_Notificacion { get; set; }
    public int Prioridad { get; set; }
    public int? Dias_Para_Vencer { get; set; }
}

/// <summary>
/// Respuesta paginada de alertas
/// </summary>
public class PaginatedAlertaResponse
{
    public List<AlertaResponse> Items { get; set; } = new();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int Cantidad_Por_Pagina { get; set; }
    public int Total_Paginas => (Total + Cantidad_Por_Pagina - 1) / Cantidad_Por_Pagina;
}

/// <summary>
/// Estadísticas y resumen de alertas del sistema
/// </summary>
public class ResumenAlertasResponse
{
    public int Total_Alertas { get; set; }
    public int Pendientes { get; set; }
    public int Asignadas { get; set; }
    public int En_Proceso { get; set; }
    public int Resueltas { get; set; }
    public int Vencidas { get; set; }
    public int Ignoradas { get; set; }

    public int Alertas_Criticas { get; set; }
    public int Alertas_Altas { get; set; }
    public int Alertas_Medias { get; set; }
    public int Alertas_Bajas { get; set; }

    public int Mantenimiento_Necesario { get; set; }
    public int Deprecio_Proximo { get; set; }
    public int Garantia_Vencida { get; set; }
    public int Vencimiento_Proximo { get; set; }

    public double Tiempo_Promedio_Resolucion_Dias { get; set; }
    public int Activos_Con_Alertas { get; set; }
}

/// <summary>
/// Resumen de alertas por activo
/// </summary>
public class ResumenAlertasPorActivoResponse
{
    public int Activo_Id { get; set; }
    public string? NombreActivo { get; set; }
    public int Total_Alertas { get; set; }
    public int Pendientes { get; set; }
    public int Criticas { get; set; }
    public List<AlertaResponse> Alertas { get; set; } = new();
}

/// <summary>
/// Respuesta de alerta generada automáticamente
/// </summary>
public class AlertaAutomaticaResponse
{
    public int Id { get; set; }
    public string Tipo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public string Severidad { get; set; } = null!;
    public int Activo_Id { get; set; }
    public DateTime Fecha_Generacion { get; set; }
}

/// <summary>
/// Respuesta de generación de alertas en lote
/// </summary>
public class ResultadoGeneracionAlertasResponse
{
    public int Total_Procesados { get; set; }
    public int Alertas_Creadas { get; set; }
    public int Alertas_Actualizadas { get; set; }
    public int Errores { get; set; }
    public List<string> Mensajes { get; set; } = new();
}
