namespace CodeDesignPlus.Net.Microservice.Tenants.Domain;

public class TenantAggregate(Guid id) : AggregateRootBase(id)
{
    public string Name { get; private set; } = null!;
    public Uri? Domain { get; private set; }
    public License License { get; private set; } = null!;
    public Location Location { get; private set; } = null!;

    private TenantAggregate(Guid id, string name, Uri? domain, License license, Location location,  Guid createdBy): this(id)
    {
        this.Name = name;
        this.Domain = domain;
        this.IsActive = true;
        this.License = license;
        this.Location = location;
        this.CreatedBy = createdBy;
        this.CreatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(TenantCreatedDomainEvent.Create(id, name, domain, License, Location, IsActive));
    }

    public static TenantAggregate Create(Guid id, string name, Uri? domain, License license, Location location,  Guid createdBy)
    {
        DomainGuard.GuidIsEmpty(id, Errors.IdTenantIsInvalid);
        DomainGuard.IsNullOrEmpty(name, Errors.NameTenantIsInvalid);
        DomainGuard.GuidIsEmpty(createdBy, Errors.CreatedByIsInvalid);

        return new TenantAggregate(id, name, domain, license, location, createdBy);
    }

    public void Update(string name, Uri? domain, bool isActive, Guid updatedBy)
    {
        DomainGuard.IsNullOrEmpty(name, Errors.NameTenantIsInvalid);
        DomainGuard.GuidIsEmpty(updatedBy, Errors.CreatedByIsInvalid);

        Name = name;
        Domain = domain;
        IsActive = isActive;
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
