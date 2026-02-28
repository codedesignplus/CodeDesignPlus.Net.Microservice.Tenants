namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;

[EventKey<TenantAggregate>(1, "TenantLocationUpdatedDomainEvent")]
public class TenantLocationUpdatedDomainEvent(
    Guid aggregateId,
    Location location,
    Guid updatedBy,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Location Location { get; } = location;
    public Guid UpdatedBy { get; } = updatedBy;

    public static TenantLocationUpdatedDomainEvent Create(Guid aggregateId, Location location, Guid updatedBy)
    {
        return new TenantLocationUpdatedDomainEvent(aggregateId, location, updatedBy);
    }
}
