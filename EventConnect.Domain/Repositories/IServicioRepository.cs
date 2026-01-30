using EventConnect.Domain.Entities;

namespace EventConnect.Domain.Repositories;

public interface IServicioRepository
{
    Task<IEnumerable<Servicio>> GetAllAsync();
    Task<IEnumerable<Servicio>> GetByActivoAsync(bool activo);
    Task<Servicio?> GetByIdAsync(int id);
    Task<int> CreateAsync(Servicio servicio);
    Task<bool> UpdateAsync(Servicio servicio);
    Task<bool> SoftDeleteAsync(int id);
}
