using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetAllTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using Moq;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Tenant.Queries.GetAllTenant;

public class GetAllTenantQueryHandlerTest
{
    private readonly Mock<ITenantRepository> repositoryMock;
    private readonly Mock<IMapper> mapperMock;
    private readonly GetAllTenantQueryHandler handler;

    public GetAllTenantQueryHandlerTest()
    {
        repositoryMock = new Mock<ITenantRepository>();
        mapperMock = new Mock<IMapper>();
        handler = new GetAllTenantQueryHandler(repositoryMock.Object, mapperMock.Object);
    }

    [Fact]
    public async Task Handle_RequestIsNull_ThrowsCodeDesignPlusException()
    {
        // Arrange
        GetAllTenantQuery request = null!;
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => handler.Handle(request, cancellationToken));

        Assert.Equal(Errors.InvalidRequest.GetMessage(), exception.Message);
        Assert.Equal(Errors.InvalidRequest.GetCode(), exception.Code);
        Assert.Equal(Layer.Application, exception.Layer);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsMappedTenants()
    {
        // Arrange
        var request = new GetAllTenantQuery(null!);
        var cancellationToken = CancellationToken.None;
        var tenantAggregate = TenantAggregate.Create(Guid.NewGuid(), "TenantName", new Uri("http://example.com"), Utils.License, Utils.Location, Guid.NewGuid());
        var tenantDto = new TenantDto
        {
            Id = tenantAggregate.Id,
            Name = tenantAggregate.Name,
            Domain = tenantAggregate.Domain,
            License = tenantAggregate.License,
            Location = tenantAggregate.Location
        };

        var tenantsAggregate = new List<TenantAggregate> { tenantAggregate };

        var pagination = Pagination<TenantAggregate>.Create(tenantsAggregate, tenantsAggregate.Count, 10, 0);

        repositoryMock
            .Setup(repo => repo.MatchingAsync<TenantAggregate>(request.Criteria, cancellationToken))
            .ReturnsAsync(pagination);

        var tenantsDto = new List<TenantDto> { tenantDto };

        mapperMock
            .Setup(mapper => mapper.Map<Pagination<TenantDto>>(pagination))
            .Returns(Pagination<TenantDto>.Create(tenantsDto, tenantsDto.Count, 10, 0));

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.Contains(result.Data, x => x.Id == tenantDto.Id);
    }
}
