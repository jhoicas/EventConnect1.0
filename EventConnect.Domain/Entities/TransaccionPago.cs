namespace EventConnect.Domain.Entities;

public class TransaccionPago
{
    public int Id { get; set; }
    public int Reserva_Id { get; set; }
    public decimal Monto { get; set; }
    public string Tipo { get; set; } = null!; // Pago, Devolucion_Fianza, Reembolso
    public string Metodo { get; set; } = null!; // Tarjeta, Transferencia, Efectivo
    public string? Referencia_Externa { get; set; } // ID de transacción de Stripe/PayU/Banco
    public string? Comprobante_URL { get; set; }
    public DateTime Fecha_Transaccion { get; set; }
    public int Registrado_Por_Usuario_Id { get; set; }
}
