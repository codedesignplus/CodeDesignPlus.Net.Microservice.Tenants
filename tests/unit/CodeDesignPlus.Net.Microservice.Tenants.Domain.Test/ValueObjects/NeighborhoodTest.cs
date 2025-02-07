using System;
using Xunit;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.Test.ValueObjects;

public class NeighborhoodTest
{
    [Fact]
    public void Create_ValidParameters_ShouldReturnNeighborhood()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Neighborhood";

        // Act
        var neighborhood = Neighborhood.Create(id, name);

        // Assert
        Assert.NotNull(neighborhood);
        Assert.Equal(id, neighborhood.Id);
        Assert.Equal(name, neighborhood.Name);
    }

    [Fact]
    public void Create_EmptyName_ShouldThrowCodeDesignPlusException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = string.Empty;

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => Neighborhood.Create(id, name));

        Assert.Equal(Errors.NeighborhoodNameIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.NeighborhoodNameIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }

    [Fact]
    public void Create_EmptyId_ShouldThrowCodeDesignPlusException()
    {
        // Arrange
        var id = Guid.Empty;
        var name = "Test Neighborhood";

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => Neighborhood.Create(id, name));

        Assert.Equal(Errors.NeighborhoodIdIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.NeighborhoodIdIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }
}
