using ACT.API.Extensions;
using ACT.Application.Services.Interfaces;

namespace ACT.API.Services;

/// <summary>
/// Reads the caller's tenant scope from the current HTTP request's JWT claims.
/// Outside of an HTTP request (background workers, EF migrations, startup seeding) there is no
/// caller to restrict, so it reports as an unrestricted/system caller rather than locking every
/// query to a non-existent tenant.
/// </summary>
public class HttpContextTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? CompanyId => _httpContextAccessor.HttpContext?.User?.GetCompanyId();

    public bool IsSuperAdmin
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user == null || user.IsSuperAdmin();
        }
    }
}
