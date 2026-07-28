using CodeDesignPlus.Net.Cache.Abstractions;
using CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Services;
using CodeDesignPlus.Net.Security.Abstractions;
using Microsoft.Extensions.Logging;
using VO = CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using Models = CodeDesignPlus.Net.Security.Abstractions.Models;

namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Test.Services;

public class TenantSnapshotPublisherTest
{
    private readonly Mock<ICacheManager> cacheManagerMock = new();

    [Fact]
    public async Task PublishAsync_ActiveTenant_WritesSnapshotAndIndexesIt()
    {
        // Arrange
        var tenant = BuildTenant(isActive: true);
        var publisher = BuildPublisher();

        // Act
        await publisher.PublishAsync(tenant);

        // Assert
        cacheManagerMock.Verify(c => c.SetGlobalAsync(TenantCacheKeys.Snapshot(tenant.Id), It.IsAny<Models.Tenant>(), null), Times.Once);
        cacheManagerMock.Verify(c => c.AddToGlobalSetAsync(TenantCacheKeys.ActiveTenants, tenant.Id.ToString()), Times.Once);
        cacheManagerMock.Verify(c => c.RemoveFromGlobalSetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PublishAsync_InactiveTenant_KeepsSnapshotButDropsItFromTheIndex()
    {
        // Arrange
        var tenant = BuildTenant(isActive: false);
        var publisher = BuildPublisher();

        // Act
        await publisher.PublishAsync(tenant);

        // Assert
        cacheManagerMock.Verify(c => c.SetGlobalAsync(TenantCacheKeys.Snapshot(tenant.Id), It.IsAny<Models.Tenant>(), null), Times.Once);
        cacheManagerMock.Verify(c => c.RemoveFromGlobalSetAsync(TenantCacheKeys.ActiveTenants, tenant.Id.ToString()), Times.Once);
        cacheManagerMock.Verify(c => c.AddToGlobalSetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PublishAsync_CarriesLocationCurrencyAndModules()
    {
        // Arrange
        var tenant = BuildTenant(isActive: true);
        Models.Tenant published = null!;

        cacheManagerMock
            .Setup(c => c.SetGlobalAsync(It.IsAny<string>(), It.IsAny<Models.Tenant>(), null))
            .Callback<string, Models.Tenant, TimeSpan?>((_, snapshot, _) => published = snapshot);

        var publisher = BuildPublisher();

        // Act
        await publisher.PublishAsync(tenant);

        // Assert
        Assert.Equal(tenant.Name, published.Name);
        Assert.Equal(170, published.Location.Country.Code);
        Assert.Equal("COP", published.Location.Country.Currency.Code);
        Assert.Single(published.License.Modules);
        Assert.Equal("Accounting", published.License.Modules[0].Name);
    }

    [Fact]
    public async Task PublishAsync_CacheIsDown_DoesNotThrow()
    {
        // Arrange: la base ya quedo consistente, asi que el comando no puede fallar por el cache.
        var tenant = BuildTenant(isActive: true);
        cacheManagerMock
            .Setup(c => c.SetGlobalAsync(It.IsAny<string>(), It.IsAny<Models.Tenant>(), null))
            .ThrowsAsync(new InvalidOperationException("redis is down"));

        var publisher = BuildPublisher();

        // Act & Assert
        await publisher.PublishAsync(tenant);
    }

    [Fact]
    public async Task RemoveAsync_DropsSnapshotAndIndexEntry()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var publisher = BuildPublisher();

        // Act
        await publisher.RemoveAsync(tenantId);

        // Assert
        cacheManagerMock.Verify(c => c.RemoveGlobalAsync(TenantCacheKeys.Snapshot(tenantId)), Times.Once);
        cacheManagerMock.Verify(c => c.RemoveFromGlobalSetAsync(TenantCacheKeys.ActiveTenants, tenantId.ToString()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_CacheIsDown_DoesNotThrow()
    {
        // Arrange
        cacheManagerMock.Setup(c => c.RemoveGlobalAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("redis is down"));

        var publisher = BuildPublisher();

        // Act & Assert
        await publisher.RemoveAsync(Guid.NewGuid());
    }

    private TenantSnapshotPublisher BuildPublisher() =>
        new(cacheManagerMock.Object, Mock.Of<ILogger<TenantSnapshotPublisher>>());

    private static TenantAggregate BuildTenant(bool isActive)
    {
        var currency = CodeDesignPlus.Net.ValueObjects.Financial.Currency.Create(Guid.NewGuid(), "Peso", "COP", "$", 2, 170);

        var location = Location.Create(
            Country.Create(Guid.NewGuid(), "Colombia", "CO", "COL", 170, "+57", "America/Bogota", currency),
            State.Create(Guid.NewGuid(), "Antioquia", "ANT"),
            City.Create(Guid.NewGuid(), "Medellin", "America/Bogota"),
            Locality.Create(Guid.NewGuid(), "El Poblado"),
            Neighborhood.Create(Guid.NewGuid(), "Provenza"),
            "Calle 10 #40-20",
            "050021");

        var license = VO.License.Create(
            Guid.NewGuid(),
            "Enterprise",
            SystemClock.Instance.GetCurrentInstant(),
            SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(365)),
            [new VO.ModuleInfo(Guid.NewGuid(), "Accounting")],
            new Dictionary<string, string> { { "plan", "enterprise" } });

        return TenantAggregate.Create(
            Guid.NewGuid(),
            "Acme",
            VO.TypeDocument.Create("NIT", "Numero de Identificacion Tributaria"),
            "900123456",
            new Uri("https://acme.example.com"),
            "+573001112233",
            "admin@acme.com",
            location,
            license,
            isActive,
            Guid.NewGuid());
    }
}
