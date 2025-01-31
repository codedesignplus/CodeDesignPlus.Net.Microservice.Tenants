namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;

public abstract class TenantBaseDomainEvent(
    Guid aggregateId,
    string name,
    Uri? domain,
    License license,
    Location location,
    bool isActive,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public string Name { get; private set; } = name;
    public Uri? Domain { get; private set; } = domain;
    public License License { get; private set; } = license;
    public Location Location { get; private set; } = location;
    public bool IsActive { get; private set; } = isActive;
}
