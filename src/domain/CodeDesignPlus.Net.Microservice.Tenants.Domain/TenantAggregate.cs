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

    // Los miembros que siguen no van al constructor (regla 31): las copropiedades guardadas antes de los paquetes no los
    // traen y leen nulo (pendings/739).

    /// <summary>
    /// Los paquetes complementarios pagados, con su periodo, tal como los vendió ms-licenses (pendings/739).
    /// </summary>
    public List<TenantPackage>? Packages { get; private set; }

    /// <summary>
    /// Los atributos efectivos que se publicaron por última vez: los de la licencia combinados con los de los paquetes que
    /// contaban. Nulo hasta el primer cálculo; entonces valen los de la licencia.
    /// </summary>
    public Dictionary<string, string>? EffectiveMetadata { get; private set; }

    /// <summary>
    /// La licencia como la ven los demás micros: con los atributos efectivos (pendings/739). Es la que viaja en
    /// <see cref="TenantLicenseUpdatedDomainEvent"/> y en la foto.
    /// </summary>
    /// <returns>La licencia con los atributos efectivos.</returns>
    /// <remarks>
    /// Es una lectura: copia la licencia guardada sin validarla, para que una guardada con datos que hoy se rechazarían no
    /// tumbe la consulta ni la foto (pendings/737).
    /// </remarks>
    public License EffectiveLicense() => License is null
        ? null!
        : new License(License.Id, License.Name, License.StartDate, License.EndDate, License.Modules, new Dictionary<string, string>(EffectiveMetadata ?? License.Metadata));

    /// <summary>
    /// Suma los paquetes pagados: una línea que ya estaba no se repite, la que llega reemplaza a la que cita desde su inicio y
    /// las ya vencidas se descartan. Recalcula los atributos efectivos (pendings/739).
    /// </summary>
    /// <param name="incoming">Los paquetes de la orden pagada.</param>
    /// <param name="now">El instante.</param>
    /// <param name="updatedBy">Quien pagó.</param>
    /// <returns><c>false</c> si ya estaban aplicados: una reentrega no cambia nada.</returns>
    public bool ApplyPackages(IReadOnlyCollection<TenantPackage> incoming, Instant now, Guid updatedBy)
    {
        DomainGuard.IsNull(incoming, Errors.LicenseMetadataIsNull);

        // Se publica la licencia con los paquetes: una guardada con datos inválidos no se vuelve a publicar (pendings/737).
        License?.EnsureValid();

        var current = Packages ?? [];
        var known = current.Select(x => x.LineId).ToHashSet();
        var added = incoming.Where(x => !known.Contains(x.LineId)).ToList();

        if (added.Count == 0)
        {
            return false;
        }

        // Una línea vencida ya no cuenta ni puede volver a contar: se descarta para que el documento no crezca sin fin.
        Packages = [.. current.Concat(added).Where(x => x.EndDate > now)];
        UpdatedBy = updatedBy;
        UpdatedAt = now;

        RefreshEffectiveLicense(now, updatedBy, force: true);

        return true;
    }

    /// <summary>
    /// Recalcula los atributos efectivos y, si cambiaron, publica <see cref="TenantLicenseUpdatedDomainEvent"/>: lo llama
    /// la reconciliación de la foto, para que un paquete que vence o una licencia que deja de estar pagada se noten sin
    /// esperar a otro evento (pendings/739).
    /// </summary>
    /// <param name="now">El instante.</param>
    /// <param name="updatedBy">Quien lo pide (el sistema en la reconciliación).</param>
    /// <param name="force">Publicar aunque no hayan cambiado.</param>
    /// <returns><c>true</c> si cambiaron (o se forzó) y se publicó el evento.</returns>
    public bool RefreshEffectiveLicense(Instant now, Guid updatedBy, bool force = false)
    {
        if (License is null)
        {
            return false;
        }

        var effective = ServiceDomain.EffectiveAttributes.Combine(License.Metadata, Packages ?? [], License.EndDate, now);
        var published = EffectiveMetadata ?? License.Metadata;
        // Sin cálculo previo, lo publicado era la licencia sola: si da lo mismo no hay nada que avisar (al publicar el micro no
        // salen avisos de todas las copropiedades).
        var changed = effective.Count != published.Count || effective.Any(x => !published.TryGetValue(x.Key, out var value) || value != x.Value);

        if (!changed && !force)
        {
            return false;
        }

        EffectiveMetadata = effective;
        UpdatedBy = updatedBy;
        UpdatedAt = now;

        AddEvent(TenantLicenseUpdatedDomainEvent.Create(Id, EffectiveLicense(), updatedBy));

        return true;
    }

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
        typeDocument.EnsureValid();
        DomainGuard.IsNull(license, Errors.LicenseMetadataIsNull);
        license.EnsureValid();

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
        typeDocument.EnsureValid();

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
        // La licencia que llega de una petición se hidrata sin validar: aquí es donde se valida (pendings/737).
        license.EnsureValid();

        License = license;

        // Los demás micros reciben la licencia con los paquetes que cuentan (pendings/739).
        RefreshEffectiveLicense(SystemClock.Instance.GetCurrentInstant(), updatedBy, force: true);
    }

    /// <summary>
    /// Aplica la renovación pagada de la licencia (pendings/735): la vigencia llega hasta el fin del periodo renovado. Si la
    /// licencia ya estaba vencida cuando empieza el periodo nuevo, la vigencia empieza ahí; si no, conserva su inicio.
    /// </summary>
    /// <param name="renewed">La licencia con el periodo renovado, tal como la vendió ms-licenses.</param>
    /// <param name="updatedBy">Quien pagó.</param>
    /// <returns><c>false</c> si ya estaba aplicada: una reentrega del evento no cambia nada.</returns>
    public bool RenewLicense(License renewed, Guid updatedBy)
    {
        DomainGuard.IsNull(renewed, Errors.LicenseMetadataIsNull);

        // Nunca acorta: si llega tarde la renovación de un periodo anterior, la vigencia ya va más allá.
        var end = License is null || renewed.EndDate > License.EndDate ? renewed.EndDate : License.EndDate;
        var start = License is null || License.EndDate < renewed.StartDate ? renewed.StartDate : License.StartDate;

        return ApplyLicense(License.Create(renewed.Id, renewed.Name, start, end, renewed.Modules, renewed.Metadata), updatedBy);
    }

    /// <summary>
    /// Aplica el cambio pagado a otra licencia (pendings/738): la licencia nueva, con sus módulos, sus atributos y su periodo,
    /// reemplaza entera a la anterior.
    /// </summary>
    /// <param name="changed">La licencia nueva con su periodo.</param>
    /// <param name="updatedBy">Quien pagó.</param>
    /// <returns><c>false</c> si ya estaba aplicada.</returns>
    public bool ChangeLicense(License changed, Guid updatedBy)
    {
        DomainGuard.IsNull(changed, Errors.LicenseMetadataIsNull);

        return ApplyLicense(changed, updatedBy);
    }

    private bool ApplyLicense(License license, Guid updatedBy)
    {
        if (License is not null && SameLicense(License, license))
        {
            return false;
        }

        UpdateLicense(license, updatedBy);

        return true;
    }

    private static bool SameLicense(License a, License b) =>
        a.Id == b.Id
        && a.Name == b.Name
        && a.StartDate == b.StartDate
        && a.EndDate == b.EndDate
        && a.Modules.Select(x => x.Id).OrderBy(x => x).SequenceEqual(b.Modules.Select(x => x.Id).OrderBy(x => x))
        && a.Metadata.Count == b.Metadata.Count
        && a.Metadata.All(x => b.Metadata.TryGetValue(x.Key, out var value) && value == x.Value);

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
