using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.RestoreTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Tenant.Commands.RestoreTenant;

public class RestoreTenantCommandHandlerTest
{
    private readonly Mock<ITenantRepository> repositoryMock = new();
    private readonly Mock<IUserContext> userContextMock = new();
    private readonly Mock<IPubSub> pubSubMock = new();
    private readonly Mock<ITenantSnapshotPublisher> snapshotPublisherMock = new();
    private readonly RestoreTenantCommandHandler handler;

    public RestoreTenantCommandHandlerTest()
    {
        userContextMock.SetupGet(u => u.IdUser).Returns(Guid.NewGuid());

        handler = new RestoreTenantCommandHandler(repositoryMock.Object, userContextMock.Object, pubSubMock.Object, snapshotPublisherMock.Object);
    }

    private static TenantAggregate DeletedTenant()
    {
        var tenant = TenantAggregate.Create(Guid.NewGuid(), "Test Tenant", Utils.TypeDocument, "123456789", null, "3107845123", "fake@fake.com", Utils.Location, Utils.License, true, Guid.NewGuid());

        tenant.Delete(Guid.NewGuid(), Duration.FromDays(30));
        tenant.GetAndClearEvents();

        return tenant;
    }

    [Fact]
    public async Task Handle_DeletedTenant_RestoresAndPublishesTheSnapshot()
    {
        // Arrange
        var tenant = DeletedTenant();

        repositoryMock.Setup(r => r.FindDeletedAsync(tenant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        repositoryMock.Setup(r => r.RestoreAsync(tenant, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        await handler.Handle(new RestoreTenantCommand(tenant.Id), CancellationToken.None);

        // Assert
        Assert.False(tenant.IsDeleted);
        snapshotPublisherMock.Verify(p => p.PublishAsync(tenant, It.IsAny<CancellationToken>()), Times.Once);
        pubSubMock.Verify(p => p.PublishAsync(It.Is<IReadOnlyList<IDomainEvent>>(e => e.OfType<TenantRestoredDomainEvent>().Any()), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TenantNotDeleted_ThrowsTenantNotFound()
    {
        // Arrange
        repositoryMock.Setup(r => r.FindDeletedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TenantAggregate?)null);

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => handler.Handle(new RestoreTenantCommand(Guid.NewGuid()), CancellationToken.None));

        // Assert
        Assert.Equal(Errors.TenantNotFound.GetCode(), exception.Code);
    }

    [Fact]
    public async Task Handle_PurgedMeanwhile_ThrowsTenantNotFoundAndPublishesNothing()
    {
        // Arrange
        var tenant = DeletedTenant();

        repositoryMock.Setup(r => r.FindDeletedAsync(tenant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        repositoryMock.Setup(r => r.RestoreAsync(tenant, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => handler.Handle(new RestoreTenantCommand(tenant.Id), CancellationToken.None));

        // Assert
        Assert.Equal(Errors.TenantNotFound.GetCode(), exception.Code);
        snapshotPublisherMock.Verify(p => p.PublishAsync(It.IsAny<TenantAggregate>(), It.IsAny<CancellationToken>()), Times.Never);
        pubSubMock.Verify(p => p.PublishAsync(It.IsAny<IReadOnlyList<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
