using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;

namespace ACT.Tests.TestHelpers;

/// <summary>
/// No-op IAuditService for tests that construct a service directly (bypassing DI) and don't
/// care about audit trail side effects — avoids wiring a real AuditLogRepository per test.
/// </summary>
public class NoopAuditService : IAuditService
{
    public Task LogAsync(int? userId, string userEmail, int? companyId, string action, string entityType, int? entityId, string? details = null)
        => Task.CompletedTask;

    public Task LogChangesAsync(int? userId, string userEmail, int? companyId, string entityType, int entityId,
        List<(string Field, string? Old, string? New)> changes) => Task.CompletedTask;

    public Task<PagedResult<AuditLogDto>> GetPagedAsync(int? companyId, int page, int pageSize)
        => Task.FromResult(new PagedResult<AuditLogDto> { Items = [], TotalCount = 0, Page = page, PageSize = pageSize });

    public Task<PagedResult<AuditLogDto>> GetForEntityPagedAsync(int? companyId, string entityType, int entityId,
        string? search, DateTime? from, DateTime? to, int page, int pageSize)
        => Task.FromResult(new PagedResult<AuditLogDto> { Items = [], TotalCount = 0, Page = page, PageSize = pageSize });

    public Task<PagedResult<LoginHistoryDto>> GetLoginHistoryPagedAsync(int? companyId, int page, int pageSize)
        => Task.FromResult(new PagedResult<LoginHistoryDto> { Items = [], TotalCount = 0, Page = page, PageSize = pageSize });
}
