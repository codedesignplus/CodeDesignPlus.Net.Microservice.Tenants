namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ServiceDomain;

/// <summary>
/// Keys under which the queries of ms-tenants keep their own copy of a tenant in the cache.
/// </summary>
/// <remarks>
/// Unlike <c>TenantCacheKeys</c> of the Security SDK, these keys are namespaced by business and application name, so only
/// ms-tenants reads them. Every write of the tenant evicts them through <see cref="ITenantSnapshotPublisher"/> (pendings/739).
/// </remarks>
public static class TenantQueryCacheKeys
{
    /// <summary>
    /// Builds the key of the <c>TenantDto</c> served by <c>GET /Tenant/{id}</c>.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <returns>The key of the cached tenant.</returns>
    public static string ById(Guid tenantId) => tenantId.ToString();
}
