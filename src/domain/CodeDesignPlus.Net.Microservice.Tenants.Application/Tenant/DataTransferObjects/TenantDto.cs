using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Location;
using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.DataTransferObjects;

public class TenantDto: IDtoBase
{
    public required Guid Id { get; set; }    
    public string Name { get; set; } = null!;
    public TypeDocument TypeDocument { get; set; } = null!;
    public string NumberDocument { get; set; } = null!;
    public Uri? Domain { get; set; }
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public Location Location { get; set; } = null!;
    /// <summary>
    /// La licencia con los atributos efectivos: los de la licencia combinados con los paquetes que cuentan hoy
    /// (pendings/739). Es la misma que viaja en la foto y en <c>TenantLicenseUpdated</c>; de aquí leen las pantallas.
    /// </summary>
    public License License { get; set; } = null!;

    /// <summary>
    /// La licencia guardada, sin paquetes: la que edita el Super Admin. Mandarle de vuelta <see cref="License"/> sumaría
    /// los paquetes dos veces (pendings/739).
    /// </summary>
    public License? BaseLicense { get; set; }
    public bool IsActive { get; set; }
    public Instant? DeletedAt { get; set; }
    public Instant? PurgeAfter { get; set; }
}