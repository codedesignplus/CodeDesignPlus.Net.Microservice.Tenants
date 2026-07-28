using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;
using CodeDesignPlus.Net.ValueObjects.Location;

namespace CodeDesignPlus.Net.Microservice.Tenants.Default.Test.Helpers;

/// <summary>
/// Valid building blocks for the explicit validation tests. They are hand-written because the
/// reflection-driven attributes of the SDK feed every string with "Test", which violates the
/// invariant of <see cref="TypeDocument"/> (a code of at most three characters).
/// </summary>
public static class Utils
{
    public static readonly TypeDocument TypeDocument = TypeDocument.Create("NIT", "Numero de Identificacion Tributaria");

    public static readonly Currency Currency = Currency.Create(Guid.NewGuid(), "Colombian Peso", "COP", "$", 2, 170);

    public static readonly Location Location = Location.Create(
        Country.Create(Guid.NewGuid(), "Colombia", "CO", "COL", 170, "+57", "America/Bogota", Currency),
        State.Create(Guid.NewGuid(), "Antioquia", "ANT"),
        City.Create(Guid.NewGuid(), "Medellin", "America/Bogota"),
        Locality.Create(Guid.NewGuid(), "El Poblado"),
        Neighborhood.Create(Guid.NewGuid(), "Provenza"),
        "Calle 10 #40-20",
        "050021");

    public static readonly License License = License.Create(
        Guid.NewGuid(),
        "Enterprise",
        SystemClock.Instance.GetCurrentInstant(),
        SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(365)),
        [new ModuleInfo(Guid.NewGuid(), "Accounting")],
        new Dictionary<string, string> { { "plan", "enterprise" } });

    public static readonly Uri Domain = new("https://acme.example.com");

    public const string Name = "Acme";
    public const string NumberDocument = "900123456";
    public const string Phone = "+573001112233";
    public const string Email = "admin@acme.com";
}
