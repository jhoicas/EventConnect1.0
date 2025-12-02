namespace EventConnect.Application.Services;

/// <summary>
/// Servicio de auditoría con hash SHA-256
/// </summary>
public interface IAuditoriaService
{
    /// <summary>
    /// Registra una acción de auditoría con hash de integridad
    /// </summary>
    Task RegistrarAccionAsync(int usuarioId, string entidad, string accion, string detalles);
    
    /// <summary>
    /// Verifica la integridad de los logs de auditoría
    /// </summary>
    Task<bool> VerificarIntegridadLogsAsync();
}
