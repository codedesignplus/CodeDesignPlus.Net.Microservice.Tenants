using System.Text.Json.Serialization;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

public sealed partial class City
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Timezone { get; private set; }

    [JsonConstructor]
    public City(Guid id, string name, string timezone)
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
}
