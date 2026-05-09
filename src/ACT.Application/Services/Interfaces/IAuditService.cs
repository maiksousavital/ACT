using ACT.Application.Dtos;

namespace ACT.Application.Services.Interfaces;

public interface IAuditService
{
    Task LogAsync(int? userId, string userEmail, int? companyId, string action, string entityType, int? entityId, string? details = null);
    Task<PagedResult<AuditLogDto>> GetPagedAsync(int? companyId, int page, int pageSize);
    Task<PagedResult<LoginHistoryDto>> GetLoginHistoryPagedAsync(int? companyId, int page, int pageSize);
}

