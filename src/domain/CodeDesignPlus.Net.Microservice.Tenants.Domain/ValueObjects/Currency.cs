using System.Text.Json.Serialization;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

public sealed partial class Currency
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string Symbol { get; private set; }

    [JsonConstructor]
    private Currency(Guid id, string name, string code, string symbol)
    {
        DomainGuard.GuidIsEmpty(id, Errors.CurrencyIdIsEmpty);
        DomainGuard.IsNullOrEmpty(name, Errors.CurrencyNameIsEmpty);
        DomainGuard.IsNullOrEmpty(code, Errors.CurrencyCodeIsEmpty);
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
}