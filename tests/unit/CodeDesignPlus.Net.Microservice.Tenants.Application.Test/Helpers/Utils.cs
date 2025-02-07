using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Helpers;

public static class Utils
{
    public readonly static Currency Currency = Currency.Create(Guid.NewGuid(), "COP", "Colombian Peso", "COP");
    public readonly static Country Country = Country.Create(Guid.NewGuid(), "Colombia", "CO", "America/Bogota", Currency);
    public readonly static State State = State.Create(Guid.NewGuid(), "Bogota", "DC");
    public readonly static City City = City.Create(Guid.NewGuid(), "Bogota", "America/Bogota");
    public readonly static Locality Locality = Locality.Create(Guid.NewGuid(), "Punta Aranda");
    public readonly static Neighborhood Neighborhood = Neighborhood.Create(Guid.NewGuid(), "Galán");
    public readonly static Location Location = Location.Create(Country, State, City, Locality, Neighborhood);
    public readonly static License License = License.Create(Guid.NewGuid(), "License Test", SystemClock.Instance.GetCurrentInstant(), SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(30)), new Dictionary<string, string>{
        { "User", "10" },
        { "Admin", "1" },
        { "Invoice", "1" }
    });
}
