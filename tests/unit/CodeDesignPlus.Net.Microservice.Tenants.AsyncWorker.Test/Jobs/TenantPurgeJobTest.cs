using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeDesignPlus.Net.Core.Abstractions;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Jobs;
using CodeDesignPlus.Net.Microservice.Tenants.Domain;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.Repositories;
using CodeDesignPlus.Net.PubSub.Abstractions;
using Hangfire;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Test.Jobs;

public class TenantPurgeJobTest
{
    private readonly Mock<ITenantRepository> repositoryMock = new();
    private readonly Mock<IPubSub> pubSubMock = new();
    private readonly Mock<IJobCancellationToken> jobCancellationToken = new();
    private readonly TenantPurgeJob job;
    private readonly List<string> calls = [];

    public TenantPurgeJobTest()
    {
        jobCancellationToken.SetupGet(x => x.ShutdownToken).Returns(CancellationToken.None);

        pubSubMock
            .Setup(p => p.PublishAsync(It.IsAny<IReadOnlyList<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("publish"))
            .Returns(Task.CompletedTask);

        repositoryMock
            .Setup(r => r.PurgeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("purge"))
            .ReturnsAsync(true);

        job = new TenantPurgeJob(repositoryMock.Object, pubSubMock.Object, Mock.Of<ILogger<TenantPurgeJob>>());
    }

    /// <summary>
    /// A tenant deleted with a retention that already ended: the job takes it as due.
    /// </summary>
    private static TenantAggregate DueTenant()
    {
        var tenant = TenantAggregate.Create(Guid.NewGuid(), "Test Tenant", Utils.TypeDocument, "123456789", null, "3107845123", "fake@fake.com", Utils.Location, Utils.License, true, Guid.NewGuid());

        tenant.Delete(Guid.NewGuid(), Duration.FromTicks(1));
        tenant.GetAndClearEvents();

        return tenant;
    }

    private void Returns(params TenantAggregate[] tenants)
    {
        repositoryMock
            .Setup(r => r.FindDueForPurgeAsync(It.IsAny<Instant>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. tenants]);
    }

    [Fact]
    public async Task ExecuteAsync_DueTenant_PublishesThePurgeBeforeDeletingTheTenant()
    {
        // Arrange
        var tenant = DueTenant();

        Returns(tenant);

        // Act
        await job.ExecuteAsync(jobCancellationToken.Object);

        // Assert
        Assert.Equal(["publish", "purge"], calls);
        pubSubMock.Verify(p => p.PublishAsync(It.Is<IReadOnlyList<IDomainEvent>>(e => e.OfType<TenantPurgedDomainEvent>().Single().AggregateId == tenant.Id), It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(r => r.PurgeAsync(tenant.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PublishFails_KeepsTheTenantForTheNextRun()
    {
        // Arrange
        var failing = DueTenant();
        var next = DueTenant();

        Returns(failing, next);

        pubSubMock
            .Setup(p => p.PublishAsync(It.Is<IReadOnlyList<IDomainEvent>>(e => e.Any(x => x.AggregateId == failing.Id)), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service Bus is down"));

        // Act
        await job.ExecuteAsync(jobCancellationToken.Object);

        // Assert
        repositoryMock.Verify(r => r.PurgeAsync(failing.Id, It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(r => r.PurgeAsync(next.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NothingDue_DoesNothing()
    {
        // Arrange
        Returns();

        // Act
        await job.ExecuteAsync(jobCancellationToken.Object);

        // Assert
        Assert.Empty(calls);
    }
}
