using System.Security.Claims;

namespace MiCv.Sitio.Security;

public static class AuthExtensions
{
    public static int? GetUsuarioId(this ClaimsPrincipal user)
    {
        var v = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(v, out var id) ? id : null;
    }
}
