namespace EventConnect.Domain.DTOs;

public class CotizacionDTO
{
    public int Id { get; set; }
    public int Empresa_Id { get; set; }
    public int Cliente_Id { get; set; }
    public string Codigo_Reserva { get; set; } = string.Empty;
    public string Estado { get; set; } = "Solicitado";
    public DateTime Fecha_Evento { get; set; }
    public DateTime? Fecha_Entrega { get; set; }
    public DateTime? Fecha_Devolucion_Programada { get; set; }
    public string? Direccion_Entrega { get; set; }
    public string? Ciudad_Entrega { get; set; }
    public string? Contacto_En_Sitio { get; set; }
    public string? Telefono_Contacto { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public decimal Fianza { get; set; }
    public string? Observaciones { get; set; }
    public DateTime Fecha_Creacion { get; set; }
    public DateTime? Fecha_Vencimiento_Cotizacion { get; set; }
    
    // Información adicional del JOIN
    public string? Cliente_Nombre { get; set; }
    public string? Cliente_Email { get; set; }
    public string? Cliente_Telefono { get; set; }
    public string? Creado_Por_Nombre { get; set; }
    public int? Dias_Para_Vencer { get; set; }
    public bool Esta_Vencida { get; set; }
}

public class CreateCotizacionRequest
{
    public int Cliente_Id { get; set; }
    public DateTime Fecha_Evento { get; set; }
    public DateTime? Fecha_Entrega { get; set; }
    public DateTime? Fecha_Devolucion_Programada { get; set; }
    public string? Direccion_Entrega { get; set; }
    public string? Ciudad_Entrega { get; set; }
    public string? Contacto_En_Sitio { get; set; }
    public string? Telefono_Contacto { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public decimal Fianza { get; set; }
    public string? Observaciones { get; set; }
    public int Dias_Validez_Cotizacion { get; set; } = 7; // Por defecto 7 días
}

public class UpdateCotizacionRequest
{
    public DateTime Fecha_Evento { get; set; }
    public DateTime? Fecha_Entrega { get; set; }
    public DateTime? Fecha_Devolucion_Programada { get; set; }
    public string? Direccion_Entrega { get; set; }
    public string? Ciudad_Entrega { get; set; }
    public string? Contacto_En_Sitio { get; set; }
    public string? Telefono_Contacto { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public decimal Fianza { get; set; }
    public string? Observaciones { get; set; }
}

public class ConvertirCotizacionRequest
{
    public int Cotizacion_Id { get; set; }
    public string Metodo_Pago { get; set; } = "Efectivo";
    public string? Observaciones_Adicionales { get; set; }
}

public class ExtenderVencimientoCotizacionRequest
{
    public int Cotizacion_Id { get; set; }
    public int Dias_Extension { get; set; }
}

public class EstadisticasCotizacionesDTO
{
    public int Total_Cotizaciones { get; set; }
    public int Cotizaciones_Vigentes { get; set; }
    public int Cotizaciones_Vencidas { get; set; }
    public int Cotizaciones_Convertidas { get; set; }
    public decimal Tasa_Conversion { get; set; }
    public decimal Valor_Total_Cotizado { get; set; }
    public decimal Valor_Total_Convertido { get; set; }
}
