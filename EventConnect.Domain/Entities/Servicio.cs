using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Servicios")]
public class Servicio
{
    [Column("Id_Servicio")]
    public int Id_Servicio { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string? Icono { get; set; }

    [Column("Imagen_Url")]
    public string Imagen_Url { get; set; } = string.Empty;

    public int Orden { get; set; } = 0;

    public bool Activo { get; set; } = true;

    [Column("Fecha_Creacion")]
    public DateTime Fecha_Creacion { get; set; }

    [Column("Fecha_Actualizacion")]
    public DateTime Fecha_Actualizacion { get; set; }
}
