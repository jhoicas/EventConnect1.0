using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Usuario")]
public class Usuario
{
    public int Id { get; set; }
    public int? Empresa_Id { get; set; }
    public int Rol_Id { get; set; }
    
    [Column("Usuario")]
    public string Usuario1 { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    public string Password_Hash { get; set; } = string.Empty;
    public string Nombre_Completo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Avatar_URL { get; set; }
    public string Estado { get; set; } = "Activo";
    public int Intentos_Fallidos { get; set; }
    public DateTime? Ultimo_Acceso { get; set; }
    public DateTime Fecha_Creacion { get; set; }
    public DateTime Fecha_Actualizacion { get; set; }
    public bool Requiere_Cambio_Password { get; set; }
    public bool TwoFA_Activo { get; set; }
    public string? TwoFA_Secret { get; set; }
}
