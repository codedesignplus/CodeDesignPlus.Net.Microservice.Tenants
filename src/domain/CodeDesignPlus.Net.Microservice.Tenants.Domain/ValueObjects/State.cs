using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

public sealed partial class State
{
    [GeneratedRegex(@"^0x[0-9]{32}$")]
    private static partial Regex Regex();

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }

    private State()
    {
        this.Id = Guid.Empty;
        this.Name = string.Empty;
        this.Code = string.Empty;
    }

    [JsonConstructor]
    private State(Guid id, string name, string code)
    {
        DomainGuard.IsNullOrEmpty(name, Errors.StateNameIsEmpty);
        DomainGuard.GuidIsEmpty(id, Errors.StateIdIsEmpty);
        DomainGuard.IsNullOrEmpty(code, Errors.StateCodeIsEmpty);

        this.Id = id;
        this.Name = name;
        this.Code = code;
    }

    public static State Create(Guid id, string name, string code)
    {
        return new State(id, name, code);
    }

    public static State Create()
    {
        return new State();
    }
}
