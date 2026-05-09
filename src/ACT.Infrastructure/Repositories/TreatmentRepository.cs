using ACT.Domain.Entities;
using ACT.Domain.Interfaces;
using ACT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ACT.Infrastructure.Repositories;

public class TreatmentRepository : ITreatmentRepository
{
    private readonly AppDbContext _context;

    public TreatmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Treatment>> GetDueAsync(int? companyId)
    {
        var query = _context.Treatments
            .Include(t => t.Client)
            .Include(t => t.TreatmentType)
            .Where(t => t.FollowedUpAt == null && t.NextFollowUpDate.Date <= DateTime.UtcNow.Date);
        if (companyId.HasValue)
            query = query.Where(t => t.CompanyId == companyId.Value);
        return await query.OrderBy(t => t.NextFollowUpDate).ToListAsync();
    }
    
    public async Task<IEnumerable<Treatment>> GetTodayAsync(int? companyId)
    {
        var today = DateTime.UtcNow.Date;
        var query = _context.Treatments
            .Include(t => t.Client)
            .Include(t => t.TreatmentType)
            .Where(t => t.FollowedUpAt == null && t.NextFollowUpDate.Date == today);
        if (companyId.HasValue)
            query = query.Where(t => t.CompanyId == companyId.Value);
        return await query.OrderBy(t => t.Client.LastName).ToListAsync();
    }

    public async Task<IEnumerable<Treatment>> GetByClientAsync(int clientId)
    {
        return await _context.Treatments
            .Include(t => t.Client)
            .Include(t => t.TreatmentType)
            .Where(t => t.ClientId == clientId)
            .ToListAsync();
    }
    
    public async Task<Treatment?> GetByIdAsync(int id)
    {
        return await _context.Treatments
            .Include(t => t.Client)
            .Include(t => t.TreatmentType)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task AddAsync(Treatment treatment)
    {
        await _context.Treatments.AddAsync(treatment);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Treatment> Items, int TotalCount)> GetPagedAsync(int? companyId, int page, int pageSize)
    {
        var query = _context.Treatments
            .Include(t => t.Client)
            .Include(t => t.TreatmentType)
            .AsQueryable();
        if (companyId.HasValue)
            query = query.Where(t => t.CompanyId == companyId.Value);
        var ordered = query.OrderByDescending(t => t.TreatmentDate);
        var totalCount = await ordered.CountAsync();
        var items = await ordered.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }

    public async Task UpdateAsync(Treatment treatment)
    {
        _context.Treatments.Update(treatment);
        await Task.CompletedTask;
    }
}
