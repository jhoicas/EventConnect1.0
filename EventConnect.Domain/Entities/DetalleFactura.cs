using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

/// <summary>
/// Detalle de items de una factura
/// </summary>
[Table("Detalle_Factura")]
public class DetalleFactura
{
    public int Id { get; set; }
    
    [Column("Factura_Id")]
    public int Factura_Id { get; set; }
    
    /// <summary>
    /// ID del producto (si aplica, nullable para servicios personalizados)
    /// </summary>
    [Column("Producto_Id")]
    public int? Producto_Id { get; set; }
    
    /// <summary>
    /// Descripción del servicio o producto
    /// </summary>
    public string Servicio { get; set; } = string.Empty;
    
    public int Cantidad { get; set; }
    
    [Column("Precio_Unitario")]
    public decimal Precio_Unitario { get; set; }
    
    public decimal Subtotal { get; set; }
    
    /// <summary>
    /// Tasa de impuesto aplicada (ej: 0.19 para IVA 19%)
    /// </summary>
    [Column("Tasa_Impuesto")]
    public decimal Tasa_Impuesto { get; set; } = 0.19m;
    
    /// <summary>
    /// Valor del impuesto para este item
    /// </summary>
    public decimal Impuesto { get; set; }
    
    /// <summary>
    /// Total del item (Subtotal + Impuesto)
    /// </summary>
    public decimal Total { get; set; }
    
    /// <summary>
    /// Unidad de medida (ej: "Unidad", "Día", "Hora")
    /// </summary>
    [Column("Unidad_Medida")]
    public string Unidad_Medida { get; set; } = "Unidad";
    
    /// <summary>
    /// Observaciones adicionales del item
    /// </summary>
    public string? Observaciones { get; set; }
    
    [Column("Fecha_Creacion")]
    public DateTime Fecha_Creacion { get; set; }
}
