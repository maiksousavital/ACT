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

    public async Task<IEnumerable<Treatment>> GetDueAsync()
    {
        return await _context.Treatments
            .Include(t => t.Client)
            .Include(t => t.TreatmentType)
            .Where(t =>
                t.FollowedUpAt == null &&
                t.NextFollowUpDate.Date <= DateTime.UtcNow.Date)
            .OrderBy(t => t.NextFollowUpDate)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Treatment>> GetTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Treatments
            .Include(t => t.Client)
            .Include(t => t.TreatmentType)
            .Where(t =>
                t.FollowedUpAt == null &&
                t.NextFollowUpDate.Date == today)
            .OrderBy(t => t.Client.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Treatment>> GetByClientAsync(Guid clientId)
    {
        return await _context.Treatments
            .Include(t => t.Client)
            .Include(t => t.TreatmentType)
            .Where(t => t.ClientId == clientId)
            .ToListAsync();
    }
    
    public async Task<Treatment?> GetByIdAsync(Guid id)
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
}

