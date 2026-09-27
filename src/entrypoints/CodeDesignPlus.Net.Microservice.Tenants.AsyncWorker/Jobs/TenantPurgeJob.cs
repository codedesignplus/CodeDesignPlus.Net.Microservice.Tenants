using CodeDesignPlus.Net.Hangfire.Abstractions;
using CodeDesignPlus.Net.Hangfire.Abstractions.Attributes;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.Repositories;
using Hangfire;

namespace CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Jobs;

/// <summary>
/// Purges the deleted tenants whose retention ended: tells every microservice to delete their data, then deletes
/// the tenant itself.
/// </summary>
/// <remarks>
/// <para>
/// The event goes out <b>before</b> the tenant is deleted. The other way round, a failure between both steps would
/// leave the data of the tenant in every microservice with nobody left to ask for its purge. This way the tenant
/// stays until the event is out, and the next run publishes it again: the consumers are idempotent.
/// </para>
/// <para>
/// A restore cannot race with the purge: restoring requires the retention not to have ended, and this job only
/// takes the tenants whose retention ended.
/// </para>
/// </remarks>
[RecurringJobOptions("0 * * * *", jobId: "tenant-purge-job")]
public class TenantPurgeJob(ITenantRepository repository, IPubSub pubsub, ILogger<TenantPurgeJob> logger) : IRecurrentJob
{
    private const int BatchSize = 20;

    /// <inheritdoc/>
    [DisableConcurrentExecution(timeoutInSeconds: 30 * 60)]
    public async Task ExecuteAsync(IJobCancellationToken jobCancellationToken)
    {
        var cancellationToken = jobCancellationToken.ShutdownToken;

        var now = SystemClock.Instance.GetCurrentInstant();

        var tenants = await repository.FindDueForPurgeAsync(now, BatchSize, cancellationToken);

        foreach (var tenant in tenants)
        {
            try
            {
                tenant.Purge(now);

                await pubsub.PublishAsync(tenant.GetAndClearEvents(), cancellationToken);

                await repository.PurgeAsync(tenant.Id, cancellationToken);

                logger.LogInformation("Tenant {TenantId} ({Name}) purged; it was deleted at {DeletedAt}", tenant.Id, tenant.Name, tenant.DeletedAt);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "The purge of tenant {TenantId} failed; it is retried on the next run", tenant.Id);
            }
        }
    }
}
