using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Bodega")]
public class Bodega
{
    public int Id { get; set; }
    
    [Column("Empresa_Id")]
    public int Empresa_Id { get; set; }
    
    [Column("Codigo_Bodega")]
    public string Codigo_Bodega { get; set; } = string.Empty;
    
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Telefono { get; set; }
    
    [Column("Responsable_Id")]
    public int? Responsable_Id { get; set; }
    
    [Column("Capacidad_M3")]
    public decimal? Capacidad_M3 { get; set; }
    
    public string Estado { get; set; } = "Activo";
    
    [Column("Fecha_Creacion")]
    public DateTime Fecha_Creacion { get; set; }
    
    [Column("Fecha_Actualizacion")]
    public DateTime Fecha_Actualizacion { get; set; }
}
