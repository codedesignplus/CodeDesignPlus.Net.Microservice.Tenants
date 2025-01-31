using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

public sealed partial class Currency
{
    [GeneratedRegex(@"^0x[0-9]{32}$")]
    private static partial Regex Regex();

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string Symbol { get; private set; }

    private Currency()
    {
        this.Id = Guid.Empty;
        this.Name = string.Empty;
        this.Code = string.Empty;
        this.Symbol = string.Empty;
    }

    [JsonConstructor]
    private Currency(Guid id, string name, string code, string symbol)
    {
        DomainGuard.GuidIsEmpty(id, Errors.CurrencyIdIsEmpty);
        DomainGuard.IsNullOrEmpty(name, Errors.CurrencyNameIsEmpty);
        DomainGuard.IsNullOrEmpty(code, Errors.CurrencyTimezoneIsEmpty);
        DomainGuard.IsNullOrEmpty(symbol, Errors.CurrencySymbolIsEmpty);

        this.Id = id;
        this.Name = name;
        this.Code = code;
        this.Symbol = symbol;
    }

    public static Currency Create(Guid id, string name, string code, string symbol)
    {
        return new Currency(id, name, code, symbol);
    }

    public static Currency Create()
    {
        return new Currency();
    }
}