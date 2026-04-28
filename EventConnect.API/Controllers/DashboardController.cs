using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventConnect.Domain.DTOs;
using EventConnect.Domain.Services;

namespace EventConnect.API.Controllers;

/// <summary>
/// Controller de Dashboard y Reportes (Analítica y BI)
/// Proporciona métricas, KPIs y análisis del negocio
/// Acceso restringido a Admin y SuperAdmin
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene métricas generales del dashboard
    /// </summary>
    /// <returns>Métricas de ingresos, reservas, clientes, activos, alertas</returns>
    [HttpGet("metricas")]
    public async Task<ActionResult<DashboardMetricasResponse>> ObtenerMetricas()
    {
        try
        {
            var metricas = await _dashboardService.ObtenerMetricasGeneralesAsync();
            return Ok(metricas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo métricas del dashboard");
            return StatusCode(500, new { message = "Error interno al obtener métricas" });
        }
    }

    /// <summary>
    /// Obtiene el dashboard completo con todas las métricas
    /// </summary>
    /// <returns>Dashboard con métricas, KPIs, top activos y clientes</returns>
    [HttpGet("completo")]
    public async Task<ActionResult<DashboardCompletoResponse>> ObtenerDashboardCompleto()
    {
        try
        {
            var dashboard = await _dashboardService.ObtenerDashboardCompletoAsync();
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo dashboard completo");
            return StatusCode(500, new { message = "Error interno al obtener dashboard completo" });
        }
    }

    /// <summary>
    /// Genera reporte de rentabilidad por período
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio del reporte</param>
    /// <param name="fechaFin">Fecha de fin del reporte</param>
    /// <returns>Reporte con ingresos, por categoría y por mes</returns>
    [HttpGet("rentabilidad")]
    public async Task<ActionResult<ReporteRentabilidadResponse>> GenerarReporteRentabilidad(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin)
    {
        try
        {
            if (fechaInicio > fechaFin)
            {
                return BadRequest(new { message = "La fecha de inicio no puede ser mayor a la fecha de fin" });
            }

            var reporte = await _dashboardService.GenerarReporteRentabilidadAsync(fechaInicio, fechaFin);
            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte de rentabilidad");
            return StatusCode(500, new { message = "Error interno al generar reporte" });
        }
    }

    /// <summary>
    /// Obtiene tendencias temporales (diarias y mensuales)
    /// </summary>
    /// <returns>Tendencias de reservas e ingresos</returns>
    [HttpGet("tendencias")]
    public async Task<ActionResult<TendenciasResponse>> ObtenerTendencias()
    {
        try
        {
            var tendencias = await _dashboardService.ObtenerTendenciasAsync();
            return Ok(tendencias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo tendencias");
            return StatusCode(500, new { message = "Error interno al obtener tendencias" });
        }
    }

    /// <summary>
    /// Obtiene los KPIs del sistema
    /// </summary>
    /// <returns>Indicadores de conversión, completitud, cancelación, tiempos, etc.</returns>
    [HttpGet("kpis")]
    public async Task<ActionResult<KPIsResponse>> ObtenerKPIs()
    {
        try
        {
            var kpis = await _dashboardService.ObtenerKPIsAsync();
            return Ok(kpis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo KPIs");
            return StatusCode(500, new { message = "Error interno al obtener KPIs" });
        }
    }

    /// <summary>
    /// Obtiene los activos más rentados
    /// </summary>
    /// <param name="top">Cantidad de activos a retornar (default: 10)</param>
    /// <returns>Lista de activos más rentados con ingresos generados</returns>
    [HttpGet("top-activos")]
    public async Task<ActionResult<List<ActivoRentadoResponse>>> ObtenerTopActivos([FromQuery] int top = 10)
    {
        try
        {
            if (top <= 0 || top > 100)
            {
                return BadRequest(new { message = "El parámetro top debe estar entre 1 y 100" });
            }

            var activos = await _dashboardService.ObtenerActivosMasRentadosAsync(top);
            return Ok(activos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo activos más rentados");
            return StatusCode(500, new { message = "Error interno al obtener activos" });
        }
    }

    /// <summary>
    /// Obtiene los mejores clientes
    /// </summary>
    /// <param name="top">Cantidad de clientes a retornar (default: 10)</param>
    /// <returns>Clientes por ingresos y por frecuencia</returns>
    [HttpGet("top-clientes")]
    public async Task<ActionResult<TopClientesResponse>> ObtenerTopClientes([FromQuery] int top = 10)
    {
        try
        {
            if (top <= 0 || top > 100)
            {
                return BadRequest(new { message = "El parámetro top debe estar entre 1 y 100" });
            }

            var clientes = await _dashboardService.ObtenerTopClientesAsync(top);
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo top clientes");
            return StatusCode(500, new { message = "Error interno al obtener clientes" });
        }
    }

    /// <summary>
    /// Obtiene estadísticas por estado de reserva
    /// </summary>
    /// <returns>Distribución de reservas por estado</returns>
    [HttpGet("estados")]
    public async Task<ActionResult<EstadisticasEstadosResponse>> ObtenerEstadisticasEstados()
    {
        try
        {
            var estadisticas = await _dashboardService.ObtenerEstadisticasEstadosAsync();
            return Ok(estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo estadísticas de estados");
            return StatusCode(500, new { message = "Error interno al obtener estadísticas" });
        }
    }

    /// <summary>
    /// Obtiene distribución geográfica de clientes
    /// </summary>
    /// <returns>Clientes, reservas e ingresos por ciudad</returns>
    [HttpGet("geografica")]
    public async Task<ActionResult<List<DistribucionGeograficaResponse>>> ObtenerDistribucionGeografica()
    {
        try
        {
            var distribucion = await _dashboardService.ObtenerDistribucionGeograficaAsync();
            return Ok(distribucion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo distribución geográfica");
            return StatusCode(500, new { message = "Error interno al obtener distribución" });
        }
    }

    /// <summary>
    /// Obtiene comportamiento y segmentación de clientes
    /// </summary>
    /// <param name="segmento">Filtro por segmento: VIP, Frecuente, Ocasional, Nuevo (opcional)</param>
    /// <returns>Análisis de comportamiento con segmentación automática</returns>
    [HttpGet("comportamiento-clientes")]
    public async Task<ActionResult<List<ComportamientoClienteResponse>>> ObtenerComportamientoClientes(
        [FromQuery] string? segmento = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(segmento))
            {
                var segmentosValidos = new[] { "VIP", "Frecuente", "Ocasional", "Nuevo" };
                if (!segmentosValidos.Contains(segmento))
                {
                    return BadRequest(new { message = "Segmento inválido. Valores permitidos: VIP, Frecuente, Ocasional, Nuevo" });
                }
            }

            var comportamiento = await _dashboardService.ObtenerComportamientoClientesAsync(segmento);
            return Ok(comportamiento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo comportamiento de clientes");
            return StatusCode(500, new { message = "Error interno al obtener comportamiento" });
        }
    }

    /// <summary>
    /// Obtiene rentabilidad por categoría
    /// </summary>
    /// <returns>Ingresos y porcentaje por categoría</returns>
    [HttpGet("rentabilidad-categoria")]
    public async Task<ActionResult<List<RentabilidadPorCategoriaResponse>>> ObtenerRentabilidadPorCategoria()
    {
        try
        {
            var rentabilidad = await _dashboardService.ObtenerRentabilidadPorCategoriaAsync();
            return Ok(rentabilidad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo rentabilidad por categoría");
            return StatusCode(500, new { message = "Error interno al obtener rentabilidad" });
        }
    }
}
