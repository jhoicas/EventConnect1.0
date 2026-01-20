using Dapper;
using EventConnect.Domain.Entities;
using Npgsql;

namespace EventConnect.Infrastructure.Repositories;

public class DetalleFacturaRepository : RepositoryBase<DetalleFactura>
{
    public DetalleFacturaRepository(string connectionString) : base(connectionString) { }

    public async Task<IEnumerable<DetalleFactura>> GetByFacturaIdAsync(int facturaId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = @"
            SELECT * FROM Detalle_Factura 
            WHERE Factura_Id = @FacturaId 
            ORDER BY Id";
        return await connection.QueryAsync<DetalleFactura>(query, new { FacturaId = facturaId });
    }

    public async Task<bool> DeleteByFacturaIdAsync(int facturaId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = "DELETE FROM Detalle_Factura WHERE Factura_Id = @FacturaId";
        var affected = await connection.ExecuteAsync(query, new { FacturaId = facturaId });
        return affected > 0;
    }
}
