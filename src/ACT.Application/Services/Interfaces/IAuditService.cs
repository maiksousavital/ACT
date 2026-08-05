using ACT.Application.Dtos;

namespace ACT.Application.Services.Interfaces;

public interface IAuditService
{
    Task LogAsync(int? userId, string userEmail, int? companyId, string action, string entityType, int? entityId, string? details = null);

    /// <summary>
    /// Writes one AuditLog row per changed field (Action = "Update"). No-op when changes is empty.
    /// </summary>
    Task LogChangesAsync(int? userId, string userEmail, int? companyId, string entityType, int entityId,
        List<(string Field, string? Old, string? New)> changes);

    Task<PagedResult<AuditLogDto>> GetPagedAsync(int? companyId, int page, int pageSize);

    Task<PagedResult<AuditLogDto>> GetForEntityPagedAsync(int? companyId, string entityType, int entityId,
        string? search, DateTime? from, DateTime? to, int page, int pageSize);

    Task<PagedResult<LoginHistoryDto>> GetLoginHistoryPagedAsync(int? companyId, int page, int pageSize);
}

