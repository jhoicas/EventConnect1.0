using EventConnect.Domain.DTOs;
using EventConnect.Domain.Entities;
using EventConnect.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[ApiController]
[Route("api/solicitudes-cotizacion")]
[Authorize]
public class SolicitudCotizacionController : BaseController
{
    private readonly ISolicitudCotizacionRepository _repository;
    private readonly ILogger<SolicitudCotizacionController> _logger;

    public SolicitudCotizacionController(
        ISolicitudCotizacionRepository repository, 
        ILogger<SolicitudCotizacionController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Obtener cotizaciones del usuario autenticado
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyCotizaciones()
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            IEnumerable<Cotizacion> cotizaciones;

            // Cliente ve solo sus cotizaciones
            if (userRole == "Cliente")
            {
                cotizaciones = await _repository.GetByClienteIdAsync(userId);
            }
            // SuperAdmin ve todas
            else if (IsSuperAdmin())
            {
                cotizaciones = await _repository.GetAllAsync();
            }
            // Otros roles (Admin-Proveedor) no pueden acceder
            else
            {
                return StatusCode(403, new { message = "No autorizado para ver cotizaciones" });
            }

            var dtos = cotizaciones.Select(c => MapToDDTO(c)).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotizaciones");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener detalle de una cotización específica
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var cotizacion = await _repository.GetByIdAsync(id);
            if (cotizacion == null)
            {
                return NotFound(new { message = "Cotización no encontrada" });
            }

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            // Validar permiso: cliente solo ve sus propias cotizaciones
            if (userRole == "Cliente" && cotizacion.Cliente_Id != userId)
            {
                return StatusCode(403, new { message = "No tienes acceso a esta cotización" });
            }

            var dto = MapToDDTO(cotizacion);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotización {CotizacionId}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crear nueva solicitud de cotización
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSolicitudCotizacionRequest request)
    {
        try
        {
            var userRole = GetCurrentUserRole();
            if (userRole != "Cliente")
            {
                return StatusCode(403, new { message = "Solo clientes pueden solicitar cotizaciones" });
            }

            var errors = ValidateCreateRequest(request);
            if (errors.Count > 0)
            {
                return BadRequest(new { error = "Validación fallida", errors });
            }

            var userId = GetCurrentUserId();
            var cotizacion = new Cotizacion
            {
                Cliente_Id = userId, // Asignación automática desde usuario autenticado
                Producto_Id = request.Producto_Id!.Value,
                Cantidad_Solicitada = request.Cantidad_Solicitada!.Value,
                Observaciones = string.IsNullOrWhiteSpace(request.Observaciones) ? null : request.Observaciones.Trim(),
                Estado = "Solicitada",
                Monto_Cotizacion = 0 // Se actualiza cuando se responde
            };

            var id = await _repository.CreateAsync(cotizacion);
            cotizacion.Id = id;

            _logger.LogInformation("Cotización {CotizacionId} creada por Cliente {ClienteId} para Producto {ProductoId}",
                id, userId, request.Producto_Id);

            return StatusCode(201, MapToDDTO(cotizacion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cotización");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualizar cotización (responder con presupuesto)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSolicitudCotizacionRequest request)
    {
        try
        {
            var userRole = GetCurrentUserRole();
            if (userRole != "Admin-Proveedor" && !IsSuperAdmin())
            {
                return StatusCode(403, new { message = "Solo proveedores pueden actualizar cotizaciones" });
            }

            var cotizacion = await _repository.GetByIdAsync(id);
            if (cotizacion == null)
            {
                return NotFound(new { message = "Cotización no encontrada" });
            }

            var errors = ValidateUpdateRequest(request);
            if (errors.Count > 0)
            {
                return BadRequest(new { error = "Validación fallida", errors });
            }

            if (request.Monto_Cotizacion.HasValue)
                cotizacion.Monto_Cotizacion = request.Monto_Cotizacion.Value;

            if (!string.IsNullOrWhiteSpace(request.Estado))
                cotizacion.Estado = request.Estado.Trim();

            if (!string.IsNullOrWhiteSpace(request.Observaciones))
                cotizacion.Observaciones = request.Observaciones.Trim();

            if (request.Fecha_Respuesta.HasValue)
                cotizacion.Fecha_Respuesta = request.Fecha_Respuesta.Value;
            else if (cotizacion.Estado == "Respondida" && !cotizacion.Fecha_Respuesta.HasValue)
                cotizacion.Fecha_Respuesta = DateTime.Now;

            var success = await _repository.UpdateAsync(cotizacion);
            if (!success)
            {
                return NotFound(new { message = "Cotización no encontrada" });
            }

            _logger.LogInformation("Cotización {CotizacionId} actualizada a estado {Estado}", id, cotizacion.Estado);

            return Ok(MapToDDTO(cotizacion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cotización {CotizacionId}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Eliminar/cancelar cotización
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var cotizacion = await _repository.GetByIdAsync(id);
            if (cotizacion == null)
            {
                return NotFound(new { message = "Cotización no encontrada" });
            }

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            // Solo cliente propietario puede cancelar
            if (userRole == "Cliente" && cotizacion.Cliente_Id != userId)
            {
                return StatusCode(403, new { message = "No puedes cancelar una cotización que no es tuya" });
            }

            var success = await _repository.DeleteAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Cotización no encontrada" });
            }

            _logger.LogInformation("Cotización {CotizacionId} cancelada por Usuario {UserId}", id, userId);

            return Ok(new { message = "Cotización cancelada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cotización {CotizacionId}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    private static SolicitudCotizacionDTO MapToDDTO(Cotizacion cotizacion)
    {
        return new SolicitudCotizacionDTO
        {
            Id = cotizacion.Id,
            Cliente_Id = cotizacion.Cliente_Id,
            Producto_Id = cotizacion.Producto_Id,
            Fecha_Solicitud = cotizacion.Fecha_Solicitud,
            Cantidad_Solicitada = cotizacion.Cantidad_Solicitada,
            Monto_Cotizacion = cotizacion.Monto_Cotizacion,
            Estado = cotizacion.Estado,
            Observaciones = cotizacion.Observaciones,
            Fecha_Respuesta = cotizacion.Fecha_Respuesta,
            Fecha_Creacion = cotizacion.Fecha_Creacion,
            Fecha_Actualizacion = cotizacion.Fecha_Actualizacion
        };
    }

    private static Dictionary<string, string> ValidateCreateRequest(CreateSolicitudCotizacionRequest request)
    {
        var errors = new Dictionary<string, string>();

        if (!request.Producto_Id.HasValue || request.Producto_Id <= 0)
        {
            errors["producto_Id"] = "Producto es requerido";
        }

        if (!request.Cantidad_Solicitada.HasValue || request.Cantidad_Solicitada <= 0)
        {
            errors["cantidad_Solicitada"] = "Cantidad debe ser mayor a 0";
        }

        return errors;
    }

    private static Dictionary<string, string> ValidateUpdateRequest(UpdateSolicitudCotizacionRequest request)
    {
        var errors = new Dictionary<string, string>();

        if (request.Monto_Cotizacion.HasValue && request.Monto_Cotizacion < 0)
        {
            errors["monto_Cotizacion"] = "Monto no puede ser negativo";
        }

        var estadosValidos = new[] { "Solicitada", "Respondida", "Aceptada", "Rechazada" };
        if (!string.IsNullOrWhiteSpace(request.Estado) && !estadosValidos.Contains(request.Estado))
        {
            errors["estado"] = "Estado no válido";
        }

        return errors;
    }
}
