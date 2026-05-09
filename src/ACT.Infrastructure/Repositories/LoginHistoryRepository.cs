using ACT.Domain.Entities;
using ACT.Domain.Interfaces;
using ACT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ACT.Infrastructure.Repositories;

public class LoginHistoryRepository : ILoginHistoryRepository
{
    private readonly AppDbContext _context;

    public LoginHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(LoginHistory record)
    {
        await _context.LoginHistories.AddAsync(record);
    }

    public async Task<IEnumerable<LoginHistory>> GetByUserAsync(int userId, int limit = 50)
    {
        return await _context.LoginHistories
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<LoginHistory>> GetByCompanyAsync(int companyId, int limit = 100)
    {
        return await _context.LoginHistories
            .Where(l => l.CompanyId == companyId)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<LoginHistory>> GetAllAsync(int limit = 100)
    {
        return await _context.LoginHistories
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<(IEnumerable<LoginHistory> Items, int TotalCount)> GetPagedAsync(int? companyId, int page, int pageSize)
    {
        var query = _context.LoginHistories.AsQueryable();
        if (companyId.HasValue)
            query = query.Where(l => l.CompanyId == companyId.Value);
        query = query.OrderByDescending(l => l.Timestamp);
        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

