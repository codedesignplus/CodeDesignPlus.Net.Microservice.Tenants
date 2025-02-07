using System;
using System.Threading;
using System.Threading.Tasks;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.DeleteTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using Moq;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Tenant.Commands.DeleteTenant;

public class DeleteTenantCommandHandlerTest
{
    private readonly Mock<ITenantRepository> repositoryMock;
    private readonly Mock<IUserContext> userContextMock;
    private readonly Mock<IPubSub> pubSubMock;
    private readonly DeleteTenantCommandHandler handler;
    
    public DeleteTenantCommandHandlerTest()
    {
        repositoryMock = new Mock<ITenantRepository>();
        userContextMock = new Mock<IUserContext>();
        pubSubMock = new Mock<IPubSub>();
        handler = new DeleteTenantCommandHandler(repositoryMock.Object, userContextMock.Object, pubSubMock.Object);
    }

    [Fact]
    public async Task Handle_RequestIsNull_ThrowsCodeDesignPlusException()
    {
        // Arrange
        DeleteTenantCommand request = null!;
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => handler.Handle(request, cancellationToken));

        Assert.Equal(Errors.InvalidRequest.GetMessage(), exception.Message);
        Assert.Equal(Errors.InvalidRequest.GetCode(), exception.Code);
        Assert.Equal(Layer.Application, exception.Layer);
    }

    [Fact]
    public async Task Handle_TenantNotFound_ThrowsCodeDesignPlusException()
    {
        // Arrange
        var request = new DeleteTenantCommand(Guid.NewGuid());
        var cancellationToken = CancellationToken.None;

        repositoryMock
            .Setup(r => r.FindAsync<TenantAggregate>(It.IsAny<Guid>(), cancellationToken))
            .ReturnsAsync((TenantAggregate)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => handler.Handle(request, cancellationToken));

        Assert.Equal(Errors.TenantNotFound.GetMessage(), exception.Message);
        Assert.Equal(Errors.TenantNotFound.GetCode(), exception.Code);
        Assert.Equal(Layer.Application, exception.Layer);
    }

    [Fact]
    public async Task Handle_ValidRequest_DeletesTenantAndPublishesEvents()
    {
        // Arrange
        var request = new DeleteTenantCommand(Guid.NewGuid());
        var cancellationToken = CancellationToken.None;
        var tenantAggregate = TenantAggregate.Create(Guid.NewGuid(), "Test Tenant", new Uri("http://test.com"), Utils.License, Utils.Location, Guid.NewGuid());

        repositoryMock
            .Setup(r => r.FindAsync<TenantAggregate>(It.IsAny<Guid>(), cancellationToken))
            .ReturnsAsync(tenantAggregate);

        userContextMock.SetupGet(u => u.IdUser).Returns(Guid.NewGuid());

        // Act
        await handler.Handle(request, cancellationToken);

        // Assert
        repositoryMock.Verify(r => r.DeleteAsync<TenantAggregate>(It.IsAny<Guid>(),  cancellationToken), Times.Once);
        pubSubMock.Verify(p => p.PublishAsync(It.IsAny<List<TenantDeletedDomainEvent>>(), cancellationToken), Times.AtMostOnce);
    }
}
