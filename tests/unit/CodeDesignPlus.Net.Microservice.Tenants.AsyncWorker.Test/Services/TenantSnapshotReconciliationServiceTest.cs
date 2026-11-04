using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Services;
using CodeDesignPlus.Net.Microservice.Tenants.Domain;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.Repositories;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ServiceDomain;
using CodeDesignPlus.Net.PubSub.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using C = CodeDesignPlus.Net.Core.Abstractions.Models.Criteria;

namespace CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Test.Services;

/// <summary>
/// One tenant that fails does not leave the others without their snapshot: the pass used to run in a single try, so the
/// first exception stopped it for every tenant after it (pendings/737).
/// </summary>
public class TenantSnapshotReconciliationServiceTest
{
    private readonly Mock<ITenantRepository> repositoryMock = new();
    private readonly Mock<ITenantSnapshotPublisher> publisherMock = new();
    private readonly Mock<ILogger<TenantSnapshotReconciliationService>> loggerMock = new();
    private readonly TenantSnapshotReconciliationService service;

    public TenantSnapshotReconciliationServiceTest()
    {
        var services = new ServiceCollection()
            .AddSingleton(repositoryMock.Object)
            .AddSingleton(publisherMock.Object)
            .AddSingleton(Mock.Of<IPubSub>())
            .BuildServiceProvider();

        service = new TenantSnapshotReconciliationService(services, loggerMock.Object);
    }

    private static TenantAggregate Tenant()
    {
        var tenant = TenantAggregate.Create(Guid.NewGuid(), "Conjunto", Utils.TypeDocument, "900123456", null, "3107845123", "admin@example.com", Utils.Location, Utils.License, true, Guid.NewGuid());

        tenant.GetAndClearEvents();

        return tenant;
    }

    private void Returns(params TenantAggregate[] tenants)
    {
        repositoryMock
            .Setup(r => r.MatchingAsync<TenantAggregate>(It.IsAny<C.Criteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Pagination<TenantAggregate>(tenants, tenants.Length, 500, 0));
    }

    [Fact]
    public async Task ReconcileAsync_OneTenantFails_PublishesTheOthers()
    {
        // Arrange
        var broken = Tenant();
        var healthy = Tenant();

        Returns(broken, healthy);

        publisherMock
            .Setup(p => p.PublishAsync(It.Is<TenantAggregate>(t => t.Id == broken.Id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("broken tenant"));

        // Act
        await service.ReconcileAsync(CancellationToken.None);

        // Assert
        publisherMock.Verify(p => p.PublishAsync(It.Is<TenantAggregate>(t => t.Id == healthy.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReconcileAsync_OneTenantFails_LogsTheErrorWithItsId()
    {
        // Arrange
        var broken = Tenant();

        Returns(broken, Tenant());

        publisherMock
            .Setup(p => p.PublishAsync(It.Is<TenantAggregate>(t => t.Id == broken.Id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("broken tenant"));

        // Act
        await service.ReconcileAsync(CancellationToken.None);

        // Assert
        var errors = loggerMock.Invocations
            .Where(i => i.Method.Name == nameof(ILogger.Log) && (LogLevel)i.Arguments[0] == LogLevel.Error)
            .Select(i => i.Arguments[2]?.ToString())
            .ToList();

        Assert.Contains(errors, message => message!.Contains(broken.Id.ToString()));
    }
}
