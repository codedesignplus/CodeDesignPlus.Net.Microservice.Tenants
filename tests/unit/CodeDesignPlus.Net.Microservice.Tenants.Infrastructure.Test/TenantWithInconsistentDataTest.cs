using CodeDesignPlus.Net.Core.Abstractions;
using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Exceptions.Extensions;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Services;
using CodeDesignPlus.Net.Mongo.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using VoFinancial = CodeDesignPlus.Net.ValueObjects.Financial;
using VoLocation = CodeDesignPlus.Net.ValueObjects.Location;

namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Test;

/// <summary>
/// A tenant stored with data its write paths would reject (a license edited by hand to start after it ends) is still
/// read: reading never runs the creation guards. Before the fix <c>GET /Tenant/{id}</c> answered 500 and the session of
/// the frontend lost its license and currency (pendings/737).
/// </summary>
public class TenantWithInconsistentDataTest
{
    // NodaTime serializers are registered by the application startup; without them the test fails on the dates.
    static TenantWithInconsistentDataTest() => MongoSerializerRegistration.RegisterSerializers();

    private static readonly Instant Now = SystemClock.Instance.GetCurrentInstant();

    private static TenantAggregate ValidTenant()
    {
        var currency = VoFinancial.Currency.Create(Guid.NewGuid(), "Peso colombiano", "COP", "$", 2, 170);
        var location = VoLocation.Location.Create(
            VoLocation.Country.Create(Guid.NewGuid(), "Colombia", "CO", "COL", 170, "+57", "America/Bogota", currency),
            VoLocation.State.Create(Guid.NewGuid(), "Cundinamarca", "CUN"),
            VoLocation.City.Create(Guid.NewGuid(), "Chía", "America/Bogota"),
            null,
            null,
            "Calle 10 # 5-20",
            "250001");

        var license = License.Create(Guid.NewGuid(), "Élite", Now.Minus(Duration.FromDays(5)), Now.Plus(Duration.FromDays(30)), [], new Dictionary<string, string> { ["meetings.perYear"] = "12" });

        var tenant = TenantAggregate.Create(Guid.NewGuid(), "Conjunto Chía", TypeDocument.Create("NIT", "Número de Identificación Tributaria"), "901234567", null, "+573000000001", "admin@conjunto.com", location, license, true, Guid.NewGuid());

        tenant.GetAndClearEvents();

        return tenant;
    }

    /// <summary>
    /// The stored document with the license start and end swapped: what the hand edit left in db-ms-tenants.
    /// </summary>
    private static BsonDocument DocumentWithLicenseStartingAfterItsEnd()
    {
        var document = ValidTenant().ToBsonDocument();
        var license = document["License"].AsBsonDocument;

        (license["StartDate"], license["EndDate"]) = (license["EndDate"], license["StartDate"]);

        return document;
    }

    private static TenantAggregate TenantWithLicenseStartingAfterItsEnd() => BsonSerializer.Deserialize<TenantAggregate>(DocumentWithLicenseStartingAfterItsEnd());

    [Fact]
    public void Deserialize_LicenseStartingAfterItsEnd_ReadsTheTenant()
    {
        // Act
        var tenant = TenantWithLicenseStartingAfterItsEnd();

        // Assert
        Assert.True(tenant.License.StartDate > tenant.License.EndDate);
    }

    [Fact]
    public void EffectiveLicense_LicenseStartingAfterItsEnd_DoesNotThrow()
    {
        // Arrange
        var tenant = TenantWithLicenseStartingAfterItsEnd();

        // Act
        var exception = Record.Exception(() => tenant.EffectiveLicense());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Map_LicenseStartingAfterItsEnd_BuildsTheSnapshotWithTheStoredDates()
    {
        // Arrange
        var tenant = TenantWithLicenseStartingAfterItsEnd();

        // Act
        var snapshot = TenantSnapshotMapper.Map(tenant);

        // Assert
        Assert.Equal(tenant.License.EndDate, snapshot.License.ExpirationDate);
    }

    [Fact]
    public void RefreshEffectiveLicense_LicenseStartingAfterItsEnd_DoesNotThrow()
    {
        // Arrange
        var tenant = TenantWithLicenseStartingAfterItsEnd();

        // Act
        var exception = Record.Exception(() => tenant.RefreshEffectiveLicense(Now, Guid.NewGuid(), force: true));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Deserialize_TypeDocumentCodeLongerThanAllowed_ReadsTheTenant()
    {
        // Arrange
        var document = ValidTenant().ToBsonDocument();

        document["TypeDocument"]["Code"] = "NITX";

        // Act
        var tenant = BsonSerializer.Deserialize<TenantAggregate>(document);

        // Assert
        Assert.Equal("NITX", tenant.TypeDocument.Code);
    }

    [Fact]
    public void RoundTrip_CachedLicenseStartingAfterItsEnd_ReadsItBack()
    {
        // Arrange
        var license = TenantWithLicenseStartingAfterItsEnd().License;

        // Act
        var back = JsonSerializer.Deserialize<License>(JsonSerializer.Serialize(license));

        // Assert
        Assert.Equal(license.StartDate, back.StartDate);
    }

    [Fact]
    public void ApplyPackages_LicenseStartingAfterItsEnd_ThrowsStartDateGreaterThanEndDate()
    {
        // Arrange
        var tenant = TenantWithLicenseStartingAfterItsEnd();
        var package = new TenantPackage(Guid.NewGuid(), Guid.NewGuid(), "Virtual Básico", Now, Now.Plus(Duration.FromDays(365)), null, [new TenantPackageAttribute("meetings.perYear", "3", "Sum")]);

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => tenant.ApplyPackages([package], Now, Guid.NewGuid()));

        // Assert
        Assert.Equal(Domain.Errors.LicenseStartDateGreaterThanEndDate.GetCode(), exception.Code);
    }
}
