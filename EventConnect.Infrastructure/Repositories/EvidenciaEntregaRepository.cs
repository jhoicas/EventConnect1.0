using Dapper;
using EventConnect.Domain.Entities;
using Npgsql;

namespace EventConnect.Infrastructure.Repositories;

public class EvidenciaEntregaRepository : RepositoryBase<EvidenciaEntrega>
{
    public EvidenciaEntregaRepository(string connectionString) : base(connectionString) { }

    /// <summary>
    /// Obtiene todas las evidencias de una reserva
    /// </summary>
    public async Task<IEnumerable<EvidenciaEntrega>> GetByReservaIdAsync(int reservaId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = @"
            SELECT * FROM Evidencia_Entrega 
            WHERE Reserva_Id = @ReservaId 
            ORDER BY Fecha_Creacion DESC";
        return await connection.QueryAsync<EvidenciaEntrega>(query, new { ReservaId = reservaId });
    }

    /// <summary>
    /// Obtiene evidencias filtradas por empresa (multi-tenant)
    /// </summary>
    public async Task<IEnumerable<EvidenciaEntrega>> GetByEmpresaIdAsync(int empresaId, int? reservaId = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = @"
            SELECT * FROM Evidencia_Entrega 
            WHERE Empresa_Id = @EmpresaId";
        
        var parameters = new DynamicParameters();
        parameters.Add("EmpresaId", empresaId);
        
        if (reservaId.HasValue)
        {
            query += " AND Reserva_Id = @ReservaId";
            parameters.Add("ReservaId", reservaId.Value);
        }
        
        query += " ORDER BY Fecha_Creacion DESC";
        
        return await connection.QueryAsync<EvidenciaEntrega>(query, parameters);
    }

    /// <summary>
    /// Obtiene evidencias por tipo y empresa
    /// </summary>
    public async Task<IEnumerable<EvidenciaEntrega>> GetByTipoAsync(int empresaId, string tipo)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = @"
            SELECT * FROM Evidencia_Entrega 
            WHERE Empresa_Id = @EmpresaId 
            AND Tipo = @Tipo 
            ORDER BY Fecha_Creacion DESC";
        return await connection.QueryAsync<EvidenciaEntrega>(query, new { EmpresaId = empresaId, Tipo = tipo });
    }

    /// <summary>
    /// Verifica si una reserva tiene evidencias de un tipo específico
    /// </summary>
    public async Task<bool> HasEvidenciaTipoAsync(int reservaId, string tipo)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = @"
            SELECT COUNT(*) > 0 
            FROM Evidencia_Entrega 
            WHERE Reserva_Id = @ReservaId 
            AND Tipo = @Tipo";
        return await connection.QueryFirstOrDefaultAsync<bool>(query, new { ReservaId = reservaId, Tipo = tipo });
    }

    /// <summary>
    /// Obtiene evidencias con información de usuario y reserva
    /// </summary>
    public async Task<IEnumerable<EvidenciaConDetallesDto>> GetWithDetailsAsync(int empresaId, int? reservaId = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        
        var sql = @"
            SELECT 
                e.*,
                u.Nombre_Completo AS Usuario_Nombre,
                r.Codigo_Reserva,
                c.Nombre AS Cliente_Nombre
            FROM Evidencia_Entrega e
            INNER JOIN Usuario u ON e.Usuario_Id = u.Id
            INNER JOIN Reserva r ON e.Reserva_Id = r.Id
            INNER JOIN Cliente c ON r.Cliente_Id = c.Id
            WHERE e.Empresa_Id = @EmpresaId";
        
        var parameters = new DynamicParameters();
        parameters.Add("EmpresaId", empresaId);
        
        if (reservaId.HasValue)
        {
            sql += " AND e.Reserva_Id = @ReservaId";
            parameters.Add("ReservaId", reservaId.Value);
        }
        
        sql += " ORDER BY e.Fecha_Creacion DESC";
        
        return await connection.QueryAsync<EvidenciaConDetallesDto>(sql, parameters);
    }

    /// <summary>
    /// DTO interno para evidencia con detalles
    /// </summary>
    public class EvidenciaConDetallesDto : EvidenciaEntrega
    {
        public string? Usuario_Nombre { get; set; }
        public string? Codigo_Reserva { get; set; }
        public string? Cliente_Nombre { get; set; }
    }
}
