using System;
using System.Collections.Generic;
using Xunit;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using NodaTime;
using CodeDesignPlus.Net.Core.Abstractions;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.Test.ValueObjects;

public class LicenseTest
{
    [Fact]
    public void Create_ValidParameters_ShouldCreateLicense()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Valid License Name";
        var startDate = Instant.FromDateTimeUtc(DateTime.UtcNow);
        var endDate = startDate.Plus(Duration.FromDays(30));
        var metadata = new Dictionary<string, string> { { "key", "value" } };

        // Act
        var license = License.Create(id, name, startDate, endDate, [], metadata);

        // Assert
        Assert.Equal(id, license.Id);
        Assert.Equal(name, license.Name);
        Assert.Equal(startDate, license.StartDate);
        Assert.Equal(endDate, license.EndDate);
        Assert.Equal(metadata, license.Metadata);
    }

    [Fact]
    public void Create_InvalidName_ShouldThrowException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Invalid@Name";
        var startDate = Instant.FromDateTimeUtc(DateTime.UtcNow);
        var endDate = startDate.Plus(Duration.FromDays(30));
        var metadata = new Dictionary<string, string> { { "key", "value" } };

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => License.Create(id, name, startDate, endDate, [], metadata));

        Assert.Equal(Errors.LicenseNameIsInvalid.With(FieldLength.Name).GetMessage(), exception.Message);
        Assert.Equal(Errors.LicenseNameIsInvalid.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }

    [Fact]
    public void Create_StartDateGreaterThanEndDate_ShouldThrowException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Valid License Name";
        var startDate = Instant.FromDateTimeUtc(DateTime.UtcNow);
        var endDate = startDate.Minus(Duration.FromDays(1));
        var metadata = new Dictionary<string, string> { { "key", "value" } };

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => License.Create(id, name, startDate, endDate, [], metadata));

        Assert.Equal(Errors.LicenseStartDateGreaterThanEndDate.GetMessage(), exception.Message);
        Assert.Equal(Errors.LicenseStartDateGreaterThanEndDate.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }

    [Fact]
    public void Create_MetadataIsNull_ShouldThrowException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Valid License Name";
        var startDate = Instant.FromDateTimeUtc(DateTime.UtcNow);
        var endDate = startDate.Plus(Duration.FromDays(30));
        Dictionary<string, string> metadata = null!;

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => License.Create(id, name, startDate, endDate, [], metadata));

        Assert.Equal(Errors.LicenseMetadataIsNull.GetMessage(), exception.Message);
        Assert.Equal(Errors.LicenseMetadataIsNull.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }
    /// <summary>
    /// La copropiedad recibe los nombres que el catálogo de ms-licenses acepta: antes «Élite Plus · …», «Plan 2026» y «Élite+»
    /// lanzaban el error 129 y el cambio de licencia pagado no llegaba (pendings/738).
    /// </summary>
    [Theory]
    [InlineData("Élite Plus · Conjunto Prueba Licencias")]
    [InlineData("Plan 2026")]
    [InlineData("Élite+")]
    [InlineData("Estándar - Torre (A/B)")]
    public void Create_ACommercialName_IsAccepted(string name)
    {
        // Arrange
        var startDate = Instant.FromUtc(2026, 11, 3, 11, 59);

        // Act
        var license = License.Create(Guid.NewGuid(), name, startDate, startDate.Plus(Duration.FromDays(30)), [], []);

        // Assert
        Assert.Equal(name, license.Name);
    }

    /// <summary>
    /// La regla es la misma que la del catálogo: fuera de ella, o más largo que el estándar de un nombre, se rechaza.
    /// </summary>
    [Theory]
    [InlineData("+++")]
    [InlineData("Élite ★")]
    [InlineData("<b>Élite</b>")]
    public void Create_ANameOutsideTheRule_IsRejected(string name)
    {
        // Arrange
        var startDate = Instant.FromUtc(2026, 11, 3, 11, 59);

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => License.Create(Guid.NewGuid(), name, startDate, startDate.Plus(Duration.FromDays(30)), [], []));

        // Assert
        Assert.Equal(Errors.LicenseNameIsInvalid.GetCode(), exception.Code);
    }

    /// <summary>
    /// La longitud es la estándar de un nombre (regla 48): cabe justo, y uno más ya no.
    /// </summary>
    [Fact]
    public void Create_TheStandardNameLength_IsTheLimit()
    {
        // Arrange
        var startDate = Instant.FromUtc(2026, 11, 3, 11, 59);
        var longest = new string('a', FieldLength.Name);

        // Act
        var license = License.Create(Guid.NewGuid(), longest, startDate, startDate.Plus(Duration.FromDays(30)), [], []);
        var exception = Record.Exception(() => License.Create(Guid.NewGuid(), longest + "a", startDate, startDate.Plus(Duration.FromDays(30)), [], []));

        // Assert
        Assert.Equal((longest, Errors.LicenseNameIsInvalid.GetCode()), (license.Name, (exception as CodeDesignPlusException)?.Code));
    }
}
