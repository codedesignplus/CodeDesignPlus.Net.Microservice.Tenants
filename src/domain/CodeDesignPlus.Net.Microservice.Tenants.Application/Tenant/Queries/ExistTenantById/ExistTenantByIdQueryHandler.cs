namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.ExistTenantById;

public class ExistTenantByIdQueryHandler(ITenantRepository repository, ICacheManager cacheManager)
    : IRequestHandler<ExistTenantByIdQuery, bool>
{
    public async Task<bool> Handle(ExistTenantByIdQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var exists = await cacheManager.ExistsAsync(request.Id.ToString());

        if(!exists)
            exists = await repository.ExistsAsync<TenantAggregate>(request.Id, cancellationToken);

        return exists;
    }
}
