using System.Threading;
using System.Threading.Tasks;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.UpdateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using Moq;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Tenant.Commands.UpdateTenant;

public class UpdateTenantCommandHandlerTest
{
    private readonly Mock<ITenantRepository> repositoryMock;
    private readonly Mock<IUserContext> userContextMock;
    private readonly Mock<IPubSub> pubSubMock;
    private readonly UpdateTenantCommandHandler handler;

    public UpdateTenantCommandHandlerTest()
    {
        repositoryMock = new Mock<ITenantRepository>();
        userContextMock = new Mock<IUserContext>();
        pubSubMock = new Mock<IPubSub>();
        handler = new UpdateTenantCommandHandler(repositoryMock.Object, userContextMock.Object, pubSubMock.Object);
    }

    [Fact]
    public async Task Handle_RequestIsNull_ThrowsCodeDesignPlusException()
    {
        // Arrange
        UpdateTenantCommand request = null!;
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
        var request = new UpdateTenantCommand(Guid.NewGuid(), "Test Tenant", Utils.TypeDocument, "12345678", new Uri("http://test.com"), "3105682451", Utils.Location, Utils.License, true);
        var cancellationToken = CancellationToken.None;

        repositoryMock
            .Setup(r => r.FindAsync<TenantAggregate>(request.Id, cancellationToken))
            .ReturnsAsync((TenantAggregate)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => handler.Handle(request, cancellationToken));

        Assert.Equal(Errors.TenantNotFound.GetMessage(), exception.Message);
        Assert.Equal(Errors.TenantNotFound.GetCode(), exception.Code);
        Assert.Equal(Layer.Application, exception.Layer);
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesTenantAndPublishesEvents()
    {
        // Arrange
        var tenant = TenantAggregate.Create(Guid.NewGuid(), "Test Old Tenant", Utils.TypeDocument, "12345678", new Uri("http://test.com"), "31112364578", Utils.Location, Utils.License, true, Guid.NewGuid());
        var request = new UpdateTenantCommand(tenant.Id, "Test New Tenant", Utils.TypeDocument, "123456789", new Uri("http://test.com"), "31112364578", Utils.Location, Utils.License, true);
        var cancellationToken = CancellationToken.None;

        repositoryMock.
            Setup(r => r.FindAsync<TenantAggregate>(request.Id, cancellationToken))
            .ReturnsAsync(tenant);

        userContextMock.SetupGet(u => u.IdUser).Returns(Guid.NewGuid());

        // Act
        await handler.Handle(request, cancellationToken);

        // Assert
        repositoryMock.Verify(r => r.UpdateAsync(tenant, cancellationToken), Times.Once);
        pubSubMock.Verify(p => p.PublishAsync(It.IsAny<List<TenantUpdatedDomainEvent>>(), cancellationToken), Times.AtMostOnce);
        pubSubMock.Verify(p => p.PublishAsync(It.IsAny<List<TenantLocationUpdatedDomainEvent>>(), cancellationToken), Times.AtMostOnce);
        pubSubMock.Verify(p => p.PublishAsync(It.IsAny<List<TenantLicenseUpdatedDomainEvent>>(), cancellationToken), Times.AtMostOnce);
    }
}
