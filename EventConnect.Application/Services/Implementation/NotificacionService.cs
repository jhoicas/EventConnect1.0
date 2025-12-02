using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace EventConnect.Application.Services.Implementation;

public class NotificacionService : INotificacionService
{
    private readonly LoteRepository _loteRepository;
    private readonly MantenimientoRepository _mantenimientoRepository;
    private readonly ProductoRepository _productoRepository;
    private readonly NotificacionRepository _notificacionRepository;
    private readonly UsuarioRepository _usuarioRepository;
    private readonly ILogger<NotificacionService> _logger;

    public NotificacionService(
        LoteRepository loteRepository,
        MantenimientoRepository mantenimientoRepository,
        ProductoRepository productoRepository,
        NotificacionRepository notificacionRepository,
        UsuarioRepository usuarioRepository,
        ILogger<NotificacionService> logger)
    {
        _loteRepository = loteRepository;
        _mantenimientoRepository = mantenimientoRepository;
        _productoRepository = productoRepository;
        _notificacionRepository = notificacionRepository;
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    public async Task NotificarLotesProximosVencerAsync(int empresaId, int dias)
    {
        _logger.LogInformation($"Verificando lotes próximos a vencer ({dias} días) - Empresa: {empresaId}");
        
        var lotes = await _loteRepository.GetAllAsync();
        var lotesVencimiento = lotes
            .Where(l => l.Fecha_Vencimiento.HasValue && 
                   (l.Fecha_Vencimiento.Value - DateTime.Now).TotalDays <= dias)
            .ToList();
    }

    public async Task NotificarMantenimientosVencidosAsync(int empresaId)
    {
        _logger.LogInformation($"Verificando mantenimientos vencidos - Empresa: {empresaId}");
    }

    public async Task NotificarStockBajoAsync(int empresaId)
    {
        _logger.LogInformation($"Verificando stock bajo - Empresa: {empresaId}");
        var productos = await _productoRepository.GetByEmpresaIdAsync(empresaId);
        var productosStockBajo = productos.Where(p => p.Cantidad_Stock <= p.Stock_Minimo).ToList();
    }

    public async Task EnviarNotificacionAsync(int usuarioId, string titulo, string mensaje, string tipo)
    {
        var notificacion = new Notificacion
        {
            Usuario_Id = usuarioId,
            Tipo = tipo,
            Titulo = titulo,
            Mensaje = mensaje,
            Fecha_Creacion = DateTime.Now
        };

        await _notificacionRepository.AddAsync(notificacion);
        _logger.LogInformation($"Notificación {tipo} enviada a usuario {usuarioId}");
    }
}
