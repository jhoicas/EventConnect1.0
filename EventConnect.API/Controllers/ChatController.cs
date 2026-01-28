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
    private readonly ClienteRepository _clienteRepository;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IConfiguration configuration, ILogger<ChatController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _conversacionRepository = new ConversacionRepository(connectionString);
        _mensajeRepository = new MensajeRepository(connectionString);
        _clienteRepository = new ClienteRepository(connectionString);
        _logger = logger;
    }

    /// <summary>
    /// Obtiene conversaciones según el rol del usuario autenticado
    /// - Cliente: Ve sus conversaciones con empresas (proveedores)
    /// - Empresa (Admin-Proveedor): Ve conversaciones con sus clientes
    /// - SuperAdmin: Ve todas las conversaciones
    /// </summary>
    [HttpGet("conversaciones")]
    public async Task<IActionResult> GetConversaciones()
    {
        try
        {
            var userRole = GetCurrentUserRole();
            var userId = GetCurrentUserId();

            // SuperAdmin ve todas las conversaciones
            if (IsSuperAdmin())
            {
                var todas = await _conversacionRepository.GetAllAsync();
                
                // Obtener último mensaje de cada conversación
                var conversacionesConMensajes = new List<ConversacionDTO>();
                foreach (var conv in todas)
                {
                    var ultimo = await _mensajeRepository.GetUltimoMensajeAsync(conv.Id);
                    conv.Ultimo_Mensaje = ultimo;
                    conversacionesConMensajes.Add(conv);
                }
                
                return Ok(conversacionesConMensajes);
            }

            // Cliente: Ve sus conversaciones con empresas
            if (userRole == "Cliente")
            {
                var cliente = await _clienteRepository.GetByUsuarioIdAsync(userId);
                if (cliente == null)
                {
                    _logger.LogWarning("Usuario {UserId} con rol Cliente no tiene registro de cliente", userId);
                    return Ok(new List<ConversacionDTO>()); // Retornar lista vacía
                }

                var conversaciones = await _conversacionRepository.GetConversacionesByClienteIdAsync(cliente.Id);
                
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

            // Empresa (Admin-Proveedor): Ve conversaciones con clientes
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null)
            {
                _logger.LogWarning("Usuario {UserId} sin empresa asignada", userId);
                return Ok(new List<ConversacionDTO>()); // Retornar lista vacía
            }

            var conversacionesEmpresa = await _conversacionRepository.GetConversacionesByEmpresaIdAsync(empresaId.Value);
            
            // Obtener último mensaje de cada conversación
            var conversacionesEmpresaConMensajes = new List<ConversacionDTO>();
            foreach (var conv in conversacionesEmpresa)
            {
                var ultimo = await _mensajeRepository.GetUltimoMensajeAsync(conv.Id);
                conv.Ultimo_Mensaje = ultimo;
                conversacionesEmpresaConMensajes.Add(conv);
            }

            return Ok(conversacionesEmpresaConMensajes);
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

            var userRole = GetCurrentUserRole();
            var userId = GetCurrentUserId();

            // Verificar permisos según rol
            if (!IsSuperAdmin())
            {
                if (userRole == "Cliente")
                {
                    // Verificar que el cliente pertenece a esta conversación
                    var cliente = await _clienteRepository.GetByUsuarioIdAsync(userId);
                    if (cliente == null || conversacion.Cliente_Id != cliente.Id)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    // Verificar que la empresa pertenece a esta conversación
                    if (conversacion.Empresa_Id != GetCurrentEmpresaId())
                    {
                        return Forbid();
                    }
                }
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
            var userRole = GetCurrentUserRole();
            var userId = GetCurrentUserId();
            int empresaIdFinal;
            int? clienteIdFinal = null;

            if (userRole == "Cliente")
            {
                // Cliente creando conversación con una empresa
                if (request.Empresa_Id == null)
                {
                    return BadRequest(new { message = "Debe especificar la empresa con la que desea iniciar la conversación" });
                }

                var cliente = await _clienteRepository.GetByUsuarioIdAsync(userId);
                if (cliente == null)
                {
                    return BadRequest(new { message = "Usuario cliente no encontrado" });
                }

                empresaIdFinal = request.Empresa_Id.Value;
                clienteIdFinal = cliente.Id;
            }
            else
            {
                // Empresa creando conversación (necesita especificar cliente en request o inferir de reserva)
                var empresaId = GetCurrentEmpresaId();
                if (empresaId == null && !IsSuperAdmin())
                {
                    return BadRequest(new { message = "Empresa no válida" });
                }

                empresaIdFinal = empresaId ?? 1; // SuperAdmin usa empresa 1 por defecto
                // clienteIdFinal se puede obtener de la reserva si existe
            }

            var conversacion = new Conversacion
            {
                Empresa_Id = empresaIdFinal,
                Cliente_Id = clienteIdFinal,
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
                    Emisor_Usuario_Id = userId,
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

            var userRole = GetCurrentUserRole();
            var userId = GetCurrentUserId();

            // Verificar permisos según rol
            if (!IsSuperAdmin())
            {
                if (userRole == "Cliente")
                {
                    var cliente = await _clienteRepository.GetByUsuarioIdAsync(userId);
                    if (cliente == null || conversacion.Cliente_Id != cliente.Id)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    if (conversacion.Empresa_Id != GetCurrentEmpresaId())
                    {
                        return Forbid();
                    }
                }
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

            var userRole = GetCurrentUserRole();
            var userId = GetCurrentUserId();

            // Verificar permisos según rol
            if (!IsSuperAdmin())
            {
                if (userRole == "Cliente")
                {
                    var cliente = await _clienteRepository.GetByUsuarioIdAsync(userId);
                    if (cliente == null || conversacion.Cliente_Id != cliente.Id)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    if (conversacion.Empresa_Id != GetCurrentEmpresaId())
                    {
                        return Forbid();
                    }
                }
            }

            var mensaje = new Mensaje
            {
                Conversacion_Id = request.Conversacion_Id,
                Emisor_Usuario_Id = userId,
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
