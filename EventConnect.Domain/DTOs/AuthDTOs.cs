namespace EventConnect.Domain.DTOs;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? Expiration { get; set; }
    public UsuarioDto Usuario { get; set; } = new();
    public string? Message { get; set; }
}

public class UsuarioDto
{
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Nombre_Completo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Avatar_URL { get; set; }
    public int? Empresa_Id { get; set; }
    public string? Empresa_Nombre { get; set; }
    public int Rol_Id { get; set; }
    public string Rol { get; set; } = string.Empty;
    public int Nivel_Acceso { get; set; }
}

public class RegisterRequest
{
    public string Usuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Nombre_Completo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public int Empresa_Id { get; set; }
    public int Rol_Id { get; set; } = 3; // Cliente por defecto
}

public class UpdateProfileRequest
{
    public int UsuarioId { get; set; }
    public string Nombre_Completo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Avatar_URL { get; set; }
}

public class ChangePasswordRequest
{
    public int UsuarioId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class RegisterClienteRequest
{
    // Datos de Usuario
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Nombre_Completo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    
    // Datos de Cliente
    public int Empresa_Id { get; set; }
    public string Tipo_Cliente { get; set; } = "Persona";
    public string Documento { get; set; } = string.Empty;
    public string Tipo_Documento { get; set; } = "CC";
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
}
