using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Mantenimiento")]
public class Mantenimiento
{
    public int Id { get; set; }
    
    [Column("Activo_Id")]
    public int Activo_Id { get; set; }
    
    [Column("Tipo_Mantenimiento")]
    public string Tipo_Mantenimiento { get; set; } = string.Empty; // Preventivo, Correctivo
    
    [Column("Fecha_Programada")]
    public DateTime? Fecha_Programada { get; set; }
    
    [Column("Fecha_Realizada")]
    public DateTime? Fecha_Realizada { get; set; }
    
    public string? Descripcion { get; set; }
    
    [Column("Responsable_Id")]
    public int? Responsable_Id { get; set; }
    
    [Column("Proveedor_Servicio")]
    public string? Proveedor_Servicio { get; set; }
    
    public decimal? Costo { get; set; }
    
    public string Estado { get; set; } = "Pendiente"; // Pendiente, En Proceso, Completado, Cancelado
    
    public string? Observaciones { get; set; }
    
    [Column("Fecha_Creacion")]
    public DateTime Fecha_Creacion { get; set; }
}
