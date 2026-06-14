namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;

[EventKey<TenantAggregate>(1, "TenantProvisionedForOrderDomainEvent")]
public class TenantProvisionedForOrderDomainEvent(
    Guid aggregateId,
    Guid orderId,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Guid OrderId { get; } = orderId;

    public static TenantProvisionedForOrderDomainEvent Create(Guid aggregateId, Guid orderId)
        => new(aggregateId, orderId);
}
