namespace EventConnect.Domain.Services;

using EventConnect.Domain.DTOs;

/// <summary>
/// Interfaz para el servicio de Dashboard y Reportes (Analítica y BI)
/// Proporciona métricas, KPIs y reportes del negocio
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Obtiene las métricas generales del dashboard
    /// </summary>
    Task<DashboardMetricasResponse> ObtenerMetricasGeneralesAsync();

    /// <summary>
    /// Obtiene el dashboard completo con todas las métricas
    /// </summary>
    Task<DashboardCompletoResponse> ObtenerDashboardCompletoAsync();

    /// <summary>
    /// Genera reporte de rentabilidad para un período específico
    /// </summary>
    Task<ReporteRentabilidadResponse> GenerarReporteRentabilidadAsync(DateTime fechaInicio, DateTime fechaFin);

    /// <summary>
    /// Obtiene tendencias temporales (diaria, semanal, mensual)
    /// </summary>
    Task<TendenciasResponse> ObtenerTendenciasAsync();

    /// <summary>
    /// Obtiene los KPIs (Key Performance Indicators) del negocio
    /// </summary>
    Task<KPIsResponse> ObtenerKPIsAsync();

    /// <summary>
    /// Obtiene los activos más rentados (top N)
    /// </summary>
    Task<List<ActivoRentadoResponse>> ObtenerActivosMasRentadosAsync(int top = 10);

    /// <summary>
    /// Obtiene el top de clientes por diferentes métricas
    /// </summary>
    Task<TopClientesResponse> ObtenerTopClientesAsync(int top = 10);

    /// <summary>
    /// Obtiene estadísticas de estados de reservas
    /// </summary>
    Task<EstadisticasEstadosResponse> ObtenerEstadisticasEstadosAsync();

    /// <summary>
    /// Obtiene distribución geográfica de clientes
    /// </summary>
    Task<List<DistribucionGeograficaResponse>> ObtenerDistribucionGeograficaAsync();

    /// <summary>
    /// Obtiene análisis de comportamiento de clientes
    /// </summary>
    Task<List<ComportamientoClienteResponse>> ObtenerComportamientoClientesAsync(string? segmento = null);

    /// <summary>
    /// Obtiene reporte de rentabilidad por categoría
    /// </summary>
    Task<List<RentabilidadPorCategoriaResponse>> ObtenerRentabilidadPorCategoriaAsync();
}
