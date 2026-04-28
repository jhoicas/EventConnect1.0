using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventConnect.Domain.DTOs;
using EventConnect.Domain.Services;
using System.Security.Claims;

namespace EventConnect.API.Controllers;

/// <summary>
/// Controller para el Portal de Autogestión de Clientes
/// Permite a los clientes gestionar sus reservas, cotizaciones y ver su historial
/// </summary>
[ApiController]
[Route("api/portal-cliente")]
[Authorize(Roles = "Cliente")]
public class ClientePortalController : BaseController
{
    private readonly IClientePortalService _portalService;

    public ClientePortalController(IClientePortalService portalService)
    {
        _portalService = portalService;
    }

    /// <summary>
    /// Obtiene el ID del cliente desde el token JWT
    /// </summary>
    private async Task<int> ObtenerClienteIdAsync()
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // TODO: Obtener cliente_id desde la tabla usuarios o clientes
        // Por ahora, asumimos que existe una relación directa
        return usuarioId; // Simplificación temporal
    }

    /// <summary>
    /// Obtiene todas las reservas del cliente autenticado
    /// </summary>
    /// <param name="estado">Filtro opcional por estado</param>
    /// <returns>Lista de reservas del cliente</returns>
    [HttpGet("mis-reservas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MiReservaResponse>>> ObtenerMisReservas([FromQuery] string? estado = null)
    {
        try
        {
            var clienteId = await ObtenerClienteIdAsync();
            var reservas = await _portalService.ObtenerMisReservasAsync(clienteId, estado);
            return Ok(reservas);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el seguimiento detallado de una reserva específica
    /// </summary>
    /// <param name="reservaId">ID de la reserva</param>
    /// <returns>Información completa de seguimiento</returns>
    [HttpGet("seguimiento/{reservaId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeguimientoReservaResponse>> ObtenerSeguimientoReserva(int reservaId)
    {
        try
        {
            var clienteId = await ObtenerClienteIdAsync();
            var seguimiento = await _portalService.ObtenerSeguimientoReservaAsync(reservaId, clienteId);
            
            if (seguimiento == null)
                return NotFound(new { error = "Reserva no encontrada o no pertenece al cliente" });

            return Ok(seguimiento);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Crea una nueva reserva desde el portal del cliente
    /// </summary>
    /// <param name="request">Datos de la reserva a crear</param>
    /// <returns>Información de seguimiento de la reserva creada</returns>
    [HttpPost("crear-reserva")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SeguimientoReservaResponse>> CrearReserva([FromBody] CrearReservaClienteRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var clienteId = await ObtenerClienteIdAsync();
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var reserva = await _portalService.CrearReservaAsync(request, clienteId, usuarioId);
            
            return CreatedAtAction(nameof(ObtenerSeguimientoReserva), 
                new { reservaId = reserva.Reserva_Id }, reserva);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Verifica la disponibilidad de activos para fechas específicas
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio de la reserva</param>
    /// <param name="fechaFin">Fecha de fin de la reserva</param>
    /// <param name="activoIds">Lista de IDs de activos a verificar</param>
    /// <returns>Disponibilidad de cada activo</returns>
    [HttpPost("verificar-disponibilidad")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<VerificarDisponibilidadResponse>> VerificarDisponibilidad(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromBody] List<int> activoIds)
    {
        try
        {
            var disponibilidad = await _portalService.VerificarDisponibilidadAsync(fechaInicio, fechaFin, activoIds);
            return Ok(disponibilidad);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todas las cotizaciones solicitadas por el cliente
    /// </summary>
    /// <returns>Lista de cotizaciones</returns>
    [HttpGet("mis-cotizaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MiCotizacionResponse>>> ObtenerMisCotizaciones()
    {
        try
        {
            var clienteId = await ObtenerClienteIdAsync();
            var cotizaciones = await _portalService.ObtenerMisCotizacionesAsync(clienteId);
            return Ok(cotizaciones);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Solicita una nueva cotización
    /// </summary>
    /// <param name="request">Datos de la cotización</param>
    /// <returns>Cotización creada</returns>
    [HttpPost("solicitar-cotizacion")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MiCotizacionResponse>> SolicitarCotizacion([FromBody] SolicitarCotizacionClienteRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var clienteId = await ObtenerClienteIdAsync();
            var cotizacion = await _portalService.SolicitarCotizacionAsync(request, clienteId);
            return CreatedAtAction(nameof(ObtenerMisCotizaciones), cotizacion);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene estadísticas del cliente (gastos, reservas, etc.)
    /// </summary>
    /// <returns>Estadísticas completas</returns>
    [HttpGet("mis-estadisticas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<EstadisticasClienteResponse>> ObtenerEstadisticas()
    {
        try
        {
            var clienteId = await ObtenerClienteIdAsync();
            var stats = await _portalService.ObtenerEstadisticasAsync(clienteId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancela una reserva (solo si está en estado Pendiente o Confirmada)
    /// </summary>
    /// <param name="reservaId">ID de la reserva a cancelar</param>
    /// <param name="motivo">Motivo de la cancelación</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPut("cancelar-reserva/{reservaId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> CancelarReserva(int reservaId, [FromQuery] string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            return BadRequest(new { error = "Debe especificar un motivo de cancelación" });

        try
        {
            var clienteId = await ObtenerClienteIdAsync();
            var resultado = await _portalService.CancelarReservaAsync(reservaId, clienteId, motivo);
            
            if (!resultado)
                return BadRequest(new { error = "No se pudo cancelar la reserva" });

            return Ok(new { mensaje = "Reserva cancelada exitosamente", reserva_id = reservaId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el historial de pagos del cliente
    /// </summary>
    /// <returns>Lista de pagos realizados</returns>
    [HttpGet("historial-pagos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PagoReservaResponse>>> ObtenerHistorialPagos()
    {
        try
        {
            var clienteId = await ObtenerClienteIdAsync();
            var pagos = await _portalService.ObtenerHistorialPagosAsync(clienteId);
            return Ok(pagos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
