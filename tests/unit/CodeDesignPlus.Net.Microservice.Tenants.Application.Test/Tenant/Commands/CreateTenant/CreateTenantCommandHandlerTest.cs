using System;
using System.Threading;
using System.Threading.Tasks;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using Moq;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Tenant.Commands.CreateTenant;

public class CreateTenantCommandHandlerTest
{
    private readonly Mock<ITenantRepository> repositoryMock;
    private readonly Mock<IUserContext> userContextMock;
    private readonly Mock<IPubSub> pubSubMock;
    private readonly CreateTenantCommandHandler handler;

    public CreateTenantCommandHandlerTest()
    {
        repositoryMock = new Mock<ITenantRepository>();
        userContextMock = new Mock<IUserContext>();
        pubSubMock = new Mock<IPubSub>();
        handler = new CreateTenantCommandHandler(repositoryMock.Object, userContextMock.Object, pubSubMock.Object);
    }

    [Fact]
    public async Task Handle_RequestIsNull_ThrowsCodeDesignPlusException()
    {
        // Arrange
        CreateTenantCommand request = null!;
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => handler.Handle(request, cancellationToken));

        Assert.Equal(Errors.InvalidRequest.GetMessage(), exception.Message);
        Assert.Equal(Errors.InvalidRequest.GetCode(), exception.Code);
        Assert.Equal(Layer.Application, exception.Layer);
    }

    [Fact]
    public async Task Handle_TenantAlreadyExists_ThrowsCodeDesignPlusException()
    {
        // Arrange
        var request = new CreateTenantCommand(Guid.NewGuid(), "Test Tenant", Utils.TypeDocument, "123456789", new Uri("http://test.com"), "1236456","fake@fake.com", Utils.Location, Utils.License, true);

        var cancellationToken = CancellationToken.None;

        repositoryMock
            .Setup(r => r.ExistsAsync<TenantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => handler.Handle(request, cancellationToken));

        Assert.Equal(Errors.TenantAlreadyExists.GetMessage(), exception.Message);
        Assert.Equal(Errors.TenantAlreadyExists.GetCode(), exception.Code);
        Assert.Equal(Layer.Application, exception.Layer);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesTenantAndPublishesEvents()
    {
        // Arrange
        var request = new CreateTenantCommand(Guid.NewGuid(), "Test Tenant", Utils.TypeDocument, "123456789", new Uri("http://test.com"), "1236456", "fake@fake.com",Utils.Location, Utils.License, true);
        var cancellationToken = CancellationToken.None;

        repositoryMock
            .Setup(r => r.ExistsAsync<TenantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        userContextMock.Setup(u => u.IdUser).Returns(Guid.NewGuid());

        // Act
        await handler.Handle(request, cancellationToken);

        // Assert
        repositoryMock.Verify(r => r.CreateAsync(It.IsAny<TenantAggregate>(), It.IsAny<CancellationToken>()), Times.Once);
        pubSubMock.Verify(p => p.PublishAsync(It.IsAny<List<TenantCreatedDomainEvent>>(), It.IsAny<CancellationToken>()), Times.AtMostOnce);
    }
}
