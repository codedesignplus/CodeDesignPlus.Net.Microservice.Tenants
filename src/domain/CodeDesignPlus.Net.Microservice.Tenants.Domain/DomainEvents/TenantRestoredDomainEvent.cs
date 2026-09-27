namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;

/// <summary>
/// Se restauró una copropiedad eliminada antes de que venciera su plazo.
/// </summary>
[EventKey<TenantAggregate>(1, "TenantRestoredDomainEvent")]
public class TenantRestoredDomainEvent(
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
    Guid restoredBy,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : TenantBaseDomainEvent(aggregateId, name, typeDocument, numberDocument, domain, phone, email, license, location, isActive, eventId, occurredAt, metadata)
{
    public Guid RestoredBy { get; } = restoredBy;

    public static TenantRestoredDomainEvent Create(Guid aggregateId, string name, TypeDocument typeDocument, string numberDocument, Uri? domain, string phone, string email, Location location, License license, bool isActive, Guid restoredBy)
    {
        return new TenantRestoredDomainEvent(aggregateId, name, typeDocument, numberDocument, domain, phone, email, location, license, isActive, restoredBy);
    }
}
