using EventConnect.Domain.Entities;
using EventConnect.Domain.DTOs;

namespace EventConnect.Infrastructure.Repositories;

public class ClienteRepository : RepositoryBase<Cliente>
{
    public ClienteRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<IEnumerable<Cliente>> GetByEmpresaIdAsync(int empresaId)
    {
        var sql = "SELECT * FROM Cliente WHERE Empresa_Id = @EmpresaId AND Estado = 'Activo' ORDER BY Nombre";
        return await QueryAsync(sql, new { EmpresaId = empresaId });
    }

    /// <summary>
    /// Obtiene todos los clientes con información de empresa opcionalmente filtrados por Empresa_Id
    /// </summary>
    /// <param name="empresaId">ID de la empresa. null = todos (solo SuperAdmin), valor = filtrar por empresa</param>
    public async Task<IEnumerable<ClienteConEmpresaDTO>> GetAllWithEmpresaAsync(int? empresaId = null)
    {
        var sql = @"
            SELECT 
                c.*,
                e.Razon_Social as Empresa_Nombre
            FROM Cliente c
            INNER JOIN Empresa e ON c.Empresa_Id = e.Id
            WHERE c.Estado = 'Activo'";
        
        // Aplicar filtro multi-tenant si se proporciona empresaId
        if (empresaId.HasValue)
        {
            sql += " AND c.Empresa_Id = @EmpresaId";
        }
        
        sql += " ORDER BY e.Razon_Social, c.Nombre";
        
        using var connection = GetConnection();
        return await Dapper.SqlMapper.QueryAsync<ClienteConEmpresaDTO>(
            connection, 
            sql, 
            empresaId.HasValue ? new { EmpresaId = empresaId.Value } : null);
    }

    public async Task<Cliente?> GetByDocumentoAsync(int empresaId, string documento)
    {
        var sql = "SELECT * FROM Cliente WHERE Empresa_Id = @EmpresaId AND Documento = @Documento";
        return await QueryFirstOrDefaultAsync(sql, new { EmpresaId = empresaId, Documento = documento });
    }

    public async Task<Cliente?> GetByUsuarioIdAsync(int usuarioId)
    {
        var sql = "SELECT * FROM Cliente WHERE Usuario_Id = @UsuarioId";
        return await QueryFirstOrDefaultAsync(sql, new { UsuarioId = usuarioId });
    }
}
