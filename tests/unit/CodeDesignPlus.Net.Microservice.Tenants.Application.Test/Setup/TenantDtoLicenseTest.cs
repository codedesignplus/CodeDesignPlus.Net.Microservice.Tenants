using CodeDesignPlus.Net.Microservice.Tenants.Application.Setup;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Setup;

/// <summary>
/// The license the screen reads from <c>GET /Tenant/{id}</c> must be the effective one (license plus the packages that
/// count), the same the snapshot carries; the stored license travels apart for the Super Admin form (pendings/739).
/// </summary>
public class TenantDtoLicenseTest
{
    private static readonly Instant Now = SystemClock.Instance.GetCurrentInstant();

    private static TenantAggregate TenantWithBasicPackage()
    {
        var license = License.Create(Guid.NewGuid(), "Élite", Now.Minus(Duration.FromDays(1)), Now.Plus(Duration.FromDays(30)), [], new Dictionary<string, string>
        {
            ["assemblies_number_meetings"] = "12",
            ["assemblies_virtual_enable"] = "false",
        });

        var tenant = TenantAggregate.Create(Guid.NewGuid(), "Conjunto", Utils.TypeDocument, "900123456", null, "3107845123", "admin@example.com", Utils.Location, license, true, Guid.NewGuid());

        tenant.ApplyPackages(
        [
            new TenantPackage(Guid.NewGuid(), Guid.NewGuid(), "Virtual Básico", Now.Minus(Duration.FromMinutes(1)), Now.Plus(Duration.FromDays(365)), null,
            [
                new TenantPackageAttribute("assemblies_number_meetings", "3", "Sum"),
                new TenantPackageAttribute("assemblies_virtual_enable", "true", "Or"),
            ]),
        ], Now, Guid.NewGuid());

        return tenant;
    }

    private static IMapper Mapper()
    {
        MapsterConfigTenant.Configure();

        return new Mapper(TypeAdapterConfig.GlobalSettings);
    }

    [Fact]
    public void Map_TenantWithAPackage_ExposesTheEffectiveLicense()
    {
        // Arrange
        var tenant = TenantWithBasicPackage();

        // Act
        var dto = Mapper().Map<TenantDto>(tenant);

        // Assert
        Assert.Equal(("15", "true"), (dto.License.Metadata["assemblies_number_meetings"], dto.License.Metadata["assemblies_virtual_enable"]));
    }

    [Fact]
    public void Map_TenantWithAPackage_ExposesTheStoredLicenseApart()
    {
        // Arrange
        var tenant = TenantWithBasicPackage();

        // Act
        var dto = Mapper().Map<TenantDto>(tenant);

        // Assert
        Assert.Equal(("12", "false", tenant.License.Id), (dto.BaseLicense!.Metadata["assemblies_number_meetings"], dto.BaseLicense.Metadata["assemblies_virtual_enable"], dto.License.Id));
    }

    [Fact]
    public void Map_TenantWithAPackage_MatchesTheSnapshotFunction()
    {
        // Arrange
        var tenant = TenantWithBasicPackage();

        // Act
        var dto = Mapper().Map<TenantDto>(tenant);

        // Assert
        Assert.Equal(tenant.EffectiveLicense().Metadata, dto.License.Metadata);
    }
}
