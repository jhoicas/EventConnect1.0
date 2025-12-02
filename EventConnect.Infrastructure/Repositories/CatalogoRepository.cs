using EventConnect.Domain.Entities;
using Dapper;

namespace EventConnect.Infrastructure.Repositories;

// Repositorio genérico para catálogos
public class CatalogoRepository<T> : RepositoryBase<T> where T : class
{
    public CatalogoRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<IEnumerable<T>> GetActivosAsync()
    {
        var sql = $"SELECT * FROM {_tableName} WHERE Activo = 1 ORDER BY Orden, Nombre";
        return await QueryAsync(sql);
    }

    public async Task<T?> GetByCodigoAsync(string codigo)
    {
        var sql = $"SELECT * FROM {_tableName} WHERE Codigo = @Codigo";
        return await QueryFirstOrDefaultAsync(sql, new { Codigo = codigo });
    }

    public async Task<bool> ExisteCodigoAsync(string codigo, int? exceptoId = null)
    {
        var sql = $"SELECT COUNT(*) FROM {_tableName} WHERE Codigo = @Codigo";
        if (exceptoId.HasValue)
            sql += " AND Id != @ExceptoId";

        using var connection = GetConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Codigo = codigo, ExceptoId = exceptoId });
        return count > 0;
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        var sql = $"UPDATE {_tableName} SET Activo = 0 WHERE Id = @Id AND Sistema = 0";
        using var connection = GetConnection();
        var affected = await connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<bool> ActivarAsync(int id)
    {
        var sql = $"UPDATE {_tableName} SET Activo = 1 WHERE Id = @Id";
        using var connection = GetConnection();
        var affected = await connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }
}

// Repositorios específicos
public class EstadoReservaRepository : CatalogoRepository<CatalogoEstadoReserva>
{
    public EstadoReservaRepository(string connectionString) : base(connectionString) { }
}

public class EstadoActivoRepository : CatalogoRepository<CatalogoEstadoActivo>
{
    public EstadoActivoRepository(string connectionString) : base(connectionString) { }

    public async Task<IEnumerable<CatalogoEstadoActivo>> GetPermiteReservaAsync()
    {
        var sql = "SELECT * FROM catalogo_estado_activo WHERE Activo = 1 AND Permite_Reserva = 1 ORDER BY Orden";
        return await QueryAsync(sql);
    }
}

public class MetodoPagoRepository : CatalogoRepository<CatalogoMetodoPago>
{
    public MetodoPagoRepository(string connectionString) : base(connectionString) { }
}

public class TipoMantenimientoRepository : CatalogoRepository<CatalogoTipoMantenimiento>
{
    public TipoMantenimientoRepository(string connectionString) : base(connectionString) { }

    public async Task<IEnumerable<CatalogoTipoMantenimiento>> GetPreventivosAsync()
    {
        var sql = "SELECT * FROM catalogo_tipo_mantenimiento WHERE Activo = 1 AND Es_Preventivo = 1 ORDER BY Orden";
        return await QueryAsync(sql);
    }
}
