using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

public sealed partial class Locality
{
    [GeneratedRegex(@"^0x[0-9]{32}$")]
    private static partial Regex Regex();

    public Guid Id { get; private set; }
    public string Name { get; private set; }

    private Locality()
    {
        this.Id = Guid.Empty;
        this.Name = string.Empty;
    }

    [JsonConstructor]
    private Locality(Guid id, string name)
    {
        DomainGuard.IsNullOrEmpty(name, Errors.LocalityNameIsEmpty);
        DomainGuard.GuidIsEmpty(id, Errors.LocalityIdIsEmpty);

        this.Id = id;
        this.Name = name;
    }

    public static Locality Create(Guid id, string name)
    {
        return new Locality(id, name);
    }

    public static Locality Create()
    {
        return new Locality();
    }
}
