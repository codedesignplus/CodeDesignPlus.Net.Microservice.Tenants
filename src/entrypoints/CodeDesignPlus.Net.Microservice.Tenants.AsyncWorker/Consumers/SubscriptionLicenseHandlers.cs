using CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.DomainEvents;
using CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Dtos;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.Repositories;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ServiceDomain;

namespace CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Consumers;

/// <summary>
/// Lo común de aplicar a la copropiedad una licencia pagada en ms-licenses (pendings/735, 738).
/// </summary>
public static class SubscriptionLicenseSync
{
    /// <summary>
    /// Aplica la licencia, guarda la copropiedad, republica su foto y publica <c>TenantLicenseUpdated</c>.
    /// </summary>
    /// <param name="repository">El repositorio de copropiedades.</param>
    /// <param name="snapshots">Quien publica la foto del tenant.</param>
    /// <param name="pubsub">El bus.</param>
    /// <param name="tenantId">La copropiedad.</param>
    /// <param name="apply">El cambio; devuelve <c>false</c> si ya estaba aplicado.</param>
    /// <param name="logger">El registro.</param>
    /// <param name="token">Token de cancelación.</param>
    /// <returns>La tarea.</returns>
    public static async Task ApplyAsync(ITenantRepository repository, ITenantSnapshotPublisher snapshots, IPubSub pubsub, Guid tenantId, Func<TenantAggregate, bool> apply, ILogger logger, CancellationToken token)
    {
        var tenant = await repository.FindAsync<TenantAggregate>(tenantId, token);

        if (tenant is null)
        {
            // La copropiedad se purgó: no hay a quién aplicarle la licencia.
            logger.LogWarning("Tenant {TenantId} not found: the paid license is not applied.", tenantId);

            return;
        }

        if (!apply(tenant))
        {
            logger.LogInformation("The license of tenant {TenantId} was already applied.", tenantId);

            return;
        }

        await repository.UpdateAsync(tenant, token);

        // La foto lleva la fecha de fin que leen los micros por el SDK; ms-organization y los demás reciben la licencia nueva
        // por TenantLicenseUpdated.
        await snapshots.PublishAsync(tenant, token);

        await pubsub.PublishAsync(tenant.GetAndClearEvents(), token);
    }

    /// <summary>
    /// Los paquetes del tenant a partir de los que vendió ms-licenses (pendings/739).
    /// </summary>
    /// <param name="packages">Los paquetes del evento.</param>
    /// <returns>Los paquetes del tenant.</returns>
    public static List<Domain.ValueObjects.TenantPackage> ToPackages(IEnumerable<PackageTenant>? packages) => [.. (packages ?? []).Select(x => x.ToPackage())];

    /// <summary>
    /// La licencia del tenant a partir de la que vendió ms-licenses.
    /// </summary>
    /// <param name="license">La licencia del evento.</param>
    /// <returns>La licencia del tenant.</returns>
    public static Domain.ValueObjects.License ToLicense(LicenseTenant license) => Domain.ValueObjects.License.Create(
        license.Id,
        license.Name,
        license.StartDate,
        license.EndDate,
        [.. (license.Modules ?? []).Select(x => new Domain.ValueObjects.ModuleInfo(x.Id, x.Name))],
        license.Metadata ?? []);
}

/// <summary>
/// Alarga la vigencia de la licencia de la copropiedad al pagarse su renovación (pendings/735).
/// </summary>
[QueueName<TenantAggregate>("RenewTenantLicenseHandler")]
public class RenewTenantLicenseHandler(ITenantRepository repository, ITenantSnapshotPublisher snapshots, IPubSub pubsub, ILogger<RenewTenantLicenseHandler> logger) : IEventHandler<SubscriptionRenewedDomainEvent>
{
    /// <inheritdoc/>
    public Task HandleAsync(SubscriptionRenewedDomainEvent data, CancellationToken token)
    {
        var license = SubscriptionLicenseSync.ToLicense(data.License);
        var packages = SubscriptionLicenseSync.ToPackages(data.Packages);

        // La licencia y los paquetes que se renuevan con ella (pendings/739); los dos son idempotentes.
        return SubscriptionLicenseSync.ApplyAsync(repository, snapshots, pubsub, data.TenantDetail.Id, x => x.RenewLicense(license, data.BuyerId) | x.ApplyPackages(packages, SystemClock.Instance.GetCurrentInstant(), data.BuyerId), logger, token);
    }
}

/// <summary>
/// Aplica a la copropiedad la licencia a la que se cambió, con sus módulos y atributos, al pagarse el cambio (pendings/738).
/// </summary>
[QueueName<TenantAggregate>("ChangeTenantLicenseHandler")]
public class ChangeTenantLicenseHandler(ITenantRepository repository, ITenantSnapshotPublisher snapshots, IPubSub pubsub, ILogger<ChangeTenantLicenseHandler> logger) : IEventHandler<SubscriptionChangedDomainEvent>
{
    /// <inheritdoc/>
    public Task HandleAsync(SubscriptionChangedDomainEvent data, CancellationToken token)
    {
        var license = SubscriptionLicenseSync.ToLicense(data.License);
        var packages = SubscriptionLicenseSync.ToPackages(data.Packages);

        // Con licencia anual llegan los paquetes recotizados; con mensual, ninguno y los vigentes siguen (pendings/739).
        return SubscriptionLicenseSync.ApplyAsync(repository, snapshots, pubsub, data.TenantDetail.Id, x => x.ChangeLicense(license, data.BuyerId) | x.ApplyPackages(packages, SystemClock.Instance.GetCurrentInstant(), data.BuyerId), logger, token);
    }
}

/// <summary>
/// Suma a la copropiedad el paquete comprado o ampliado (pendings/739): sus atributos cuentan en los efectivos desde que
/// empieza y, si amplía otro, reemplaza al anterior.
/// </summary>
[QueueName<TenantAggregate>("ApplyTenantPackagesHandler")]
public class ApplyTenantPackagesHandler(ITenantRepository repository, ITenantSnapshotPublisher snapshots, IPubSub pubsub, ILogger<ApplyTenantPackagesHandler> logger) : IEventHandler<SubscriptionPackagesPurchasedDomainEvent>
{
    /// <inheritdoc/>
    public Task HandleAsync(SubscriptionPackagesPurchasedDomainEvent data, CancellationToken token)
    {
        var packages = SubscriptionLicenseSync.ToPackages(data.Packages);

        return SubscriptionLicenseSync.ApplyAsync(repository, snapshots, pubsub, data.TenantDetail.Id, x => x.ApplyPackages(packages, SystemClock.Instance.GetCurrentInstant(), data.BuyerId), logger, token);
    }
}
