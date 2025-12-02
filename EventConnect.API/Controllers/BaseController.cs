using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    protected int? GetCurrentEmpresaId()
    {
        var empresaIdClaim = User.FindFirst("EmpresaId")?.Value;
        return int.TryParse(empresaIdClaim, out var empresaId) ? empresaId : null;
    }

    protected string GetCurrentUsername()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    }

    protected string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }

    protected int GetCurrentNivelAcceso()
    {
        var nivelClaim = User.FindFirst("NivelAcceso")?.Value;
        return int.TryParse(nivelClaim, out var nivel) ? nivel : 999;
    }

    protected bool IsSuperAdmin()
    {
        return GetCurrentNivelAcceso() == 0;
    }

    protected bool IsAdminProveedor()
    {
        return GetCurrentNivelAcceso() <= 1;
    }
}
