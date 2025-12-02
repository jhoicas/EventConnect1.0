using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Depreciacion")]
public class Depreciacion
{
    public int Id { get; set; }
    
    [Column("Activo_Id")]
    public int Activo_Id { get; set; }
    
    public int Periodo { get; set; } // Mes desde adquisición
    
    [Column("Fecha_Periodo")]
    public DateTime Fecha_Periodo { get; set; }
    
    [Column("Valor_Inicial")]
    public decimal Valor_Inicial { get; set; }
    
    [Column("Depreciacion_Mensual")]
    public decimal Depreciacion_Mensual { get; set; }
    
    [Column("Depreciacion_Acumulada")]
    public decimal Depreciacion_Acumulada { get; set; }
    
    [Column("Valor_Neto")]
    public decimal Valor_Neto { get; set; }
    
    [Column("Fecha_Calculo")]
    public DateTime Fecha_Calculo { get; set; }
}
