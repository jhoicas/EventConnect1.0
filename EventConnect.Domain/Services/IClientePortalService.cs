namespace EventConnect.Domain.Services;

using EventConnect.Domain.DTOs;

/// <summary>
/// Interfaz para el servicio de Portal de Autogestión de Clientes
/// Permite a los clientes gestionar sus reservas, cotizaciones y seguimiento
/// </summary>
public interface IClientePortalService
{
    /// <summary>
    /// Obtiene todas las reservas del cliente autenticado
    /// </summary>
    Task<List<MiReservaResponse>> ObtenerMisReservasAsync(int clienteId, string? estado = null);

    /// <summary>
    /// Obtiene el seguimiento detallado de una reserva específica
    /// </summary>
    Task<SeguimientoReservaResponse?> ObtenerSeguimientoReservaAsync(int reservaId, int clienteId);

    /// <summary>
    /// Crea una nueva reserva desde el portal del cliente
    /// </summary>
    Task<SeguimientoReservaResponse> CrearReservaAsync(CrearReservaClienteRequest request, int clienteId, int usuarioId);

    /// <summary>
    /// Verifica la disponibilidad de activos para fechas específicas
    /// </summary>
    Task<VerificarDisponibilidadResponse> VerificarDisponibilidadAsync(DateTime fechaInicio, DateTime fechaFin, List<int> activoIds);

    /// <summary>
    /// Obtiene todas las cotizaciones del cliente autenticado
    /// </summary>
    Task<List<MiCotizacionResponse>> ObtenerMisCotizacionesAsync(int clienteId);

    /// <summary>
    /// Solicita una nueva cotización
    /// </summary>
    Task<MiCotizacionResponse> SolicitarCotizacionAsync(SolicitarCotizacionClienteRequest request, int clienteId);

    /// <summary>
    /// Obtiene las estadísticas del cliente (total gastado, reservas, etc.)
    /// </summary>
    Task<EstadisticasClienteResponse> ObtenerEstadisticasAsync(int clienteId);

    /// <summary>
    /// Cancela una reserva (solo si está en estado Pendiente o Confirmada)
    /// </summary>
    Task<bool> CancelarReservaAsync(int reservaId, int clienteId, string motivo);

    /// <summary>
    /// Obtiene el historial de pagos del cliente
    /// </summary>
    Task<List<PagoReservaResponse>> ObtenerHistorialPagosAsync(int clienteId);
}
