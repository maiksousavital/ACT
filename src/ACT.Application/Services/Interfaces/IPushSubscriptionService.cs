using ACT.Application.Dtos;

namespace ACT.Application.Services.Interfaces;

public interface IPushSubscriptionService
{
    Task SubscribeAsync(int userId, SubscribePushRequest request);
    Task UnsubscribeAsync(string endpoint);
}
