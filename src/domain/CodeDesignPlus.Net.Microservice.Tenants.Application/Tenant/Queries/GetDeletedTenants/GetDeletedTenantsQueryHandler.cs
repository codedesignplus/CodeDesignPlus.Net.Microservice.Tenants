namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetDeletedTenants;

/// <summary>
/// Lists the deleted tenants that can still be restored.
/// </summary>
public class GetDeletedTenantsQueryHandler(ITenantRepository repository, IMapper mapper) : IRequestHandler<GetDeletedTenantsQuery, List<TenantDto>>
{
    public async Task<List<TenantDto>> Handle(GetDeletedTenantsQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var tenants = await repository.GetDeletedAsync(cancellationToken);

        return mapper.Map<List<TenantDto>>(tenants);
    }
}
