using EventConnect.Domain.Entities;
using EventConnect.Domain.Repositories;
using Dapper;
using MySqlConnector;

namespace EventConnect.Infrastructure.Repositories;

public class UsuarioRepository : RepositoryBase<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<Usuario?> GetByUsernameAsync(string username)
    {
        var sql = "SELECT * FROM Usuario WHERE Usuario = @Username";
        return await QueryFirstOrDefaultAsync(sql, new { Username = username });
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        var sql = "SELECT * FROM Usuario WHERE Email = @Email";
        return await QueryFirstOrDefaultAsync(sql, new { Email = email });
    }

    public async Task<IEnumerable<Usuario>> GetByEmpresaIdAsync(int empresaId)
    {
        var sql = "SELECT * FROM Usuario WHERE Empresa_Id = @EmpresaId";
        return await QueryAsync(sql, new { EmpresaId = empresaId });
    }

    public async Task<bool> IncrementFailedAttemptsAsync(int userId)
    {
        var sql = "UPDATE Usuario SET Intentos_Fallidos = Intentos_Fallidos + 1 WHERE Id = @UserId";
        var affected = await ExecuteAsync(sql, new { UserId = userId });
        return affected > 0;
    }

    public async Task<bool> ResetFailedAttemptsAsync(int userId)
    {
        var sql = "UPDATE Usuario SET Intentos_Fallidos = 0, Ultimo_Acceso = NOW() WHERE Id = @UserId";
        var affected = await ExecuteAsync(sql, new { UserId = userId });
        return affected > 0;
    }

    public async Task<IEnumerable<dynamic>> GetAllWithDetailsAsync()
    {
        var sql = @"
            SELECT 
                u.Id as id,
                u.Empresa_Id as empresa_Id,
                u.Rol_Id as rol_Id,
                u.Usuario as usuario1,
                u.Email as email,
                u.Nombre_Completo as nombre_Completo,
                u.Telefono as telefono,
                u.Avatar_URL as avatar_URL,
                u.Estado as estado,
                u.Intentos_Fallidos as intentos_Fallidos,
                u.Ultimo_Acceso as ultimo_Acceso,
                u.Fecha_Creacion as fecha_Creacion,
                u.Fecha_Actualizacion as fecha_Actualizacion,
                u.Requiere_Cambio_Password as requiere_Cambio_Password,
                u.TwoFA_Activo as twoFA_Activo,
                r.Nombre as rol,
                e.Razon_Social as empresa_Nombre
            FROM Usuario u
            LEFT JOIN Rol r ON u.Rol_Id = r.Id
            LEFT JOIN Empresa e ON u.Empresa_Id = e.Id
            ORDER BY u.Fecha_Creacion DESC";
        
        using var connection = new MySqlConnection(_connectionString);
        return await connection.QueryAsync(sql);
    }

    public async Task<bool> UpdatePerfilAsync(int userId, string? nombreCompleto, string? telefono, string? avatarUrl)
    {
        var updates = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);

        if (!string.IsNullOrWhiteSpace(nombreCompleto))
        {
            updates.Add("Nombre_Completo = @NombreCompleto");
            parameters.Add("NombreCompleto", nombreCompleto);
        }

        if (!string.IsNullOrWhiteSpace(telefono))
        {
            updates.Add("Telefono = @Telefono");
            parameters.Add("Telefono", telefono);
        }

        if (avatarUrl != null)
        {
            updates.Add("Avatar_URL = @AvatarUrl");
            parameters.Add("AvatarUrl", avatarUrl);
        }

        if (updates.Count == 0)
        {
            return false; // No hay nada que actualizar
        }

        updates.Add("Fecha_Actualizacion = NOW()");

        var sql = $"UPDATE Usuario SET {string.Join(", ", updates)} WHERE Id = @UserId";
        var affected = await ExecuteAsync(sql, parameters);
        return affected > 0;
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        var sql = "UPDATE Usuario SET Password_Hash = @PasswordHash, Fecha_Actualizacion = NOW() WHERE Id = @UserId";
        var affected = await ExecuteAsync(sql, new { PasswordHash = newPasswordHash, UserId = userId });
        return affected > 0;
    }

    public async Task<int> GetPendingUsersCountAsync()
    {
        var sql = @"SELECT COUNT(*) 
                    FROM Usuario u
                    INNER JOIN Rol r ON u.Rol_Id = r.Id
                    WHERE u.Estado = 'Inactivo' 
                    AND r.Nombre IN ('Admin-Proveedor', 'Usuario-Proveedor')";
        
        using var connection = new MySqlConnection(_connectionString);
        var count = await connection.ExecuteScalarAsync<int>(sql);
        return count;
    }
}
