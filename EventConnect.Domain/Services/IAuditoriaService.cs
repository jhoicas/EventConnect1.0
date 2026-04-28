using EventConnect.Domain.DTOs;

namespace EventConnect.Domain.Services;

public interface IAuditoriaService
{
    /// <summary>
    /// Registra un cambio en la auditoría
    /// </summary>
    Task<bool> RegistrarCambioAsync(
        string tablaAfectada,
        int registroId,
        int usuarioId,
        string accion,
        string datosNuevos,
        string? datosAnteriores = null,
        string? detalles = null,
        string? ipOrigen = null,
        string? userAgent = null);

    /// <summary>
    /// Obtiene el historial completo de un registro con timeline
    /// </summary>
    Task<HistorialResponse?> ObtenerHistorialAsync(string tablaAfectada, int registroId);

    /// <summary>
    /// Obtiene el historial de una tabla con filtros
    /// </summary>
    Task<PaginatedAuditoriaResponse<AuditoriaDto>> ObtenerHistorialFiltradoAsync(FiltroAuditoriaRequest filtro);

    /// <summary>
    /// Obtiene un resumen de cambios en un registro
    /// </summary>
    Task<ResumenAuditoriaResponse?> ObtenerResumenAsync(string tablaAfectada, int registroId);

    /// <summary>
    /// Obtiene cambios recientes de la plataforma
    /// </summary>
    Task<List<AuditoriaDto>> ObtenerCambiosRecientesAsync(int top = 50);

    /// <summary>
    /// Obtiene cambios realizados por un usuario específico
    /// </summary>
    Task<List<AuditoriaDto>> ObtenerCambiosPorUsuarioAsync(int usuarioId, int top = 50);

    /// <summary>
    /// Busca en el historial de auditoría
    /// </summary>
    Task<List<AuditoriaDto>> BuscarAsync(string termino, string? tablaAfectada = null);

    /// <summary>
    /// Limpia registros de auditoría antiguos
    /// </summary>
    Task<int> LimpiarAuditoriaAntiguaAsync(int diasAntiguos = 90);
}
