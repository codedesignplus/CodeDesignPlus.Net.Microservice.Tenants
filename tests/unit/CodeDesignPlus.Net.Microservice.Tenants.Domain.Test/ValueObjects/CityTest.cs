using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.Test.ValueObjects;

public class CityTest
{
    [Fact]
    public void Create_ValidParameters_ShouldReturnCity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test City";
        var timezone = "UTC";

        // Act
        var city = City.Create(id, name, timezone);

        // Assert
        Assert.NotNull(city);
        Assert.Equal(id, city.Id);
        Assert.Equal(name, city.Name);
        Assert.Equal(timezone, city.Timezone);
    }

    [Fact]
    public void Create_EmptyName_ShouldThrowException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = string.Empty;
        var timezone = "UTC";

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => City.Create(id, name, timezone));

        Assert.Equal(Errors.CityNameIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.CityNameIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain , exception.Layer);
    }

    [Fact]
    public void Create_EmptyId_ShouldThrowException()
    {
        // Arrange
        var id = Guid.Empty;
        var name = "Test City";
        var timezone = "UTC";

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => City.Create(id, name, timezone));

        Assert.Equal(Errors.CityIdIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.CityIdIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain , exception.Layer);
    }

    [Fact]
    public void Create_EmptyTimezone_ShouldThrowException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test City";
        var timezone = string.Empty;

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => City.Create(id, name, timezone));
        
        Assert.Equal(Errors.CityTimezoneIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.CityTimezoneIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain , exception.Layer);
    }
}
