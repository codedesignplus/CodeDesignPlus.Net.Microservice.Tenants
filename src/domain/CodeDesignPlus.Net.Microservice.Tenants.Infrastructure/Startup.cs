namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure
{
    public class Startup : IStartup
    {
        public void Initialize(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<Domain.ServiceDomain.ITenantSnapshotPublisher, Services.TenantSnapshotPublisher>();
        }
    }
}
