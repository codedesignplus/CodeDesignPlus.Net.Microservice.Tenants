using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Tenants.Default.Test.Helpers;

namespace CodeDesignPlus.Net.Microservice.Tenants.Default.Test.Validations;

/// <summary>
/// Validates the data transfer objects of the microservice.
/// </summary>
/// <remarks>
/// Asserted explicitly instead of through the reflection-driven attribute of the SDK: that
/// attribute feeds every string with "Test", which a <see cref="Domain.ValueObjects.TypeDocument"/>
/// code cannot be.
/// </remarks>
public class DataTransferObjectTests
{
    [Fact]
    public void TenantDto_GetAndSet_RoundTripsEveryValue()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var dto = new TenantDto
        {
            Id = id,
            Name = Utils.Name,
            TypeDocument = Utils.TypeDocument,
            NumberDocument = Utils.NumberDocument,
            Domain = Utils.Domain,
            Phone = Utils.Phone,
            Email = Utils.Email,
            Location = Utils.Location,
            License = Utils.License,
            IsActive = true
        };

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(Utils.Name, dto.Name);
        Assert.Equal(Utils.TypeDocument, dto.TypeDocument);
        Assert.Equal(Utils.NumberDocument, dto.NumberDocument);
        Assert.Equal(Utils.Domain, dto.Domain);
        Assert.Equal(Utils.Phone, dto.Phone);
        Assert.Equal(Utils.Email, dto.Email);
        Assert.Equal(Utils.Location, dto.Location);
        Assert.Equal(Utils.License, dto.License);
        Assert.True(dto.IsActive);
    }

    [Fact]
    public void TenantDto_WithoutDomain_IsAllowed()
    {
        // Act
        var dto = new TenantDto { Id = Guid.NewGuid(), Domain = null };

        // Assert
        Assert.Null(dto.Domain);
    }
}
