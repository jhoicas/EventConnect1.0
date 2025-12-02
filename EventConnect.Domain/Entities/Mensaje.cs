using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Mensaje")]
public class Mensaje
{
    public long Id { get; set; }
    public int Conversacion_Id { get; set; }
    public int Emisor_Usuario_Id { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public bool Leido { get; set; }
    public DateTime Fecha_Envio { get; set; }
}
