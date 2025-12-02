using Dapper;
using EventConnect.Domain.Entities;
using MySqlConnector;

namespace EventConnect.Infrastructure.Repositories;

public class MovimientoInventarioRepository : RepositoryBase<MovimientoInventario>
{
    public MovimientoInventarioRepository(string connectionString) : base(connectionString) { }

    public async Task<IEnumerable<MovimientoInventario>> GetByEmpresaIdAsync(int empresaId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        using var connection = new MySqlConnection(_connectionString);
        var query = @"
            SELECT * FROM Movimiento_Inventario 
            WHERE Empresa_Id = @EmpresaId
            AND (@FechaInicio IS NULL OR Fecha_Movimiento >= @FechaInicio)
            AND (@FechaFin IS NULL OR Fecha_Movimiento <= @FechaFin)
            ORDER BY Fecha_Movimiento DESC";
        return await connection.QueryAsync<MovimientoInventario>(query, new { EmpresaId = empresaId, FechaInicio = fechaInicio, FechaFin = fechaFin });
    }

    public async Task<IEnumerable<MovimientoInventario>> GetByProductoIdAsync(int productoId)
    {
        using var connection = new MySqlConnection(_connectionString);
        var query = "SELECT * FROM Movimiento_Inventario WHERE Producto_Id = @ProductoId ORDER BY Fecha_Movimiento DESC";
        return await connection.QueryAsync<MovimientoInventario>(query, new { ProductoId = productoId });
    }

    public async Task<IEnumerable<MovimientoInventario>> GetByActivoIdAsync(int activoId)
    {
        using var connection = new MySqlConnection(_connectionString);
        var query = "SELECT * FROM Movimiento_Inventario WHERE Activo_Id = @ActivoId ORDER BY Fecha_Movimiento DESC";
        return await connection.QueryAsync<MovimientoInventario>(query, new { ActivoId = activoId });
    }

    public async Task<IEnumerable<MovimientoInventario>> GetByTipoMovimientoAsync(int empresaId, string tipoMovimiento)
    {
        using var connection = new MySqlConnection(_connectionString);
        var query = "SELECT * FROM Movimiento_Inventario WHERE Empresa_Id = @EmpresaId AND Tipo_Movimiento = @TipoMovimiento ORDER BY Fecha_Movimiento DESC";
        return await connection.QueryAsync<MovimientoInventario>(query, new { EmpresaId = empresaId, TipoMovimiento = tipoMovimiento });
    }
}
