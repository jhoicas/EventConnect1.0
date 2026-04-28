using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventConnect.API.Helpers;
using EventConnect.Domain.DTOs;
using EventConnect.Domain.Services;
using System.Security.Claims;

namespace EventConnect.API.Controllers;

/// <summary>
/// Controller para la gestión de daños y discrepancias en activos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DanioController : BaseController
{
    private readonly IDanioService _danioService;
    private readonly AuditoriaHelper _auditoriaHelper;

    public DanioController(IDanioService danioService, AuditoriaHelper auditoriaHelper)
    {
        _danioService = danioService;
        _auditoriaHelper = auditoriaHelper;
    }

    /// <summary>
    /// Registra un nuevo daño en el sistema
    /// </summary>
    /// <param name="request">Datos del daño a reportar</param>
    /// <returns>Daño creado con detalles completos</returns>
    [HttpPost("registrar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DanioDetalladoResponse>> RegistrarDanio([FromBody] CrearDanioRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (usuarioId == 0)
                return Unauthorized("Usuario no identificado");

            var daño = await _danioService.RegistrarDanioAsync(request, usuarioId);

            return CreatedAtAction(nameof(ObtenerDanioPorId), new { id = daño.Id }, daño);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene daños con filtros opcionales y paginación
    /// </summary>
    /// <param name="filtro">Criterios de filtrado</param>
    /// <returns>Lista paginada de daños</returns>
    [HttpPost("filtrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedDanioResponse>> ObtenerDaniosFiltrados([FromBody] FiltrarDaniosRequest filtro)
    {
        try
        {
            var danios = await _danioService.ObtenerDaniosAsync(filtro);
            return Ok(danios);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene detalles de un daño específico
    /// </summary>
    /// <param name="id">ID del daño</param>
    /// <returns>Detalles completos del daño</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DanioDetalladoResponse>> ObtenerDanioPorId(int id)
    {
        try
        {
            var daño = await _danioService.ObtenerDanioPorIdAsync(id);
            if (daño == null)
                return NotFound(new { error = "Daño no encontrado" });

            return Ok(daño);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todos los daños de una reserva específica
    /// </summary>
    /// <param name="reservaId">ID de la reserva</param>
    /// <returns>Lista de daños asociados a la reserva</returns>
    [HttpGet("reserva/{reservaId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DanioResponse>>> ObtenerDanioPorReserva(int reservaId)
    {
        try
        {
            var danios = await _danioService.ObtenerDanioPorReservaAsync(reservaId);
            return Ok(danios);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene resumen de daños para un activo
    /// </summary>
    /// <param name="activoId">ID del activo</param>
    /// <returns>Resumen de daños con estadísticas</returns>
    [HttpGet("activo/{activoId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumenDanioActivoResponse>> ObtenerDanioPorActivo(int activoId)
    {
        try
        {
            var resumen = await _danioService.ObtenerDanioPorActivoAsync(activoId);
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza el estado de un daño (evaluación, confirmación, resolución)
    /// </summary>
    /// <param name="id">ID del daño</param>
    /// <param name="request">Datos de actualización</param>
    /// <returns>Daño actualizado</returns>
    [HttpPut("{id}/actualizar-estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DanioDetalladoResponse>> ActualizarEstadoDanio(int id, [FromBody] ActualizarDanioRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var daño = await _danioService.ActualizarEstadoDanioAsync(id, request);
            return Ok(daño);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Marca un daño como resuelto
    /// </summary>
    /// <param name="id">ID del daño</param>
    /// <param name="resolucion">Descripción de la resolución</param>
    /// <param name="montoFinal">Costo final de la reparación</param>
    /// <returns>Daño resuelto</returns>
    [HttpPut("{id}/resolver")]
    [Authorize(Roles = "Admin,Operario")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DanioDetalladoResponse>> ResolverDanio(
        int id, 
        [FromQuery] string resolucion,
        [FromQuery] decimal montoFinal)
    {
        if (string.IsNullOrWhiteSpace(resolucion))
            return BadRequest(new { error = "La resolución es requerida" });

        try
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var daño = await _danioService.ResolverDanioAsync(id, resolucion, montoFinal, usuarioId);
            return Ok(daño);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Rechaza un daño reportado
    /// </summary>
    /// <param name="id">ID del daño</param>
    /// <param name="motivo">Motivo del rechazo</param>
    /// <returns>Daño rechazado</returns>
    [HttpPut("{id}/rechazar")]
    [Authorize(Roles = "Admin,Operario")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DanioDetalladoResponse>> RechazarDanio(
        int id,
        [FromQuery] string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            return BadRequest(new { error = "El motivo es requerido" });

        try
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var daño = await _danioService.RechazarDanioAsync(id, motivo, usuarioId);
            return Ok(daño);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene estadísticas generales del sistema de daños
    /// </summary>
    /// <returns>Estadísticas de daños</returns>
    [HttpGet("estadisticas/resumen")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<EstadisticasDaniosResponse>> ObtenerEstadisticas()
    {
        try
        {
            var stats = await _danioService.ObtenerEstadisticasAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
