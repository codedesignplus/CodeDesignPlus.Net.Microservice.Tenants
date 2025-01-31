namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Repositories;

public class TenantRepository(IServiceProvider serviceProvider, IOptions<MongoOptions> mongoOptions, ILogger<TenantRepository> logger) 
    : RepositoryBase(serviceProvider, mongoOptions, logger), ITenantRepository
{
   
}