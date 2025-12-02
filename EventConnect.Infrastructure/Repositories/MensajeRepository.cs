using EventConnect.Domain.Entities;
using EventConnect.Domain.DTOs;

namespace EventConnect.Infrastructure.Repositories;

public class MensajeRepository : RepositoryBase<Mensaje>
{
    public MensajeRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<IEnumerable<MensajeDTO>> GetByConversacionIdAsync(int conversacionId)
    {
        var sql = @"
            SELECT 
                m.*,
                u.Nombre_Completo as Emisor_Nombre,
                u.Avatar_URL as Emisor_Avatar
            FROM Mensaje m
            INNER JOIN Usuario u ON m.Emisor_Usuario_Id = u.Id
            WHERE m.Conversacion_Id = @ConversacionId
            ORDER BY m.Fecha_Envio ASC";
        
        using var connection = GetConnection();
        return await Dapper.SqlMapper.QueryAsync<MensajeDTO>(connection, sql, new { ConversacionId = conversacionId });
    }

    public async Task<MensajeDTO?> GetUltimoMensajeAsync(int conversacionId)
    {
        var sql = @"
            SELECT 
                m.*,
                u.Nombre_Completo as Emisor_Nombre,
                u.Avatar_URL as Emisor_Avatar
            FROM Mensaje m
            INNER JOIN Usuario u ON m.Emisor_Usuario_Id = u.Id
            WHERE m.Conversacion_Id = @ConversacionId
            ORDER BY m.Fecha_Envio DESC
            LIMIT 1";
        
        using var connection = GetConnection();
        return await Dapper.SqlMapper.QueryFirstOrDefaultAsync<MensajeDTO>(connection, sql, new { ConversacionId = conversacionId });
    }

    public async Task<int> MarcarLeidosAsync(int conversacionId, int usuarioId)
    {
        var sql = @"
            UPDATE Mensaje 
            SET Leido = 1 
            WHERE Conversacion_Id = @ConversacionId 
            AND Emisor_Usuario_Id != @UsuarioId 
            AND Leido = 0";
        
        return await ExecuteAsync(sql, new { ConversacionId = conversacionId, UsuarioId = usuarioId });
    }

    public async Task<int> GetNoLeidosCountAsync(int conversacionId, int usuarioId)
    {
        var sql = @"
            SELECT COUNT(*) 
            FROM Mensaje 
            WHERE Conversacion_Id = @ConversacionId 
            AND Emisor_Usuario_Id != @UsuarioId 
            AND Leido = 0";
        
        using var connection = GetConnection();
        return await Dapper.SqlMapper.ExecuteScalarAsync<int>(connection, sql, new { ConversacionId = conversacionId, UsuarioId = usuarioId });
    }
}
