using CodeDesignPlus.Net.Core.Abstractions;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Options;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Setup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application
{
    public class Startup : IStartup
    {
        public void Initialize(IServiceCollection services, IConfiguration configuration)
        {
            MapsterConfigTenant.Configure();

            services.AddSingleton<IValidateOptions<TenantPurgeOptions>, TenantPurgeOptionsValidator>();

            services.AddOptions<TenantPurgeOptions>()
                .Bind(configuration.GetSection(TenantPurgeOptions.Section))
                .ValidateOnStart();
        }
    }
}
