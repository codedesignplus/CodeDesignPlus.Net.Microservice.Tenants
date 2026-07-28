using CodeDesignPlus.Net.Microservice.Tenants.Domain;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.Repositories;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ServiceDomain;
using C = CodeDesignPlus.Net.Core.Abstractions.Models.Criteria;

namespace CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Services;

/// <summary>
/// Republishes every tenant snapshot on a fixed interval. The snapshots are kept up to date by the
/// command handlers on each mutation; this exists as a safety net for what write-through cannot
/// cover — a flushed cache, a write that failed, a freshly provisioned environment.
/// </summary>
/// <param name="serviceProvider">The root service provider.</param>
/// <param name="logger">The logger service.</param>
public class TenantSnapshotReconciliationService(IServiceProvider serviceProvider, ILogger<TenantSnapshotReconciliationService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);
    private const int PageSize = 500;

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        do
        {
            try
            {
                await ReconcileAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "The tenant snapshot reconciliation failed; retrying on the next interval");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ReconcileAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
        var publisher = scope.ServiceProvider.GetRequiredService<ITenantSnapshotPublisher>();

        var skip = 0;
        var published = 0;

        while (true)
        {
            var criteria = new C.Criteria { Limit = PageSize, Skip = skip };

            var page = await repository.MatchingAsync<TenantAggregate>(criteria, cancellationToken);

            var tenants = page.Data.ToList();

            if (tenants.Count == 0)
                break;

            foreach (var tenant in tenants)
            {
                await publisher.PublishAsync(tenant, cancellationToken);
                published++;
            }

            skip += tenants.Count;

            if (skip >= page.TotalCount)
                break;
        }

        logger.LogInformation("Tenant snapshot reconciliation completed: {Published} snapshots republished", published);
    }
}
