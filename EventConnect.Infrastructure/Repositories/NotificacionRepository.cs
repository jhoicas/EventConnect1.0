using Dapper;
using EventConnect.Domain.Entities;

namespace EventConnect.Infrastructure.Repositories;

public class NotificacionRepository : RepositoryBase<Notificacion>
{
    public NotificacionRepository(string connectionString) : base(connectionString) { }

    public async Task<IEnumerable<Notificacion>> GetByUsuarioIdAsync(int usuarioId)
    {
        var sql = "SELECT * FROM Notificacion WHERE Usuario_Id = @UsuarioId ORDER BY Fecha_Envio DESC";
        return await QueryAsync(sql, new { UsuarioId = usuarioId });
    }
}
