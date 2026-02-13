namespace CodeDesignPlus.Net.Microservice.Tenants.Domain;

public partial class TenantAggregate(Guid id) : AggregateRootBase(id)
{

    [GeneratedRegex(@"^\+?[1-9]\d{1,14}$", RegexOptions.Compiled)]
    private static partial Regex PhoneRegex();

    public string Name { get; private set; } = null!;
    public TypeDocument TypeDocument { get; private set; } = null!;
    public string NumberDocument { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public Uri? Domain { get; private set; }
    public License License { get; private set; } = null!;
    public Location Location { get; private set; } = null!;

    public static TenantAggregate Create(Guid id, string name, TypeDocument typeDocument, string numberDocument, Uri? domain, string phone, string email, Location location, License license, bool isActive, Guid createdBy)
    {
        DomainGuard.GuidIsEmpty(id, Errors.IdTenantIsInvalid);
        DomainGuard.IsNullOrEmpty(name, Errors.NameTenantIsInvalid);
        DomainGuard.GuidIsEmpty(createdBy, Errors.CreatedByIsInvalid);
        DomainGuard.IsNull(typeDocument, Errors.TypeDocumentIsInvalid);
        DomainGuard.IsNullOrEmpty(phone, Errors.PhoneTenantIsInvalid);
        DomainGuard.IsNullOrEmpty(numberDocument, Errors.NumberDocumentTenantIsInvalid);
        DomainGuard.IsFalse(PhoneRegex().IsMatch(phone), Errors.PhoneTenantIsInvalid);
        DomainGuard.IsNullOrEmpty(email, Errors.EmailTenantIsInvalid);

        var aggregate = new TenantAggregate(id)
        {
            Name = name,
            TypeDocument = typeDocument,
            Phone = phone,
            NumberDocument = numberDocument,
            Domain = domain,
            Email = email,
            IsActive = isActive,
            License = license,
            Location = location,
            CreatedBy = createdBy,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        aggregate.AddEvent(TenantCreatedDomainEvent.Create(id, name, typeDocument, numberDocument, domain, phone, email, location, license, isActive, createdBy));

        return aggregate;
    }

    public void Update(string name, TypeDocument typeDocument, string numberDocument, Uri? domain, string phone, string email, bool isActive, Guid updatedBy)
    {
        DomainGuard.IsNullOrEmpty(name, Errors.NameTenantIsInvalid);
        DomainGuard.GuidIsEmpty(updatedBy, Errors.UpdatedByIsInvalid);
        DomainGuard.IsNull(typeDocument, Errors.TypeDocumentIsInvalid);
        DomainGuard.IsNullOrEmpty(phone, Errors.PhoneTenantIsInvalid);
        DomainGuard.IsNullOrEmpty(numberDocument, Errors.NumberDocumentTenantIsInvalid);
        DomainGuard.IsFalse(PhoneRegex().IsMatch(phone), Errors.PhoneTenantIsInvalid);
        DomainGuard.IsNullOrEmpty(email, Errors.EmailTenantIsInvalid);

        Name = name;
        Domain = domain;
        TypeDocument = typeDocument;
        Phone = phone;
        Email = email;
        NumberDocument = numberDocument;
        IsActive = isActive;
        UpdatedBy = updatedBy;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(TenantUpdatedDomainEvent.Create(Id, Name, TypeDocument, NumberDocument, Domain, Phone, Email, Location, License, IsActive, updatedBy));
    }

    public void UpdateLicense(License license, Guid updatedBy)
    {
        DomainGuard.IsNull(license, Errors.LicenseMetadataIsNull);
        DomainGuard.GuidIsEmpty(updatedBy, Errors.UpdatedByIsInvalid);

        License = license;
        UpdatedBy = updatedBy;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(TenantLicenseUpdatedDomainEvent.Create(Id, license, updatedBy));
    }

    public void UpdateLocation(Location location, Guid updatedBy)
    {
        DomainGuard.IsNull(location, Errors.LicenseMetadataIsNull);
        DomainGuard.GuidIsEmpty(updatedBy, Errors.UpdatedByIsInvalid);

        Location = location;
        UpdatedBy = updatedBy;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(TenantLocationUpdatedDomainEvent.Create(Id, location, updatedBy));
    }

    public void Delete(Guid deletedBy)
    {
        DomainGuard.GuidIsEmpty(deletedBy, Errors.DeletedByIsInvalid);

        this.IsDeleted = true;
        this.IsActive = false;
        this.DeletedAt = SystemClock.Instance.GetCurrentInstant();
        this.DeletedBy = deletedBy;

        AddEvent(TenantDeletedDomainEvent.Create(Id, Name, TypeDocument, NumberDocument, Domain, Phone, Email, Location, License, IsActive, deletedBy));
    }

}
