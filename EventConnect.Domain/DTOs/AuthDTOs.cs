using System.ComponentModel.DataAnnotations;

namespace EventConnect.Domain.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La contraseña es requerida")]
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
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [MinLength(3, ErrorMessage = "El usuario debe tener al menos 3 caracteres")]
    [MaxLength(50, ErrorMessage = "El usuario no puede exceder 50 caracteres")]
    public string Usuario { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El nombre completo es requerido")]
    [MinLength(3, ErrorMessage = "El nombre completo debe tener al menos 3 caracteres")]
    public string Nombre_Completo { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    public string? Telefono { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "El ID de empresa debe ser mayor a 0")]
    public int? Empresa_Id { get; set; }
    
    public int? Rol_Id { get; set; } // Si es null, se asigna el rol por defecto
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
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El nombre completo es requerido")]
    [MinLength(3, ErrorMessage = "El nombre completo debe tener al menos 3 caracteres")]
    public string Nombre_Completo { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    public string? Telefono { get; set; }
    
    // Datos de Cliente
    [Required(ErrorMessage = "El ID de empresa es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID de empresa debe ser mayor a 0")]
    public int Empresa_Id { get; set; }
    
    [Required(ErrorMessage = "El tipo de cliente es requerido")]
    [RegularExpression("^(Persona|Empresa)$", ErrorMessage = "El tipo de cliente debe ser 'Persona' o 'Empresa'")]
    public string Tipo_Cliente { get; set; } = "Persona";
    
    [Required(ErrorMessage = "El documento es requerido")]
    [MinLength(5, ErrorMessage = "El documento debe tener al menos 5 caracteres")]
    public string Documento { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El tipo de documento es requerido")]
    [RegularExpression("^(CC|CE|NIT|PP)$", ErrorMessage = "El tipo de documento debe ser CC, CE, NIT o PP")]
    public string Tipo_Documento { get; set; } = "CC";
    
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
}
