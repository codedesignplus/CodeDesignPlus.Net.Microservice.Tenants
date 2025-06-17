namespace CodeDesignPlus.Net.Microservice.Tenants.Domain;

public partial class TenantAggregate(Guid id) : AggregateRootBase(id)
{
    
    [GeneratedRegex(@"^\+?[1-9]\d{1,14}$", RegexOptions.Compiled)]
    private static partial Regex PhoneRegex();

    public string Name { get; private set; } = null!;
    public TypeDocument TypeDocument { get; private set; } = null!;
    public string NumberDocument { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public Uri? Domain { get; private set; }
    public License License { get; private set; } = null!;
    public Location Location { get; private set; } = null!;

    private TenantAggregate(Guid id, string name, TypeDocument typeDocument, string numberDocument, Uri? domain, string phone, Location location, License license, bool isActive, Guid createdBy) : this(id)
    {
        DomainGuard.GuidIsEmpty(id, Errors.IdTenantIsInvalid);
        DomainGuard.IsNullOrEmpty(name, Errors.NameTenantIsInvalid);
        DomainGuard.GuidIsEmpty(createdBy, Errors.CreatedByIsInvalid);
        DomainGuard.IsNull(typeDocument, Errors.TypeDocumentIsInvalid);
        DomainGuard.IsNullOrEmpty(phone, Errors.PhoneTenantIsInvalid);
        DomainGuard.IsNullOrEmpty(numberDocument, Errors.NumberDocumentTenantIsInvalid);
        DomainGuard.IsTrue(PhoneRegex().IsMatch(phone), Errors.PhoneTenantIsInvalid);

        this.Name = name;
        this.TypeDocument = typeDocument;
        this.Phone = phone;
        this.NumberDocument = numberDocument;
        this.Domain = domain;
        this.IsActive = isActive;
        this.License = license;
        this.Location = location;
        this.CreatedBy = createdBy;
        this.CreatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(TenantCreatedDomainEvent.Create(id, name, domain, License, Location, IsActive));
    }

    public static TenantAggregate Create(
        Guid id,
        string name,
        TypeDocument typeDocument,
        string numberDocument,
        Uri? domain,
        string phone,
        Location location,
        License license,
        bool isActive,
        Guid createdBy)
    {
        return new TenantAggregate(id, name, typeDocument, numberDocument, domain, phone, location, license, isActive, createdBy);
    }

    public void Update(string name, TypeDocument typeDocument, string numberDocument, Uri? domain, string phone, bool isActive, Guid updatedBy)
    {
        DomainGuard.IsNullOrEmpty(name, Errors.NameTenantIsInvalid);
        DomainGuard.GuidIsEmpty(updatedBy, Errors.CreatedByIsInvalid);
        DomainGuard.IsNull(typeDocument, Errors.TypeDocumentIsInvalid);
        DomainGuard.IsNullOrEmpty(phone, Errors.PhoneTenantIsInvalid);
        DomainGuard.IsNullOrEmpty(numberDocument, Errors.NumberDocumentTenantIsInvalid);
        DomainGuard.IsTrue(PhoneRegex().IsMatch(phone), Errors.PhoneTenantIsInvalid);

        Name = name;
        Domain = domain;
        TypeDocument = typeDocument;
        Phone = phone;
        NumberDocument = numberDocument;
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
