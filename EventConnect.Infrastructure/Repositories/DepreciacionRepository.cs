using Dapper;
using EventConnect.Domain.Entities;

namespace EventConnect.Infrastructure.Repositories;

public class DepreciacionRepository : RepositoryBase<Depreciacion>
{
    public DepreciacionRepository(string connectionString) : base(connectionString) { }

    public async Task<IEnumerable<Depreciacion>> GetByActivoIdAsync(int activoId)
    {
        var sql = "SELECT * FROM Depreciacion WHERE Activo_Id = @ActivoId ORDER BY Fecha_Calculo DESC";
        return await QueryAsync(sql, new { ActivoId = activoId });
    }
}
