using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventConnect.Application.Services.Implementation;

public class DepreciacionService : IDepreciacionService
{
    private readonly ActivoRepository _activoRepository;
    private readonly DepreciacionRepository _depreciacionRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DepreciacionService> _logger;

    public DepreciacionService(
        ActivoRepository activoRepository,
        DepreciacionRepository depreciacionRepository,
        IConfiguration configuration,
        ILogger<DepreciacionService> logger)
    {
        _activoRepository = activoRepository;
        _depreciacionRepository = depreciacionRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task CalcularDepreciacionMensualAsync()
    {
        _logger.LogInformation("Calculando depreciaci�n mensual...");
        var activos = await _activoRepository.GetAllAsync();
        
        foreach (var activo in activos)
        {
            if (activo.Costo_Compra.HasValue && activo.Vida_Util_Anos.HasValue)
            {
                await CalcularDepreciacionActivoAsync(activo.Id);
            }
        }
    }

    public async Task<decimal> CalcularDepreciacionActivoAsync(int activoId)
    {
        var activo = await _activoRepository.GetByIdAsync(activoId);
        if (activo == null || !activo.Costo_Compra.HasValue || !activo.Vida_Util_Anos.HasValue)
            return 0;

        var valorDepreciacionMensual = activo.Costo_Compra.Value / (activo.Vida_Util_Anos.Value * 12);
        
        var depreciacion = new Depreciacion
        {
            Activo_Id = activoId,
            Valor_Inicial = activo.Costo_Compra.Value,
            Depreciacion_Mensual = valorDepreciacionMensual,
            Fecha_Calculo = DateTime.Now,
            Fecha_Periodo = DateTime.Now
        };

        await _depreciacionRepository.AddAsync(depreciacion);
        return valorDepreciacionMensual;
    }

    public async Task<decimal> ObtenerValorLibrosAsync(int activoId)
    {
        var activo = await _activoRepository.GetByIdAsync(activoId);
        if (activo == null || !activo.Costo_Compra.HasValue)
            return 0;

        var depreciaciones = await _depreciacionRepository.GetByActivoIdAsync(activoId);
        var depreciacionTotal = depreciaciones.Sum(d => d.Depreciacion_Mensual);
        
        return activo.Costo_Compra.Value - depreciacionTotal;
    }
}
