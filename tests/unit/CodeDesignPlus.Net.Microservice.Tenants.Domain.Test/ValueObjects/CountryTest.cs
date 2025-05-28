using System;
using Xunit;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.Test.ValueObjects;

public class CountryTest
{
    [Fact]
    public void Create_ValidParameters_ReturnsCountry()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "United States";
        var code = (ushort)625;
        var timezone = "America/New_York";
        var currency = Currency.Create(Guid.NewGuid(), "USD", "Dollar", "$");

        // Act
        var country = Country.Create(id, name, code, timezone, currency);

        // Assert
        Assert.Equal(id, country.Id);
        Assert.Equal(name, country.Name);
        Assert.Equal(code, country.Code);
        Assert.Equal(timezone, country.TimeZone);
        Assert.Equal(currency, country.Currency);
    }

    [Fact]
    public void Create_IdIsEmpty_ThrowsException()
    {
        // Arrange
        var id = Guid.Empty;
        var name = "United States";
        var code = (ushort)625;
        var timezone = "America/New_York";
        var currency = Currency.Create(Guid.NewGuid(), "USD", "Dollar", "$");

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => Country.Create(id, name, code, timezone, currency));

        Assert.Equal(Errors.CountryIdIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.CountryIdIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }

    [Fact]
    public void Create_NameIsEmpty_ThrowsException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = string.Empty;
        var code = (ushort)625;
        var timezone = "America/New_York";
        var currency = Currency.Create(Guid.NewGuid(), "USD", "Dollar", "$");

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => Country.Create(id, name, code, timezone, currency));

        Assert.Equal(Errors.CountryNameIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.CountryNameIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }

    [Fact]
    public void Create_CodeIsEmpty_ThrowsException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "United States";
        var code = (ushort)0;
        var timezone = "America/New_York";
        var currency = Currency.Create(Guid.NewGuid(), "USD", "Dollar", "$");

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => Country.Create(id, name, code, timezone, currency));

        Assert.Equal(Errors.CountryCodeIsInvalid.GetMessage(), exception.Message);
        Assert.Equal(Errors.CountryCodeIsInvalid.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }

    [Fact]
    public void Create_TimeZoneIsEmpty_ThrowsException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "United States";
        var code = (ushort)625;
        var timezone = string.Empty;
        var currency = Currency.Create(Guid.NewGuid(), "USD", "Dollar", "$");

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => Country.Create(id, name, code, timezone, currency));

        Assert.Equal(Errors.CountryTimeZoneIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.CountryTimeZoneIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }
}
