using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Reserva")]
public class Reserva
{
    public int Id { get; set; }
    public int Empresa_Id { get; set; }
    public int Cliente_Id { get; set; }
    public string Codigo_Reserva { get; set; } = string.Empty;
    public string Estado { get; set; } = "Solicitado";
    public DateTime Fecha_Evento { get; set; }
    public DateTime? Fecha_Entrega { get; set; }
    public DateTime? Fecha_Devolucion_Programada { get; set; }
    public DateTime? Fecha_Devolucion_Real { get; set; }
    public string? Direccion_Entrega { get; set; }
    public string? Ciudad_Entrega { get; set; }
    public string? Contacto_En_Sitio { get; set; }
    public string? Telefono_Contacto { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public decimal Fianza { get; set; }
    public bool Fianza_Devuelta { get; set; }
    public string Metodo_Pago { get; set; } = "Efectivo";
    public string Estado_Pago { get; set; } = "Pendiente";
    public string? Observaciones { get; set; }
    public int Creado_Por_Id { get; set; }
    public int? Aprobado_Por_Id { get; set; }
    public DateTime? Fecha_Aprobacion { get; set; }
    public DateTime Fecha_Creacion { get; set; }
    public DateTime? Fecha_Vencimiento_Cotizacion { get; set; }
    public DateTime Fecha_Actualizacion { get; set; }
    public int? Cancelado_Por_Id { get; set; }
    public string? Razon_Cancelacion { get; set; }
}
