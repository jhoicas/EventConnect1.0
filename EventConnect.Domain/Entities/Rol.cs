using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Rol")]
public class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Nivel_Acceso { get; set; }
    public string? Permisos { get; set; }
    public DateTime Fecha_Creacion { get; set; }
}
