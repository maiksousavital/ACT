using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ACT.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetCompanyId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue("companyId");
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    public static bool IsSuperAdmin(this ClaimsPrincipal user)
        => user.FindFirstValue("role") == "SuperAdmin";

    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }

    public static string GetEmail(this ClaimsPrincipal user)
        => user.FindFirstValue("email") ?? string.Empty;
}
