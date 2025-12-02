using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Lote")]
public class Lote
{
    public int Id { get; set; }
    
    [Column("Producto_Id")]
    public int Producto_Id { get; set; }
    
    [Column("Bodega_Id")]
    public int? Bodega_Id { get; set; }
    
    [Column("Codigo_Lote")]
    public string Codigo_Lote { get; set; } = string.Empty;
    
    [Column("Fecha_Fabricacion")]
    public DateTime? Fecha_Fabricacion { get; set; }
    
    [Column("Fecha_Vencimiento")]
    public DateTime? Fecha_Vencimiento { get; set; }
    
    [Column("Cantidad_Inicial")]
    public int Cantidad_Inicial { get; set; }
    
    [Column("Cantidad_Actual")]
    public int Cantidad_Actual { get; set; }
    
    [Column("Costo_Unitario")]
    public decimal Costo_Unitario { get; set; }
    
    public string Estado { get; set; } = "Disponible";
    
    [Column("Fecha_Creacion")]
    public DateTime Fecha_Creacion { get; set; }
}
