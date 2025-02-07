using System;
using System.Collections.Generic;
using Xunit;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using NodaTime;

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
        var license = License.Create(id, name, startDate, endDate, metadata);

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
        var exception = Assert.Throws<CodeDesignPlusException>(() => License.Create(id, name, startDate, endDate, metadata));

        Assert.Equal(Errors.LicenseNameIsInvalid.GetMessage(), exception.Message);
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
        var exception = Assert.Throws<CodeDesignPlusException>(() => License.Create(id, name, startDate, endDate, metadata));

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
        var exception = Assert.Throws<CodeDesignPlusException>(() => License.Create(id, name, startDate, endDate, metadata));

        Assert.Equal(Errors.LicenseMetadataIsNull.GetMessage(), exception.Message);
        Assert.Equal(Errors.LicenseMetadataIsNull.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }
}
