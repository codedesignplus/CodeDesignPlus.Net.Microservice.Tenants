using System.Text.Json.Serialization;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

public sealed partial class Country
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string Timezone { get; private set; }
    public Currency Currency { get; private set; }

    [JsonConstructor]
    private Country(Guid id, string name, string code, string timezone, Currency currency)
    {
        DomainGuard.GuidIsEmpty(id, Errors.CountryIdIsEmpty);
        DomainGuard.IsNullOrEmpty(name, Errors.CountryNameIsEmpty);
        DomainGuard.IsNullOrEmpty(code, Errors.CountryCodeIsEmpty);
        DomainGuard.IsNullOrEmpty(timezone, Errors.CountryTimezoneIsEmpty);

        this.Id = id;
        this.Name = name;
        this.Code = code;
        this.Timezone = timezone;
        this.Currency = currency;
    }

    public static Country Create(Guid id, string name, string code, string timezone, Currency currency)
    {
        return new Country(id, name, code, timezone, currency);
    }
}
