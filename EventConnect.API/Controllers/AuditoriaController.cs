using EventConnect.Domain.DTOs;
using EventConnect.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditoriaController : BaseController
{
    private readonly IAuditoriaService _auditoriaService;
    private readonly ILogger<AuditoriaController> _logger;

    public AuditoriaController(IAuditoriaService auditoriaService, ILogger<AuditoriaController> logger)
    {
        _auditoriaService = auditoriaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el historial completo de un registro con timeline visual
    /// </summary>
    /// <param name="tablaAfectada">Nombre de la tabla (ej: Reserva, Activo, Usuario)</param>
    /// <param name="registroId">ID del registro</param>
    [HttpGet("historial/{tablaAfectada}/{registroId}")]
    public async Task<IActionResult> ObtenerHistorial(string tablaAfectada, int registroId)
    {
        try
        {
            var historial = await _auditoriaService.ObtenerHistorialAsync(tablaAfectada, registroId);
            
            if (historial == null)
            {
                return NotFound(new { message = "No hay historial disponible para este registro" });
            }

            _logger.LogInformation("Historial consultado para {Tabla} ID: {Id}", tablaAfectada, registroId);
            return Ok(new { message = "Historial obtenido exitosamente", data = historial });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo historial de {Tabla} ID: {Id}", tablaAfectada, registroId);
            return StatusCode(500, new { message = "Error al obtener historial", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene un resumen de cambios en un registro
    /// </summary>
    [HttpGet("resumen/{tablaAfectada}/{registroId}")]
    public async Task<IActionResult> ObtenerResumen(string tablaAfectada, int registroId)
    {
        try
        {
            var resumen = await _auditoriaService.ObtenerResumenAsync(tablaAfectada, registroId);
            
            if (resumen == null)
            {
                return NotFound(new { message = "No hay cambios registrados para este registro" });
            }

            _logger.LogInformation("Resumen consultado para {Tabla} ID: {Id}", tablaAfectada, registroId);
            return Ok(new { message = "Resumen obtenido exitosamente", data = resumen });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo resumen de {Tabla} ID: {Id}", tablaAfectada, registroId);
            return StatusCode(500, new { message = "Error al obtener resumen", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene historial de auditoría con filtros avanzados
    /// </summary>
    [HttpPost("filtrado")]
    public async Task<IActionResult> ObtenerHistorialFiltrado([FromBody] FiltroAuditoriaRequest filtro)
    {
        try
        {
            if (filtro.Pagina < 1)
                filtro.Pagina = 1;

            if (filtro.Registros_Por_Pagina < 1 || filtro.Registros_Por_Pagina > 500)
                filtro.Registros_Por_Pagina = 50;

            var resultado = await _auditoriaService.ObtenerHistorialFiltradoAsync(filtro);
            
            _logger.LogInformation("Historial filtrado consultado - Página: {Pagina}, Total: {Total}", 
                filtro.Pagina, resultado.Total_Registros);
            
            return Ok(new 
            { 
                message = "Historial obtenido exitosamente",
                data = resultado
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo historial filtrado");
            return StatusCode(500, new { message = "Error al obtener historial filtrado", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene los cambios más recientes en la plataforma
    /// </summary>
    /// <param name="top">Número de registros a devolver (máximo 100)</param>
    [HttpGet("recientes")]
    public async Task<IActionResult> ObtenerCambiosRecientes([FromQuery] int top = 50)
    {
        try
        {
            if (top < 1 || top > 100)
                top = 50;

            var cambios = await _auditoriaService.ObtenerCambiosRecientesAsync(top);
            
            _logger.LogInformation("Cambios recientes consultados - {Count} registros", cambios.Count);
            return Ok(new 
            { 
                message = "Cambios obtenidos exitosamente",
                data = cambios
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo cambios recientes");
            return StatusCode(500, new { message = "Error al obtener cambios recientes", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todos los cambios realizados por un usuario específico
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="top">Número de registros a devolver (máximo 100)</param>
    [HttpGet("usuario/{usuarioId}")]
    public async Task<IActionResult> ObtenerCambiosPorUsuario(int usuarioId, [FromQuery] int top = 50)
    {
        try
        {
            if (top < 1 || top > 100)
                top = 50;

            var cambios = await _auditoriaService.ObtenerCambiosPorUsuarioAsync(usuarioId, top);
            
            _logger.LogInformation("Cambios del usuario {UserId} consultados - {Count} registros", usuarioId, cambios.Count);
            return Ok(new 
            { 
                message = "Cambios obtenidos exitosamente",
                data = cambios
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo cambios del usuario {UserId}", usuarioId);
            return StatusCode(500, new { message = "Error al obtener cambios del usuario", error = ex.Message });
        }
    }

    /// <summary>
    /// Busca en el historial de auditoría
    /// </summary>
    /// <param name="termino">Término a buscar</param>
    /// <param name="tablaAfectada">Tabla específica (opcional)</param>
    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar([FromQuery] string termino, [FromQuery] string? tablaAfectada = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(termino) || termino.Length < 2)
            {
                return BadRequest(new { message = "El término de búsqueda debe tener al menos 2 caracteres" });
            }

            var resultados = await _auditoriaService.BuscarAsync(termino, tablaAfectada);
            
            _logger.LogInformation("Búsqueda en auditoría - Término: {Termino}, Resultados: {Count}", 
                termino, resultados.Count);
            
            return Ok(new 
            { 
                message = $"Se encontraron {resultados.Count} coincidencias",
                data = resultados
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en búsqueda de auditoría - Término: {Termino}", termino);
            return StatusCode(500, new { message = "Error al buscar en auditoría", error = ex.Message });
        }
    }

    /// <summary>
    /// Limpia registros de auditoría antiguos (requiere rol SuperAdmin)
    /// </summary>
    /// <param name="diasAntiguos">Días de antigüedad para considerar como antiguo (default: 90)</param>
    [HttpPost("limpiar")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> LimpiarAuditoriaAntigua([FromQuery] int diasAntiguos = 90)
    {
        try
        {
            if (diasAntiguos < 30)
            {
                return BadRequest(new { message = "No se pueden eliminar registros de menos de 30 días de antigüedad" });
            }

            var registrosEliminados = await _auditoriaService.LimpiarAuditoriaAntiguaAsync(diasAntiguos);
            
            _logger.LogWarning("Limpieza de auditoría realizada por {Usuario} - {Count} registros eliminados", 
                GetCurrentUsername(), registrosEliminados);
            
            return Ok(new 
            { 
                message = $"Se eliminaron {registrosEliminados} registros de auditoría",
                registros_eliminados = registrosEliminados
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error limpiando auditoría antigua");
            return StatusCode(500, new { message = "Error al limpiar auditoría", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene estadísticas de auditoría por tabla
    /// </summary>
    [HttpGet("estadisticas")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ObtenerEstadisticas()
    {
        try
        {
            var filtro = new FiltroAuditoriaRequest
            {
                Pagina = 1,
                Registros_Por_Pagina = 1
            };

            var resultado = await _auditoriaService.ObtenerHistorialFiltradoAsync(filtro);
            
            return Ok(new 
            { 
                message = "Estadísticas obtenidas exitosamente",
                total_registros_auditoria = resultado.Total_Registros,
                ultima_actualizacion = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo estadísticas de auditoría");
            return StatusCode(500, new { message = "Error al obtener estadísticas", error = ex.Message });
        }
    }
}
