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

    public async Task<IEnumerable<ClienteConEmpresaDTO>> GetAllWithEmpresaAsync()
    {
        var sql = @"
            SELECT 
                c.*,
                e.Razon_Social as Empresa_Nombre
            FROM Cliente c
            INNER JOIN Empresa e ON c.Empresa_Id = e.Id
            WHERE c.Estado = 'Activo'
            ORDER BY e.Razon_Social, c.Nombre";
        
        using var connection = GetConnection();
        return await Dapper.SqlMapper.QueryAsync<ClienteConEmpresaDTO>(connection, sql);
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
