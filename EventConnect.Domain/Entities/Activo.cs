using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Activo")]
public class Activo
{
    public int Id { get; set; }
    
    [Column("Empresa_Id")]
    public int Empresa_Id { get; set; }
    
    [Column("Producto_Id")]
    public int Producto_Id { get; set; }
    
    [Column("Bodega_Id")]
    public int Bodega_Id { get; set; }
    
    [Column("Codigo_Activo")]
    public string Codigo_Activo { get; set; } = string.Empty;
    
    [Column("Numero_Serie")]
    public string? Numero_Serie { get; set; }
    
    [Column("Estado_Fisico")]
    public string Estado_Fisico { get; set; } = "Nuevo"; // Nuevo, Excelente, Bueno, Regular, Malo
    
    [Column("Estado_Disponibilidad")]
    public string Estado_Disponibilidad { get; set; } = "Disponible"; // Disponible, Alquilado, En_Mantenimiento, Dado_de_Baja
    
    [Column("Fecha_Compra")]
    public DateTime? Fecha_Compra { get; set; }
    
    [Column("Costo_Compra")]
    public decimal? Costo_Compra { get; set; }
    
    public string? Proveedor { get; set; }
    
    [Column("Vida_Util_Anos")]
    public int? Vida_Util_Anos { get; set; }
    
    [Column("Valor_Residual")]
    public decimal? Valor_Residual { get; set; }
    
    [Column("Depreciacion_Acumulada")]
    public decimal? Depreciacion_Acumulada { get; set; }
    
    [Column("Foto_Registro_URL")]
    public string? Foto_Registro_URL { get; set; }
    
    [Column("QR_Code_URL")]
    public string? QR_Code_URL { get; set; }
    
    [Column("Codigo_QR")]
    public string? CodigoQR { get; set; }
    
    [Column("Vida_Util_Meses")]
    public int? VidaUtilMeses { get; set; }
    
    [Column("Costo_Adquisicion")]
    public decimal? CostoAdquisicion { get; set; }
    
    [Column("Total_Alquileres")]
    public int Total_Alquileres { get; set; }
    
    [Column("Ingresos_Totales")]
    public decimal Ingresos_Totales { get; set; }
    
    [Column("Costos_Mantenimiento")]
    public decimal Costos_Mantenimiento { get; set; }
    
    [Column("Ultimo_Mantenimiento")]
    public DateTime? Ultimo_Mantenimiento { get; set; }
    
    [Column("Proximo_Mantenimiento")]
    public DateTime? Proximo_Mantenimiento { get; set; }
    
    public string? Observaciones { get; set; }
    
    [Column("Activo")]
    public bool Esta_Activo { get; set; } = true;
    
    [Column("Fecha_Creacion")]
    public DateTime Fecha_Creacion { get; set; }
    
    [Column("Fecha_Actualizacion")]
    public DateTime Fecha_Actualizacion { get; set; }
}
