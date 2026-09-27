namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;

/// <summary>
/// Venció el plazo de una copropiedad eliminada: cada micro borra lo que guarda de ella.
/// </summary>
/// <remarks>
/// Puede llegar más de una vez —el job de purga lo publica antes de borrar la copropiedad, y reintenta mientras
/// siga ahí—, así que todo consumidor tiene que ser idempotente.
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
