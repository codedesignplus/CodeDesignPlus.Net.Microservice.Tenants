namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ServiceDomain;

/// <summary>
/// Ties together how long a tenant snapshot lives in the shared cache and how often it is
/// republished. Both numbers belong to whoever writes the snapshot, which is why they live here and
/// not next to <see cref="CodeDesignPlus.Net.Security.Abstractions.TenantCacheKeys"/>: the services
/// that only read it have no business knowing either.
/// </summary>
public static class TenantSnapshotCadence
{
    /// <summary>
    /// How often the reconciliation job republishes every snapshot.
    /// </summary>
    public static readonly TimeSpan ReconciliationInterval = TimeSpan.FromMinutes(5);

    /// <summary>
    /// How long a published snapshot survives in the shared cache.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Derived from <see cref="ReconciliationInterval"/> so the two cannot drift apart. They already
    /// did once, and it cost an outage: the snapshot inherited the global
    /// <c>RedisCache:Expiration</c> of five minutes while the job ran every fifteen, so the cache
    /// sat empty ten minutes out of every fifteen. Every request landing in that window fell back to
    /// ms-tenants over gRPC.
    /// </para>
    /// <para>
    /// The factor of three is the margin. Two consecutive reconciliation runs can fail —a slow
    /// Mongo, a pod restart— before anyone notices; a tighter margin would turn a single missed run
    /// into an outage, which is the trap the previous setup fell into.
    /// </para>
    /// </remarks>
    public static readonly TimeSpan SnapshotTtl = ReconciliationInterval * 3;
}
