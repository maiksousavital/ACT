using ACT.Domain.Entities;
using ACT.Domain.Interfaces;
using ACT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ACT.Infrastructure.Repositories;

public class PushSubscriptionRepository : IPushSubscriptionRepository
{
    private readonly AppDbContext _context;

    public PushSubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PushSubscription?> GetByEndpointAsync(string endpoint)
    {
        return await _context.PushSubscriptions
            .FirstOrDefaultAsync(p => p.Endpoint == endpoint);
    }

    public async Task<IEnumerable<PushSubscription>> GetByCompanyIdAsync(int companyId)
    {
        return await _context.PushSubscriptions
            .Where(p => p.User.CompanyId == companyId)
            .ToListAsync();
    }

    public async Task AddAsync(PushSubscription subscription)
    {
        await _context.PushSubscriptions.AddAsync(subscription);
    }

    public Task RemoveAsync(PushSubscription subscription)
    {
        _context.PushSubscriptions.Remove(subscription);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
