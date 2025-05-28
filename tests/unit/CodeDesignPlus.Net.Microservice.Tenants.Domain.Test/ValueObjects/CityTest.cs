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
        var timeZone = "UTC";

        // Act
        var city = City.Create(id, name, timeZone);

        // Assert
        Assert.NotNull(city);
        Assert.Equal(id, city.Id);
        Assert.Equal(name, city.Name);
        Assert.Equal(timeZone, city.TimeZone);
    }

    [Fact]
    public void Create_EmptyName_ShouldThrowException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = string.Empty;
        var timeZone = "UTC";

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => City.Create(id, name, timeZone));

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
        var timeZone = "UTC";

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => City.Create(id, name, timeZone));

        Assert.Equal(Errors.CityIdIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.CityIdIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain , exception.Layer);
    }

    [Fact]
    public void Create_EmptyTimeZone_ShouldThrowException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test City";
        var timeZone = string.Empty;

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => City.Create(id, name, timeZone));
        
        Assert.Equal(Errors.CityTimeZoneIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.CityTimeZoneIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain , exception.Layer);
    }
}
