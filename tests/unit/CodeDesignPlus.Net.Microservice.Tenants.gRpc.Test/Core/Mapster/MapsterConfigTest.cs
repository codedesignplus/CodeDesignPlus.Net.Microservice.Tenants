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
