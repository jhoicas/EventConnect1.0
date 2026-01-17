using Dapper;
using EventConnect.Domain.Entities;
using Npgsql;

namespace EventConnect.Infrastructure.Repositories;

public class ActivoRepository : RepositoryBase<Activo>
{
    public ActivoRepository(string connectionString) : base(connectionString) { }

    public async Task<IEnumerable<Activo>> GetByEmpresaIdAsync(int empresaId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = "SELECT * FROM Activo WHERE Empresa_Id = @EmpresaId ORDER BY Fecha_Creacion DESC";
        return await connection.QueryAsync<Activo>(query, new { EmpresaId = empresaId });
    }

    public async Task<IEnumerable<Activo>> GetByBodegaIdAsync(int bodegaId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = "SELECT * FROM Activo WHERE Bodega_Id = @BodegaId AND Activo = true ORDER BY Codigo_Activo";
        return await connection.QueryAsync<Activo>(query, new { BodegaId = bodegaId });
    }

    public async Task<IEnumerable<Activo>> GetByEstadoDisponibilidadAsync(int empresaId, string estadoDisponibilidad)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = "SELECT * FROM Activo WHERE Empresa_Id = @EmpresaId AND Estado_Disponibilidad = @EstadoDisponibilidad ORDER BY Fecha_Creacion DESC";
        return await connection.QueryAsync<Activo>(query, new { EmpresaId = empresaId, EstadoDisponibilidad = estadoDisponibilidad });
    }

    public async Task<Activo?> GetByCodigoActivoAsync(string codigoActivo)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = "SELECT * FROM Activo WHERE Codigo_Activo = @CodigoActivo";
        return await connection.QueryFirstOrDefaultAsync<Activo>(query, new { CodigoActivo = codigoActivo });
    }

    public async Task<Activo?> GetByQRCodeAsync(string qrCode)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = "SELECT * FROM Activo WHERE QR_Code = @QRCode";
        return await connection.QueryFirstOrDefaultAsync<Activo>(query, new { QRCode = qrCode });
    }

    public async Task<IEnumerable<Activo>> GetActivosParaDepreciacionAsync(int empresaId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = @"
            SELECT * FROM Activo 
            WHERE Empresa_Id = @EmpresaId 
            AND Estado = 'Activo'
            AND Valor_Adquisicion IS NOT NULL
            AND Vida_Util_Meses IS NOT NULL
            AND Fecha_Adquisicion IS NOT NULL";
        return await connection.QueryAsync<Activo>(query, new { EmpresaId = empresaId });
    }
}
