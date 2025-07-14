using System;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.Test;

public class TenantAggregateTest
{
    [Fact]
    public void Create_ValidParameters_ShouldCreateTenantAggregate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Tenant";
        var domain = new Uri("http://test.com");
        var createdBy = Guid.NewGuid();

        // Act
        var tenant = TenantAggregate.Create(id, name, Utils.TypeDocument, "123456789", domain, "3107845123", "fake@fake.com", Utils.Location, Utils.License, true, createdBy);

        // Assert
        Assert.NotNull(tenant);
        Assert.Equal(id, tenant.Id);
        Assert.Equal(name, tenant.Name);
        Assert.Equal(domain, tenant.Domain);
        Assert.Equal(Utils.License, tenant.License);
        Assert.Equal(Utils.Location, tenant.Location);
        Assert.True(tenant.IsActive);
    }

    [Fact]
    public void Update_ValidParameters_ShouldUpdateTenantAggregate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Tenant";
        var domain = new Uri("http://test.com");
        var createdBy = Guid.NewGuid();
        var tenant = TenantAggregate.Create(id, name, Utils.TypeDocument, "123456789", domain, "3107845123", "fake@fake.com", Utils.Location, Utils.License, true, createdBy);

        // New values for update
        var newName = "Updated Tenant";
        var newTypeDocument = Utils.TypeDocument;
        var newDocument = "987654321";
        var newPhone = "3101234567";
        var newDomain = new Uri("http://updated.com");
        var updatedBy = Guid.NewGuid();
        var newEmail = "fake2@fake.com";

        // Act
        tenant.Update(newName, newTypeDocument, newDocument, newDomain, newPhone, newEmail, false, updatedBy);

        // Assert
        Assert.Equal(newName, tenant.Name);
        Assert.Equal(newTypeDocument, tenant.TypeDocument);
        Assert.Equal(newDocument, tenant.NumberDocument);
        Assert.Equal(newDomain, tenant.Domain);
        Assert.Equal(newPhone, tenant.Phone);
        Assert.False(tenant.IsActive);
    }

    [Fact]
    public void UpdateLicense_ValidParameters_ShouldUpdateTenantLicense()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Tenant";
        var domain = new Uri("http://test.com");
        var createdBy = Guid.NewGuid();
        var tenant = TenantAggregate.Create(id, name, Utils.TypeDocument, "123456789", domain, "3107845123", "fake@fake.com", Utils.Location, Utils.License, true, createdBy);

        var newLicense = License.Create(Guid.NewGuid(), "License Test", SystemClock.Instance.GetCurrentInstant(), SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(30)), new Dictionary<string, string>{
            { "User", "20" },
            { "Admin", "2" },
            { "Invoice", "2" }
        });
        var updatedBy = Guid.NewGuid();

        // Act
        tenant.UpdateLicense(newLicense, updatedBy);

        // Assert
        Assert.Equal(newLicense, tenant.License);
    }

    [Fact]
    public void UpdateLocation_ValidParameters_ShouldUpdateTenantLocation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Tenant";
        var domain = new Uri("http://test.com");
        var createdBy = Guid.NewGuid();
        var tenant = TenantAggregate.Create(id, name, Utils.TypeDocument, "123456789", domain, "3107845123", "fake@fake.com", Utils.Location, Utils.License, true, createdBy);

        var country = Country.Create(Guid.NewGuid(), "Mexico", 602, "America/Mexico_City", Utils.Currency);
        var newLocation = Location.Create(country, Utils.State, Utils.City, Utils.Locality, Utils.Neighborhood, "Calle Falsa 123", "01000");
        var updatedBy = Guid.NewGuid();

        // Act
        tenant.UpdateLocation(newLocation, updatedBy);

        // Assert
        Assert.Equal(newLocation, tenant.Location);
    }

    [Fact]
    public void Delete_ValidParameters_ShouldDeactivateTenant()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Tenant";
        var domain = new Uri("http://test.com");
        var createdBy = Guid.NewGuid();
        var tenant = TenantAggregate.Create(id, name, Utils.TypeDocument, "123456789", domain, "3107845123", "fake@fake.com", Utils.Location, Utils.License, true, createdBy);

        var deleteBy = Guid.NewGuid();

        // Act
        tenant.Delete(deleteBy);

        // Assert
        Assert.False(tenant.IsActive);
    }
}
