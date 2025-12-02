using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Log_Auditoria")]
public class LogAuditoria
{
    public int Id { get; set; }
    
    [Column("Empresa_Id")]
    public int? Empresa_Id { get; set; }
    
    [Column("Usuario_Id")]
    public int? Usuario_Id { get; set; }
    
    [Column("Nombre_Usuario")]
    public string? Nombre_Usuario { get; set; }
    
    [Column("Tipo_Operacion")]
    public string Tipo_Operacion { get; set; } = string.Empty; // INSERT, UPDATE, DELETE, LOGIN
    
    [Column("Tabla_Afectada")]
    public string? Tabla_Afectada { get; set; }
    
    [Column("Registro_Id")]
    public int? Registro_Id { get; set; }
    
    [Column("Valores_Anteriores")]
    public string? Valores_Anteriores { get; set; } // JSON
    
    [Column("Valores_Nuevos")]
    public string? Valores_Nuevos { get; set; } // JSON
    
    [Column("IP_Address")]
    public string? IP_Address { get; set; }
    
    [Column("User_Agent")]
    public string? User_Agent { get; set; }
    
    [Column("Hash_Integridad")]
    public string? Hash_Integridad { get; set; } // SHA-256
    
    [Column("Fecha_Operacion")]
    public DateTime Fecha_Operacion { get; set; }
}
