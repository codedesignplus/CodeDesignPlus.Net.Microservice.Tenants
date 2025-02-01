namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetAllTenant;

public class GetAllTenantQueryHandler(ITenantRepository repository, IMapper mapper) : IRequestHandler<GetAllTenantQuery, List<TenantDto>>
{
    public async Task<List<TenantDto>> Handle(GetAllTenantQuery request, CancellationToken cancellationToken)
    {        
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var tenants = await repository.MatchingAsync<TenantAggregate>(request.Criteria, cancellationToken);

        return mapper.Map<List<TenantDto>>(tenants);
    }
}
