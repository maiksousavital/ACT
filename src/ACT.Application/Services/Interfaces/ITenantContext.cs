namespace ACT.Application.Services.Interfaces;

/// <summary>
/// Resolves the calling user's tenant scope for the current operation. Implemented against
/// HttpContext in the API layer; when there is no HTTP context (background workers, migrations,
/// startup seeding), the implementation should behave as an unrestricted/system caller.
/// </summary>
public interface ITenantContext
{
    int? CompanyId { get; }
    bool IsSuperAdmin { get; }
}
