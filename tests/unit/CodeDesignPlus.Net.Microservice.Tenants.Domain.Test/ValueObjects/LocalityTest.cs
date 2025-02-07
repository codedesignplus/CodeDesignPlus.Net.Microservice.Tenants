using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.Test.ValueObjects;

public class LocalityTest
{
    [Fact]
    public void Create_ValidParameters_ReturnsLocality()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Locality";

        // Act
        var locality = Locality.Create(id, name);

        // Assert
        Assert.NotNull(locality);
        Assert.Equal(id, locality.Id);
        Assert.Equal(name, locality.Name);
    }

    [Fact]
    public void Create_EmptyName_ThrowsCodeDesignPlusException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = string.Empty;

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => Locality.Create(id, name));

        Assert.Equal(Errors.LocalityNameIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.LocalityNameIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }

    [Fact]
    public void Create_EmptyId_ThrowsCodeDesignPlusException()
    {
        // Arrange
        var id = Guid.Empty;
        var name = "Test Locality";

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => Locality.Create(id, name));

        Assert.Equal(Errors.LocalityIdIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.LocalityIdIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }
}
