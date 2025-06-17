namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;

[EventKey<TenantAggregate>(1, "TenantDeletedDomainEvent")]
public class TenantDeletedDomainEvent(
    Guid aggregateId,
    string name,
    TypeDocument typeDocument,
    string numberDocument,
    Uri? domain,
    string phone,
    string email,
    Location location,
    License license,
    bool isActive,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : TenantBaseDomainEvent(aggregateId, name, typeDocument, numberDocument, domain, phone, email, license, location, isActive, eventId, occurredAt, metadata)
{
    public static TenantDeletedDomainEvent Create(Guid aggregateId, string name, TypeDocument typeDocument, string numberDocument, Uri? domain, string phone, string email, Location location, License license, bool isActive)
    {
        return new TenantDeletedDomainEvent(aggregateId, name, typeDocument, numberDocument, domain, phone, email, location, license, isActive);
    }
}
