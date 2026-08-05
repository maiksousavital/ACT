using ACT.Application.Services.Interfaces;
using ACT.Infrastructure.Persistence;
using ACT.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace ACT.Infrastructure.Services;

public class FollowUpNotificationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FollowUpNotificationWorker> _logger;

    public FollowUpNotificationWorker(IServiceScopeFactory scopeFactory,
        ILogger<FollowUpNotificationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = DateTime.Today.AddDays(now.Hour >= 8 ? 1 : 0).AddHours(8);
            await Task.Delay(nextRun - now, ct);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var pushSender = scope.ServiceProvider.GetRequiredService<IPushNotificationSender>();
            var subscriptionRepo = scope.ServiceProvider.GetRequiredService<IPushSubscriptionRepository>();

            // Grouped per company — a company's users should only ever hear about their own due
            // follow-ups. No .IgnoreQueryFilters() needed: there's no HttpContext in a background
            // worker, so the tenant query filter already treats this as an unrestricted caller
            // (see HttpContextTenantContext).
            var dueByCompany = await db.Treatments
                .Where(t => t.FollowedUpAt == null && t.NextFollowUpDate.Date <= DateTime.UtcNow.Date)
                .GroupBy(t => t.CompanyId)
                .Select(g => new { CompanyId = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            foreach (var group in dueByCompany)
            {
                var subscriptions = await subscriptionRepo.GetByCompanyIdAsync(group.CompanyId);
                var body = group.Count == 1 ? "1 follow-up needs attention" : $"{group.Count} follow-ups need attention";

                foreach (var subscription in subscriptions)
                {
                    await pushSender.SendAsync(subscription, "Follow-ups due", body, "/follow-ups");
                }
            }

            _logger.LogInformation("Follow-up push run complete: {CompanyCount} companies with due follow-ups", dueByCompany.Count);
        }
    }
}