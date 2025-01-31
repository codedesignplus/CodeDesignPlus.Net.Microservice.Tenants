using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

public sealed partial class Location
{
    [GeneratedRegex(@"^0x[0-9]{32}$")]
    private static partial Regex Regex();

    public Country Country { get; private set; }
    public State State { get; private set; }
    public City City { get; private set; }
    public Locality Locality { get; private set; }
    public Neighborhood Neighborhood { get; private set; }

    private Location()
    {
        this.Country = Country.Create();
        this.State = State.Create();
        this.City = City.Create();
        this.Locality = Locality.Create();
        this.Neighborhood = Neighborhood.Create();
    }

    [JsonConstructor]
    private Location(Country country, State state, City city, Locality locality, Neighborhood neighborhood)
    {
        this.Country = country;
        this.State = state;
        this.City = city;
        this.Locality = locality;
        this.Neighborhood = neighborhood;
    }

    public static Location Create(Country country, State state, City city, Locality locality, Neighborhood neighborhood)
    {
        return new Location(country, state, city, locality, neighborhood);
    }

    public static Location Create()
    {
        return new Location();
    }
}
