using CodeDesignPlus.Net.Hangfire.Abstractions;
using CodeDesignPlus.Net.Hangfire.Abstractions.Attributes;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.Repositories;
using Hangfire;

namespace CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Jobs;

/// <summary>
/// Purga las copropiedades eliminadas cuyo plazo venció: avisa a cada micro de que borre sus datos y después borra
/// la copropiedad.
/// </summary>
/// <remarks>
/// <para>
/// El evento sale <b>antes</b> de borrar la copropiedad. Al revés, un fallo entre los dos pasos dejaría sus datos en
/// cada micro sin nadie que pidiera purgarlos. Así la copropiedad se queda hasta que el evento sale, y la siguiente
/// pasada lo vuelve a publicar: los consumidores son idempotentes.
/// </para>
/// <para>
/// Una restauración no puede cruzarse con la purga: restaurar exige que el plazo no haya vencido, y este job solo
/// toma las copropiedades cuyo plazo venció.
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
