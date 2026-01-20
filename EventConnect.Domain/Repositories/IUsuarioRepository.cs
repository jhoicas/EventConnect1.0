using EventConnect.Domain.Entities;

namespace EventConnect.Domain.Repositories;

public interface IUsuarioRepository : IRepositoryBase<Usuario>
{
    Task<Usuario?> GetByUsernameAsync(string username);
    Task<Usuario?> GetByEmailAsync(string email);
    /// <summary>
    /// Obtiene todos los usuarios con detalles opcionalmente filtrados por Empresa_Id
    /// </summary>
    /// <param name="empresaId">ID de la empresa. null = todos (solo SuperAdmin), valor = filtrar por empresa</param>
    Task<IEnumerable<dynamic>> GetAllWithDetailsAsync(int? empresaId = null);
    Task<bool> UpdatePerfilAsync(int userId, string? nombreCompleto, string? telefono, string? avatarUrl);
    Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash);
    Task<int> GetPendingUsersCountAsync();
}
