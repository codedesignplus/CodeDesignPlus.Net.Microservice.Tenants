using System;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;

[EventKey<TenantAggregate>(1, "TenantLicenseUpdatedDomainEvent")]
public class TenantLicenseUpdatedDomainEvent(
    Guid aggregateId,
    License license,
    Guid updatedBy,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public License License { get; } = license;
    public Guid UpdatedBy { get; } = updatedBy;

    public static TenantLicenseUpdatedDomainEvent Create(Guid aggregateId, License license, Guid updatedBy)
    {
        return new TenantLicenseUpdatedDomainEvent(aggregateId, license, updatedBy);
    }
}
