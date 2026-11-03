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
    public static Models.Tenant Map(TenantAggregate tenant)
    {
        // Los atributos efectivos (la licencia más los paquetes que cuentan) salen de una sola función del agregado, la
        // misma que arma el TenantDto de la pantalla: si divergieran, la pantalla y los micros verían cupos distintos
        // (pendings/739).
        var license = tenant.EffectiveLicense();

        return new()
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Domain = tenant.Domain,
            Location = tenant.Location,
            Metadata = license?.Metadata ?? [],
            License = new Models.License
            {
                Id = license?.Id ?? Guid.Empty,
                Name = license?.Name,
                StartDate = license?.StartDate ?? Instant.MinValue,
                ExpirationDate = license?.EndDate ?? Instant.MaxValue,
                Metadata = license?.Metadata ?? [],
                Modules = [.. (license?.Modules ?? []).Select(module => new Models.LicenseModule
                {
                    Id = module.Id,
                    Name = module.Name
                })]
            }
        };
    }
}
