using CodeDesignPlus.Net.Microservice.Tenants.Domain.Repositories;
using CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using VO = CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Test.Services;

/// <summary>
/// Cubre que ms-tenants resuelva el snapshot de una copropiedad <b>desde su base</b>, y no por la red.
/// </summary>
/// <remarks>
/// El cliente gRPC de tenants apunta a ms-tenants, asi que en ms-tenants apuntaba a si mismo. Con la cache
/// fria, una peticion con <c>X-Tenant</c> disparaba su propio middleware, no encontraba el snapshot y se
/// llamaba a si misma para pedirselo; esa llamada llevaba otra vez la cabecera. Cada salto escribia su aviso:
/// 795.416 lineas en 22 minutos. Pendiente 137.
/// <para>
/// Lo que se fija aqui es que el dueno del dato lo lea de donde lo guarda.
/// </para>
/// </remarks>
public class TenantSnapshotLocalFallbackTest
{
    private readonly Mock<ITenantRepository> repositoryMock = new();

    [Fact]
    public async Task GetAsync_TenantExiste_LoResuelveDesdeLaBase()
    {
        var tenant = BuildTenant();

        repositoryMock
            .Setup(x => x.FindAsync<TenantAggregate>(tenant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var snapshot = await BuildFallback().GetAsync(tenant.Id);

        Assert.NotNull(snapshot);
        Assert.Equal(tenant.Id, snapshot.Id);
        Assert.Equal(tenant.Name, snapshot.Name);
    }

    [Fact]
    public async Task GetAsync_NoHayTenant_DevuelveNuloYNoInventa()
    {
        var id = Guid.NewGuid();

        repositoryMock
            .Setup(x => x.FindAsync<TenantAggregate>(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantAggregate)null!);

        Assert.Null(await BuildFallback().GetAsync(id));
    }

    [Fact]
    public async Task GetAsync_DevuelveLoMismoQuePublicaElSnapshot()
    {
        // Las dos caras tienen que producir exactamente el mismo snapshot: el publicador lo escribe en la
        // cache y este lo reconstruye desde la base. Si divergieran, una copropiedad se comportaria distinto
        // segun por donde se resolviera, y eso no lo delata ninguna prueba de las dos por separado.
        var tenant = BuildTenant();

        repositoryMock
            .Setup(x => x.FindAsync<TenantAggregate>(tenant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var porLaBase = await BuildFallback().GetAsync(tenant.Id);
        var elQueSePublica = TenantSnapshotMapper.Map(tenant);

        Assert.Equal(elQueSePublica.Id, porLaBase!.Id);
        Assert.Equal(elQueSePublica.Name, porLaBase.Name);
        Assert.Equal(elQueSePublica.License.Id, porLaBase.License.Id);
        Assert.Equal(elQueSePublica.License.Modules.Count, porLaBase.License.Modules.Count);
        Assert.Equal(elQueSePublica.Location.Country.Code, porLaBase.Location.Country.Code);
    }

    private TenantSnapshotLocalFallback BuildFallback() =>
        new(repositoryMock.Object, Mock.Of<ILogger<TenantSnapshotLocalFallback>>());

    private static TenantAggregate BuildTenant()
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
            true,
            Guid.NewGuid());
    }
}
