using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Pago")]
public class Pago
{
    public int Id { get; set; }
    
    [Column("Reserva_Id")]
    public int Reserva_Id { get; set; }
    
    [Column("Cliente_Id")]
    public int Cliente_Id { get; set; }
    
    [Column("Metodo_Pago")]
    public string Metodo_Pago { get; set; } = string.Empty; // Efectivo, Tarjeta, Transferencia
    
    public decimal Monto { get; set; }
    
    [Column("Fecha_Pago")]
    public DateTime Fecha_Pago { get; set; }
    
    [Column("Numero_Transaccion")]
    public string? Numero_Transaccion { get; set; }
    
    public string Estado { get; set; } = "Completado"; // Pendiente, Completado, Rechazado, Reembolsado
    
    [Column("Comprobante_URL")]
    public string? Comprobante_URL { get; set; }
    
    public string? Observaciones { get; set; }
    
    [Column("Registrado_Por_Id")]
    public int Registrado_Por_Id { get; set; }
    
    [Column("Fecha_Creacion")]
    public DateTime Fecha_Creacion { get; set; }
}
