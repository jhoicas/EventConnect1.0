using EventConnect.Domain.DTOs;
using EventConnect.Domain.Entities;
using EventConnect.Domain.Services;
using EventConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

/// <summary>
/// Controlador para gestión de logística y evidencias de entregas
/// </summary>
[Authorize]
[ApiController]
[Route("api/logistica")]
public class LogisticaController : BaseController
{
    private readonly EvidenciaEntregaRepository _evidenciaRepository;
    private readonly ReservaRepository _reservaRepository;
    private readonly DetalleReservaRepository _detalleReservaRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<LogisticaController> _logger;
    private const string EVIDENCIAS_FOLDER = "evidencias";

    public LogisticaController(
        IConfiguration configuration,
        IFileStorageService fileStorageService,
        ILogger<LogisticaController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _evidenciaRepository = new EvidenciaEntregaRepository(connectionString);
        _reservaRepository = new ReservaRepository(connectionString);
        _detalleReservaRepository = new DetalleReservaRepository(connectionString);
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Sube una evidencia (imagen) de entrega, devolución o daño
    /// </summary>
    /// <param name="archivo">Archivo de imagen</param>
    /// <param name="request">Datos de la evidencia</param>
    /// <returns>Evidencia creada</returns>
    /// <response code="200">Evidencia creada exitosamente</response>
    /// <response code="400">Datos inválidos o archivo no válido</response>
    /// <response code="404">Reserva no encontrada</response>
    /// <response code="403">No autorizado para esta reserva</response>
    [HttpPost("evidencia")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB
    public async Task<IActionResult> SubirEvidencia(
        [FromForm] IFormFile archivo,
        [FromForm] CrearEvidenciaRequest request)
    {
        try
        {
            // Validar archivo
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest(new { message = "El archivo es requerido" });
            }

            // Validar tamaño
            if (archivo.Length > _fileStorageService.MaxFileSize)
            {
                return BadRequest(new { 
                    message = $"El archivo excede el tamaño máximo permitido ({_fileStorageService.MaxFileSize / 1024 / 1024} MB)" 
                });
            }

            // Validar tipo de imagen
            if (!_fileStorageService.IsValidImage(archivo.FileName, archivo.ContentType))
            {
                return BadRequest(new { message = "El archivo debe ser una imagen válida (JPG, PNG, GIF, WEBP)" });
            }

            // Validar tipo de evidencia
            if (!new[] { "Entrega", "Devolucion", "Dano" }.Contains(request.Tipo))
            {
                return BadRequest(new { message = "Tipo de evidencia inválido. Debe ser: Entrega, Devolucion o Dano" });
            }

            // Validar que la reserva existe y el usuario tiene acceso
            var reserva = await _reservaRepository.GetByIdAsync(request.ReservaId);
            if (reserva == null)
            {
                return NotFound(new { message = "Reserva no encontrada" });
            }

            // MultiVendedor: Obtener empresa(s) de los detalles
            var detalles = (await _detalleReservaRepository.GetByReservaIdAsync(request.ReservaId)).ToList();
            if (!detalles.Any())
            {
                return BadRequest(new { message = "La reserva no tiene detalles" });
            }

            // Validar que el usuario (empresa) está en los detalles si no es SuperAdmin
            if (!IsSuperAdmin())
            {
                var empresaId = GetCurrentEmpresaId();
                if (empresaId.HasValue && !detalles.Any(d => d.Empresa_Id == empresaId.Value))
                {
                    return Forbid();
                }
            }

            // Guardar archivo
            string urlImagen;
            using (var stream = archivo.OpenReadStream())
            {
                urlImagen = await _fileStorageService.SaveFileAsync(
                    stream, 
                    archivo.FileName, 
                    EVIDENCIAS_FOLDER);
            }

            // MultiVendedor: Obtener empresa de los detalles (usar la primera como referencia)
            var detallesEvidencia = detalles.FirstOrDefault();
            if (detallesEvidencia == null)
            {
                return BadRequest(new { message = "La reserva no tiene detalles de empresa" });
            }

            // Crear evidencia en BD
            var evidencia = new EvidenciaEntrega
            {
                Reserva_Id = request.ReservaId,
                Empresa_Id = detallesEvidencia.Empresa_Id, // Usar empresa del primer detalle
                Usuario_Id = GetCurrentUserId(),
                Tipo = request.Tipo,
                Url_Imagen = urlImagen,
                Comentario = request.Comentario,
                Latitud = request.Latitud,
                Longitud = request.Longitud,
                Nombre_Recibe = request.NombreRecibe,
                Fecha_Creacion = DateTime.Now
            };

            var evidenciaId = await _evidenciaRepository.AddAsync(evidencia);
            evidencia.Id = evidenciaId;

            _logger.LogInformation(
                "Evidencia {EvidenciaId} creada para reserva {ReservaId} por usuario {UserId}",
                evidenciaId, request.ReservaId, GetCurrentUserId());

            // Retornar respuesta con detalles
            var response = new EvidenciaResponse
            {
                Id = evidencia.Id,
                ReservaId = evidencia.Reserva_Id,
                EmpresaId = evidencia.Empresa_Id,
                UsuarioId = evidencia.Usuario_Id,
                Tipo = evidencia.Tipo,
                UrlImagen = evidencia.Url_Imagen,
                Comentario = evidencia.Comentario,
                Latitud = evidencia.Latitud,
                Longitud = evidencia.Longitud,
                NombreRecibe = evidencia.Nombre_Recibe,
                UrlFirma = evidencia.Url_Firma,
                FechaCreacion = evidencia.Fecha_Creacion
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir evidencia para reserva {ReservaId}", request?.ReservaId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene todas las evidencias de una reserva
    /// </summary>
    [HttpGet("evidencias/reserva/{reservaId}")]
    public async Task<IActionResult> GetEvidenciasPorReserva(int reservaId)
    {
        try
        {
            // Validar que la reserva existe y el usuario tiene acceso
            var reserva = await _reservaRepository.GetByIdAsync(reservaId);
            if (reserva == null)
            {
                return NotFound(new { message = "Reserva no encontrada" });
            }

            // MultiVendedor: Obtener empresa(s) de los detalles
            var detalles = (await _detalleReservaRepository.GetByReservaIdAsync(reservaId)).ToList();
            if (!detalles.Any() && !IsSuperAdmin())
            {
                return NotFound(new { message = "La reserva no tiene detalles" });
            }

            // Validar que el usuario (empresa) está en los detalles si no es SuperAdmin
            if (!IsSuperAdmin())
            {
                var empresaId = GetCurrentEmpresaId();
                if (empresaId.HasValue && !detalles.Any(d => d.Empresa_Id == empresaId.Value))
                {
                    return Forbid();
                }
            }

            var empresaIdTemp = GetCurrentEmpresaId();
            if (empresaIdTemp == null && !IsSuperAdmin())
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            // MultiVendedor: Para GetWithDetailsAsync, usar la primera empresa en los detalles o SuperAdmin
            var empresaIdFiltro = IsSuperAdmin() ? (detalles.FirstOrDefault()?.Empresa_Id ?? 0) : empresaIdTemp!.Value;
            var evidencias = await _evidenciaRepository.GetWithDetailsAsync(
                empresaIdFiltro, 
                reservaId);

            return Ok(evidencias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener evidencias de reserva {ReservaId}", reservaId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene todas las evidencias de la empresa del usuario actual
    /// </summary>
    [HttpGet("evidencias")]
    public async Task<IActionResult> GetEvidencias()
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null && !IsSuperAdmin())
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            if (empresaId == null)
            {
                // SuperAdmin puede ver todas (pasar null)
                var todasEvidencias = await _evidenciaRepository.GetAllAsync(null);
                return Ok(todasEvidencias);
            }

            var evidencias = await _evidenciaRepository.GetWithDetailsAsync(empresaId.Value);
            return Ok(evidencias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener evidencias");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Completa la entrega de una reserva (valida que haya evidencias y cambia estado)
    /// </summary>
    /// <param name="reservaId">ID de la reserva</param>
    /// <param name="request">Comentarios adicionales</param>
    /// <returns>Reserva actualizada</returns>
    /// <response code="200">Entrega completada exitosamente</response>
    /// <response code="400">No hay evidencias de entrega o reserva no está en estado válido</response>
    /// <response code="404">Reserva no encontrada</response>
    /// <response code="403">No autorizado para esta reserva</response>
    [HttpPost("completar-entrega/{reservaId}")]
    public async Task<IActionResult> CompletarEntrega(
        int reservaId,
        [FromBody] CompletarEntregaRequest? request = null)
    {
        try
        {
            // Validar que la reserva existe
            var reserva = await _reservaRepository.GetByIdAsync(reservaId);
            if (reserva == null)
            {
                return NotFound(new { message = "Reserva no encontrada" });
            }

            // MultiVendedor: Obtener empresa(s) de los detalles
            var detalles = (await _detalleReservaRepository.GetByReservaIdAsync(reservaId)).ToList();
            if (!detalles.Any() && !IsSuperAdmin())
            {
                return NotFound(new { message = "La reserva no tiene detalles" });
            }

            // Validar que el usuario (empresa) está en los detalles si no es SuperAdmin
            if (!IsSuperAdmin())
            {
                var empresaId = GetCurrentEmpresaId();
                if (empresaId.HasValue && !detalles.Any(d => d.Empresa_Id == empresaId.Value))
                {
                    return Forbid();
                }
            }

            // Validar estado de la reserva (debe estar Confirmado o En_Alistamiento)
            var estadosValidos = new[] { "Confirmado", "En_Alistamiento", "En_Transito_Entrega" };
            if (!estadosValidos.Contains(reserva.Estado))
            {
                return BadRequest(new { 
                    message = $"La reserva debe estar en estado 'Confirmado', 'En_Alistamiento' o 'En_Transito_Entrega'. Estado actual: {reserva.Estado}" 
                });
            }

            // Validar que hay evidencias de entrega
            var tieneEvidenciaEntrega = await _evidenciaRepository.HasEvidenciaTipoAsync(reservaId, "Entrega");
            if (!tieneEvidenciaEntrega)
            {
                return BadRequest(new { 
                    message = "No se puede completar la entrega sin evidencia. Por favor, suba al menos una imagen de evidencia de entrega." 
                });
            }

            // Actualizar estado de la reserva
            reserva.Estado = "En_Cliente"; // Cambiar a "En Cliente" después de entregar
            reserva.Fecha_Entrega = DateTime.Now;
            reserva.Fecha_Actualizacion = DateTime.Now;

            // Agregar comentarios si se proporcionan
            if (!string.IsNullOrWhiteSpace(request?.Comentarios))
            {
                reserva.Observaciones = string.IsNullOrWhiteSpace(reserva.Observaciones)
                    ? $"[Entrega completada] {request.Comentarios}"
                    : $"{reserva.Observaciones}\n\n[Entrega completada] {request.Comentarios}";
            }
            else if (string.IsNullOrWhiteSpace(reserva.Observaciones))
            {
                reserva.Observaciones = "[Entrega completada]";
            }

            var success = await _reservaRepository.UpdateAsync(reserva);
            if (!success)
            {
                return StatusCode(500, new { message = "No se pudo actualizar la reserva" });
            }

            _logger.LogInformation(
                "Entrega completada para reserva {ReservaId} por usuario {UserId}",
                reservaId, GetCurrentUserId());

            return Ok(new { 
                message = "Entrega completada exitosamente",
                reserva = reserva 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al completar entrega de reserva {ReservaId}", reservaId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina una evidencia
    /// </summary>
    [HttpDelete("evidencia/{id}")]
    public async Task<IActionResult> EliminarEvidencia(int id)
    {
        try
        {
            var evidencia = await _evidenciaRepository.GetByIdAsync(id);
            if (evidencia == null)
            {
                return NotFound(new { message = "Evidencia no encontrada" });
            }

            // Validar multi-tenant
            if (!IsSuperAdmin() && evidencia.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            // Eliminar archivo físico
            await _fileStorageService.DeleteFileAsync(evidencia.Url_Imagen);
            if (!string.IsNullOrWhiteSpace(evidencia.Url_Firma))
            {
                await _fileStorageService.DeleteFileAsync(evidencia.Url_Firma);
            }

            // Eliminar registro de BD
            var success = await _evidenciaRepository.DeleteAsync(id);
            if (!success)
            {
                return StatusCode(500, new { message = "No se pudo eliminar la evidencia" });
            }

            _logger.LogInformation("Evidencia {EvidenciaId} eliminada por usuario {UserId}", id, GetCurrentUserId());

            return Ok(new { message = "Evidencia eliminada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar evidencia {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
