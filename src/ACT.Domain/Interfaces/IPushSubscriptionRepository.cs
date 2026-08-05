using ACT.Domain.Entities;

namespace ACT.Domain.Interfaces;

public interface IPushSubscriptionRepository
{
    Task<PushSubscription?> GetByEndpointAsync(string endpoint);
    Task<IEnumerable<PushSubscription>> GetByCompanyIdAsync(int companyId);
    Task AddAsync(PushSubscription subscription);
    Task RemoveAsync(PushSubscription subscription);
    Task SaveChangesAsync();
}
