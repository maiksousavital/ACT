using ACT.Application.Common;
using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Domain.Interfaces;

namespace ACT.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditRepo;
    private readonly ILoginHistoryRepository _loginRepo;

    public AuditService(IAuditLogRepository auditRepo, ILoginHistoryRepository loginRepo)
    {
        _auditRepo = auditRepo;
        _loginRepo = loginRepo;
    }

    public async Task LogAsync(int? userId, string userEmail, int? companyId, string action, string entityType, int? entityId, string? details = null)
    {
        var log = new AuditLog
        {
            UserId = userId,
            UserEmail = userEmail,
            CompanyId = companyId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details
        };
        await _auditRepo.AddAsync(log);
        await _auditRepo.SaveChangesAsync();
    }

    public async Task LogChangesAsync(int? userId, string userEmail, int? companyId, string entityType, int entityId,
        List<(string Field, string? Old, string? New)> changes)
    {
        if (changes.Count == 0)
            return;

        foreach (var (field, oldValue, newValue) in changes)
        {
            await _auditRepo.AddAsync(new AuditLog
            {
                UserId = userId,
                UserEmail = userEmail,
                CompanyId = companyId,
                Action = "Update",
                EntityType = entityType,
                EntityId = entityId,
                FieldName = field,
                OldValue = oldValue,
                NewValue = newValue
            });
        }
        await _auditRepo.SaveChangesAsync();
    }

    public async Task<PagedResult<AuditLogDto>> GetPagedAsync(int? companyId, int page, int pageSize)
    {
        (page, pageSize) = Paging.Clamp(page, pageSize);
        var (items, totalCount) = await _auditRepo.GetPagedAsync(companyId, page, pageSize);
        return new PagedResult<AuditLogDto>
        {
            Items = items.Select(ToAuditDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<AuditLogDto>> GetForEntityPagedAsync(int? companyId, string entityType, int entityId,
        string? search, DateTime? from, DateTime? to, int page, int pageSize)
    {
        (page, pageSize) = Paging.Clamp(page, pageSize);
        var (items, totalCount) = await _auditRepo.GetForEntityPagedAsync(companyId, entityType, entityId, search, from, to, page, pageSize);
        return new PagedResult<AuditLogDto>
        {
            Items = items.Select(ToAuditDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<LoginHistoryDto>> GetLoginHistoryPagedAsync(int? companyId, int page, int pageSize)
    {
        (page, pageSize) = Paging.Clamp(page, pageSize);
        var (items, totalCount) = await _loginRepo.GetPagedAsync(companyId, page, pageSize);
        return new PagedResult<LoginHistoryDto>
        {
            Items = items.Select(ToLoginDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static AuditLogDto ToAuditDto(AuditLog a) => new()
    {
        Id = a.Id,
        UserId = a.UserId,
        UserEmail = a.UserEmail,
        CompanyId = a.CompanyId,
        Action = a.Action,
        EntityType = a.EntityType,
        EntityId = a.EntityId,
        Details = a.Details,
        FieldName = a.FieldName,
        OldValue = a.OldValue,
        NewValue = a.NewValue,
        Timestamp = a.Timestamp
    };

    private static LoginHistoryDto ToLoginDto(LoginHistory l) => new()
    {
        Id = l.Id,
        UserId = l.UserId,
        Email = l.Email,
        CompanyId = l.CompanyId,
        IpAddress = l.IpAddress,
        UserAgent = l.UserAgent,
        Success = l.Success,
        FailureReason = l.FailureReason,
        Timestamp = l.Timestamp
    };
}

