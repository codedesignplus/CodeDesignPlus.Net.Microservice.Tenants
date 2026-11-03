using CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Dtos;

namespace CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.DomainEvents;

/// <summary>
/// Gemelo de <c>SubscriptionRenewedDomainEvent</c> de ms-licenses: se pagó la renovación de la licencia de una copropiedad
/// (pendings/735). Solo lo que usa este micro; los nombres de los parámetros son los de las propiedades.
/// </summary>
/// <param name="aggregateId">La orden de renovación.</param>
/// <param name="tenantDetail">La copropiedad.</param>
/// <param name="license">La licencia con el periodo renovado.</param>
/// <param name="buyerId">Quien pagó.</param>
/// <param name="eventId">El identificador del evento.</param>
/// <param name="occurredAt">Cuándo ocurrió.</param>
/// <param name="metadata">Los metadatos.</param>
[EventKey("OrderAggregate", 1, "SubscriptionRenewedDomainEvent", "ms-licenses")]
public class SubscriptionRenewedDomainEvent(
    Guid aggregateId,
    Tenant tenantDetail,
    LicenseTenant license,
    Guid buyerId,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    /// <summary>La copropiedad.</summary>
    public Tenant TenantDetail { get; } = tenantDetail;

    /// <summary>La licencia con el periodo renovado.</summary>
    public LicenseTenant License { get; } = license;

    /// <summary>Quien pagó.</summary>
    public Guid BuyerId { get; } = buyerId;

    /// <summary>Los paquetes pagados con su periodo (pendings/739); vacío si la orden no trae.</summary>
    public List<PackageTenant> Packages { get; init; } = [];
}

/// <summary>
/// Gemelo de <c>SubscriptionChangedDomainEvent</c> de ms-licenses: se pagó el cambio de la licencia de una copropiedad por
/// una mayor (pendings/738). Solo lo que usa este micro.
/// </summary>
/// <param name="aggregateId">La orden del cambio.</param>
/// <param name="tenantDetail">La copropiedad.</param>
/// <param name="license">La licencia nueva con su periodo.</param>
/// <param name="previousLicenseId">La licencia que se reemplaza.</param>
/// <param name="buyerId">Quien pagó.</param>
/// <param name="eventId">El identificador del evento.</param>
/// <param name="occurredAt">Cuándo ocurrió.</param>
/// <param name="metadata">Los metadatos.</param>
[EventKey("OrderAggregate", 1, "SubscriptionChangedDomainEvent", "ms-licenses")]
public class SubscriptionChangedDomainEvent(
    Guid aggregateId,
    Tenant tenantDetail,
    LicenseTenant license,
    Guid previousLicenseId,
    Guid buyerId,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    /// <summary>La copropiedad.</summary>
    public Tenant TenantDetail { get; } = tenantDetail;

    /// <summary>La licencia nueva con su periodo.</summary>
    public LicenseTenant License { get; } = license;

    /// <summary>La licencia que se reemplaza.</summary>
    public Guid PreviousLicenseId { get; } = previousLicenseId;

    /// <summary>Quien pagó.</summary>
    public Guid BuyerId { get; } = buyerId;

    /// <summary>Los paquetes pagados con su periodo (pendings/739); vacío si la orden no trae.</summary>
    public List<PackageTenant> Packages { get; init; } = [];
}

/// <summary>
/// Gemelo de <c>SubscriptionPackagesPurchasedDomainEvent</c> de ms-licenses: se pagó la compra o la ampliación de un paquete
/// de una copropiedad que ya existe (pendings/739). Solo lo que usa este micro.
/// </summary>
/// <param name="aggregateId">La orden del paquete.</param>
/// <param name="tenantDetail">La copropiedad.</param>
/// <param name="packages">El paquete con su periodo.</param>
/// <param name="buyerId">Quien pagó.</param>
/// <param name="eventId">El identificador del evento.</param>
/// <param name="occurredAt">Cuándo ocurrió.</param>
/// <param name="metadata">Los metadatos.</param>
[EventKey("OrderAggregate", 1, "SubscriptionPackagesPurchasedDomainEvent", "ms-licenses")]
public class SubscriptionPackagesPurchasedDomainEvent(
    Guid aggregateId,
    Tenant tenantDetail,
    List<PackageTenant> packages,
    Guid buyerId,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    /// <summary>La copropiedad.</summary>
    public Tenant TenantDetail { get; } = tenantDetail;

    /// <summary>El paquete con su periodo.</summary>
    public List<PackageTenant> Packages { get; } = packages ?? [];

    /// <summary>Quien pagó.</summary>
    public Guid BuyerId { get; } = buyerId;
}
