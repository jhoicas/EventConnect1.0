using System.ComponentModel.DataAnnotations;

namespace EventConnect.Domain.DTOs;

/// <summary>
/// DTO para crear un nuevo reporte de daño
/// </summary>
public class CrearDanioRequest
{
    [Required(ErrorMessage = "La reserva es requerida")]
    [Range(1, int.MaxValue, ErrorMessage = "ID de reserva inválido")]
    public int Reserva_Id { get; set; }

    [Required(ErrorMessage = "El activo es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "ID de activo inválido")]
    public int Activo_Id { get; set; }

    [Required(ErrorMessage = "El cliente es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "ID de cliente inválido")]
    public int Cliente_Id { get; set; }

    [Required(ErrorMessage = "La descripción es requerida")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 1000 caracteres")]
    public string Descripcion { get; set; } = null!;

    [Range(0, 999999.99, ErrorMessage = "El monto debe ser un valor válido")]
    public decimal? Monto_Estimado { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }

    [MaxLength(500)]
    public string? Imagen_URL { get; set; }
}

/// <summary>
/// DTO para actualizar el estado de un daño
/// </summary>
public class ActualizarDanioRequest
{
    [Required(ErrorMessage = "El estado es requerido")]
    [StringLength(50)]
    public string Estado { get; set; } = null!;

    [StringLength(1000)]
    public string? Resolucion { get; set; }

    [Range(0, 999999.99, ErrorMessage = "El monto debe ser un valor válido")]
    public decimal? Monto_Final { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "ID de usuario evaluador inválido")]
    public int? Usuario_Evaluador_Id { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para filtrar daños con opciones avanzadas
/// </summary>
public class FiltrarDaniosRequest
{
    public int? Reserva_Id { get; set; }
    public int? Activo_Id { get; set; }
    public int? Cliente_Id { get; set; }
    public string? Estado { get; set; }
    public DateTime? Fecha_Desde { get; set; }
    public DateTime? Fecha_Hasta { get; set; }
    public int? Usuario_Reportador_Id { get; set; }

    [Range(1, 100)]
    public int Pagina { get; set; } = 1;

    [Range(5, 100)]
    public int Cantidad_Por_Pagina { get; set; } = 20;
}

/// <summary>
/// DTO básico de daño para listar
/// </summary>
public class DanioResponse
{
    public int Id { get; set; }
    public int Reserva_Id { get; set; }
    public int Activo_Id { get; set; }
    public int Cliente_Id { get; set; }
    public string Descripcion { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public DateTime Fecha_Reporte { get; set; }
    public string? NombreActivo { get; set; }
    public string? NombreCliente { get; set; }
    public decimal? Monto_Estimado { get; set; }
    public decimal? Monto_Final { get; set; }
}

/// <summary>
/// DTO detallado de daño con toda la información
/// </summary>
public class DanioDetalladoResponse
{
    public int Id { get; set; }
    public int Reserva_Id { get; set; }
    public int Activo_Id { get; set; }
    public int Cliente_Id { get; set; }
    public string Descripcion { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public DateTime Fecha_Reporte { get; set; }
    public DateTime? Fecha_Resolucion { get; set; }
    public string? Imagen_URL { get; set; }
    public decimal? Monto_Estimado { get; set; }
    public decimal? Monto_Final { get; set; }
    public string? Resolucion { get; set; }
    public int Usuario_Reportador_Id { get; set; }
    public int? Usuario_Evaluador_Id { get; set; }
    public string? NombreReportador { get; set; }
    public string? NombreEvaluador { get; set; }
    public string? NombreActivo { get; set; }
    public string? NombreCliente { get; set; }
    public string? Observaciones { get; set; }
    public DateTime Fecha_Creacion { get; set; }
    public DateTime Fecha_Actualizacion { get; set; }
}

/// <summary>
/// Estadísticas de daños del sistema
/// </summary>
public class EstadisticasDaniosResponse
{
    public int Total_Danios { get; set; }
    public int Reportados { get; set; }
    public int En_Evaluacion { get; set; }
    public int Confirmados { get; set; }
    public int Rechazados { get; set; }
    public int En_Reparacion { get; set; }
    public int Reparados { get; set; }
    public int Perdida_Total { get; set; }
    public decimal Monto_Total_Estimado { get; set; }
    public decimal Monto_Total_Final { get; set; }
    public decimal Promedio_Resolucion_Dias { get; set; }
    public int Danios_Este_Mes { get; set; }
    public int Danios_Por_Cliente_Top_5 { get; set; }
}

/// <summary>
/// Respuesta paginada de daños
/// </summary>
public class PaginatedDanioResponse
{
    public List<DanioResponse> Items { get; set; } = new();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int Cantidad_Por_Pagina { get; set; }
    public int Total_Paginas => (Total + Cantidad_Por_Pagina - 1) / Cantidad_Por_Pagina;
}

/// <summary>
/// Respuesta de resumen de daños por activo
/// </summary>
public class ResumenDanioActivoResponse
{
    public int Activo_Id { get; set; }
    public string? NombreActivo { get; set; }
    public int Total_Danios { get; set; }
    public int Confirmados { get; set; }
    public int En_Reparacion { get; set; }
    public decimal Monto_Total { get; set; }
    public List<DanioResponse> Danios { get; set; } = new();
}
