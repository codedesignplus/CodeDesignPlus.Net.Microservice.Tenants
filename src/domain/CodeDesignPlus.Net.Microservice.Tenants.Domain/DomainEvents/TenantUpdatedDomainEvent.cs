namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;

[EventKey<TenantAggregate>(1, "TenantUpdatedDomainEvent")]
public class TenantUpdatedDomainEvent(
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
    Guid updatedBy,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : TenantBaseDomainEvent(aggregateId, name, typeDocument, numberDocument, domain, phone, email, license, location, isActive, eventId, occurredAt, metadata)
{
    public Guid UpdatedBy { get; } = updatedBy;

    public static TenantUpdatedDomainEvent Create(Guid aggregateId, string name, TypeDocument typeDocument, string numberDocument, Uri? domain, string phone, string email, Location location, License license, bool isActive, Guid updatedBy)
    {
        return new TenantUpdatedDomainEvent(aggregateId, name, typeDocument, numberDocument, domain, phone, email, location, license, isActive, updatedBy);
    }
}
