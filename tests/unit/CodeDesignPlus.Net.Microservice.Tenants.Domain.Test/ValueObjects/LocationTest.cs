using CodeDesignPlus.Net.Microservice.Tenants.Domain.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.Test.ValueObjects;

public class LocationTest
{
    [Fact]
    public void Create_ValidParameters_ReturnsLocation()
    {
        // Act
        var location = Location.Create(Utils.Country, Utils.State, Utils.City, Utils.Locality, Utils.Neighborhood);

        // Assert
        Assert.NotNull(location);
        Assert.Equal(Utils.Country, location.Country);
        Assert.Equal(Utils.State, location.State);
        Assert.Equal(Utils.City, location.City);
        Assert.Equal(Utils.Locality, location.Locality);
        Assert.Equal(Utils.Neighborhood, location.Neighborhood);
    }

    [Fact]
    public void Create_NullCountry_ThrowsCodeDesignPlusException()
    {
        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => Location.Create(null!, Utils.State, Utils.City, Utils.Locality, Utils.Neighborhood));

        Assert.Equal(Errors.CountryIsNull.GetMessage(), exception.Message);
        Assert.Equal(Errors.CountryIsNull.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }

    [Fact]
    public void Create_NullState_ThrowsCodeDesignPlusException()
    {
        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => Location.Create(Utils.Country, null!, Utils.City, Utils.Locality, Utils.Neighborhood));

        Assert.Equal(Errors.StateIsNull.GetMessage(), exception.Message);
        Assert.Equal(Errors.StateIsNull.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }

    [Fact]
    public void Create_NullCity_ThrowsCodeDesignPlusException()
    {
        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => Location.Create(Utils.Country, Utils.State, null!, Utils.Locality, Utils.Neighborhood));

        Assert.Equal(Errors.CityIsNull.GetMessage(), exception.Message);
        Assert.Equal(Errors.CityIsNull.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }

    [Fact]
    public void Create_NullLocality_ThrowsCodeDesignPlusException()
    {
        // Act & Assert
        var exception =Assert.Throws<CodeDesignPlusException>(() => Location.Create(Utils.Country, Utils.State, Utils.City, null!, Utils.Neighborhood));

        Assert.Equal(Errors.LocalityIsNull.GetMessage(), exception.Message);
        Assert.Equal(Errors.LocalityIsNull.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }

    [Fact]
    public void Create_NullNeighborhood_ThrowsCodeDesignPlusException()
    {
        // Act & Assert
        var exception =Assert.Throws<CodeDesignPlusException>(() => Location.Create(Utils.Country, Utils.State, Utils.City, Utils.Locality, null!));

        Assert.Equal(Errors.NeighborhoodIsNull.GetMessage(), exception.Message);
        Assert.Equal(Errors.NeighborhoodIsNull.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }
}
