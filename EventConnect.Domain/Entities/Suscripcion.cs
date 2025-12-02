using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Suscripcion")]
public class Suscripcion
{
    public int Id { get; set; }
    public int Empresa_Id { get; set; }
    public int Plan_Id { get; set; }
    public string Modulo { get; set; } = string.Empty;
    public string Estado { get; set; } = "Prueba";
    public DateTime Fecha_Inicio { get; set; }
    public DateTime? Fecha_Fin_Prueba { get; set; }
    public DateTime? Fecha_Vencimiento { get; set; }
    public bool Auto_Renovar { get; set; } = true;
    public decimal Costo_Mensual { get; set; }
    public DateTime? Fecha_Ultimo_Pago { get; set; }
    public string? Metodo_Pago { get; set; }
    public DateTime Fecha_Creacion { get; set; }
    public DateTime? Fecha_Cancelacion { get; set; }
    public string? Razon_Cancelacion { get; set; }
}
