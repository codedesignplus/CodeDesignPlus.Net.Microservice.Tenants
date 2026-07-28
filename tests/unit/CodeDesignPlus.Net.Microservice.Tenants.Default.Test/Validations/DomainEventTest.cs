using CodeDesignPlus.Net.Microservice.Tenants.Default.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;

namespace CodeDesignPlus.Net.Microservice.Tenants.Default.Test.Validations;

/// <summary>
/// Validates the domain events of the microservice.
/// </summary>
/// <remarks>
/// Asserted explicitly instead of through the reflection-driven attribute of the SDK: that
/// attribute feeds every string with "Test", which a <see cref="Domain.ValueObjects.TypeDocument"/>
/// code cannot be.
/// </remarks>
public class DomainEventTest
{
    [Fact]
    public void TenantCreatedDomainEvent_Create_CarriesTheWholeTenant()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();

        // Act
        var @event = TenantCreatedDomainEvent.Create(aggregateId, Utils.Name, Utils.TypeDocument, Utils.NumberDocument, Utils.Domain, Utils.Phone, Utils.Email, Utils.Location, Utils.License, true, createdBy);

        // Assert
        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(Utils.Name, @event.Name);
        Assert.Equal(Utils.TypeDocument, @event.TypeDocument);
        Assert.Equal(Utils.NumberDocument, @event.NumberDocument);
        Assert.Equal(Utils.Domain, @event.Domain);
        Assert.Equal(Utils.Phone, @event.Phone);
        Assert.Equal(Utils.Email, @event.Email);
        Assert.Equal(Utils.Location, @event.Location);
        Assert.Equal(Utils.License, @event.License);
        Assert.True(@event.IsActive);
        Assert.Equal(createdBy, @event.CreatedBy);
        Assert.NotEqual(Guid.Empty, @event.EventId);
    }

    [Fact]
    public void TenantUpdatedDomainEvent_Create_CarriesTheWholeTenant()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var updatedBy = Guid.NewGuid();

        // Act
        var @event = TenantUpdatedDomainEvent.Create(aggregateId, Utils.Name, Utils.TypeDocument, Utils.NumberDocument, Utils.Domain, Utils.Phone, Utils.Email, Utils.Location, Utils.License, true, updatedBy);

        // Assert
        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(Utils.Location, @event.Location);
        Assert.Equal(Utils.License, @event.License);
        Assert.Equal(updatedBy, @event.UpdatedBy);
    }

    [Fact]
    public void TenantDeletedDomainEvent_Create_CarriesTheWholeTenant()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var deletedBy = Guid.NewGuid();

        // Act
        var @event = TenantDeletedDomainEvent.Create(aggregateId, Utils.Name, Utils.TypeDocument, Utils.NumberDocument, Utils.Domain, Utils.Phone, Utils.Email, Utils.Location, Utils.License, false, deletedBy);

        // Assert
        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.False(@event.IsActive);
        Assert.Equal(deletedBy, @event.DeletedBy);
    }

    [Fact]
    public void TenantLicenseUpdatedDomainEvent_Create_CarriesTheLicense()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var updatedBy = Guid.NewGuid();

        // Act
        var @event = TenantLicenseUpdatedDomainEvent.Create(aggregateId, Utils.License, updatedBy);

        // Assert
        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(Utils.License, @event.License);
        Assert.Equal(updatedBy, @event.UpdatedBy);
    }

    [Fact]
    public void TenantLocationUpdatedDomainEvent_Create_CarriesTheLocation()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var updatedBy = Guid.NewGuid();

        // Act
        var @event = TenantLocationUpdatedDomainEvent.Create(aggregateId, Utils.Location, updatedBy);

        // Assert
        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(Utils.Location, @event.Location);
        Assert.Equal(updatedBy, @event.UpdatedBy);
    }

    [Fact]
    public void TenantProvisionedForOrderDomainEvent_Create_CarriesTheOrder()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        // Act
        var @event = TenantProvisionedForOrderDomainEvent.Create(aggregateId, orderId);

        // Assert
        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(orderId, @event.OrderId);
    }
}
