using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Notificacion")]
public class Notificacion
{
    public int Id { get; set; }
    
    [Column("Empresa_Id")]
    public int? Empresa_Id { get; set; }
    
    [Column("Usuario_Id")]
    public int? Usuario_Id { get; set; }
    
    public string Tipo { get; set; } = string.Empty; // Email, SMS, Push, Sistema
    
    public string Titulo { get; set; } = string.Empty;
    
    public string Mensaje { get; set; } = string.Empty;
    
    [Column("Destinatario_Email")]
    public string? Destinatario_Email { get; set; }
    
    [Column("Destinatario_Telefono")]
    public string? Destinatario_Telefono { get; set; }
    
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Enviado, Error
    
    public bool Leido { get; set; } = false;
    
    [Column("Fecha_Envio")]
    public DateTime? Fecha_Envio { get; set; }
    
    [Column("Fecha_Lectura")]
    public DateTime? Fecha_Lectura { get; set; }
    
    [Column("Error_Mensaje")]
    public string? Error_Mensaje { get; set; }
    
    [Column("Fecha_Creacion")]
    public DateTime Fecha_Creacion { get; set; }
}
