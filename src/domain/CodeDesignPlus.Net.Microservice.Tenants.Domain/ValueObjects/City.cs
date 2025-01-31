using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

public sealed partial class City
{
    [GeneratedRegex(@"^0x[0-9]{32}$")]
    private static partial Regex Regex();

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Timezone { get; private set; }

    private City()
    {
        this.Id = Guid.Empty;
        this.Name = string.Empty;
        this.Timezone = string.Empty;
    }

    [JsonConstructor]
    private City(Guid id, string name, string timezone)
    {
        DomainGuard.IsNullOrEmpty(name, Errors.CityNameIsEmpty);
        DomainGuard.GuidIsEmpty(id, Errors.CityIdIsEmpty);
        DomainGuard.IsNullOrEmpty(timezone, Errors.CityTimezoneIsEmpty);

        this.Id = id;
        this.Name = name;
        this.Timezone = timezone;
    }

    public static City Create(Guid id, string name, string timezone)
    {
        return new City(id, name, timezone);
    }

    public static City Create()
    {
        return new City();
    }
}
