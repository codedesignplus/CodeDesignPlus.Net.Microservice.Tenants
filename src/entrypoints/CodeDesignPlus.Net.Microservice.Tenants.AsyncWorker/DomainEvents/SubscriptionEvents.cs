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
}
