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
    // Del mismo sitio que el TTL del snapshot, para que no se puedan separar. Cuando estuvieron
    // sueltos (job cada 15 min, TTL de 5) la cache quedaba fria dos tercios del tiempo.
    private static readonly TimeSpan Interval = TenantSnapshotCadence.ReconciliationInterval;
    private const int PageSize = 500;

    /// <summary>
    /// El usuario del sistema que firma lo que cambia la reconciliación (regla 64).
    /// </summary>
    public static readonly Guid SystemUserId = Guid.Parse("10000000-0000-0000-0000-000000000001");

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
        var pubsub = scope.ServiceProvider.GetRequiredService<IPubSub>();
        var now = SystemClock.Instance.GetCurrentInstant();

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
                // Un paquete que venció o una licencia que dejó de estar pagada cambian los atributos efectivos sin que llegue
                // ningún evento: aquí se notan, se guardan y se avisa con TenantLicenseUpdated (pendings/739).
                var current = tenant.RefreshEffectiveLicense(now, SystemUserId)
                    ? await RefreshAsync(repository, pubsub, tenant.Id, now, cancellationToken) ?? tenant
                    : tenant;

                await publisher.PublishAsync(current, cancellationToken);
                published++;
            }

            skip += tenants.Count;

            if (skip >= page.TotalCount)
                break;
        }

        logger.LogInformation("Tenant snapshot reconciliation completed: {Published} snapshots republished", published);
    }

    /// <summary>
    /// Vuelve a leer la copropiedad justo antes de guardar, para no pisar con la copia de la página lo que otro proceso
    /// escribió mientras tanto, y publica el cambio de sus atributos efectivos.
    /// </summary>
    private async Task<TenantAggregate?> RefreshAsync(ITenantRepository repository, IPubSub pubsub, Guid tenantId, Instant now, CancellationToken cancellationToken)
    {
        var fresh = await repository.FindAsync<TenantAggregate>(tenantId, cancellationToken);

        if (fresh is null || !fresh.RefreshEffectiveLicense(now, SystemUserId))
        {
            return fresh;
        }

        await repository.UpdateAsync(fresh, cancellationToken);
        await pubsub.PublishAsync(fresh.GetAndClearEvents(), cancellationToken);

        logger.LogInformation("The effective attributes of tenant {TenantId} changed: {Count} keys published.", tenantId, fresh.EffectiveMetadata?.Count ?? 0);

        return fresh;
    }
}
