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

    /// <summary>
    /// Cuándo una copropiedad eliminada deja de poder restaurarse y se purgan sus datos; <c>null</c> si no está eliminada.
    /// </summary>
    public Instant? PurgeAfter { get; private set; }

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

    /// <summary>
    /// Da de baja la copropiedad en la plataforma, conservando todos sus datos hasta <see cref="PurgeAfter"/>.
    /// </summary>
    /// <remarks>
    /// Eliminar es dar de baja, no purgar: la copropiedad deja de poder usarse en el acto y se puede restaurar hasta
    /// que vence el plazo. Solo entonces <see cref="Purge"/> avisa a cada micro de que borre lo que guarda de ella.
    /// </remarks>
    /// <param name="deletedBy">Quien la elimina.</param>
    /// <param name="retention">Cuánto tiempo se puede restaurar antes de purgar sus datos.</param>
    public void Delete(Guid deletedBy, Duration retention)
    {
        DomainGuard.GuidIsEmpty(deletedBy, Errors.DeletedByIsInvalid);
        DomainGuard.IsTrue(retention <= Duration.Zero, Errors.RetentionIsInvalid);

        var now = SystemClock.Instance.GetCurrentInstant();

        this.IsDeleted = true;
        this.IsActive = false;
        this.DeletedAt = now;
        this.DeletedBy = deletedBy;
        this.PurgeAfter = now + retention;

        AddEvent(TenantDeletedDomainEvent.Create(Id, Name, TypeDocument, NumberDocument, Domain, Phone, Email, Location, License, IsActive, deletedBy, this.PurgeAfter.Value));
    }

    /// <summary>
    /// Devuelve una copropiedad eliminada mientras no haya vencido su plazo.
    /// </summary>
    /// <param name="restoredBy">Quien la restaura.</param>
    /// <param name="now">El instante actual, que se compara con <see cref="PurgeAfter"/>.</param>
    public void Restore(Guid restoredBy, Instant now)
    {
        DomainGuard.GuidIsEmpty(restoredBy, Errors.UpdatedByIsInvalid);
        DomainGuard.IsFalse(IsDeleted, Errors.TenantIsNotDeleted);
        DomainGuard.IsTrue(PurgeAfter is null || now >= PurgeAfter.Value, Errors.RestoreWindowExpired);

        this.IsDeleted = false;
        this.IsActive = true;
        this.DeletedAt = null;
        this.DeletedBy = null;
        this.PurgeAfter = null;
        this.UpdatedBy = restoredBy;
        this.UpdatedAt = now;

        AddEvent(TenantRestoredDomainEvent.Create(Id, Name, TypeDocument, NumberDocument, Domain, Phone, Email, Location, License, IsActive, restoredBy));
    }

    /// <summary>
    /// Anuncia que venció el plazo de una copropiedad eliminada y que cada micro debe borrar sus datos.
    /// </summary>
    /// <param name="now">El instante actual, que se compara con <see cref="PurgeAfter"/>.</param>
    public void Purge(Instant now)
    {
        DomainGuard.IsFalse(IsDeleted, Errors.TenantIsNotDeleted);
        DomainGuard.IsTrue(PurgeAfter is null || now < PurgeAfter.Value, Errors.TenantIsNotDueForPurge);

        AddEvent(TenantPurgedDomainEvent.Create(Id, Name, TypeDocument, NumberDocument, Domain, Phone, Email, Location, License, IsActive));
    }
}
