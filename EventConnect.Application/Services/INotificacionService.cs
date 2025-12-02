namespace EventConnect.Application.Services;

/// <summary>
/// Servicio de notificaciones (Email, SMS, Push)
/// </summary>
public interface INotificacionService
{
    /// <summary>
    /// Envía notificación de lotes próximos a vencer
    /// </summary>
    Task NotificarLotesProximosVencerAsync(int empresaId, int dias);
    
    /// <summary>
    /// Envía notificación de mantenimientos vencidos
    /// </summary>
    Task NotificarMantenimientosVencidosAsync(int empresaId);
    
    /// <summary>
    /// Envía notificación de stock bajo
    /// </summary>
    Task NotificarStockBajoAsync(int empresaId);
    
    /// <summary>
    /// Envía notificación personalizada
    /// </summary>
    Task EnviarNotificacionAsync(int usuarioId, string titulo, string mensaje, string tipo);
}
