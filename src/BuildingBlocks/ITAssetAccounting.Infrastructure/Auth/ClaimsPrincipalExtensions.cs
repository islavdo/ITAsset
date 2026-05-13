using System.Security.Claims;
using ITAssetAccounting.Shared.Enums;

namespace ITAssetAccounting.Infrastructure.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }

    public static bool HasRole(this ClaimsPrincipal user, UserRole role) =>
        user.IsInRole(role.ToString());
}
