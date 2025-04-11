using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetAllTenant;

public class GetAllTenantQueryHandler(ITenantRepository repository, IMapper mapper) : IRequestHandler<GetAllTenantQuery, Pagination<TenantDto>>
{
    public async Task<Pagination<TenantDto>> Handle(GetAllTenantQuery request, CancellationToken cancellationToken)
    {        
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var tenants = await repository.MatchingAsync<TenantAggregate>(request.Criteria, cancellationToken);

        return mapper.Map<Pagination<TenantDto>>(tenants);
    }
}
