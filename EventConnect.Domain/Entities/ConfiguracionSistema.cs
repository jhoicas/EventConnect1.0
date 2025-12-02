using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Configuracion_Sistema")]
public class ConfiguracionSistema
{
    public int Id { get; set; }
    
    [Column("Empresa_Id")]
    public int? Empresa_Id { get; set; }
    
    public string Clave { get; set; } = string.Empty;
    
    public string? Valor { get; set; }
    
    public string? Descripcion { get; set; }
    
    [Column("Tipo_Dato")]
    public string Tipo_Dato { get; set; } = "string"; // string, int, bool, json
    
    [Column("Es_Global")]
    public bool Es_Global { get; set; } = false;
    
    [Column("Fecha_Actualizacion")]
    public DateTime Fecha_Actualizacion { get; set; }
}
