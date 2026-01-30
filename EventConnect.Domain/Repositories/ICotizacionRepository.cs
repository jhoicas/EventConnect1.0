using EventConnect.Domain.Entities;

namespace EventConnect.Domain.Repositories;

public interface ISolicitudCotizacionRepository
{
    Task<IEnumerable<Cotizacion>> GetByClienteIdAsync(int clienteId);
    Task<Cotizacion?> GetByIdAsync(int id);
    Task<IEnumerable<Cotizacion>> GetAllAsync();
    Task<IEnumerable<Cotizacion>> GetByProductoIdAsync(int productoId);
    Task<IEnumerable<Cotizacion>> GetByEstadoAsync(string estado);
    Task<int> CreateAsync(Cotizacion cotizacion);
    Task<bool> UpdateAsync(Cotizacion cotizacion);
    Task<bool> DeleteAsync(int id);
}
