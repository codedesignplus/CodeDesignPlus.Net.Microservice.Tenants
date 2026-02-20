namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;

[EventKey<TenantAggregate>(1, "TenantCreatedDomainEvent", autoCreate: false)]
public class TenantCreatedDomainEvent(
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
    Guid createdBy,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : TenantBaseDomainEvent(aggregateId, name, typeDocument, numberDocument, domain, phone, email, license, location, isActive, eventId, occurredAt, metadata)
{
    public Guid CreatedBy { get; } = createdBy;
    public static TenantCreatedDomainEvent Create(Guid aggregateId, string name, TypeDocument typeDocument, string numberDocument, Uri? domain, string phone, string email, Location location, License license, bool isActive, Guid createdBy)
    {
        return new TenantCreatedDomainEvent(aggregateId, name, typeDocument, numberDocument, domain, phone, email, location, license, isActive, createdBy);
    }
}
