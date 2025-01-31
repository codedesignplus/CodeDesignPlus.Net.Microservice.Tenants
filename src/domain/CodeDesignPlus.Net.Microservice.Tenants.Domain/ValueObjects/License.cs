using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

public sealed partial class License
{
    [GeneratedRegex(@"^0x[0-9]{32}$")]
    private static partial Regex Regex();

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Instant StartDate { get; private set; }
    public Instant EndDate { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; }

    public License()
    {
        this.Id = Guid.Empty;
        this.Name = string.Empty;
        this.StartDate = Instant.MinValue;
        this.EndDate = Instant.MinValue;
        this.Metadata = [];
    }

    [JsonConstructor]
    private License(Guid Id, string Name, Instant StartDate, Instant EndDate, Dictionary<string, string> Metadata)
    {        
        DomainGuard.GuidIsEmpty(Id, Errors.LicenseIdIsEmpty);
        DomainGuard.IsNullOrEmpty(Name, Errors.LicenseNameIsEmpty);
        DomainGuard.IsGreaterThan(StartDate, EndDate, Errors.LicenseStartDateIsGreaterThanEndDate);
        DomainGuard.IsLessThan(EndDate, StartDate, Errors.LicenseEndDateIsLessThanStartDate);
        DomainGuard.IsNull(Metadata, Errors.LicenseMetadataIsNull);

        this.Id = Id;
        this.Name = Name;
        this.StartDate = StartDate;
        this.EndDate = EndDate;
        this.Metadata = Metadata;
    }

    public static License Create(Guid Id, string Name, Instant StartDate, Instant EndDate, Dictionary<string, string> Metadata)
    {
        return new License(Id, Name, StartDate, EndDate, Metadata);
    }

    public static License Create()
    {
        return new License();
    }
}
