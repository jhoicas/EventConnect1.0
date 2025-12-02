using EventConnect.Domain.Entities;

namespace EventConnect.Infrastructure.Repositories;

public class EmpresaRepository : RepositoryBase<Empresa>
{
    public EmpresaRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<IEnumerable<Empresa>> GetActivasAsync()
    {
        var sql = "SELECT * FROM Empresa WHERE Estado = 'Activa' ORDER BY Razon_Social";
        return await QueryAsync(sql);
    }
}
