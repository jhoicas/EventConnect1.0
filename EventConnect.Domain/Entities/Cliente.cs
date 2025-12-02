using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Cliente")]
public class Cliente
{
    public int Id { get; set; }
    public int Empresa_Id { get; set; }
    public int? Usuario_Id { get; set; }
    public string Tipo_Cliente { get; set; } = "Persona";
    public string Nombre { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Tipo_Documento { get; set; } = "CC";
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Contacto_Nombre { get; set; }
    public string? Contacto_Telefono { get; set; }
    public string? Observaciones { get; set; }
    public decimal Rating { get; set; } = 5.00m;
    public int Total_Alquileres { get; set; }
    public int Total_Danos_Reportados { get; set; }
    public string Estado { get; set; } = "Activo";
    public DateTime Fecha_Registro { get; set; }
    public DateTime Fecha_Actualizacion { get; set; }
}
