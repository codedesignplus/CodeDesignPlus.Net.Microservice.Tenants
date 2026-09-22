namespace CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.DomainEvents;

[EventKey<TenantAggregate>(1, "TenantProvisioningFailedForOrderDomainEvent")]
public class TenantProvisioningFailedForOrderDomainEvent(
    Guid aggregateId,
    Guid orderId,
    string reason,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Guid OrderId { get; } = orderId;
    public string Reason { get; } = reason;

    public static TenantProvisioningFailedForOrderDomainEvent Create(Guid tenantId, Guid orderId, string reason)
        => new(tenantId, orderId, reason);
}
