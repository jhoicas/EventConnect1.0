using EventConnect.Application.Services;
using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[Authorize]
public class DetalleReservaController : BaseController
{
    private readonly DetalleReservaRepository _repository;
    private readonly IDetalleReservaValidacionService _validacionService;
    private readonly ILogger<DetalleReservaController> _logger;

    public DetalleReservaController(
        IConfiguration configuration,
        IDetalleReservaValidacionService validacionService,
        ILogger<DetalleReservaController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection")
            ?? throw new InvalidOperationException("Connection string not found");
        _repository = new DetalleReservaRepository(connectionString);
        _validacionService = validacionService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los detalles de una reserva
    /// </summary>
    [HttpGet("reserva/{reservaId}")]
    public async Task<IActionResult> GetByReservaId(int reservaId)
    {
        try
        {
            var detalles = await _repository.GetByReservaIdAsync(reservaId);
            return Ok(detalles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles de reserva {ReservaId}", reservaId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene detalles con información completa (producto y activo)
    /// </summary>
    [HttpGet("reserva/{reservaId}/completo")]
    public async Task<IActionResult> GetDetallesCompletos(int reservaId)
    {
        try
        {
            var detalles = await _repository.GetDetallesConInfoCompletaAsync(reservaId);
            return Ok(detalles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles completos de reserva {ReservaId}", reservaId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea un nuevo detalle de reserva con validación de integridad
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DetalleReserva detalle)
    {
        try
        {
            // Validar y normalizar el detalle
            var (esValido, mensaje, detalleNormalizado) = await _validacionService
                .ValidarYNormalizarDetalleAsync(detalle);

            if (!esValido)
            {
                _logger.LogWarning("Validación fallida: {Mensaje}", mensaje);
                return BadRequest(new { message = mensaje });
            }

            // Guardar el detalle normalizado
            detalleNormalizado.Fecha_Creacion = DateTime.Now;
            var id = await _repository.AddAsync(detalleNormalizado);
            detalleNormalizado.Id = id;

            _logger.LogInformation(
                "Detalle creado: ID={Id}, Reserva={ReservaId}, Producto={ProductoId}, Activo={ActivoId}",
                id, detalleNormalizado.Reserva_Id, detalleNormalizado.Producto_Id, detalleNormalizado.Activo_Id);

            return CreatedAtAction(nameof(GetById), new { id }, detalleNormalizado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear detalle de reserva");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza un detalle de reserva con validación de integridad
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] DetalleReserva detalle)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Detalle de reserva no encontrado" });

            // Validar y normalizar
            var (esValido, mensaje, detalleNormalizado) = await _validacionService
                .ValidarYNormalizarDetalleAsync(detalle);

            if (!esValido)
            {
                _logger.LogWarning("Validación fallida en actualización: {Mensaje}", mensaje);
                return BadRequest(new { message = mensaje });
            }

            detalleNormalizado.Id = id;
            detalleNormalizado.Reserva_Id = existing.Reserva_Id; // Mantener la reserva original
            
            var success = await _repository.UpdateAsync(detalleNormalizado);
            if (!success)
                return BadRequest(new { message = "No se pudo actualizar el detalle" });

            _logger.LogInformation("Detalle actualizado: ID={Id}", id);
            return Ok(detalleNormalizado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar detalle {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un detalle por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var detalle = await _repository.GetByIdAsync(id);
            if (detalle == null)
                return NotFound(new { message = "Detalle no encontrado" });

            return Ok(detalle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina un detalle de reserva
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _repository.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "Detalle no encontrado" });

            _logger.LogInformation("Detalle eliminado: ID={Id}", id);
            return Ok(new { message = "Detalle eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar detalle {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Valida la integridad Producto-Activo sin guardar
    /// </summary>
    [HttpPost("validar")]
    public async Task<IActionResult> ValidarIntegridad([FromBody] ValidacionRequest request)
    {
        try
        {
            var (esValido, mensaje, productoIdReal) = await _validacionService
                .ValidarProductoActivoAsync(request.ProductoId, request.ActivoId);

            return Ok(new
            {
                esValido,
                mensaje,
                productoIdReal,
                autoCompletado = request.ActivoId.HasValue && !request.ProductoId.HasValue && productoIdReal.HasValue
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar integridad");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene activos disponibles de un producto
    /// </summary>
    [HttpGet("producto/{productoId}/activos-disponibles")]
    public async Task<IActionResult> GetActivosDisponibles(int productoId)
    {
        try
        {
            var activos = await _validacionService.ObtenerActivosDisponiblesAsync(productoId);
            var count = await _validacionService.ContarActivosDisponiblesAsync(productoId);

            return Ok(new
            {
                productoId,
                cantidadDisponible = count,
                activos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener activos disponibles del producto {ProductoId}", productoId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// [ADMIN] Obtiene todos los detalles con problemas de integridad
    /// </summary>
    [HttpGet("integridad/problemas")]
    [Authorize(Roles = "SuperAdmin,Admin-Proveedor")]
    public async Task<IActionResult> GetProblemasIntegridad()
    {
        try
        {
            var problemas = await _repository.GetDetallesConProblemasIntegridadAsync();
            return Ok(problemas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener problemas de integridad");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// [ADMIN] Corrige automáticamente problemas de integridad
    /// </summary>
    [HttpPost("integridad/corregir")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CorregirIntegridad()
    {
        try
        {
            var corregidos = await _repository.CorregirIntegridadAsync();
            
            _logger.LogWarning(
                "Corrección masiva de integridad ejecutada por usuario {UserId}. Registros corregidos: {Count}",
                GetCurrentUserId(), corregidos);

            return Ok(new
            {
                message = "Corrección completada",
                registrosCorregidos = corregidos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al corregir integridad");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

public class ValidacionRequest
{
    public int? ProductoId { get; set; }
    public int? ActivoId { get; set; }
}
