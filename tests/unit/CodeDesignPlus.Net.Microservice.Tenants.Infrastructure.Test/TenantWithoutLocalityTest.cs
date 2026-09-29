using CodeDesignPlus.Net.Microservice.Tenants.Domain;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using CodeDesignPlus.Net.Mongo.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using VoLocation = CodeDesignPlus.Net.ValueObjects.Location;
using VoFinancial = CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Test;

/// <summary>
/// Una copropiedad de un municipio sin localidades ni barrios se guarda y se vuelve a leer tal cual.
/// </summary>
/// <remarks>
/// La mayoría de los municipios no tiene localidades en el catálogo, y la dirección las exigía: solo se podía crear
/// una copropiedad en Bogotá (pendings/130). La dirección vive dentro del agregado, así que lo que importa es que Mongo
/// la lea de vuelta sin localidad.
/// </remarks>
public class TenantWithoutLocalityTest
{
    // Los serializadores de NodaTime los registra el arranque de la aplicación. Sin ellos la prueba falla por la fecha
    // y no por lo que mira.
    static TenantWithoutLocalityTest() => MongoSerializerRegistration.RegisterSerializers();

    private static TenantAggregate TenantInChia()
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

        var now = SystemClock.Instance.GetCurrentInstant();
        var license = License.Create(Guid.NewGuid(), "Élite", now, now.Plus(Duration.FromDays(30)), [], []);

        return TenantAggregate.Create(Guid.NewGuid(), "Conjunto Chía", TypeDocument.Create("NIT", "Número de Identificación Tributaria"),
            "901234567", null, "+573000000001", "admin@conjunto.com", location, license, true, Guid.NewGuid());
    }

    [Fact]
    public void Create_WithoutLocalityAndNeighborhood_KeepsThemNull()
    {
        var tenant = TenantInChia();

        Assert.Null(tenant.Location.Locality);
        Assert.Null(tenant.Location.Neighborhood);
    }

    [Fact]
    public void Deserialize_TenantWithoutLocality_ReadsBackTheSameLocation()
    {
        var tenant = TenantInChia();

        var back = BsonSerializer.Deserialize<TenantAggregate>(tenant.ToBsonDocument());

        Assert.Equal("Chía", back.Location.City.Name);
        Assert.Null(back.Location.Locality);
        Assert.Null(back.Location.Neighborhood);
        Assert.Equal(tenant.Location, back.Location);
    }
}
