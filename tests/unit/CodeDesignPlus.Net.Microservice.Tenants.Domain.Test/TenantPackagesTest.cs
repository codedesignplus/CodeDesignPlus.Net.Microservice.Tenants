using System.Collections.Generic;
using System.Linq;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ServiceDomain;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.Test;

/// <summary>
/// Los atributos efectivos de la copropiedad: su licencia combinada con sus paquetes vigentes (pendings/739). Es lo que
/// leen los micros en la foto y en <c>TenantLicenseUpdated</c>, así que <c>useMeetingEntitlements</c> no cambia.
/// </summary>
public class TenantPackagesTest
{
    private static Instant Now => SystemClock.Instance.GetCurrentInstant();

    private static License LicenseUntil(Instant end) => License.Create(Guid.NewGuid(), "Estándar", Now.Minus(Duration.FromDays(10)), end, [], new Dictionary<string, string>
    {
        ["assemblies_number_meetings"] = "2",
        ["assemblies_max_participants"] = "200",
        ["assemblies_enable_record"] = "false",
    });

    private static TenantPackage Basic(Instant start, Instant end, Guid? lineId = null, Guid? replaces = null, string meetings = "3") => new(
        lineId ?? Guid.NewGuid(), Guid.NewGuid(), "Virtual Básico", start, end, replaces,
        [
            new TenantPackageAttribute("assemblies_number_meetings", meetings, "Sum"),
            new TenantPackageAttribute("assemblies_max_participants", "100", "Max"),
            new TenantPackageAttribute("assemblies_enable_record", "true", "Or"),
            new TenantPackageAttribute("assemblies_virtual_enable", "true", "Or"),
        ]);

    private static TenantAggregate Tenant(License license) => TenantAggregate.Create(Guid.NewGuid(), "Conjunto", Utils.TypeDocument, "900123456", null, "3107845123", "admin@example.com", Utils.Location, license, true, Guid.NewGuid());

    /// <summary>
    /// Sum suma, Max toma el mayor y Or hace «o»; la clave que la licencia no trae la pone el paquete.
    /// </summary>
    [Fact]
    public void Combine_LicenseAndAPackage_AppliesEachRule()
    {
        // Arrange
        var license = LicenseUntil(Now.Plus(Duration.FromDays(20)));

        // Act
        var effective = EffectiveAttributes.Combine(license.Metadata, [Basic(Now.Minus(Duration.FromDays(1)), Now.Plus(Duration.FromDays(300)))], license.EndDate, Now);

        // Assert
        Assert.Equal(("5", "200", "true", "true"), (effective["assemblies_number_meetings"], effective["assemblies_max_participants"], effective["assemblies_enable_record"], effective["assemblies_virtual_enable"]));
    }

    /// <summary>
    /// Con la licencia vencida el paquete no cuenta aunque su periodo siga, y vuelve al renovarla.
    /// </summary>
    [Fact]
    public void Combine_LicenseNotPaid_SuspendsThePackage()
    {
        // Arrange
        var license = LicenseUntil(Now.Minus(Duration.FromDays(1)));

        // Act
        var effective = EffectiveAttributes.Combine(license.Metadata, [Basic(Now.Minus(Duration.FromDays(30)), Now.Plus(Duration.FromDays(300)))], license.EndDate, Now);

        // Assert
        Assert.Equal("2", effective["assemblies_number_meetings"]);
    }

    /// <summary>
    /// Una ampliación reemplaza a la línea anterior desde que empieza: no se suman las dos.
    /// </summary>
    [Fact]
    public void Combine_AnExtension_ReplacesThePreviousLine()
    {
        // Arrange
        var license = LicenseUntil(Now.Plus(Duration.FromDays(20)));
        var previous = Basic(Now.Minus(Duration.FromDays(30)), Now.Plus(Duration.FromDays(20)));
        var extension = Basic(Now.Minus(Duration.FromHours(1)), Now.Plus(Duration.FromDays(20)), replaces: previous.LineId, meetings: "6");

        // Act
        var effective = EffectiveAttributes.Combine(license.Metadata, [previous, extension], license.EndDate, Now);

        // Assert
        Assert.Equal("8", effective["assemblies_number_meetings"]);
    }

    /// <summary>
    /// Al sumar un paquete se publica la licencia con los atributos efectivos, y una reentrega no repite nada.
    /// </summary>
    [Fact]
    public void ApplyPackages_TwiceTheSameLine_PublishesTheEffectiveLicenseOnce()
    {
        // Arrange
        var tenant = Tenant(LicenseUntil(Now.Plus(Duration.FromDays(20))));
        var package = Basic(Now.Minus(Duration.FromMinutes(1)), Now.Plus(Duration.FromDays(20)));
        tenant.GetAndClearEvents();

        // Act
        var first = tenant.ApplyPackages([package], Now, Guid.NewGuid());
        var second = tenant.ApplyPackages([package], Now, Guid.NewGuid());

        // Assert
        var @event = Assert.IsType<TenantLicenseUpdatedDomainEvent>(tenant.GetAndClearEvents().Single());
        Assert.Equal((true, false, "5", "2"), (first, second, @event.License.Metadata["assemblies_number_meetings"], tenant.License.Metadata["assemblies_number_meetings"]));
    }

    /// <summary>
    /// Cuando el paquete vence, la reconciliación lo nota sin que llegue ningún evento y avisa con la licencia sola.
    /// </summary>
    [Fact]
    public void RefreshEffectiveLicense_APackageThatExpired_PublishesTheChange()
    {
        // Arrange
        var tenant = Tenant(LicenseUntil(Now.Plus(Duration.FromDays(60))));
        tenant.ApplyPackages([Basic(Now.Minus(Duration.FromDays(1)), Now.Plus(Duration.FromDays(5)))], Now, Guid.NewGuid());
        tenant.GetAndClearEvents();

        // Act
        var changed = tenant.RefreshEffectiveLicense(Now.Plus(Duration.FromDays(6)), Guid.NewGuid());

        // Assert
        var @event = Assert.IsType<TenantLicenseUpdatedDomainEvent>(tenant.GetAndClearEvents().Single());
        Assert.Equal((true, "2"), (changed, @event.License.Metadata["assemblies_number_meetings"]));
    }

    /// <summary>
    /// Sin paquetes, la reconciliación no avisa nada: al publicar el micro no salen avisos de todas las copropiedades.
    /// </summary>
    [Fact]
    public void RefreshEffectiveLicense_WithoutPackages_DoesNothing()
    {
        // Arrange
        var tenant = Tenant(LicenseUntil(Now.Plus(Duration.FromDays(20))));
        tenant.GetAndClearEvents();

        // Act
        var changed = tenant.RefreshEffectiveLicense(Now, Guid.NewGuid());

        // Assert
        Assert.Equal((false, 0), (changed, tenant.GetAndClearEvents().Count));
    }

    /// <summary>
    /// Una copropiedad guardada antes de los paquetes se sigue leyendo (regla 31).
    /// </summary>
    [Fact]
    public void Deserialize_ATenantStoredBeforePackages_StillReads()
    {
        // Arrange
        CodeDesignPlus.Net.Mongo.Extensions.MongoSerializerRegistration.RegisterSerializers();

        var document = Tenant(LicenseUntil(Now.Plus(Duration.FromDays(20)))).ToBsonDocument();
        document.Remove("Packages");
        document.Remove("EffectiveMetadata");

        // Act
        var stored = BsonSerializer.Deserialize<TenantAggregate>(document);

        // Assert
        Assert.Equal("2", stored.EffectiveLicense().Metadata["assemblies_number_meetings"]);
    }
}
