namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;

/// <summary>
/// The retention of a deleted tenant ended: every microservice deletes what it keeps of the tenant.
/// </summary>
/// <remarks>
/// It can arrive more than once — the purge job publishes it before deleting the tenant, and retries while the
/// tenant is still there — so every consumer must be idempotent.
/// </remarks>
[EventKey<TenantAggregate>(1, "TenantPurgedDomainEvent")]
public class TenantPurgedDomainEvent(
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
    public static TenantPurgedDomainEvent Create(Guid aggregateId, string name, TypeDocument typeDocument, string numberDocument, Uri? domain, string phone, string email, Location location, License license, bool isActive)
    {
        return new TenantPurgedDomainEvent(aggregateId, name, typeDocument, numberDocument, domain, phone, email, location, license, isActive);
    }
}
