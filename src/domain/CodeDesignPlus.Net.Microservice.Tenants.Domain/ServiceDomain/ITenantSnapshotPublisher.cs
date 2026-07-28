namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ServiceDomain;

/// <summary>
/// Publishes the tenant snapshot to the shared cache, where every service of the platform reads it
/// through the Security SDK. ms-tenants owns the whole snapshot, so no other service needs to be
/// queried to build it.
/// </summary>
/// <remarks>
/// Implementations must not throw: the source of truth is the database, and the snapshot is derived
/// state that the reconciliation job repairs. Failing a tenant mutation because a cache write failed
/// would be worse than a short window of staleness.
/// </remarks>
public interface ITenantSnapshotPublisher
{
    /// <summary>
    /// Publishes the snapshot of a tenant and keeps the active tenants index in sync with its state.
    /// </summary>
    /// <param name="tenant">The tenant to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task PublishAsync(TenantAggregate tenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the snapshot of a tenant and drops it from the active tenants index.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
