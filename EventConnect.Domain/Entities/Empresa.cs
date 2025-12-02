using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Empresa")]
public class Empresa
{
    public int Id { get; set; }
    public string Razon_Social { get; set; } = string.Empty;
    public string NIT { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string Pais { get; set; } = "Colombia";
    public string? Logo_URL { get; set; }
    public string Estado { get; set; } = "Activa";
    public DateTime Fecha_Registro { get; set; }
    public DateTime Fecha_Actualizacion { get; set; }
}
