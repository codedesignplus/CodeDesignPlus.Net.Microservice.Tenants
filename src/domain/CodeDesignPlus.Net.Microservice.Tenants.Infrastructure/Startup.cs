using Microsoft.Extensions.DependencyInjection.Extensions;
namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure
{
    public class Startup : IStartup
    {
        public void Initialize(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<Domain.ServiceDomain.ITenantSnapshotPublisher, Services.TenantSnapshotPublisher>();

            // El respaldo del snapshot se resuelve AQUI contra la base, y no con el cliente gRPC que
            // registra el SDK: ese apunta a ms-tenants, o sea a nosotros mismos. Con la cache fria, una
            // peticion que llegue con X-Tenant se llamaba a si misma para resolverse, y cada salto de esa
            // cadena escribia su aviso: 795.416 lineas en 22 minutos el 2026-07-29. Pendiente 137.
            //
            // Va con Replace y no con AddScoped para no depender del orden de arranque: AddGrpcClients
            // registra el suyo tambien, y quien gane no puede ser una casualidad.
            services.Replace(ServiceDescriptor.Scoped<CodeDesignPlus.Net.Security.Abstractions.ITenantSnapshotFallback, Services.TenantSnapshotLocalFallback>());
        }
    }
}
