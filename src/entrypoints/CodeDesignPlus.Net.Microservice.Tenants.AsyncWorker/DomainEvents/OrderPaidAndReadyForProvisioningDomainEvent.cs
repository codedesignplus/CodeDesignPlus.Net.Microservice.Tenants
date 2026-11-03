using CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Dtos;

namespace CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.DomainEvents;

[EventKey("OrderAggregate", 1, "OrderPaidAndReadyForProvisioningDomainEvent", "ms-licenses")]
public class OrderPaidAndReadyForProvisioningDomainEvent(
    Guid aggregateId,
    Tenant tenantDetail,
    LicenseTenant license,
    Guid buyerId,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Tenant TenantDetail { get; } = tenantDetail;
    public LicenseTenant License { get; } = license;
    public Guid BuyerId { get; } = buyerId;

    /// <summary>Los paquetes pagados con su periodo (pendings/739); vacío si la orden no trae.</summary>
    public List<PackageTenant> Packages { get; init; } = [];

    public static OrderPaidAndReadyForProvisioningDomainEvent Create(Guid aggregateId, Tenant tenantDetail, LicenseTenant license, Guid buyerId)
    {
        return new OrderPaidAndReadyForProvisioningDomainEvent(aggregateId, tenantDetail, license, buyerId);
    }
}
