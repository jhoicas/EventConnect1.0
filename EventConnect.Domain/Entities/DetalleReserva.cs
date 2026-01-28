using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Detalle_Reserva")]
public class DetalleReserva
{
    public int Id { get; set; }
    public int Reserva_Id { get; set; }
    public int Empresa_Id { get; set; } // Ahora se define aquí para soporte multivendedor
    public int? Producto_Id { get; set; }
    public int? Activo_Id { get; set; }
    public int Cantidad { get; set; } = 1;
    public decimal Precio_Unitario { get; set; }
    public decimal Subtotal { get; set; }
    public int Dias_Alquiler { get; set; } = 1;
    public string? Observaciones { get; set; }
    public string Estado_Item { get; set; } = "OK";
    public DateTime Fecha_Creacion { get; set; }
}
