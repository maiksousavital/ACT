using ACT.Domain.Entities;

namespace ACT.Application.Services.Interfaces;

public interface IPushNotificationSender
{
    Task SendAsync(PushSubscription subscription, string title, string body, string url);
}
