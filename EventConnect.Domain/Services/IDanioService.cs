namespace EventConnect.Domain.Services;

using EventConnect.Domain.DTOs;

/// <summary>
/// Interfaz para el servicio de gestión de daños y discrepancias
/// Maneja el registro, evaluación y resolución de daños en activos
/// </summary>
public interface IDanioService
{
    /// <summary>
    /// Registra un nuevo daño en el sistema
    /// </summary>
    Task<DanioDetalladoResponse> RegistrarDanioAsync(CrearDanioRequest request, int usuarioReportadorId);

    /// <summary>
    /// Obtiene todos los daños con filtros opcionales y paginación
    /// </summary>
    Task<PaginatedDanioResponse> ObtenerDaniosAsync(FiltrarDaniosRequest filtro);

    /// <summary>
    /// Obtiene los detalles completos de un daño específico
    /// </summary>
    Task<DanioDetalladoResponse?> ObtenerDanioPorIdAsync(int danioId);

    /// <summary>
    /// Obtiene todos los daños asociados a una reserva específica
    /// </summary>
    Task<List<DanioResponse>> ObtenerDanioPorReservaAsync(int reservaId);

    /// <summary>
    /// Obtiene todos los daños de un activo específico
    /// </summary>
    Task<ResumenDanioActivoResponse> ObtenerDanioPorActivoAsync(int activoId);

    /// <summary>
    /// Actualiza el estado de un daño (de Reportado a Confirmado, Rechazado, etc.)
    /// </summary>
    Task<DanioDetalladoResponse> ActualizarEstadoDanioAsync(int danioId, ActualizarDanioRequest request);

    /// <summary>
    /// Obtiene estadísticas generales del sistema de daños
    /// </summary>
    Task<EstadisticasDaniosResponse> ObtenerEstadisticasAsync();

    /// <summary>
    /// Marca un daño como resuelto (cambio de estado a Reparado o Pérdida Total)
    /// </summary>
    Task<DanioDetalladoResponse> ResolverDanioAsync(int danioId, string resolucion, decimal montoFinal, int usuarioEvaluadorId);

    /// <summary>
    /// Rechaza un daño reportado
    /// </summary>
    Task<DanioDetalladoResponse> RechazarDanioAsync(int danioId, string motivo, int usuarioEvaluadorId);
}
