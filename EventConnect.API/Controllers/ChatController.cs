using EventConnect.Domain.Entities;
using EventConnect.Domain.DTOs;
using EventConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[Authorize]
public class ChatController : BaseController
{
    private readonly ConversacionRepository _conversacionRepository;
    private readonly MensajeRepository _mensajeRepository;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IConfiguration configuration, ILogger<ChatController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _conversacionRepository = new ConversacionRepository(connectionString);
        _mensajeRepository = new MensajeRepository(connectionString);
        _logger = logger;
    }

    [HttpGet("conversaciones")]
    public async Task<IActionResult> GetConversaciones()
    {
        try
        {
            if (IsSuperAdmin())
            {
                var todas = await _conversacionRepository.GetAllAsync();
                return Ok(todas);
            }

            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null)
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            var conversaciones = await _conversacionRepository.GetByEmpresaIdAsync(empresaId.Value);
            
            // Obtener último mensaje de cada conversación
            var conversacionesConMensajes = new List<ConversacionDTO>();
            foreach (var conv in conversaciones)
            {
                var ultimo = await _mensajeRepository.GetUltimoMensajeAsync(conv.Id);
                conv.Ultimo_Mensaje = ultimo;
                conversacionesConMensajes.Add(conv);
            }

            return Ok(conversacionesConMensajes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener conversaciones");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("conversaciones/{id}")]
    public async Task<IActionResult> GetConversacion(int id)
    {
        try
        {
            var conversacion = await _conversacionRepository.GetByIdAsync(id);
            if (conversacion == null)
                return NotFound(new { message = "Conversación no encontrada" });

            if (!IsSuperAdmin() && conversacion.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            return Ok(conversacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener conversación {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost("conversaciones")]
    public async Task<IActionResult> CreateConversacion([FromBody] CreateConversacionRequest request)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null && !IsSuperAdmin())
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            var conversacion = new Conversacion
            {
                Empresa_Id = empresaId ?? 1, // SuperAdmin usa empresa 1 por defecto
                Asunto = request.Asunto,
                Reserva_Id = request.Reserva_Id,
                Fecha_Creacion = DateTime.Now,
                Estado = "Abierta"
            };

            var conversacionId = await _conversacionRepository.AddAsync(conversacion);

            // Si hay mensaje inicial, crearlo
            if (!string.IsNullOrWhiteSpace(request.Mensaje_Inicial))
            {
                var mensaje = new Mensaje
                {
                    Conversacion_Id = conversacionId,
                    Emisor_Usuario_Id = GetCurrentUserId(),
                    Contenido = request.Mensaje_Inicial,
                    Leido = false,
                    Fecha_Envio = DateTime.Now
                };

                await _mensajeRepository.AddAsync(mensaje);
            }

            return Ok(new { id = conversacionId, message = "Conversación creada correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear conversación");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("mensajes/{conversacionId}")]
    public async Task<IActionResult> GetMensajes(int conversacionId)
    {
        try
        {
            var conversacion = await _conversacionRepository.GetByIdAsync(conversacionId);
            if (conversacion == null)
                return NotFound(new { message = "Conversación no encontrada" });

            if (!IsSuperAdmin() && conversacion.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            var mensajes = await _mensajeRepository.GetByConversacionIdAsync(conversacionId);
            return Ok(mensajes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mensajes de conversación {Id}", conversacionId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost("mensajes")]
    public async Task<IActionResult> SendMensaje([FromBody] SendMensajeRequest request)
    {
        try
        {
            var conversacion = await _conversacionRepository.GetByIdAsync(request.Conversacion_Id);
            if (conversacion == null)
                return NotFound(new { message = "Conversación no encontrada" });

            if (!IsSuperAdmin() && conversacion.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            var mensaje = new Mensaje
            {
                Conversacion_Id = request.Conversacion_Id,
                Emisor_Usuario_Id = GetCurrentUserId(),
                Contenido = request.Contenido,
                Leido = false,
                Fecha_Envio = DateTime.Now
            };

            var mensajeId = await _mensajeRepository.AddAsync(mensaje);

            return Ok(new { id = mensajeId, message = "Mensaje enviado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar mensaje");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost("mensajes/marcar-leidos")]
    public async Task<IActionResult> MarcarLeidos([FromBody] MarcarLeidoRequest request)
    {
        try
        {
            var conversacion = await _conversacionRepository.GetByIdAsync(request.Conversacion_Id);
            if (conversacion == null)
                return NotFound(new { message = "Conversación no encontrada" });

            if (!IsSuperAdmin() && conversacion.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            var usuarioId = GetCurrentUserId();
            await _mensajeRepository.MarcarLeidosAsync(request.Conversacion_Id, usuarioId);

            return Ok(new { message = "Mensajes marcados como leídos" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar mensajes como leídos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("conversaciones/{id}/cerrar")]
    public async Task<IActionResult> CerrarConversacion(int id)
    {
        try
        {
            var conversacion = await _conversacionRepository.GetByIdAsync(id);
            if (conversacion == null)
                return NotFound(new { message = "Conversación no encontrada" });

            if (!IsSuperAdmin() && conversacion.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            conversacion.Estado = "Cerrada";
            await _conversacionRepository.UpdateAsync(conversacion);

            return Ok(new { message = "Conversación cerrada correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cerrar conversación {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
