using CodeDesignPlus.Net.Microservice.Tenants.Default.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain;

namespace CodeDesignPlus.Net.Microservice.Tenants.Default.Test.Validations;

/// <summary>
/// Validates the aggregates of the microservice.
/// </summary>
/// <remarks>
/// Construction through the named constructor is asserted explicitly instead of through the
/// reflection-driven attribute of the SDK: that attribute feeds every string with "Test", which a
/// <see cref="Domain.ValueObjects.TypeDocument"/> code cannot be.
/// </remarks>
public class AggregateTest
{
    [Theory]
    [Aggregate<Domain.Errors>(false)]
    public void Aggregate_Constructor_ShouldSetAndRetrievePropertiesCorrectly(Type aggregate, object instance, Dictionary<ParameterInfo, object> values)
    {
        // Assert
        Assert.NotNull(instance);

        var value = aggregate.GetProperty(nameof(AggregateRoot.Id))!.GetValue(instance, null);
        var valueExpected = values.First(x => x.Key.Name!.Equals(nameof(AggregateRoot.Id), StringComparison.OrdinalIgnoreCase)).Value;
        Assert.Equal(valueExpected, value);
    }

    [Fact]
    public void TenantAggregate_Create_SetsEveryProperty()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdBy = Guid.NewGuid();

        // Act
        var tenant = TenantAggregate.Create(id, Utils.Name, Utils.TypeDocument, Utils.NumberDocument, Utils.Domain, Utils.Phone, Utils.Email, Utils.Location, Utils.License, true, createdBy);

        // Assert
        Assert.Equal(id, tenant.Id);
        Assert.Equal(Utils.Name, tenant.Name);
        Assert.Equal(Utils.TypeDocument, tenant.TypeDocument);
        Assert.Equal(Utils.NumberDocument, tenant.NumberDocument);
        Assert.Equal(Utils.Domain, tenant.Domain);
        Assert.Equal(Utils.Phone, tenant.Phone);
        Assert.Equal(Utils.Email, tenant.Email);
        Assert.Equal(Utils.Location, tenant.Location);
        Assert.Equal(Utils.License, tenant.License);
        Assert.True(tenant.IsActive);
        Assert.Equal(createdBy, tenant.CreatedBy);
    }

    [Fact]
    public void TenantAggregate_Create_WithoutDomain_IsAllowed()
    {
        // Act
        var tenant = TenantAggregate.Create(Guid.NewGuid(), Utils.Name, Utils.TypeDocument, Utils.NumberDocument, null, Utils.Phone, Utils.Email, Utils.Location, Utils.License, true, Guid.NewGuid());

        // Assert
        Assert.Null(tenant.Domain);
    }

    [Fact]
    public void TenantAggregate_Create_RaisesTenantCreatedDomainEvent()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var tenant = TenantAggregate.Create(id, Utils.Name, Utils.TypeDocument, Utils.NumberDocument, Utils.Domain, Utils.Phone, Utils.Email, Utils.Location, Utils.License, true, Guid.NewGuid());

        // Assert
        var @event = Assert.Single(tenant.GetAndClearEvents());
        Assert.Equal(id, @event.AggregateId);
    }
}
