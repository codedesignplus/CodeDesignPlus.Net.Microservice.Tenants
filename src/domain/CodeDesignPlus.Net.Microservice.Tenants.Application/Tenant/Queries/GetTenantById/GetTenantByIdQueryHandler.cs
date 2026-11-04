namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetTenantById;

public class GetTenantByIdQueryHandler(ITenantRepository repository, IMapper mapper, ICacheManager cacheManager) : IRequestHandler<GetTenantByIdQuery, TenantDto>
{
    public async Task<TenantDto> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var exists = await cacheManager.ExistsAsync(TenantQueryCacheKeys.ById(request.Id));

        if (exists)
            return await cacheManager.GetAsync<TenantDto>(TenantQueryCacheKeys.ById(request.Id));

        var tenant = await repository.FindAsync<TenantAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsNull(tenant, Errors.TenantNotFound);

        var dto = mapper.Map<TenantDto>(tenant);

        await cacheManager.SetAsync(TenantQueryCacheKeys.ById(request.Id), dto);

        return dto;
    }
}
