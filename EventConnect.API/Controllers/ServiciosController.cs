using EventConnect.Domain.DTOs;
using EventConnect.Domain.Entities;
using EventConnect.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[ApiController]
[Route("api/servicios")]
public class ServiciosController : BaseController
{
    private readonly IServicioRepository _servicioRepository;
    private readonly ILogger<ServiciosController> _logger;

    public ServiciosController(IServicioRepository servicioRepository, ILogger<ServiciosController> logger)
    {
        _servicioRepository = servicioRepository;
        _logger = logger;
    }

    /// <summary>
    /// Lista servicios activos (público)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublic([FromQuery] bool? activo = null)
    {
        try
        {
            var activoValue = activo ?? true;
            var servicios = await _servicioRepository.GetByActivoAsync(activoValue);
            return Ok(servicios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener servicios públicos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Lista todos los servicios (admin)
    /// </summary>
    [HttpGet("admin")]
    [Authorize]
    public async Task<IActionResult> GetAdmin()
    {
        try
        {
            if (!IsAdminProveedor())
            {
                return StatusCode(403, new { message = "Solo administradores pueden acceder a este recurso" });
            }

            var servicios = await _servicioRepository.GetAllAsync();
            return Ok(servicios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener servicios (admin)");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crear servicio (admin)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateServicioRequest request)
    {
        try
        {
            if (!IsAdminProveedor())
            {
                return StatusCode(403, new { message = "Solo administradores pueden acceder a este recurso" });
            }

            var errors = ValidateCreateRequest(request);
            if (errors.Count > 0)
            {
                return BadRequest(new { error = "Validación fallida", message = "Errores en los datos enviados", errors });
            }

            var servicio = new Servicio
            {
                Titulo = request.Titulo!.Trim(),
                Descripcion = request.Descripcion!.Trim(),
                Icono = string.IsNullOrWhiteSpace(request.Icono) ? null : request.Icono.Trim(),
                Imagen_Url = request.Imagen_Url!.Trim(),
                Orden = request.Orden ?? 0,
                Activo = request.Activo ?? true
            };

            var id = await _servicioRepository.CreateAsync(servicio);
            servicio.Id_Servicio = id;

            return StatusCode(201, servicio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear servicio");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualizar servicio (admin)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServicioRequest request)
    {
        try
        {
            if (!IsAdminProveedor())
            {
                return StatusCode(403, new { message = "Solo administradores pueden acceder a este recurso" });
            }

            var existing = await _servicioRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Servicio no encontrado" });
            }

            var errors = ValidateUpdateRequest(request);
            if (errors.Count > 0)
            {
                return BadRequest(new { error = "Validación fallida", message = "Errores en los datos enviados", errors });
            }

            existing.Titulo = request.Titulo?.Trim() ?? existing.Titulo;
            existing.Descripcion = request.Descripcion?.Trim() ?? existing.Descripcion;
            existing.Icono = request.Icono != null ? request.Icono.Trim() : existing.Icono;
            existing.Imagen_Url = request.Imagen_Url?.Trim() ?? existing.Imagen_Url;
            existing.Orden = request.Orden ?? existing.Orden;
            existing.Activo = request.Activo ?? existing.Activo;

            var updated = await _servicioRepository.UpdateAsync(existing);
            if (!updated)
            {
                return NotFound(new { message = "Servicio no encontrado" });
            }

            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar servicio {ServicioId}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Eliminar servicio (soft delete) (admin)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            if (!IsAdminProveedor())
            {
                return StatusCode(403, new { message = "Solo administradores pueden acceder a este recurso" });
            }

            var success = await _servicioRepository.SoftDeleteAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Servicio no encontrado" });
            }

            return Ok(new { message = "Servicio desactivado exitosamente", id_Servicio = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar servicio {ServicioId}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    private static Dictionary<string, string> ValidateCreateRequest(CreateServicioRequest request)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(request.Titulo) || request.Titulo.Trim().Length < 3)
        {
            errors["titulo"] = "El título debe tener al menos 3 caracteres";
        }
        else if (request.Titulo.Trim().Length > 100)
        {
            errors["titulo"] = "El título no puede exceder 100 caracteres";
        }

        if (string.IsNullOrWhiteSpace(request.Descripcion) || request.Descripcion.Trim().Length < 10)
        {
            errors["descripcion"] = "La descripción debe tener al menos 10 caracteres";
        }

        if (string.IsNullOrWhiteSpace(request.Imagen_Url) || !IsValidUrl(request.Imagen_Url))
        {
            errors["imagen_Url"] = "Debe ser una URL válida";
        }

        if (request.Icono != null && request.Icono.Trim().Length > 50)
        {
            errors["icono"] = "El icono no puede exceder 50 caracteres";
        }

        if (request.Orden.HasValue && request.Orden.Value < 0)
        {
            errors["orden"] = "El orden debe ser un número entero >= 0";
        }

        return errors;
    }

    private static Dictionary<string, string> ValidateUpdateRequest(UpdateServicioRequest request)
    {
        var errors = new Dictionary<string, string>();

        if (request.Titulo != null)
        {
            if (request.Titulo.Trim().Length < 3)
            {
                errors["titulo"] = "El título debe tener al menos 3 caracteres";
            }
            else if (request.Titulo.Trim().Length > 100)
            {
                errors["titulo"] = "El título no puede exceder 100 caracteres";
            }
        }

        if (request.Descripcion != null && request.Descripcion.Trim().Length < 10)
        {
            errors["descripcion"] = "La descripción debe tener al menos 10 caracteres";
        }

        if (request.Imagen_Url != null && !IsValidUrl(request.Imagen_Url))
        {
            errors["imagen_Url"] = "Debe ser una URL válida";
        }

        if (request.Icono != null && request.Icono.Trim().Length > 50)
        {
            errors["icono"] = "El icono no puede exceder 50 caracteres";
        }

        if (request.Orden.HasValue && request.Orden.Value < 0)
        {
            errors["orden"] = "El orden debe ser un número entero >= 0";
        }

        return errors;
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
