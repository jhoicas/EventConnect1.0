using Dapper;
using EventConnect.Domain.Entities;
using Npgsql;

namespace EventConnect.Infrastructure.Repositories;

public class BodegaRepository : RepositoryBase<Bodega>
{
    public BodegaRepository(string connectionString) : base(connectionString) { }

    public async Task<IEnumerable<Bodega>> GetByEmpresaIdAsync(int empresaId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = "SELECT * FROM Bodega WHERE Empresa_Id = @EmpresaId AND Estado = ''Activo'' ORDER BY Nombre";
        return await connection.QueryAsync<Bodega>(query, new { EmpresaId = empresaId });
    }

    public async Task<Bodega?> GetByCodigoBodegaAsync(string codigoBodega)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = "SELECT * FROM Bodega WHERE Codigo_Bodega = @CodigoBodega";
        return await connection.QueryFirstOrDefaultAsync<Bodega>(query, new { CodigoBodega = codigoBodega });
    }
}
