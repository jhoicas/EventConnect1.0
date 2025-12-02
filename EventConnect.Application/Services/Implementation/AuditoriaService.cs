using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace EventConnect.Application.Services.Implementation;

public class AuditoriaService : IAuditoriaService
{
    private readonly LogAuditoriaRepository _logRepository;
    private readonly ILogger<AuditoriaService> _logger;

    public AuditoriaService(LogAuditoriaRepository logRepository, ILogger<AuditoriaService> logger)
    {
        _logRepository = logRepository;
        _logger = logger;
    }

    public async Task RegistrarAccionAsync(int usuarioId, string accion, string entidadAfectada, string detalles)
    {
        try
        {
            var log = new LogAuditoria
            {
                Usuario_Id = usuarioId,
                Tipo_Operacion = accion,
                Tabla_Afectada = entidadAfectada,
                Valores_Nuevos = detalles,
                Fecha_Operacion = DateTime.Now
            };
            
            var hashData = $"{log.Usuario_Id}|{log.Tipo_Operacion}|{log.Tabla_Afectada}|{log.Valores_Nuevos}|{log.Fecha_Operacion}";
            log.Hash_Integridad = GenerarHashSHA256(hashData);

            await _logRepository.AddAsync(log);
            _logger.LogInformation($"Auditoria registrada: {accion} en {entidadAfectada}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar auditoria");
        }
    }

    public async Task<bool> VerificarIntegridadLogsAsync()
    {
        try
        {
            var logs = await _logRepository.GetAllAsync();
            foreach (var log in logs)
            {
                var hashData = $"{log.Usuario_Id}|{log.Tipo_Operacion}|{log.Tabla_Afectada}|{log.Valores_Nuevos}|{log.Fecha_Operacion}";
                var hashCalculado = GenerarHashSHA256(hashData);
                
                if (hashCalculado != log.Hash_Integridad)
                {
                    _logger.LogWarning($"Integridad comprometida en log ID: {log.Id}");
                    return false;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar integridad");
            return false;
        }
    }

    private string GenerarHashSHA256(string texto)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(texto);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
