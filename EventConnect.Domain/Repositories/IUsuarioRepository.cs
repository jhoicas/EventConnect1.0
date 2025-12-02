using EventConnect.Domain.Entities;

namespace EventConnect.Domain.Repositories;

public interface IUsuarioRepository : IRepositoryBase<Usuario>
{
    Task<Usuario?> GetByUsernameAsync(string username);
    Task<Usuario?> GetByEmailAsync(string email);
    Task<IEnumerable<dynamic>> GetAllWithDetailsAsync();
    Task<bool> UpdatePerfilAsync(int userId, string? nombreCompleto, string? telefono, string? avatarUrl);
    Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash);
    Task<int> GetPendingUsersCountAsync();
}
