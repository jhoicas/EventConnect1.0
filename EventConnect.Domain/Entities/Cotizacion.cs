using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Cotizaciones")]
public class Cotizacion
{
    public int Id { get; set; }

    [Column("Cliente_Id")]
    public int Cliente_Id { get; set; }

    [Column("Producto_Id")]
    public int Producto_Id { get; set; }

    [Column("Fecha_Solicitud")]
    public DateTime Fecha_Solicitud { get; set; }

    [Column("Cantidad_Solicitada")]
    public int Cantidad_Solicitada { get; set; }

    [Column("Monto_Cotizacion")]
    public decimal Monto_Cotizacion { get; set; }

    public string Estado { get; set; } = "Solicitada"; // Solicitada, Respondida, Aceptada, Rechazada

    public string? Observaciones { get; set; }

    [Column("Fecha_Respuesta")]
    public DateTime? Fecha_Respuesta { get; set; }

    [Column("Fecha_Creacion")]
    public DateTime Fecha_Creacion { get; set; }

    [Column("Fecha_Actualizacion")]
    public DateTime Fecha_Actualizacion { get; set; }
}
