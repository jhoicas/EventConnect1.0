namespace EventConnect.Domain.Repositories;

public interface IRepositoryBase<T> where T : class
{
    /// <summary>
    /// Obtiene todos los registros opcionalmente filtrados por Empresa_Id
    /// </summary>
    /// <param name="empresaId">ID de la empresa. null = sin filtro (SuperAdmin), valor = filtrar por empresa</param>
    Task<IEnumerable<T>> GetAllAsync(int? empresaId = null);
    Task<T?> GetByIdAsync(int id);
    Task<int> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}
