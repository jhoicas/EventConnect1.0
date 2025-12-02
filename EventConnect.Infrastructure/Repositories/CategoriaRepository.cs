using EventConnect.Domain.Entities;

namespace EventConnect.Infrastructure.Repositories;

public class CategoriaRepository : RepositoryBase<Categoria>
{
    public CategoriaRepository(string connectionString) : base(connectionString)
    {
    }

    // Categories are now global - no empresa filter needed
    // This method is kept for backwards compatibility but returns all categories
    [Obsolete("Categories are now global. Use GetAllAsync() instead.")]
    public async Task<IEnumerable<Categoria>> GetByEmpresaIdAsync(int empresaId)
    {
        var sql = "SELECT * FROM Categoria WHERE Activo = 1 ORDER BY Nombre";
        return await QueryAsync(sql);
    }
}
