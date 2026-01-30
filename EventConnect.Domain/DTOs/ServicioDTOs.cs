namespace EventConnect.Domain.DTOs;

public class CreateServicioRequest
{
    public string? Titulo { get; set; }
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public string? Imagen_Url { get; set; }
    public int? Orden { get; set; }
    public bool? Activo { get; set; }
}

public class UpdateServicioRequest
{
    public string? Titulo { get; set; }
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public string? Imagen_Url { get; set; }
    public int? Orden { get; set; }
    public bool? Activo { get; set; }
}
