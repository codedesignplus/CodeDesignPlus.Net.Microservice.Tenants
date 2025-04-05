
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using NodaTime.Serialization.SystemTextJson;

namespace CodeDesignPlus.Net.Microservice.Tenants.Rest.Test.Controllers;

public class TenantControllerTest : ServerBase<Program>, IClassFixture<Server<Program>>
{
    private readonly System.Text.Json.JsonSerializerOptions options = new System.Text.Json.JsonSerializerOptions()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
    }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

    private readonly Currency currency;
    private readonly Country country;
    private readonly State state;
    private readonly City city;
    private readonly Locality locality;
    private readonly Neighborhood neighborhood;
    private readonly Location location;
    private readonly License license;

    public TenantControllerTest(Server<Program> server) : base(server)
    {

        this.currency = Currency.Create(Guid.NewGuid(), "COP", "Colombian Peso", "COP");
        this.country = Country.Create(Guid.NewGuid(), "Colombia", 102, "America/Bogota", this.currency);
        this.state = State.Create(Guid.NewGuid(), "Bogota", "DC");
        this.city = City.Create(Guid.NewGuid(), "Bogota", "America/Bogota");
        this.locality = Locality.Create(Guid.NewGuid(), "Punta Aranda");
        this.neighborhood = Neighborhood.Create(Guid.NewGuid(), "Galán");
        this.location = Location.Create(this.country, this.state, this.city, this.locality, this.neighborhood);

        this.license = License.Create(Guid.NewGuid(), "License Test", SystemClock.Instance.GetCurrentInstant(), SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(30)), new Dictionary<string, string>{
            { "User", "10" },
            { "Admin", "1" },
            { "Invoice", "1" }
        });

        server.InMemoryCollection = (x) =>
        {
            x.Add("Vault:Enable", "false");
            x.Add("Vault:Address", "http://localhost:8200");
            x.Add("Vault:Token", "root");
            x.Add("Solution", "CodeDesignPlus");
            x.Add("AppName", "my-test");
            x.Add("RabbitMQ:UserName", "guest");
            x.Add("RabbitMQ:Password", "guest");
            x.Add("Security:ValidAudiences:0", Guid.NewGuid().ToString());
        };
    }

    [Fact]
    public async Task GetTenants_ReturnOk()
    {
        var tenant = await this.CreateTenantAsync();

        var response = await this.RequestAsync("http://localhost/api/Tenant", null, HttpMethod.Get);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        var tenants = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<TenantDto>>(json, this.options);

        Assert.NotNull(tenants);
        Assert.NotEmpty(tenants);
        Assert.Contains(tenants, x => x.Id == tenant.Id);
    }

    [Fact]
    public async Task GetTenantById_ReturnOk()
    {
        var tenantCreated = await this.CreateTenantAsync();

        var response = await this.RequestAsync($"http://localhost/api/Tenant/{tenantCreated.Id}", null, HttpMethod.Get);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        var tenant = System.Text.Json.JsonSerializer.Deserialize<TenantDto>(json, this.options);

        Assert.NotNull(tenant);
        Assert.Equal(tenantCreated.Id, tenant.Id);
        Assert.Equal(tenantCreated.Name, tenant.Name);
        Assert.Equal(tenantCreated.Domain, tenant.Domain);
    }

    [Fact]
    public async Task CreateTenant_ReturnNoContent()
    {
        var data = new CreateTenantDto()
        {
            Id = Guid.NewGuid(),
            Name = "Tenant Test",
            Domain = new Uri("http://localhost"),
            License = this.license,
            Location = this.location
        };

        var json = System.Text.Json.JsonSerializer.Serialize(data, this.options);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await this.RequestAsync("http://localhost/api/Tenant", content, HttpMethod.Post);

        var tenant = await this.GetRecordAsync(data.Id);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        Assert.Equal(data.Id, tenant.Id);
        Assert.Equal(data.Name, tenant.Name);
        Assert.Equal(data.Domain, tenant.Domain);
        Assert.Equal(data.License.Id, tenant.License.Id);
        Assert.Equal(data.License.Name, tenant.License.Name);
        Assert.Equal(data.License.StartDate.ToString(), tenant.License.StartDate.ToString());
        Assert.Equal(data.License.EndDate.ToString(), tenant.License.EndDate.ToString());
        Assert.Equal(data.License.Metadata, tenant.License.Metadata);
        Assert.Equal(data.Location.Country.Id, tenant.Location.Country.Id);
        Assert.Equal(data.Location.Country.Name, tenant.Location.Country.Name);
        Assert.Equal(data.Location.Country.Code, tenant.Location.Country.Code);
        Assert.Equal(data.Location.Country.Timezone, tenant.Location.Country.Timezone);
        Assert.Equal(data.Location.Country.Currency.Id, tenant.Location.Country.Currency.Id);
        Assert.Equal(data.Location.Country.Currency.Code, tenant.Location.Country.Currency.Code);
        Assert.Equal(data.Location.Country.Currency.Name, tenant.Location.Country.Currency.Name);
        Assert.Equal(data.Location.State.Id, tenant.Location.State.Id);
        Assert.Equal(data.Location.State.Name, tenant.Location.State.Name);
        Assert.Equal(data.Location.City.Id, tenant.Location.City.Id);
        Assert.Equal(data.Location.City.Name, tenant.Location.City.Name);
        Assert.Equal(data.Location.City.Timezone, tenant.Location.City.Timezone);
        Assert.Equal(data.Location.Locality.Id, tenant.Location.Locality.Id);
        Assert.Equal(data.Location.Locality.Name, tenant.Location.Locality.Name);
        Assert.Equal(data.Location.Neighborhood.Id, tenant.Location.Neighborhood.Id);
        Assert.Equal(data.Location.Neighborhood.Name, tenant.Location.Neighborhood.Name);
    }

    [Fact]
    public async Task UpdateTenant_ReturnNoContent()
    {
        var tenantCreated = await this.CreateTenantAsync();

        var data = new UpdateTenantDto()
        {
            Id = tenantCreated.Id,
            Name = "Tenant Test Updated",
            Domain = new Uri("http://localhost"),
            License = this.license,
            Location = this.location
        };

        var json = System.Text.Json.JsonSerializer.Serialize(data, this.options);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await this.RequestAsync($"http://localhost/api/Tenant/{tenantCreated.Id}", content, HttpMethod.Put);

        var Tenant = await this.GetRecordAsync(data.Id);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        Assert.Equal(data.Id, Tenant.Id);
        Assert.Equal(data.Name, Tenant.Name);
        Assert.Equal(data.Domain, Tenant.Domain);
        Assert.Equal(data.License.Id, Tenant.License.Id);
        Assert.Equal(data.License.Name, Tenant.License.Name);
        Assert.Equal(data.License.StartDate.ToString(), Tenant.License.StartDate.ToString());
        Assert.Equal(data.License.EndDate.ToString(), Tenant.License.EndDate.ToString());
        Assert.Equal(data.License.Metadata, Tenant.License.Metadata);
        Assert.Equal(data.Location.Country.Id, Tenant.Location.Country.Id);
        Assert.Equal(data.Location.Country.Name, Tenant.Location.Country.Name);
        Assert.Equal(data.Location.Country.Code, Tenant.Location.Country.Code);
        Assert.Equal(data.Location.Country.Timezone, Tenant.Location.Country.Timezone);
        Assert.Equal(data.Location.Country.Currency.Id, Tenant.Location.Country.Currency.Id);
        Assert.Equal(data.Location.Country.Currency.Code, Tenant.Location.Country.Currency.Code);
        Assert.Equal(data.Location.Country.Currency.Name, Tenant.Location.Country.Currency.Name);
        Assert.Equal(data.Location.State.Id, Tenant.Location.State.Id);
        Assert.Equal(data.Location.State.Name, Tenant.Location.State.Name);
        Assert.Equal(data.Location.City.Id, Tenant.Location.City.Id);
        Assert.Equal(data.Location.City.Name, Tenant.Location.City.Name);
        Assert.Equal(data.Location.City.Timezone, Tenant.Location.City.Timezone);
        Assert.Equal(data.Location.Locality.Id, Tenant.Location.Locality.Id);
        Assert.Equal(data.Location.Locality.Name, Tenant.Location.Locality.Name);
        Assert.Equal(data.Location.Neighborhood.Id, Tenant.Location.Neighborhood.Id);
        Assert.Equal(data.Location.Neighborhood.Name, Tenant.Location.Neighborhood.Name);
    }

    [Fact]
    public async Task DeleteTenant_ReturnNoContent()
    {
        var tenantCreated = await this.CreateTenantAsync();

        var response = await this.RequestAsync($"http://localhost/api/Tenant/{tenantCreated.Id}", null, HttpMethod.Delete);

        Assert.NotNull(response);
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<CreateTenantDto> CreateTenantAsync()
    {
        var data = new CreateTenantDto()
        {
            Id = Guid.NewGuid(),
            Name = "Tenant Test",
            Domain = new Uri("http://localhost"),
            License = this.license,
            Location = this.location
        };

        var json = System.Text.Json.JsonSerializer.Serialize(data, this.options);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        await this.RequestAsync("http://localhost/api/Tenant", content, HttpMethod.Post);

        return data;
    }

    private async Task<TenantDto> GetRecordAsync(Guid id)
    {
        var response = await this.RequestAsync($"http://localhost/api/Tenant/{id}", null, HttpMethod.Get);

        var json = await response.Content.ReadAsStringAsync();

        return System.Text.Json.JsonSerializer.Deserialize<TenantDto>(json, this.options)!;
    }

    private async Task<HttpResponseMessage> RequestAsync(string uri, HttpContent? content, HttpMethod method)
    {
        var httpRequestMessage = new HttpRequestMessage()
        {
            RequestUri = new Uri(uri),
            Content = content,
            Method = method
        };
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("TestAuth");

        var response = await Client.SendAsync(httpRequestMessage);

        if (!response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsStringAsync();
            throw new Exception(data);
        }

        return response;
    }

}
