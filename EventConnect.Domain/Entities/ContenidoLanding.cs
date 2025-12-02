using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("Contenido_Landing")]
public class ContenidoLanding
{
    public int Id { get; set; }
    
    public string Seccion { get; set; } = string.Empty; // hero, servicios, nosotros, contacto
    
    public string Titulo { get; set; } = string.Empty;
    
    public string? Subtitulo { get; set; }
    
    public string? Descripcion { get; set; }
    
    [Column("Imagen_URL")]
    public string? Imagen_URL { get; set; }
    
    [Column("Icono_Nombre")]
    public string? Icono_Nombre { get; set; }
    
    public int Orden { get; set; } = 0;
    
    public bool Activo { get; set; } = true;
    
    [Column("Fecha_Actualizacion")]
    public DateTime Fecha_Actualizacion { get; set; }
}
