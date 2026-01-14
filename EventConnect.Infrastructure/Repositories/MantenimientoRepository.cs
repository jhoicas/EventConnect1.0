using Dapper;
using EventConnect.Domain.Entities;
using Npgsql;

namespace EventConnect.Infrastructure.Repositories;

public class MantenimientoRepository : RepositoryBase<Mantenimiento>
{
    public MantenimientoRepository(string connectionString) : base(connectionString) { }

    public async Task<IEnumerable<Mantenimiento>> GetByEmpresaIdAsync(int empresaId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = @"
            SELECT m.* FROM Mantenimiento m
            INNER JOIN Activo a ON m.Activo_Id = a.Id
            WHERE a.Empresa_Id = @EmpresaId
            ORDER BY m.Fecha_Programada DESC";
        return await connection.QueryAsync<Mantenimiento>(query, new { EmpresaId = empresaId });
    }

    public async Task<IEnumerable<Mantenimiento>> GetByActivoIdAsync(int activoId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = "SELECT * FROM Mantenimiento WHERE Activo_Id = @ActivoId ORDER BY Fecha_Programada DESC";
        return await connection.QueryAsync<Mantenimiento>(query, new { ActivoId = activoId });
    }

    public async Task<IEnumerable<Mantenimiento>> GetPendientesAsync(int empresaId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = @"
            SELECT m.* FROM Mantenimiento m
            INNER JOIN Activo a ON m.Activo_Id = a.Id
            WHERE a.Empresa_Id = @EmpresaId
            AND m.Estado = 'Pendiente'
            ORDER BY m.Fecha_Programada";
        return await connection.QueryAsync<Mantenimiento>(query, new { EmpresaId = empresaId });
    }

    public async Task<IEnumerable<Mantenimiento>> GetVencidosAsync(int empresaId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = @"
            SELECT m.* FROM Mantenimiento m
            INNER JOIN Activo a ON m.Activo_Id = a.Id
            WHERE a.Empresa_Id = @EmpresaId
            AND m.Estado = 'Pendiente'
            AND m.Fecha_Programada < CURDATE()
            ORDER BY m.Fecha_Programada";
        return await connection.QueryAsync<Mantenimiento>(query, new { EmpresaId = empresaId });
    }
}
