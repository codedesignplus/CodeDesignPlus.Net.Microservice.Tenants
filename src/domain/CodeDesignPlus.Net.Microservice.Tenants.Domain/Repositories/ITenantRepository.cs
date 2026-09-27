namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.Repositories;

/// <summary>
/// The tenants store.
/// </summary>
/// <remarks>
/// The SDK leaves out every document with <c>IsDeleted</c> in <c>FindAsync</c>, <c>MatchingAsync</c> and
/// <c>UpdateAsync</c>. A deleted tenant is still there until it is purged, so the methods that work with deleted
/// tenants go to the collection directly.
/// </remarks>
public interface ITenantRepository : IRepositoryBase
{
    Task<bool> ExistsByDocumentAsync(string typeDocumentCode, string numberDocument, CancellationToken cancellationToken);

    /// <summary>
    /// Finds a deleted tenant that has not been purged yet.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deleted tenant, or <c>null</c> if it does not exist or is not deleted.</returns>
    Task<TenantAggregate?> FindDeletedAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets every deleted tenant that has not been purged yet, the soonest to be purged first.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deleted tenants.</returns>
    Task<List<TenantAggregate>> GetDeletedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the deleted tenants whose retention ended.
    /// </summary>
    /// <param name="now">The current instant.</param>
    /// <param name="limit">The maximum number of tenants to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenants to purge, the oldest first.</returns>
    Task<List<TenantAggregate>> FindDueForPurgeAsync(Instant now, int limit, CancellationToken cancellationToken);

    /// <summary>
    /// Saves a restored tenant, only if it is still deleted.
    /// </summary>
    /// <param name="tenant">The restored tenant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if it was saved; <c>false</c> if it was purged or restored meanwhile.</returns>
    Task<bool> RestoreAsync(TenantAggregate tenant, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the document of a deleted tenant, once every microservice was told to purge its data.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if it was deleted; <c>false</c> if it was restored or purged meanwhile.</returns>
    Task<bool> PurgeAsync(Guid id, CancellationToken cancellationToken);
}
