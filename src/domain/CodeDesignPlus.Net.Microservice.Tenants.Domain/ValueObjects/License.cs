using System.Text.Json.Serialization;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ServiceDomain;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

public sealed class License
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Instant StartDate { get; private set; }
    public Instant EndDate { get; private set; }
    public List<ModuleInfo> Modules { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; }

    [JsonConstructor]
    public License(Guid Id, string Name, Instant StartDate, Instant EndDate, List<ModuleInfo> Modules, Dictionary<string, string> Metadata)
    {
        DomainGuard.GuidIsEmpty(Id, Errors.LicenseIdIsEmpty);
        DomainGuard.IsNullOrEmpty(Name, Errors.LicenseNameIsEmpty);
        DomainGuard.IsGreaterThan(StartDate, EndDate, Errors.LicenseStartDateGreaterThanEndDate);
        DomainGuard.IsNull(Metadata, Errors.LicenseMetadataIsNull);

        // La misma regla que el catálogo de ms-licenses: una licencia que se pudo vender se puede aplicar (pendings/738).
        DomainGuard.IsFalse(LicenseName.IsValid(Name), Errors.LicenseNameIsInvalid.With(FieldLength.Name));

        this.Id = Id;
        this.Name = Name;
        this.StartDate = StartDate;
        this.EndDate = EndDate;
        this.Modules = Modules ?? [];
        this.Metadata = Metadata;
    }

    public static License Create(Guid Id, string Name, Instant StartDate, Instant EndDate, List<ModuleInfo> Modules, Dictionary<string, string> Metadata)
    {
        return new License(Id, Name, StartDate, EndDate, Modules, Metadata);
    }
}

