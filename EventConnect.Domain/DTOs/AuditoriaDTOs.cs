namespace EventConnect.Domain.DTOs;

public class AuditoriaDto
{
    public int Id { get; set; }
    public string Tabla_Afectada { get; set; } = string.Empty;
    public int Registro_Id { get; set; }
    public int Usuario_Id { get; set; }
    public string Usuario_Nombre { get; set; } = string.Empty;
    public string Usuario_Email { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string? Datos_Anteriores { get; set; }
    public string Datos_Nuevos { get; set; } = string.Empty;
    public string? Detalles { get; set; }
    public string? IP_Origen { get; set; }
    public DateTime Fecha_Accion { get; set; }
    public string Fecha_Accion_Formateada => Fecha_Accion.ToString("dd/MM/yyyy HH:mm:ss");
    public string Accion_Descripcion => ObtenerDescripcionAccion(Accion);

    private static string ObtenerDescripcionAccion(string accion) => accion switch
    {
        "Create" => "Creación",
        "Update" => "Actualización",
        "Delete" => "Eliminación",
        "StatusChange" => "Cambio de Estado",
        "Entrega" => "Entrega",
        "Devolución" => "Devolución",
        "Confirmacion" => "Confirmación",
        _ => accion
    };
}

public class HistorialResponse
{
    public int Registro_Id { get; set; }
    public string Tabla_Afectada { get; set; } = string.Empty;
    public string Tipo_Entidad { get; set; } = string.Empty;
    public List<AuditoriaDto> Timeline { get; set; } = new();
    public int Total_Cambios { get; set; }
    public DateTime Primer_Cambio { get; set; }
    public DateTime Ultimo_Cambio { get; set; }
    public string Usuario_Creacion { get; set; } = string.Empty;
    public string Usuario_Ultima_Actualizacion { get; set; } = string.Empty;
}

public class FiltroAuditoriaRequest
{
    /// <summary>
    /// Nombre de la tabla a consultar (ej: Reserva, Activo, Usuario)
    /// </summary>
    public string? Tabla_Afectada { get; set; }
    
    /// <summary>
    /// ID del registro específico
    /// </summary>
    public int? Registro_Id { get; set; }
    
    /// <summary>
    /// ID del usuario que realizó la acción
    /// </summary>
    public int? Usuario_Id { get; set; }
    
    /// <summary>
    /// Tipo de acción: Create, Update, Delete, StatusChange
    /// </summary>
    public string? Accion { get; set; }
    
    /// <summary>
    /// Fecha desde (inclusive)
    /// </summary>
    public DateTime? Desde { get; set; }
    
    /// <summary>
    /// Fecha hasta (inclusive)
    /// </summary>
    public DateTime? Hasta { get; set; }
    
    /// <summary>
    /// Número de página (default: 1)
    /// </summary>
    public int Pagina { get; set; } = 1;
    
    /// <summary>
    /// Registros por página (default: 50)
    /// </summary>
    public int Registros_Por_Pagina { get; set; } = 50;
}

public class PaginatedAuditoriaResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int Total_Registros { get; set; }
    public int Pagina_Actual { get; set; }
    public int Total_Paginas { get; set; }
    public int Registros_Por_Pagina { get; set; }
}

public class ResumenAuditoriaRequest
{
    public string Tabla_Afectada { get; set; } = string.Empty;
    public int Registro_Id { get; set; }
}

public class ResumenAuditoriaResponse
{
    public int Registro_Id { get; set; }
    public string Tabla_Afectada { get; set; } = string.Empty;
    public int Total_Cambios { get; set; }
    public int Total_Eliminaciones { get; set; }
    public int Total_Creaciones { get; set; }
    public int Total_Actualizaciones { get; set; }
    public DateTime Primer_Cambio { get; set; }
    public DateTime Ultimo_Cambio { get; set; }
    public Dictionary<string, int> Cambios_Por_Usuario { get; set; } = new();
    public List<string> Ultimos_Usuarios { get; set; } = new();
}
