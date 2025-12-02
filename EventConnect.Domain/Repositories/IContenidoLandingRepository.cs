using EventConnect.Domain.Entities;

namespace EventConnect.Domain.Repositories;

public interface IContenidoLandingRepository
{
    Task<IEnumerable<ContenidoLanding>> GetAllAsync();
    Task<IEnumerable<ContenidoLanding>> GetBySeccionAsync(string seccion);
    Task<IEnumerable<ContenidoLanding>> GetActivosAsync();
    Task<ContenidoLanding?> GetByIdAsync(int id);
    Task<int> CreateAsync(ContenidoLanding contenido);
    Task<bool> UpdateAsync(ContenidoLanding contenido);
    Task<bool> DeleteAsync(int id);
}
