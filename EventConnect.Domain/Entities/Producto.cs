using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Producto")]
public class Producto
{
    public int Id { get; set; }
    public int Empresa_Id { get; set; }
    public int Categoria_Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Unidad_Medida { get; set; } = "Unidad";
    public decimal Precio_Alquiler_Dia { get; set; }
    public int Cantidad_Stock { get; set; }
    public int Stock_Minimo { get; set; } = 10;
    public string? Imagen_URL { get; set; }
    public bool Es_Alquilable { get; set; } = true;
    public bool Es_Vendible { get; set; } = false;
    public bool Requiere_Mantenimiento { get; set; } = false;
    public int Dias_Mantenimiento { get; set; } = 90;
    public decimal? Peso_Kg { get; set; }
    public string? Dimensiones { get; set; }
    public string? Observaciones { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime Fecha_Creacion { get; set; }
    public DateTime Fecha_Actualizacion { get; set; }
}
