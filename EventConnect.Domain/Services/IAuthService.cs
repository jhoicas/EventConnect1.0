using EventConnect.Domain.DTOs;

namespace EventConnect.Domain.Services;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> RegisterClienteAsync(RegisterClienteRequest request);
    Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
    Task<UsuarioDto?> UpdateProfileAsync(int userId, UpdateProfileRequest request);
}
