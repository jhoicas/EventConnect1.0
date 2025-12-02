using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Conversacion")]
public class Conversacion
{
    public int Id { get; set; }
    public int Empresa_Id { get; set; }
    public int? Cliente_Id { get; set; }
    public int? Usuario_Id { get; set; }
    public string? Asunto { get; set; }
    public int? Reserva_Id { get; set; }
    public DateTime Fecha_Creacion { get; set; }
    public string Estado { get; set; } = "Abierta"; // Abierta, Cerrada, Archivada
}
