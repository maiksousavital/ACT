using ACT.Infrastructure.Persistence;
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

            // Cross-company: get all due follow-ups regardless of company
            var due = await db.Treatments
                .Where(t => t.FollowedUpAt == null && t.NextFollowUpDate.Date <= DateTime.UtcNow.Date)
                .CountAsync(ct);

            _logger.LogInformation("Follow-ups due today: {Count}", due);
            // TODO: Push notification logic — group by CompanyId for per-company notifications
        }
    }
}