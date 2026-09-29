using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Tenants.gRpc;
using NodaTime;
using VO = CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.gRpc.Test.Core.Mapster;

public class MapsterConfigTest
{
    [Fact]
    public void Configure_ShouldMapProperties_Success()
    {
        // Arrange
        CodeDesignPlus.Net.Microservice.Tenants.gRpc.Core.Mapster.MapsterConfig.Configure();
        var config = TypeAdapterConfig.GlobalSettings;

        // Act
        var mapper = new Mapper(config);

        // Assert
        Assert.NotNull(mapper);
        Assert.NotEmpty(config.RuleMap);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CreateTenantRequest_WithoutLocalityAndNeighborhood_MapsThemAsNull(bool asEmptyMessages)
    {
        // La mayoría de los municipios no tiene localidades ni barrios (pendings/130): la compra los manda ausentes.
        CodeDesignPlus.Net.Microservice.Tenants.gRpc.Core.Mapster.MapsterConfig.Configure();

        var request = new CreateTenantRequest
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Conjunto Chía",
            TypeDocument = new TypeDocument { Code = "NIT", Name = "Número de Identificación Tributaria" },
            NumbreDocument = "901234567",
            Phone = "+573000000001",
            Email = "admin@conjunto.com",
            Location = new Location
            {
                Country = new Country
                {
                    Id = Guid.NewGuid().ToString(), Name = "Colombia", Alpha2 = "CO", Alpha3 = "COL", Code = 170,
                    PhoneCode = "+57", Timezone = "America/Bogota",
                    Currency = new Currency { Id = Guid.NewGuid().ToString(), Code = "COP", Name = "Peso colombiano", Symbol = "$", DecimalDigits = 2, NumericCode = 170 }
                },
                State = new State { Id = Guid.NewGuid().ToString(), Name = "Cundinamarca", Code = "CUN" },
                City = new City { Id = Guid.NewGuid().ToString(), Name = "Chía", Timezone = "America/Bogota" },
                Locality = asEmptyMessages ? new Locality() : null,
                Neighborhood = asEmptyMessages ? new Neighborhood() : null,
                Address = "Calle 10 # 5-20",
                PostalCode = "250001"
            },
            License = new License
            {
                Id = Guid.NewGuid().ToString(), Name = "Élite",
                StartDate = "2026-09-29T00:00:00Z", EndDate = "2026-10-29T00:00:00Z"
            },
            IsActive = true
        };

        // Act
        var command = request.Adapt<CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant.CreateTenantCommand>();

        // Assert
        Assert.Equal("Chía", command.Location.City.Name);
        Assert.Null(command.Location.Locality);
        Assert.Null(command.Location.Neighborhood);
    }

    [Fact]
    public void TenantDto_To_GetTenantResponse_MapsLicenseModules()
    {
        // Arrange
        CodeDesignPlus.Net.Microservice.Tenants.gRpc.Core.Mapster.MapsterConfig.Configure();
        var mapper = new Mapper(TypeAdapterConfig.GlobalSettings);

        var accounting = new VO.ModuleInfo(Guid.NewGuid(), "Accounting");
        var invoicing = new VO.ModuleInfo(Guid.NewGuid(), "Invoicing");
        var dto = BuildDto([accounting, invoicing]);

        // Act
        var response = mapper.Map<GetTenantResponse>(dto);

        // Assert
        Assert.Equal(2, response.License.Modules.Count);
        Assert.Contains(response.License.Modules, m => m.Id == accounting.Id.ToString() && m.Name == "Accounting");
        Assert.Contains(response.License.Modules, m => m.Id == invoicing.Id.ToString() && m.Name == "Invoicing");
    }

    [Fact]
    public void TenantDto_To_GetTenantResponse_WithoutModules_LeavesCollectionEmpty()
    {
        // Arrange
        CodeDesignPlus.Net.Microservice.Tenants.gRpc.Core.Mapster.MapsterConfig.Configure();
        var mapper = new Mapper(TypeAdapterConfig.GlobalSettings);

        var dto = BuildDto([]);

        // Act
        var response = mapper.Map<GetTenantResponse>(dto);

        // Assert
        Assert.Empty(response.License.Modules);
    }

    private static TenantDto BuildDto(List<VO.ModuleInfo> modules)
    {
        var currency = CodeDesignPlus.Net.ValueObjects.Financial.Currency.Create(Guid.NewGuid(), "Peso", "COP", "$", 2, 170);

        var location = CodeDesignPlus.Net.ValueObjects.Location.Location.Create(
            CodeDesignPlus.Net.ValueObjects.Location.Country.Create(Guid.NewGuid(), "Colombia", "CO", "COL", 170, "+57", "America/Bogota", currency),
            CodeDesignPlus.Net.ValueObjects.Location.State.Create(Guid.NewGuid(), "Antioquia", "ANT"),
            CodeDesignPlus.Net.ValueObjects.Location.City.Create(Guid.NewGuid(), "Medellin", "America/Bogota"),
            CodeDesignPlus.Net.ValueObjects.Location.Locality.Create(Guid.NewGuid(), "El Poblado"),
            CodeDesignPlus.Net.ValueObjects.Location.Neighborhood.Create(Guid.NewGuid(), "Provenza"),
            "Calle 10 #40-20",
            "050021");

        return new TenantDto
        {
            Id = Guid.NewGuid(),
            Name = "Acme",
            TypeDocument = VO.TypeDocument.Create("NIT", "Numero de Identificacion Tributaria"),
            NumberDocument = "900123456",
            Phone = "+573001112233",
            Email = "admin@acme.com",
            Location = location,
            License = VO.License.Create(
                Guid.NewGuid(),
                "Enterprise",
                SystemClock.Instance.GetCurrentInstant(),
                SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(365)),
                modules,
                []),
            IsActive = true
        };
    }
}
