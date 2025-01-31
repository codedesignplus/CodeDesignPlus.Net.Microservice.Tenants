namespace CodeDesignPlus.Net.Microservice.Tenants.Domain;

public class TenantAggregate(Guid id) : AggregateRoot(id)
{
    public string Name { get; private set; } = string.Empty;
    public Uri? Domain { get; private set; }
    public License License { get; private set; } = License.Create();
    public Location Location { get; private set; } = Location.Create();

    private TenantAggregate(Guid id, string name, Uri? domain, License license, Location location, Guid tenant, Guid createdBy): this(id)
    {
        this.Name = name;
        this.Domain = domain;
        this.Tenant = tenant;
        this.IsActive = true;
        this.CreatedBy = createdBy;
        this.CreatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(TenantCreatedDomainEvent.Create(id, name, domain, License, Location, IsActive));
    }

    public static TenantAggregate Create(Guid id, string name, Uri? domain, License license, Location location, Guid tenant, Guid createdBy)
    {
        DomainGuard.GuidIsEmpty(id, Errors.IdTenantIsInvalid);
        DomainGuard.IsNullOrEmpty(name, Errors.NameTenantIsInvalid);
        DomainGuard.GuidIsEmpty(tenant, Errors.TenantIsInvalid);
        DomainGuard.GuidIsEmpty(createdBy, Errors.CreatedByIsInvalid);

        return new TenantAggregate(id, name, domain, license, location, tenant, createdBy);
    }

    public void Update(string name, Uri? domain, bool isActive, Guid tenant, Guid updatedBy)
    {
        DomainGuard.IsNullOrEmpty(name, Errors.NameTenantIsInvalid);
        DomainGuard.GuidIsEmpty(tenant, Errors.TenantIsInvalid);
        DomainGuard.GuidIsEmpty(updatedBy, Errors.CreatedByIsInvalid);

        Name = name;
        Domain = domain;
        IsActive = isActive;
        Tenant = tenant;
        UpdatedBy = updatedBy;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(TenantUpdatedDomainEvent.Create(Id, Name, Domain, License, Location, IsActive));
    }

    public void UpdateLicense(License license, Guid updatedBy)
    {
        DomainGuard.IsNull(license, Errors.LicenseMetadataIsNull);
        DomainGuard.GuidIsEmpty(updatedBy, Errors.CreatedByIsInvalid);

        License = license;
        UpdatedBy = updatedBy;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(TenantLicenseUpdatedDomainEvent.Create(Id, license));
    }

    public void UpdateLocation(Location location, Guid updatedBy)
    {
        DomainGuard.IsNull(location, Errors.LicenseMetadataIsNull);
        DomainGuard.GuidIsEmpty(updatedBy, Errors.CreatedByIsInvalid);

        Location = location;
        UpdatedBy = updatedBy;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(TenantLocationUpdatedDomainEvent.Create(Id, location));
    }

    public void Delete(Guid updatedBy)
    {
        DomainGuard.GuidIsEmpty(updatedBy, Errors.CreatedByIsInvalid);

        IsActive = false;
        UpdatedBy = updatedBy;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(TenantDeletedDomainEvent.Create(Id, Name, Domain, License, Location, IsActive));
    }
    
}
