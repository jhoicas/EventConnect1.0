using EventConnect.Domain.Entities;

namespace EventConnect.Domain.Repositories;

public interface IConfiguracionSistemaRepository
{
    Task<IEnumerable<ConfiguracionSistema>> GetAllAsync();
    Task<IEnumerable<ConfiguracionSistema>> GetByEmpresaIdAsync(int empresaId);
    Task<IEnumerable<ConfiguracionSistema>> GetGlobalesAsync();
    Task<ConfiguracionSistema?> GetByIdAsync(int id);
    Task<ConfiguracionSistema?> GetByClaveAsync(string clave, int? empresaId = null);
    Task<int> CreateAsync(ConfiguracionSistema configuracion);
    Task<bool> UpdateAsync(ConfiguracionSistema configuracion);
    Task<bool> DeleteAsync(int id);
}
