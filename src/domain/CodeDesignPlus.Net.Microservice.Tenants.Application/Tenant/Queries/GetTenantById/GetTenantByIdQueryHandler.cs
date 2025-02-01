namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetTenantById;

public class GetTenantByIdQueryHandler(ITenantRepository repository, IMapper mapper, ICacheManager cacheManager) : IRequestHandler<GetTenantByIdQuery, TenantDto>
{
    public async Task<TenantDto> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var exists = await cacheManager.ExistsAsync(request.Id.ToString());

        if (exists)
            return await cacheManager.GetAsync<TenantDto>(request.Id.ToString());

        var tenant = await repository.FindAsync<TenantAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsNull(tenant, Errors.TenantNotFound);

        await cacheManager.SetAsync(request.Id.ToString(), mapper.Map<TenantDto>(tenant));

        return mapper.Map<TenantDto>(tenant);
    }
}
