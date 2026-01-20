using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

/// <summary>
/// Entidad Factura preparada para integración con DIAN (Colombia)
/// </summary>
[Table("Factura")]
public class Factura
{
    public int Id { get; set; }
    
    [Column("Empresa_Id")]
    public int Empresa_Id { get; set; }
    
    [Column("Cliente_Id")]
    public int Cliente_Id { get; set; }
    
    [Column("Reserva_Id")]
    public int? Reserva_Id { get; set; }
    
    /// <summary>
    /// Prefijo de facturación (ej: "FE" para Factura Electrónica)
    /// </summary>
    public string Prefijo { get; set; } = "FE";
    
    /// <summary>
    /// Número consecutivo de la factura
    /// </summary>
    public int Consecutivo { get; set; }
    
    /// <summary>
    /// Código Único de Facturación Electrónica (CUFE)
    /// Generado por el software de facturación electrónica
    /// </summary>
    public string? CUFE { get; set; }
    
    [Column("Fecha_Emision")]
    public DateTime Fecha_Emision { get; set; }
    
    [Column("Fecha_Vencimiento")]
    public DateTime? Fecha_Vencimiento { get; set; }
    
    public decimal Subtotal { get; set; }
    
    public decimal Impuestos { get; set; }
    
    public decimal Total { get; set; }
    
    /// <summary>
    /// Estado: Borrador, Emitida, Anulada
    /// </summary>
    public string Estado { get; set; } = "Borrador";
    
    /// <summary>
    /// Snapshot de datos del cliente al momento de la facturación (JSON)
    /// Útil para auditoría cuando el cliente cambia sus datos
    /// </summary>
    [Column("Datos_Cliente_Snapshot")]
    public string? Datos_Cliente_Snapshot { get; set; }
    
    /// <summary>
    /// Observaciones o notas adicionales
    /// </summary>
    public string? Observaciones { get; set; }
    
    /// <summary>
    /// ID del usuario que creó la factura
    /// </summary>
    [Column("Creado_Por_Id")]
    public int Creado_Por_Id { get; set; }
    
    /// <summary>
    /// ID del usuario que anuló la factura (si aplica)
    /// </summary>
    [Column("Anulado_Por_Id")]
    public int? Anulado_Por_Id { get; set; }
    
    /// <summary>
    /// Fecha de anulación (si aplica)
    /// </summary>
    [Column("Fecha_Anulacion")]
    public DateTime? Fecha_Anulacion { get; set; }
    
    /// <summary>
    /// Razón de anulación
    /// </summary>
    [Column("Razon_Anulacion")]
    public string? Razon_Anulacion { get; set; }
    
    [Column("Fecha_Creacion")]
    public DateTime Fecha_Creacion { get; set; }
    
    [Column("Fecha_Actualizacion")]
    public DateTime Fecha_Actualizacion { get; set; }
}
