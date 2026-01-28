namespace EventConnect.Domain.DTOs;

public class ConversacionDTO
{
    public int Id { get; set; }
    public int Empresa_Id { get; set; }
    public int? Cliente_Id { get; set; }
    public int? Usuario_Id { get; set; }
    public string? Asunto { get; set; }
    public int? Reserva_Id { get; set; }
    public DateTime Fecha_Creacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public MensajeDTO? Ultimo_Mensaje { get; set; }
    public int Mensajes_No_Leidos { get; set; }
    
    // Campos para mostrar información de la contraparte
    public string? Nombre_Contraparte { get; set; }
    public string? Avatar_Contraparte { get; set; }
    public string? Email_Contraparte { get; set; }
}

public class MensajeDTO
{
    public long Id { get; set; }
    public int Conversacion_Id { get; set; }
    public int Emisor_Usuario_Id { get; set; }
    public string Emisor_Nombre { get; set; } = string.Empty;
    public string? Emisor_Avatar { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public bool Leido { get; set; }
    public DateTime Fecha_Envio { get; set; }
}

public class CreateConversacionRequest
{
    public int? Empresa_Id { get; set; } // Para cuando un Cliente crea la conversación
    public string? Asunto { get; set; }
    public int? Reserva_Id { get; set; }
    public string? Mensaje_Inicial { get; set; }
}

public class SendMensajeRequest
{
    public int Conversacion_Id { get; set; }
    public string Contenido { get; set; } = string.Empty;
}

public class MarcarLeidoRequest
{
    public int Conversacion_Id { get; set; }
}
