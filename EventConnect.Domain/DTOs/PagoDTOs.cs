namespace EventConnect.Domain.DTOs;

public class TransaccionPagoDTO
{
    public int Id { get; set; }
    public int Reserva_Id { get; set; }
    public decimal Monto { get; set; }
    public string Tipo { get; set; } = null!;
    public string Metodo { get; set; } = null!;
    public string? Referencia_Externa { get; set; }
    public string? Comprobante_URL { get; set; }
    public DateTime Fecha_Transaccion { get; set; }
    public int Registrado_Por_Usuario_Id { get; set; }
    
    // Información adicional del JOIN
    public string? Registrado_Por_Nombre { get; set; }
    public string? Cliente_Nombre { get; set; }
}

public class CreateTransaccionPagoRequest
{
    public int Reserva_Id { get; set; }
    public decimal Monto { get; set; }
    public string Tipo { get; set; } = null!; // Pago, Devolucion_Fianza, Reembolso
    public string Metodo { get; set; } = null!; // Tarjeta, Transferencia, Efectivo, Nequi, Daviplata
    public string? Referencia_Externa { get; set; }
    public string? Comprobante_URL { get; set; }
}

public class ResumenPagosDTO
{
    public int Reserva_Id { get; set; }
    public decimal Total_Reserva { get; set; }
    public decimal Total_Pagado { get; set; }
    public decimal Saldo_Pendiente { get; set; }
    public decimal Porcentaje_Pagado { get; set; }
    public List<TransaccionPagoDTO> Transacciones { get; set; } = new();
}
