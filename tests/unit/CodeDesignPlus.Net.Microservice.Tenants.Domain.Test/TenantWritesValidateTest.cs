using System.Collections.Generic;
using CodeDesignPlus.Net.Core.Abstractions;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.Test;

/// <summary>
/// Reading a license or a type of document no longer validates it (pendings/737): what a request brings is hydrated
/// without checks, so every write of the aggregate is where the license and the type of document are validated.
/// </summary>
public class TenantWritesValidateTest
{
    private static readonly Instant Now = SystemClock.Instance.GetCurrentInstant();

    /// <summary>
    /// A license as a request or the database hydrates it: starting after its end, without going through the factory.
    /// </summary>
    private static License LicenseStartingAfterItsEnd() => new(Guid.NewGuid(), "Élite", Now.Plus(Duration.FromDays(5)), Now, [], new Dictionary<string, string>());

    private static TenantAggregate Tenant()
    {
        var tenant = TenantAggregate.Create(Guid.NewGuid(), "Conjunto", Utils.TypeDocument, "900123456", null, "3107845123", "admin@example.com", Utils.Location, Utils.License, true, Guid.NewGuid());

        tenant.GetAndClearEvents();

        return tenant;
    }

    [Fact]
    public void Constructor_StartDateAfterEndDate_HydratesWithoutThrowing()
    {
        // Act
        var exception = Record.Exception(() => LicenseStartingAfterItsEnd());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_CodeLongerThanAllowed_HydratesWithoutThrowing()
    {
        // Act
        var exception = Record.Exception(() => new TypeDocument("NITX", "Número de Identificación Tributaria"));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Create_LicenseStartingAfterItsEnd_ThrowsStartDateGreaterThanEndDate()
    {
        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => TenantAggregate.Create(Guid.NewGuid(), "Conjunto", Utils.TypeDocument, "900123456", null, "3107845123", "admin@example.com", Utils.Location, LicenseStartingAfterItsEnd(), true, Guid.NewGuid()));

        // Assert
        Assert.Equal(Errors.LicenseStartDateGreaterThanEndDate.GetCode(), exception.Code);
    }

    [Fact]
    public void Create_TypeDocumentCodeLongerThanAllowed_ThrowsCodeTypeDocumentIsInvalid()
    {
        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => TenantAggregate.Create(Guid.NewGuid(), "Conjunto", new TypeDocument("NITX", "NIT"), "900123456", null, "3107845123", "admin@example.com", Utils.Location, Utils.License, true, Guid.NewGuid()));

        // Assert
        Assert.Equal(Errors.CodeTypeDocumentIsInvalid.GetCode(), exception.Code);
    }

    [Fact]
    public void Update_TypeDocumentCodeLongerThanAllowed_ThrowsCodeTypeDocumentIsInvalid()
    {
        // Arrange
        var tenant = Tenant();

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => tenant.Update("Conjunto", new TypeDocument("NITX", "NIT"), "900123456", null, "3107845123", "admin@example.com", true, Guid.NewGuid()));

        // Assert
        Assert.Equal(Errors.CodeTypeDocumentIsInvalid.GetCode(), exception.Code);
    }

    [Fact]
    public void UpdateLicense_StartDateAfterEndDate_ThrowsStartDateGreaterThanEndDate()
    {
        // Arrange
        var tenant = Tenant();

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => tenant.UpdateLicense(LicenseStartingAfterItsEnd(), Guid.NewGuid()));

        // Assert
        Assert.Equal(Errors.LicenseStartDateGreaterThanEndDate.GetCode(), exception.Code);
    }

    [Fact]
    public void UpdateLicense_StartDateAfterEndDate_KeepsTheStoredLicense()
    {
        // Arrange
        var tenant = Tenant();

        // Act
        Record.Exception(() => tenant.UpdateLicense(LicenseStartingAfterItsEnd(), Guid.NewGuid()));

        // Assert
        Assert.Same(Utils.License, tenant.License);
    }

    [Fact]
    public void ChangeLicense_StartDateAfterEndDate_ThrowsStartDateGreaterThanEndDate()
    {
        // Arrange
        var tenant = Tenant();

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => tenant.ChangeLicense(LicenseStartingAfterItsEnd(), Guid.NewGuid()));

        // Assert
        Assert.Equal(Errors.LicenseStartDateGreaterThanEndDate.GetCode(), exception.Code);
    }

    [Fact]
    public void RenewLicense_RenewedPeriodStartingAfterItsEnd_ThrowsStartDateGreaterThanEndDate()
    {
        // Arrange
        var tenant = Tenant();
        var renewed = new License(Guid.NewGuid(), "Élite", Now.Plus(Duration.FromDays(400)), Now.Plus(Duration.FromDays(300)), [], new Dictionary<string, string>());

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => tenant.RenewLicense(renewed, Guid.NewGuid()));

        // Assert
        Assert.Equal(Errors.LicenseStartDateGreaterThanEndDate.GetCode(), exception.Code);
    }
}
