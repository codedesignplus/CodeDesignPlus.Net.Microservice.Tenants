namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;

public abstract class TenantBaseDomainEvent(
    Guid aggregateId,
    string name,
    TypeDocument typeDocument,
    string numberDocument,
    Uri? domain,
    string phone,
    string email,
    License license,
    Location location,
    bool isActive,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public string Name { get; private set; } = name;
    public TypeDocument TypeDocument { get; private set; } = typeDocument;
    public string NumberDocument { get; private set; } = numberDocument;
    public Uri? Domain { get; private set; } = domain;
    public string Phone { get; private set; } = phone;
    public string Email { get; private set; } = email;
    public License License { get; private set; } = license;
    public Location Location { get; private set; } = location;
    public bool IsActive { get; private set; } = isActive;
}
