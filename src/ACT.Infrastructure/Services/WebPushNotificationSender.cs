using System.Net;
using System.Text.Json;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebPush;
using DomainPushSubscription = ACT.Domain.Entities.PushSubscription;

namespace ACT.Infrastructure.Services;

/// <summary>
/// Sends Web Push notifications directly to the browser vendor's push service (FCM, APNs, Mozilla
/// autopush — whichever issued the subscription) using VAPID auth. No third-party account or
/// per-message cost, unlike SMS/email providers — this is a free, standard part of the web platform.
/// If VapidSettings isn't configured, logs instead of sending, same dev/staging fallback as
/// SmtpEmailSender.
/// </summary>
public class WebPushNotificationSender : IPushNotificationSender
{
    private readonly IConfiguration _config;
    private readonly IPushSubscriptionRepository _subscriptions;
    private readonly ILogger<WebPushNotificationSender> _logger;

    public WebPushNotificationSender(
        IConfiguration config,
        IPushSubscriptionRepository subscriptions,
        ILogger<WebPushNotificationSender> logger)
    {
        _config = config;
        _subscriptions = subscriptions;
        _logger = logger;
    }

    public async Task SendAsync(DomainPushSubscription subscription, string title, string body, string url)
    {
        var publicKey = _config["VapidSettings:PublicKey"];
        var privateKey = _config["VapidSettings:PrivateKey"];
        var subject = _config["VapidSettings:Subject"];

        if (string.IsNullOrWhiteSpace(publicKey) || string.IsNullOrWhiteSpace(privateKey) || string.IsNullOrWhiteSpace(subject))
        {
            _logger.LogWarning(
                "VapidSettings is not configured — logging push instead of sending. To: user {UserId} Title: {Title} Body: {Body}",
                subscription.UserId, title, body);
            return;
        }

        var webPushSubscription = new PushSubscription(subscription.Endpoint, subscription.P256dh, subscription.Auth);
        var vapidDetails = new VapidDetails(subject, publicKey, privateKey);
        var payload = JsonSerializer.Serialize(new { title, body, url });

        var client = new WebPushClient();
        try
        {
            await client.SendNotificationAsync(webPushSubscription, payload, vapidDetails);
        }
        catch (WebPushException ex) when (ex.StatusCode is HttpStatusCode.Gone or HttpStatusCode.NotFound)
        {
            // The browser/OS revoked this subscription (uninstalled, permission withdrawn, etc.) —
            // clean it up so future runs stop retrying a dead endpoint.
            _logger.LogInformation("Push subscription for user {UserId} is no longer valid — removing it.", subscription.UserId);
            await _subscriptions.RemoveAsync(subscription);
            await _subscriptions.SaveChangesAsync();
        }
    }
}
