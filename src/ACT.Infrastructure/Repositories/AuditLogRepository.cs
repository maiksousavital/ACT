using ACT.Domain.Entities;
using ACT.Domain.Interfaces;
using ACT.Infrastructure.Extensions;
using ACT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ACT.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog log)
    {
        await _context.AuditLogs.AddAsync(log);
    }

    public async Task<IEnumerable<AuditLog>> GetByCompanyAsync(int companyId, int limit = 100)
    {
        return await _context.AuditLogs
            .Where(a => a.CompanyId == companyId)
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync(int limit = 100)
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(int? companyId, int page, int pageSize)
    {
        var query = _context.AuditLogs.AsQueryable();
        if (companyId.HasValue)
            query = query.Where(a => a.CompanyId == companyId.Value);
        query = query.OrderByDescending(a => a.Timestamp);
        return await query.ToPagedResultAsync(page, pageSize);
    }

    public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetForEntityPagedAsync(int? companyId, string entityType,
        int entityId, string? search, DateTime? from, DateTime? to, int page, int pageSize)
    {
        var query = _context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId);

        if (companyId.HasValue)
            query = query.Where(a => a.CompanyId == companyId.Value);
        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a =>
                (a.FieldName != null && a.FieldName.Contains(search)) ||
                (a.OldValue != null && a.OldValue.Contains(search)) ||
                (a.NewValue != null && a.NewValue.Contains(search)) ||
                a.UserEmail.Contains(search));
        }

        query = query.OrderByDescending(a => a.Timestamp);
        return await query.ToPagedResultAsync(page, pageSize);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

