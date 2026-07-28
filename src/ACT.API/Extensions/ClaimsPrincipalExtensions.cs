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
}
