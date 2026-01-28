using EventConnect.Domain.Entities;
using EventConnect.Domain.DTOs;

namespace EventConnect.Infrastructure.Repositories;

public class ConversacionRepository : RepositoryBase<Conversacion>
{
    public ConversacionRepository(string connectionString) : base(connectionString)
    {
    }

    /// <summary>
    /// Obtiene conversaciones de un cliente con sus empresas (proveedores)
    /// Incluye datos de la empresa para mostrar en el listado
    /// </summary>
    public async Task<IEnumerable<ConversacionDTO>> GetConversacionesByClienteIdAsync(int clienteId)
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
                COALESCE((
                    SELECT COUNT(*)
                    FROM Mensaje m
                    WHERE m.Conversacion_Id = c.Id 
                    AND m.Leido = FALSE
                    AND m.Emisor_Usuario_Id IN (
                        SELECT Id FROM Usuario WHERE Empresa_Id = c.Empresa_Id
                    )
                ), 0) as Mensajes_No_Leidos,
                e.Razon_Social as Nombre_Contraparte,
                e.Logo_URL as Avatar_Contraparte,
                e.Email as Email_Contraparte
            FROM Conversacion c
            INNER JOIN Empresa e ON c.Empresa_Id = e.Id
            WHERE c.Cliente_Id = @ClienteId
            ORDER BY c.Fecha_Creacion DESC";
        
        return await QueryAsync<ConversacionDTO>(sql, new { ClienteId = clienteId });
    }

    /// <summary>
    /// Obtiene conversaciones de una empresa con sus clientes
    /// Incluye datos del cliente para mostrar en el listado
    /// </summary>
    public async Task<IEnumerable<ConversacionDTO>> GetConversacionesByEmpresaIdAsync(int empresaId)
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
                COALESCE((
                    SELECT COUNT(*)
                    FROM Mensaje m
                    WHERE m.Conversacion_Id = c.Id 
                    AND m.Leido = FALSE
                    AND m.Emisor_Usuario_Id IN (
                        SELECT u.Id FROM Usuario u 
                        INNER JOIN Cliente cl ON u.Id = cl.Usuario_Id
                        WHERE cl.Id = c.Cliente_Id
                    )
                ), 0) as Mensajes_No_Leidos,
                cl.Nombre as Nombre_Contraparte,
                NULL as Avatar_Contraparte,
                cl.Email as Email_Contraparte
            FROM Conversacion c
            INNER JOIN Cliente cl ON c.Cliente_Id = cl.Id
            WHERE c.Empresa_Id = @EmpresaId
            ORDER BY c.Fecha_Creacion DESC";
        
        return await QueryAsync<ConversacionDTO>(sql, new { EmpresaId = empresaId });
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
