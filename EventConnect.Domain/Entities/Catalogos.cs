using System.ComponentModel.DataAnnotations.Schema;

namespace EventConnect.Domain.Entities;

[Table("catalogo_estado_reserva")]
public class CatalogoEstadoReserva
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string? Color { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
    public bool Sistema { get; set; } // Estados del sistema no se pueden eliminar
    public DateTime Fecha_Creacion { get; set; }
}

[Table("catalogo_estado_activo")]
public class CatalogoEstadoActivo
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string? Color { get; set; }
    public bool Permite_Reserva { get; set; } = true;
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
    public bool Sistema { get; set; }
    public DateTime Fecha_Creacion { get; set; }
}

[Table("catalogo_metodo_pago")]
public class CatalogoMetodoPago
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public bool Requiere_Comprobante { get; set; }
    public bool Requiere_Referencia { get; set; }
    public bool Activo { get; set; } = true;
    public int Orden { get; set; }
    public DateTime Fecha_Creacion { get; set; }
}

[Table("catalogo_tipo_mantenimiento")]
public class CatalogoTipoMantenimiento
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public bool Es_Preventivo { get; set; }
    public bool Activo { get; set; } = true;
    public int Orden { get; set; }
    public DateTime Fecha_Creacion { get; set; }
}
