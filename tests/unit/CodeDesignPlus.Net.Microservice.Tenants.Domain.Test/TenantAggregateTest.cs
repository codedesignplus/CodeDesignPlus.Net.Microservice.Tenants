using System.Linq;
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

        var newLicense = License.Create(Guid.NewGuid(), "License Test", SystemClock.Instance.GetCurrentInstant(), SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(30)), [], new Dictionary<string, string>{
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

        var country = Country.Create(Guid.NewGuid(), "Mexico", "MX", "MEX", 484, "+52", "America/Mexico_City", Utils.Currency);
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
        tenant.Delete(deleteBy, Retention);

        // Assert
        Assert.False(tenant.IsActive);
    }

    private static readonly Duration Retention = Duration.FromDays(30);

    private static TenantAggregate NewTenant() => TenantAggregate.Create(Guid.NewGuid(), "Test Tenant", Utils.TypeDocument, "123456789", new Uri("http://test.com"), "3107845123", "fake@fake.com", Utils.Location, Utils.License, true, Guid.NewGuid());

    [Fact]
    public void Delete_ValidRetention_KeepsTheTenantUntilTheRetentionEnds()
    {
        // Arrange
        var tenant = NewTenant();
        var before = SystemClock.Instance.GetCurrentInstant();

        // Act
        tenant.Delete(Guid.NewGuid(), Retention);

        // Assert
        Assert.True(tenant.IsDeleted);
        Assert.NotNull(tenant.PurgeAfter);
        Assert.InRange(tenant.PurgeAfter!.Value, before + Retention, SystemClock.Instance.GetCurrentInstant() + Retention);
        Assert.Equal(tenant.PurgeAfter, tenant.GetAndClearEvents().OfType<TenantDeletedDomainEvent>().Single().PurgeAfter);
    }

    [Fact]
    public void Delete_RetentionIsZero_ThrowsRetentionIsInvalid()
    {
        // Arrange
        var tenant = NewTenant();

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => tenant.Delete(Guid.NewGuid(), Duration.Zero));

        // Assert
        Assert.Equal(Errors.RetentionIsInvalid.GetCode(), exception.Code);
    }

    [Fact]
    public void Restore_WithinRetention_BringsTheTenantBack()
    {
        // Arrange
        var tenant = NewTenant();
        var restoredBy = Guid.NewGuid();

        tenant.Delete(Guid.NewGuid(), Retention);
        tenant.GetAndClearEvents();

        // Act
        tenant.Restore(restoredBy, SystemClock.Instance.GetCurrentInstant());

        // Assert
        Assert.False(tenant.IsDeleted);
        Assert.True(tenant.IsActive);
        Assert.Null(tenant.PurgeAfter);
        Assert.Null(tenant.DeletedAt);
        Assert.Equal(restoredBy, tenant.GetAndClearEvents().OfType<TenantRestoredDomainEvent>().Single().RestoredBy);
    }

    [Fact]
    public void Restore_RetentionEnded_ThrowsRestoreWindowExpired()
    {
        // Arrange
        var tenant = NewTenant();

        tenant.Delete(Guid.NewGuid(), Retention);

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => tenant.Restore(Guid.NewGuid(), tenant.PurgeAfter!.Value));

        // Assert
        Assert.Equal(Errors.RestoreWindowExpired.GetCode(), exception.Code);
    }

    [Fact]
    public void Restore_TenantNotDeleted_ThrowsTenantIsNotDeleted()
    {
        // Arrange
        var tenant = NewTenant();

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => tenant.Restore(Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant()));

        // Assert
        Assert.Equal(Errors.TenantIsNotDeleted.GetCode(), exception.Code);
    }

    [Fact]
    public void Purge_RetentionEnded_AnnouncesThePurge()
    {
        // Arrange
        var tenant = NewTenant();

        tenant.Delete(Guid.NewGuid(), Retention);
        tenant.GetAndClearEvents();

        // Act
        tenant.Purge(tenant.PurgeAfter!.Value);

        // Assert
        Assert.Equal(tenant.Id, tenant.GetAndClearEvents().OfType<TenantPurgedDomainEvent>().Single().AggregateId);
    }

    [Fact]
    public void Purge_WithinRetention_ThrowsTenantIsNotDueForPurge()
    {
        // Arrange
        var tenant = NewTenant();

        tenant.Delete(Guid.NewGuid(), Retention);

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => tenant.Purge(tenant.PurgeAfter!.Value - Duration.FromSeconds(1)));

        // Assert
        Assert.Equal(Errors.TenantIsNotDueForPurge.GetCode(), exception.Code);
    }

    [Fact]
    public void Purge_TenantNotDeleted_ThrowsTenantIsNotDeleted()
    {
        // Arrange
        var tenant = NewTenant();

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => tenant.Purge(SystemClock.Instance.GetCurrentInstant()));

        // Assert
        Assert.Equal(Errors.TenantIsNotDeleted.GetCode(), exception.Code);
    }
}
