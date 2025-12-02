using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Movimiento_Inventario")]
public class MovimientoInventario
{
    public int Id { get; set; }
    
    [Column("Empresa_Id")]
    public int Empresa_Id { get; set; }
    
    [Column("Producto_Id")]
    public int? Producto_Id { get; set; }
    
    [Column("Activo_Id")]
    public int? Activo_Id { get; set; }
    
    [Column("Lote_Id")]
    public int? Lote_Id { get; set; }
    
    [Column("Bodega_Origen_Id")]
    public int? Bodega_Origen_Id { get; set; }
    
    [Column("Bodega_Destino_Id")]
    public int? Bodega_Destino_Id { get; set; }
    
    [Column("Tipo_Movimiento")]
    public string Tipo_Movimiento { get; set; } = string.Empty; // Entrada, Salida, Transferencia, Ajuste
    
    public int Cantidad { get; set; }
    
    [Column("Costo_Unitario")]
    public decimal? Costo_Unitario { get; set; }
    
    public string? Motivo { get; set; }
    
    [Column("Documento_Referencia")]
    public string? Documento_Referencia { get; set; }
    
    [Column("Usuario_Id")]
    public int Usuario_Id { get; set; }
    
    [Column("Fecha_Movimiento")]
    public DateTime Fecha_Movimiento { get; set; }
}
