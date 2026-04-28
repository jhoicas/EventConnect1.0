namespace EventConnect.Domain.Services;

using EventConnect.Domain.DTOs;

/// <summary>
/// Interfaz para el servicio de gestión de alertas de mantenimiento y depreciación
/// Mantiene activos en óptimas condiciones de funcionamiento
/// </summary>
public interface IAlertaService
{
    /// <summary>
    /// Obtiene todas las alertas con filtros opcionales y paginación
    /// </summary>
    Task<PaginatedAlertaResponse> ObtenerAlertasAsync(FiltrarAlertasRequest filtro);

    /// <summary>
    /// Obtiene los detalles completos de una alerta específica
    /// </summary>
    Task<AlertaDetalladoResponse?> ObtenerAlertaPorIdAsync(int alertaId);

    /// <summary>
    /// Obtiene todas las alertas activas de un activo
    /// </summary>
    Task<ResumenAlertasPorActivoResponse> ObtenerAlertasPorActivoAsync(int activoId);

    /// <summary>
    /// Obtiene todas las alertas pendientes o con severidad crítica
    /// </summary>
    Task<List<AlertaResponse>> ObtenerAlertasCriticasAsync(int limit = 20);

    /// <summary>
    /// Genera alertas automáticas para mantenimiento y depreciación
    /// Se ejecuta como tarea programada (ej: diariamente)
    /// </summary>
    Task<ResultadoGeneracionAlertasResponse> GenerarAlertasAutomaticasAsync();

    /// <summary>
    /// Genera alertas para un activo específico (mantenimiento + depreciación)
    /// </summary>
    Task<List<AlertaAutomaticaResponse>> GenerarAlertasParaActivoAsync(int activoId);

    /// <summary>
    /// Actualiza el estado de una alerta
    /// </summary>
    Task<AlertaDetalladoResponse> ActualizarEstadoAlertaAsync(int alertaId, ActualizarAlertaRequest request);

    /// <summary>
    /// Marca una alerta como resuelta
    /// </summary>
    Task<AlertaDetalladoResponse> ResolverAlertaAsync(int alertaId, string observaciones, int usuarioId);

    /// <summary>
    /// Obtiene estadísticas generales de alertas del sistema
    /// </summary>
    Task<ResumenAlertasResponse> ObtenerEstadisticasAsync();

    /// <summary>
    /// Obtiene alertas próximas a vencer (24-48 horas)
    /// </summary>
    Task<List<AlertaResponse>> ObtenerAlertasProximasAVencerAsync();

    /// <summary>
    /// Asigna una alerta a un usuario específico
    /// </summary>
    Task<AlertaDetalladoResponse> AsignarAlertaAsync(int alertaId, int usuarioAsignadoId);

    /// <summary>
    /// Limpia alertas resueltas más antiguas que X días
    /// </summary>
    Task<int> LimpiarAlertasAntiguasAsync(int diasAntiguos = 90);
}
