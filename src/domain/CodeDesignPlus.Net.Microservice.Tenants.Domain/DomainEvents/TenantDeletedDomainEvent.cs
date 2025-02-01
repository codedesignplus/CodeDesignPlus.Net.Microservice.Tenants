namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;

[EventKey<TenantAggregate>(1, "TenantDeletedDomainEvent")]
public class TenantDeletedDomainEvent(
    Guid aggregateId,
    string name,
    Uri? domain,
    License license,
    Location location,
    bool isActive,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : TenantBaseDomainEvent(aggregateId, name, domain, license, location, isActive, eventId, occurredAt, metadata)
{
    public static TenantDeletedDomainEvent Create(Guid aggregateId, string name, Uri? domain, License license, Location location, bool isActive)
    {
        return new TenantDeletedDomainEvent(aggregateId, name, domain, license, location, isActive);
    }
}
