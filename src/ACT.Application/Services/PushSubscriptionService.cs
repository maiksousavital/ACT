using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Domain.Interfaces;

namespace ACT.Application.Services;

public class PushSubscriptionService : IPushSubscriptionService
{
    private readonly IPushSubscriptionRepository _repo;

    public PushSubscriptionService(IPushSubscriptionRepository repo)
    {
        _repo = repo;
    }

    public async Task SubscribeAsync(int userId, SubscribePushRequest request)
    {
        // A device re-subscribing (e.g. after clearing site data, or the browser rotating keys)
        // reuses the same Endpoint — upsert rather than accumulate stale duplicates.
        var existing = await _repo.GetByEndpointAsync(request.Endpoint);
        if (existing != null)
        {
            existing.UserId = userId;
            existing.P256dh = request.P256dh;
            existing.Auth = request.Auth;
            await _repo.SaveChangesAsync();
            return;
        }

        await _repo.AddAsync(new PushSubscription
        {
            UserId = userId,
            Endpoint = request.Endpoint,
            P256dh = request.P256dh,
            Auth = request.Auth
        });
        await _repo.SaveChangesAsync();
    }

    public async Task UnsubscribeAsync(string endpoint)
    {
        var existing = await _repo.GetByEndpointAsync(endpoint);
        if (existing == null) return;

        await _repo.RemoveAsync(existing);
        await _repo.SaveChangesAsync();
    }
}
