using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Auditoria")]
public class Auditoria
{
    public int Id { get; set; }
    
    /// <summary>
    /// Nombre de la tabla afectada (ej: "Reserva", "Activo", "Usuario")
    /// </summary>
    public string Tabla_Afectada { get; set; } = string.Empty;
    
    /// <summary>
    /// ID del registro afectado en esa tabla
    /// </summary>
    public int Registro_Id { get; set; }
    
    /// <summary>
    /// ID del usuario que realizó la acción
    /// </summary>
    public int Usuario_Id { get; set; }
    
    /// <summary>
    /// Acción realizada: Create, Update, Delete, StatusChange
    /// </summary>
    public string Accion { get; set; } = string.Empty;
    
    /// <summary>
    /// Datos anteriores (JSON serializado)
    /// </summary>
    public string? Datos_Anteriores { get; set; }
    
    /// <summary>
    /// Datos nuevos (JSON serializado)
    /// </summary>
    public string Datos_Nuevos { get; set; } = string.Empty;
    
    /// <summary>
    /// Detalles adicionales de la acción
    /// </summary>
    public string? Detalles { get; set; }
    
    /// <summary>
    /// Dirección IP del usuario
    /// </summary>
    public string? IP_Origen { get; set; }
    
    /// <summary>
    /// User Agent del navegador/app
    /// </summary>
    public string? User_Agent { get; set; }
    
    /// <summary>
    /// Timestamp de la acción
    /// </summary>
    public DateTime Fecha_Accion { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Navegación a Usuario
    /// </summary>
    public virtual Usuario? Usuario { get; set; }
}
