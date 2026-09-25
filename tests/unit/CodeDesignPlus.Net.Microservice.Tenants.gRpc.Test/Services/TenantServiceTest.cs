using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetTenantById;
using CodeDesignPlus.Net.Security.Abstractions;

namespace CodeDesignPlus.Net.Microservice.Tenants.gRpc.Test.Services;

public class TenantServiceTest
{
    private static ServerCallContext CallContext() =>
        TestServerCallContext.Create("GetTenant", null, DateTime.UtcNow.AddMinutes(1), [], CancellationToken.None, null, null, null, null, null, null);

    // Un tenant que no existe tiene que salir como NotFound: es lo que permite al directorio de tenants del SDK
    // responder 400 en vez de 503. Como FailedPrecondition, quien pregunta no podia distinguirlo de una caida
    // (pendings/028).
    [Fact]
    public async Task GetTenant_TenantDoesNotExist_AnswersNotFound()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetTenantByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CodeDesignPlusException(Layer.Application, Application.Errors.TenantNotFound.GetCode(), "Tenant not found"));

        var service = new TenantService(mediator.Object, Mock.Of<IMapper>(), Mock.Of<IUserContext>());

        // Act
        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            service.GetTenant(new GetTenantRequest { Id = Guid.NewGuid().ToString() }, CallContext()));

        // Assert
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetTenant_AnyOtherError_IsNotTurnedIntoNotFound()
    {
        // Arrange: solo "no existe" es NotFound; el resto sigue su camino y el ErrorInterceptor lo traduce.
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetTenantByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CodeDesignPlusException(Layer.Application, Application.Errors.InvalidRequest.GetCode(), "Invalid request"));

        var service = new TenantService(mediator.Object, Mock.Of<IMapper>(), Mock.Of<IUserContext>());

        // Act & Assert
        await Assert.ThrowsAsync<CodeDesignPlusException>(() =>
            service.GetTenant(new GetTenantRequest { Id = Guid.NewGuid().ToString() }, CallContext()));
    }
}
