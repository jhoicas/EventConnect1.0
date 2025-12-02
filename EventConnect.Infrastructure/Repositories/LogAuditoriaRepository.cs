using Dapper;
using EventConnect.Domain.Entities;

namespace EventConnect.Infrastructure.Repositories;

public class LogAuditoriaRepository : RepositoryBase<LogAuditoria>
{
    public LogAuditoriaRepository(string connectionString) : base(connectionString) { }

    public async Task<IEnumerable<LogAuditoria>> GetByUsuarioIdAsync(int usuarioId)
    {
        var sql = "SELECT * FROM Log_Auditoria WHERE Usuario_Id = @UsuarioId ORDER BY Fecha_Accion DESC LIMIT 100";
        return await QueryAsync(sql, new { UsuarioId = usuarioId });
    }
}
