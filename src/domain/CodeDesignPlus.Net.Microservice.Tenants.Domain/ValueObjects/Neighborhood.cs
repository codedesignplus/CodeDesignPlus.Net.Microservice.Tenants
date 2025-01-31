using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

public sealed partial class Neighborhood
{
    [GeneratedRegex(@"^0x[0-9]{32}$")]
    private static partial Regex Regex();

    public Guid Id { get; private set; }
    public string Name { get; private set; }

    private Neighborhood()
    {
        this.Id = Guid.Empty;
        this.Name = string.Empty;
    }

    [JsonConstructor]
    private Neighborhood(Guid id, string name)
    {
        DomainGuard.IsNullOrEmpty(name, Errors.NeighborhoodNameIsEmpty);
        DomainGuard.GuidIsEmpty(id, Errors.NeighborhoodIdIsEmpty);

        this.Id = id;
        this.Name = name;
    }

    public static Neighborhood Create(Guid id, string name)
    {
        return new Neighborhood(id, name);
    }

    public static Neighborhood Create()
    {
        return new Neighborhood();
    }
}
