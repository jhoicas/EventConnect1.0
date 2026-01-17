using Dapper;
using EventConnect.Domain.Entities;
using Npgsql;

namespace EventConnect.Infrastructure.Repositories;

public class LoteRepository : RepositoryBase<Lote>
{
    public LoteRepository(string connectionString) : base(connectionString) { }

    public async Task<IEnumerable<Lote>> GetByProductoIdAsync(int productoId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = "SELECT * FROM Lote WHERE Producto_Id = @ProductoId ORDER BY Fecha_Vencimiento";
        return await connection.QueryAsync<Lote>(query, new { ProductoId = productoId });
    }

    public async Task<IEnumerable<Lote>> GetLotesVencidosAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = @"
            SELECT * FROM Lote 
            WHERE Fecha_Vencimiento IS NOT NULL 
            AND Fecha_Vencimiento < CURRENT_DATE
            AND Cantidad_Actual > 0
            AND Estado = 'Disponible'";
        return await connection.QueryAsync<Lote>(query);
    }

    public async Task<IEnumerable<Lote>> GetLotesPorVencerAsync(int diasAnticipacion)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = @"
            SELECT * FROM Lote 
            WHERE Fecha_Vencimiento IS NOT NULL 
            AND Fecha_Vencimiento BETWEEN CURRENT_DATE AND CURRENT_DATE + (@Dias || ' days')::INTERVAL
            AND Cantidad_Actual > 0
            AND Estado = 'Disponible'";
        return await connection.QueryAsync<Lote>(query, new { Dias = diasAnticipacion });
    }
}
