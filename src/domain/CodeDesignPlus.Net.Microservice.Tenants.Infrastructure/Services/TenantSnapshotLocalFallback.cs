using CodeDesignPlus.Net.Microservice.Tenants.Domain.Repositories;
using CodeDesignPlus.Net.Security.Abstractions;
using Models = CodeDesignPlus.Net.Security.Abstractions.Models;

namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Services;

/// <summary>
/// Resuelve el snapshot de una copropiedad <b>desde la base</b>, no por la red.
/// </summary>
/// <remarks>
/// El resto de microservicios resuelve el snapshot pidiendoselo a ms-tenants por gRPC, y para ellos esta
/// bien. Para ms-tenants no: su propio cliente de tenants apunta a <b>si mismo</b>
/// (<c>GRPCCLIENTS__TENANT = http://ms-tenants-grpc...:5001</c>), asi que con la cache fria una peticion que
/// llegue con <c>X-Tenant</c> dispara su propio middleware, no encuentra el snapshot, y se llama a si misma
/// para pedirselo. Esa llamada lleva otra vez la cabecera, y vuelve a empezar.
/// <para>
/// Cada salto de esa cadena escribe su aviso de "no esta en la cache compartida", y de ahi salian las ~600
/// lineas por segundo del incidente del 2026-07-29: 795.416 en 22 minutos. Pendiente 137.
/// </para>
/// <para>
/// El arreglo <b>no</b> es apagar el contexto de copropiedad: esa capa la quieren todos los micros y
/// <c>LicenseMiddleware</c> se encendera sobre ella. Es que el dueno del dato lo lea de donde lo guarda.
/// Aqui no hay red que pueda fallar ni ciclo que cerrar.
/// </para>
/// </remarks>
public class TenantSnapshotLocalFallback(ITenantRepository repository, ILogger<TenantSnapshotLocalFallback> logger) : ITenantSnapshotFallback
{
    public async Task<Models.Tenant> GetAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await repository.FindAsync<TenantAggregate>(tenantId, cancellationToken);

        if (tenant is null)
        {
            logger.LogWarning("Tenant {TenantId} was not found in the local store", tenantId);

            return null;
        }

        return TenantSnapshotMapper.Map(tenant);
    }
}
