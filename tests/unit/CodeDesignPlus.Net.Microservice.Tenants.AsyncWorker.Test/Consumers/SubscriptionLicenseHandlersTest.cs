using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeDesignPlus.Net.Core.Abstractions;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Consumers;
using CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.DomainEvents;
using CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Dtos;
using CodeDesignPlus.Net.Microservice.Tenants.Domain;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.Repositories;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ServiceDomain;
using CodeDesignPlus.Net.PubSub.Abstractions;
using Xunit;
using TenantDto = CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Dtos.Tenant;

namespace CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Test.Consumers;

/// <summary>
/// La licencia pagada en ms-licenses llega a la copropiedad (pendings/735, 738): se guarda, se republica la foto y sale
/// <c>TenantLicenseUpdated</c>; una reentrega no repite nada.
/// </summary>
public class SubscriptionLicenseHandlersTest
{
    private static readonly Instant Start = Instant.FromUtc(2027, 3, 1, 15, 0);

    private readonly Mock<ITenantRepository> repository = new();
    private readonly Mock<ITenantSnapshotPublisher> snapshots = new();
    private readonly Mock<IPubSub> pubsub = new();
    private readonly TenantAggregate tenant;

    public SubscriptionLicenseHandlersTest()
    {
        tenant = TenantAggregate.Create(Guid.NewGuid(), "Conjunto Prueba", Utils.TypeDocument, "123456789", null, "3107845123", "fake@fake.com", Utils.Location,
            License.Create(Guid.NewGuid(), "Esencial", Start, Start.Plus(Duration.FromDays(31)), [new ModuleInfo(Guid.NewGuid(), "Organización")], new Dictionary<string, string> { ["meetings.perYear"] = "12" }),
            true, Guid.NewGuid());
        tenant.GetAndClearEvents();

        repository.Setup(x => x.FindAsync<TenantAggregate>(tenant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
    }

    private LicenseTenant Sold(Instant start, Instant end, Guid? id = null, string name = "Esencial") => new()
    {
        Id = id ?? tenant.License.Id,
        Name = name,
        StartDate = start,
        EndDate = end,
        Modules = [.. tenant.License.Modules.Select(x => new LicenseModuleDto { Id = x.Id, Name = x.Name })],
        Metadata = new Dictionary<string, string>(tenant.License.Metadata),
    };

    private TenantDto Detail() => new() { Id = tenant.Id, TypeDocument = Utils.TypeDocument, Location = Utils.Location };

    /// <summary>
    /// Una renovación pagada por adelantado alarga el fin y conserva el inicio de la vigencia; la foto se republica y sale
    /// <c>TenantLicenseUpdated</c>.
    /// </summary>
    [Fact]
    public async Task Renewed_PaidInAdvance_ExtendsTheEndAndKeepsTheStart()
    {
        // Arrange
        var end = tenant.License.EndDate;
        var handler = new RenewTenantLicenseHandler(repository.Object, snapshots.Object, pubsub.Object, Mock.Of<ILogger<RenewTenantLicenseHandler>>());

        // Act
        await handler.HandleAsync(new SubscriptionRenewedDomainEvent(Guid.NewGuid(), Detail(), Sold(end, end.Plus(Duration.FromDays(30))), Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.Equal((Start, end.Plus(Duration.FromDays(30))), (tenant.License.StartDate, tenant.License.EndDate));
        snapshots.Verify(x => x.PublishAsync(tenant, It.IsAny<CancellationToken>()), Times.Once);
        pubsub.Verify(x => x.PublishAsync(It.Is<IReadOnlyList<IDomainEvent>>(e => e.OfType<TenantLicenseUpdatedDomainEvent>().Any()), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Renovar una licencia ya vencida empieza la vigencia el día del pago.
    /// </summary>
    [Fact]
    public async Task Renewed_AfterItExpired_StartsTheValidityAgain()
    {
        // Arrange
        var paidAt = tenant.License.EndDate.Plus(Duration.FromDays(5));
        var handler = new RenewTenantLicenseHandler(repository.Object, snapshots.Object, pubsub.Object, Mock.Of<ILogger<RenewTenantLicenseHandler>>());

        // Act
        await handler.HandleAsync(new SubscriptionRenewedDomainEvent(Guid.NewGuid(), Detail(), Sold(paidAt, paidAt.Plus(Duration.FromDays(30))), Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.Equal(paidAt, tenant.License.StartDate);
    }

    /// <summary>
    /// La reentrega del mismo evento no guarda ni republica nada.
    /// </summary>
    [Fact]
    public async Task Renewed_TheSameEventTwice_AppliesItOnce()
    {
        // Arrange
        var end = tenant.License.EndDate;
        var @event = new SubscriptionRenewedDomainEvent(Guid.NewGuid(), Detail(), Sold(end, end.Plus(Duration.FromDays(30))), Guid.NewGuid());
        var handler = new RenewTenantLicenseHandler(repository.Object, snapshots.Object, pubsub.Object, Mock.Of<ILogger<RenewTenantLicenseHandler>>());

        // Act
        await handler.HandleAsync(@event, CancellationToken.None);
        await handler.HandleAsync(@event, CancellationToken.None);

        // Assert
        repository.Verify(x => x.UpdateAsync(tenant, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// El cambio reemplaza la licencia entera: sus módulos y atributos (antes el cambio manual borraba los atributos) y el
    /// periodo que empieza el día del pago.
    /// </summary>
    [Fact]
    public async Task Changed_ReplacesTheLicenseWithItsModulesAttributesAndPeriod()
    {
        // Arrange
        var paidAt = Start.Plus(Duration.FromDays(10));
        var sold = Sold(paidAt, paidAt.Plus(Duration.FromDays(30)), Guid.NewGuid(), "Estándar");
        sold.Modules.Add(new LicenseModuleDto { Id = Guid.NewGuid(), Name = "Parqueaderos" });
        sold.Metadata["meetings.perYear"] = "24";
        var handler = new ChangeTenantLicenseHandler(repository.Object, snapshots.Object, pubsub.Object, Mock.Of<ILogger<ChangeTenantLicenseHandler>>());

        // Act
        await handler.HandleAsync(new SubscriptionChangedDomainEvent(Guid.NewGuid(), Detail(), sold, tenant.License.Id, Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.Equal((sold.Id, paidAt, 2, "24"), (tenant.License.Id, tenant.License.StartDate, tenant.License.Modules.Count, tenant.License.Metadata["meetings.perYear"]));
    }

    private static PackageTenant Package(Guid? lineId = null) => new()
    {
        LineId = lineId ?? Guid.NewGuid(),
        PackageId = Guid.NewGuid(),
        Name = "Virtual Básico",
        StartDate = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromMinutes(1)),
        EndDate = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(300)),
        Attributes = [new PackageTenantAttribute { Key = "assemblies_number_meetings", Value = "3", Combination = "Sum" }],
    };

    /// <summary>
    /// Un paquete comprado para la copropiedad se suma a sus atributos efectivos y sale en <c>TenantLicenseUpdated</c>
    /// (pendings/739); la licencia guardada no cambia.
    /// </summary>
    [Fact]
    public async Task PackagesPurchased_AddsThePackageToTheEffectiveAttributes()
    {
        // Arrange
        var handler = new ApplyTenantPackagesHandler(repository.Object, snapshots.Object, pubsub.Object, Mock.Of<ILogger<ApplyTenantPackagesHandler>>());
        IReadOnlyList<IDomainEvent>? published = null;
        pubsub.Setup(x => x.PublishAsync(It.IsAny<IReadOnlyList<IDomainEvent>>(), It.IsAny<CancellationToken>())).Callback<IReadOnlyList<IDomainEvent>, CancellationToken>((e, _) => published = e).Returns(Task.CompletedTask);

        // Act
        await handler.HandleAsync(new SubscriptionPackagesPurchasedDomainEvent(Guid.NewGuid(), Detail(), [Package()], Guid.NewGuid()), CancellationToken.None);

        // Assert
        var @event = published!.OfType<TenantLicenseUpdatedDomainEvent>().Single();
        Assert.Equal(("3", "12", false), (@event.License.Metadata["assemblies_number_meetings"], @event.License.Metadata["meetings.perYear"], tenant.License.Metadata.ContainsKey("assemblies_number_meetings")));
    }

    /// <summary>
    /// Un cambio de licencia sin paquetes (licencia mensual) conserva los que la copropiedad ya tenía.
    /// </summary>
    [Fact]
    public async Task Changed_WithoutPackages_KeepsTheCurrentPackages()
    {
        // Arrange
        tenant.ApplyPackages([Package().ToPackage()], SystemClock.Instance.GetCurrentInstant(), Guid.NewGuid());
        tenant.GetAndClearEvents();
        var handler = new ChangeTenantLicenseHandler(repository.Object, snapshots.Object, pubsub.Object, Mock.Of<ILogger<ChangeTenantLicenseHandler>>());

        // Act
        await handler.HandleAsync(new SubscriptionChangedDomainEvent(Guid.NewGuid(), Detail(), Sold(Start, Start.Plus(Duration.FromDays(31)), Guid.NewGuid(), "Élite"), tenant.License.Id, Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.Equal(("Élite", "3"), (tenant.License.Name, tenant.EffectiveLicense().Metadata["assemblies_number_meetings"]));
    }
}
