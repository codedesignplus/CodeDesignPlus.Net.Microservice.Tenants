
using System;
using Xunit;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.Test.ValueObjects;

public class CurrencyTest
{
    [Fact]
    public void Create_ValidParameters_ShouldCreateCurrency()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "US Dollar";
        var code = "USD";
        var symbol = "$";

        // Act
        var currency = Currency.Create(id, name, code, symbol);

        // Assert
        Assert.Equal(id, currency.Id);
        Assert.Equal(name, currency.Name);
        Assert.Equal(code, currency.Code);
        Assert.Equal(symbol, currency.Symbol);
    }

    [Fact]
    public void Create_EmptyId_ShouldThrowException()
    {
        // Arrange
        var id = Guid.Empty;
        var name = "US Dollar";
        var code = "USD";
        var symbol = "$";

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => Currency.Create(id, name, code, symbol));

        Assert.Equal(Errors.CurrencyIdIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.CurrencyIdIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_NullOrEmptyName_ShouldThrowException(string? name)
    {
        // Arrange
        var id = Guid.NewGuid();
        var code = "USD";
        var symbol = "$";

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => Currency.Create(id, name!, code, symbol));

        Assert.Equal(Errors.CurrencyNameIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.CurrencyNameIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_NullOrEmptyCode_ShouldThrowException(string? code)
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "US Dollar";
        var symbol = "$";

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => Currency.Create(id, name, code!, symbol));
        
        Assert.Equal(Errors.CurrencyCodeIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.CurrencyCodeIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_NullOrEmptySymbol_ShouldThrowException(string? symbol)
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "US Dollar";
        var code = "USD";

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => Currency.Create(id, name, code, symbol!));

        Assert.Equal(Errors.CurrencySymbolIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.CurrencySymbolIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }
}
