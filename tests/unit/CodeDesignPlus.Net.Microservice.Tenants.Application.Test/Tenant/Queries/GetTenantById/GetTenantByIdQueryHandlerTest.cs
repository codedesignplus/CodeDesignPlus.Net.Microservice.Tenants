using CodeDesignPlus.Net.Cache.Abstractions;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetTenantById;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Tenant.Queries.GetTenantById;

public class GetTenantByIdQueryHandlerTest
{
    private readonly Mock<ITenantRepository> repositoryMock;
    private readonly Mock<IMapper> mapperMock;
    private readonly Mock<ICacheManager> cacheManagerMock;
    private readonly GetTenantByIdQueryHandler handler;

    public GetTenantByIdQueryHandlerTest()
    {
        repositoryMock = new Mock<ITenantRepository>();
        mapperMock = new Mock<IMapper>();
        cacheManagerMock = new Mock<ICacheManager>();
        handler = new GetTenantByIdQueryHandler(repositoryMock.Object, mapperMock.Object, cacheManagerMock.Object);
    }

    [Fact]
    public async Task Handle_RequestIsNull_ThrowsCodeDesignPlusException()
    {
        // Arrange
        GetTenantByIdQuery request = null!;

        // Act & Assert
        await Assert.ThrowsAsync<CodeDesignPlusException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_TenantExistsInCache_ReturnsTenantDtoFromCache()
    {
        // Arrange
        var request = new GetTenantByIdQuery(Guid.NewGuid());
        var tenantDto = new TenantDto()
        {
            Id = request.Id,
            Name = "Tenant Test",
            Location = Utils.Location,
            License = Utils.License
        };
        cacheManagerMock.Setup(x => x.ExistsAsync(request.Id.ToString())).ReturnsAsync(true);
        cacheManagerMock.Setup(x => x.GetAsync<TenantDto>(request.Id.ToString())).ReturnsAsync(tenantDto);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal(tenantDto, result);
        cacheManagerMock.Verify(x => x.ExistsAsync(request.Id.ToString()), Times.Once);
        cacheManagerMock.Verify(x => x.GetAsync<TenantDto>(request.Id.ToString()), Times.Once);
        repositoryMock.Verify(x => x.FindAsync<TenantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TenantNotInCache_ReturnsTenantDtoFromRepository()
    {
        // Arrange
        var request = new GetTenantByIdQuery(Guid.NewGuid());
        var tenantAggregate = TenantAggregate.Create(Guid.NewGuid(), "Tenant Test", Utils.TypeDocument, "12345678", new Uri("http://example.com"), "3105682451", Utils.Location, Utils.License, true, Guid.NewGuid());
        var tenantDto = new TenantDto()
        {
            Id = tenantAggregate.Id,
            Name = tenantAggregate.Name,
            Location = tenantAggregate.Location,
            License = tenantAggregate.License
        };
        cacheManagerMock.Setup(x => x.ExistsAsync(request.Id.ToString())).ReturnsAsync(false);
        repositoryMock.Setup(x => x.FindAsync<TenantAggregate>(request.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tenantAggregate);
        mapperMock.Setup(x => x.Map<TenantDto>(tenantAggregate)).Returns(tenantDto);
        cacheManagerMock.Setup(x => x.SetAsync(request.Id.ToString(), tenantDto, It.IsAny<TimeSpan?>())).Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal(tenantDto, result);
        cacheManagerMock.Verify(x => x.ExistsAsync(request.Id.ToString()), Times.Once);
        repositoryMock.Verify(x => x.FindAsync<TenantAggregate>(request.Id, It.IsAny<CancellationToken>()), Times.Once);
        mapperMock.Verify(x => x.Map<TenantDto>(tenantAggregate), Times.Once);
        cacheManagerMock.Verify(x => x.SetAsync(request.Id.ToString(), tenantDto, It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TenantNotFoundInRepository_ThrowsCodeDesignPlusException()
    {
        // Arrange
        var request = new GetTenantByIdQuery(Guid.NewGuid());
        cacheManagerMock.Setup(x => x.ExistsAsync(request.Id.ToString())).ReturnsAsync(false);
        repositoryMock.Setup(x => x.FindAsync<TenantAggregate>(request.Id, It.IsAny<CancellationToken>())).ReturnsAsync((TenantAggregate)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(Errors.TenantNotFound.GetMessage(), exception.Message);
        Assert.Equal(Errors.TenantNotFound.GetCode(), exception.Code);
        Assert.Equal(Layer.Application, exception.Layer);
    }
}
