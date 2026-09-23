using Models = CodeDesignPlus.Net.Security.Abstractions.Models;

namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Services;

/// <summary>
/// Convierte el agregado en el snapshot que consume el resto de la plataforma.
/// </summary>
/// <remarks>
/// Vive aparte porque lo usan dos caminos que deben producir <b>exactamente</b> lo mismo: el publicador, que
/// escribe el snapshot en la cache compartida, y el respaldo local, que lo reconstruye desde la base cuando
/// esa cache no lo tiene. Si divergieran, una copropiedad se comportaria distinto segun por donde se
/// resolviera, y eso no lo delata ninguna prueba de las dos por separado.
/// </remarks>
public static class TenantSnapshotMapper
{
    public static Models.Tenant Map(TenantAggregate tenant) => new()
    {
        Id = tenant.Id,
        Name = tenant.Name,
        Domain = tenant.Domain,
        Location = tenant.Location,
        Metadata = tenant.License?.Metadata ?? [],
        License = new Models.License
        {
            Id = tenant.License?.Id ?? Guid.Empty,
            Name = tenant.License?.Name,
            StartDate = tenant.License?.StartDate ?? Instant.MinValue,
            ExpirationDate = tenant.License?.EndDate ?? Instant.MaxValue,
            Metadata = tenant.License?.Metadata ?? [],
            Modules = [.. (tenant.License?.Modules ?? []).Select(module => new Models.LicenseModule
            {
                Id = module.Id,
                Name = module.Name
            })]
        }
    };
}
