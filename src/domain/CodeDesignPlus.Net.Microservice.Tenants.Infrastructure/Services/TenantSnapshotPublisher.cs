using CodeDesignPlus.Net.Cache.Abstractions;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ServiceDomain;
using CodeDesignPlus.Net.Security.Abstractions;
using Models = CodeDesignPlus.Net.Security.Abstractions.Models;

namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Services;

/// <summary>
/// Publishes the tenant snapshot to the shared cache and evicts the copy that the queries of ms-tenants keep of it.
/// </summary>
/// <param name="cacheManager">The cache manager.</param>
/// <param name="logger">The logger service.</param>
public class TenantSnapshotPublisher(ICacheManager cacheManager, ILogger<TenantSnapshotPublisher> logger) : ITenantSnapshotPublisher
{
    /// <inheritdoc/>
    public async Task PublishAsync(TenantAggregate tenant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        await EvictQueriesAsync(tenant.Id);

        try
        {
            // Con TTL explicito y no el global de `RedisCache:Expiration`: ese lo comparte todo el
            // mundo para otras cosas y quien lo baje no tiene por que saber que deja sin tenant a la
            // plataforma entera. El de aqui va atado al job de reconciliacion.
            await cacheManager.SetGlobalAsync(TenantCacheKeys.Snapshot(tenant.Id), TenantSnapshotMapper.Map(tenant), TenantSnapshotCadence.SnapshotTtl);

            if (tenant.IsActive && !tenant.IsDeleted)
                await cacheManager.AddToGlobalSetAsync(TenantCacheKeys.ActiveTenants, tenant.Id.ToString());
            else
                await cacheManager.RemoveFromGlobalSetAsync(TenantCacheKeys.ActiveTenants, tenant.Id.ToString());

            logger.LogDebug("Snapshot of tenant {TenantId} published", tenant.Id);
        }
        catch (Exception exception)
        {
            // La base de datos ya quedo consistente; el snapshot es estado derivado y el job de
            // reconciliacion lo repone. No se propaga para no tumbar la mutacion del tenant.
            logger.LogError(exception, "The snapshot of tenant {TenantId} could not be published", tenant.Id);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        await EvictQueriesAsync(tenantId);

        try
        {
            await cacheManager.RemoveGlobalAsync(TenantCacheKeys.Snapshot(tenantId));
            await cacheManager.RemoveFromGlobalSetAsync(TenantCacheKeys.ActiveTenants, tenantId.ToString());

            logger.LogDebug("Snapshot of tenant {TenantId} removed", tenantId);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "The snapshot of tenant {TenantId} could not be removed", tenantId);
        }
    }

    /// <summary>
    /// Evicts the <c>TenantDto</c> that <c>GET /Tenant/{id}</c> keeps in the cache, so the next read comes from the database.
    /// </summary>
    /// <remarks>
    /// Every write of the tenant ends here (create, update, delete, restore, the license and package consumers and the
    /// reconciliation job), so no path can leave the copy stale for the whole <c>RedisCache:Expiration</c> (pendings/739).
    /// Goes in its own <c>try</c>: a failed snapshot must not keep the stale copy, nor the other way round.
    /// </remarks>
    /// <param name="tenantId">The tenant identifier.</param>
    private async Task EvictQueriesAsync(Guid tenantId)
    {
        try
        {
            await cacheManager.RemoveAsync(TenantQueryCacheKeys.ById(tenantId));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "The cached copy of tenant {TenantId} could not be evicted", tenantId);
        }
    }
}
