using System.Text.Json.Serialization;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ServiceDomain;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

/// <summary>
/// La licencia de la copropiedad: su vigencia, sus módulos y sus atributos.
/// </summary>
/// <remarks>
/// El constructor solo hidrata: lo usan Mongo, la caché y la deserialización de las peticiones, y leer nunca valida
/// (pendings/737). Una licencia guardada con datos que hoy se rechazarían se sigue leyendo; lo que la escribe la valida
/// con <see cref="Create"/> o <see cref="EnsureValid"/>.
/// </remarks>
public sealed class License
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Instant StartDate { get; private set; }
    public Instant EndDate { get; private set; }
    public List<ModuleInfo> Modules { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; }

    [JsonConstructor]
    public License(Guid Id, string Name, Instant StartDate, Instant EndDate, List<ModuleInfo> Modules, Dictionary<string, string> Metadata)
    {
        this.Id = Id;
        this.Name = Name;
        this.StartDate = StartDate;
        this.EndDate = EndDate;
        this.Modules = Modules ?? [];
        this.Metadata = Metadata;
    }

    /// <summary>
    /// Crea una licencia válida.
    /// </summary>
    /// <param name="Id">El identificador de la licencia.</param>
    /// <param name="Name">Su nombre.</param>
    /// <param name="StartDate">El inicio de la vigencia.</param>
    /// <param name="EndDate">El fin de la vigencia.</param>
    /// <param name="Modules">Los módulos que incluye.</param>
    /// <param name="Metadata">Sus atributos.</param>
    /// <returns>La licencia.</returns>
    public static License Create(Guid Id, string Name, Instant StartDate, Instant EndDate, List<ModuleInfo> Modules, Dictionary<string, string> Metadata)
    {
        var license = new License(Id, Name, StartDate, EndDate, Modules, Metadata);

        license.EnsureValid();

        return license;
    }

    /// <summary>
    /// Comprueba lo que una licencia tiene que cumplir para guardarse. La llaman la fábrica y el agregado en cada escritura.
    /// </summary>
    public void EnsureValid()
    {
        DomainGuard.GuidIsEmpty(Id, Errors.LicenseIdIsEmpty);
        DomainGuard.IsNullOrEmpty(Name, Errors.LicenseNameIsEmpty);
        DomainGuard.IsGreaterThan(StartDate, EndDate, Errors.LicenseStartDateGreaterThanEndDate);
        DomainGuard.IsNull(Metadata, Errors.LicenseMetadataIsNull);

        // La misma regla que el catálogo de ms-licenses: una licencia que se pudo vender se puede aplicar (pendings/738).
        DomainGuard.IsFalse(LicenseName.IsValid(Name), Errors.LicenseNameIsInvalid.With(FieldLength.Name));
    }
}
