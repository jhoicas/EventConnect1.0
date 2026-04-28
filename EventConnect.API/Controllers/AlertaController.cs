using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventConnect.Domain.DTOs;
using EventConnect.Domain.Services;
using System.Security.Claims;

namespace EventConnect.API.Controllers;

/// <summary>
/// Controller para la gestión de alertas de mantenimiento y depreciación
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlertaController : BaseController
{
    private readonly IAlertaService _alertaService;

    public AlertaController(IAlertaService alertaService)
    {
        _alertaService = alertaService;
    }

    /// <summary>
    /// Obtiene alertas con filtros opcionales y paginación
    /// </summary>
    /// <param name="filtro">Criterios de filtrado</param>
    /// <returns>Lista paginada de alertas</returns>
    [HttpPost("filtrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedAlertaResponse>> ObtenerAlertasFiltradas([FromBody] FiltrarAlertasRequest filtro)
    {
        try
        {
            var alertas = await _alertaService.ObtenerAlertasAsync(filtro);
            return Ok(alertas);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene detalles de una alerta específica
    /// </summary>
    /// <param name="id">ID de la alerta</param>
    /// <returns>Detalles completos de la alerta</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertaDetalladoResponse>> ObtenerAlertaPorId(int id)
    {
        try
        {
            var alerta = await _alertaService.ObtenerAlertaPorIdAsync(id);
            if (alerta == null)
                return NotFound(new { error = "Alerta no encontrada" });

            return Ok(alerta);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todas las alertas activas de un activo específico
    /// </summary>
    /// <param name="activoId">ID del activo</param>
    /// <returns>Resumen de alertas del activo</returns>
    [HttpGet("activo/{activoId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumenAlertasPorActivoResponse>> ObtenerAlertasPorActivo(int activoId)
    {
        try
        {
            var resumen = await _alertaService.ObtenerAlertasPorActivoAsync(activoId);
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene alertas críticas (severidad crítica o vencidas)
    /// </summary>
    /// <param name="limit">Límite de resultados (máximo 50)</param>
    /// <returns>Lista de alertas críticas</returns>
    [HttpGet("criticas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AlertaResponse>>> ObtenerAlertasCriticas([FromQuery] int limit = 20)
    {
        try
        {
            if (limit > 50) limit = 50;
            var alertas = await _alertaService.ObtenerAlertasCriticasAsync(limit);
            return Ok(alertas);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene alertas próximas a vencer (24-48 horas)
    /// </summary>
    /// <returns>Lista de alertas próximas a vencer</returns>
    [HttpGet("proximas-vencer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AlertaResponse>>> ObtenerAlertasProximasAVencer()
    {
        try
        {
            var alertas = await _alertaService.ObtenerAlertasProximasAVencerAsync();
            return Ok(alertas);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza el estado de una alerta
    /// </summary>
    /// <param name="id">ID de la alerta</param>
    /// <param name="request">Datos de actualización</param>
    /// <returns>Alerta actualizada</returns>
    [HttpPut("{id}/actualizar-estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AlertaDetalladoResponse>> ActualizarEstadoAlerta(int id, [FromBody] ActualizarAlertaRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var alerta = await _alertaService.ActualizarEstadoAlertaAsync(id, request);
            return Ok(alerta);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Marca una alerta como resuelta
    /// </summary>
    /// <param name="id">ID de la alerta</param>
    /// <param name="observaciones">Observaciones sobre la resolución</param>
    /// <returns>Alerta resuelta</returns>
    [HttpPut("{id}/resolver")]
    [Authorize(Roles = "Admin,Operario")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AlertaDetalladoResponse>> ResolverAlerta(int id, [FromQuery] string? observaciones)
    {
        try
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var alerta = await _alertaService.ResolverAlertaAsync(id, observaciones ?? "Resuelto", usuarioId);
            return Ok(alerta);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Asigna una alerta a un usuario específico
    /// </summary>
    /// <param name="id">ID de la alerta</param>
    /// <param name="usuarioId">ID del usuario asignado</param>
    /// <returns>Alerta asignada</returns>
    [HttpPut("{id}/asignar/{usuarioId}")]
    [Authorize(Roles = "Admin,Supervisor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AlertaDetalladoResponse>> AsignarAlerta(int id, int usuarioId)
    {
        try
        {
            var alerta = await _alertaService.AsignarAlertaAsync(id, usuarioId);
            return Ok(alerta);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Genera alertas automáticas para todos los activos (tarea programada)
    /// </summary>
    /// <returns>Resultado de la generación</returns>
    [HttpPost("generar-automaticas")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoGeneracionAlertasResponse>> GenerarAlertasAutomaticas()
    {
        try
        {
            var resultado = await _alertaService.GenerarAlertasAutomaticasAsync();
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene estadísticas generales del sistema de alertas
    /// </summary>
    /// <returns>Estadísticas de alertas</returns>
    [HttpGet("estadisticas/resumen")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumenAlertasResponse>> ObtenerEstadisticas()
    {
        try
        {
            var stats = await _alertaService.ObtenerEstadisticasAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Limpia alertas resueltas más antiguas que X días
    /// </summary>
    /// <param name="diasAntiguos">Días a considerar como antiguos (default 90)</param>
    /// <returns>Cantidad de registros eliminados</returns>
    [HttpDelete("limpiar")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> LimpiarAlertasAntiguas([FromQuery] int diasAntiguos = 90)
    {
        if (diasAntiguos < 30)
            return BadRequest(new { error = "Mínimo 30 días de antigüedad" });

        try
        {
            var eliminados = await _alertaService.LimpiarAlertasAntiguasAsync(diasAntiguos);
            return Ok(new { registros_eliminados = eliminados, dias_antiguos = diasAntiguos });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
