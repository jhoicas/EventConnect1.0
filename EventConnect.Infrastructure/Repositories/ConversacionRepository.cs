using EventConnect.Domain.Entities;
using EventConnect.Domain.DTOs;

namespace EventConnect.Infrastructure.Repositories;

public class ConversacionRepository : RepositoryBase<Conversacion>
{
    public ConversacionRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<IEnumerable<ConversacionDTO>> GetByEmpresaIdAsync(int empresaId)
    {
        var sql = @"
            SELECT 
                c.Id,
                c.Empresa_Id,
                c.Cliente_Id,
                c.Usuario_Id,
                c.Asunto,
                c.Reserva_Id,
                c.Estado,
                c.Fecha_Creacion,
                0 as Mensajes_No_Leidos
            FROM Conversacion c
            WHERE c.Empresa_Id = @EmpresaId
            ORDER BY c.Fecha_Creacion DESC";
        
        return await QueryAsync<ConversacionDTO>(sql, new { EmpresaId = empresaId });
    }

    public async Task<IEnumerable<ConversacionDTO>> GetAllAsync()
    {
        var sql = @"
            SELECT 
                c.Id,
                c.Empresa_Id,
                c.Cliente_Id,
                c.Usuario_Id,
                c.Asunto,
                c.Reserva_Id,
                c.Estado,
                c.Fecha_Creacion,
                0 as Mensajes_No_Leidos
            FROM Conversacion c
            ORDER BY c.Fecha_Creacion DESC";
        
        return await QueryAsync<ConversacionDTO>(sql);
    }
}
