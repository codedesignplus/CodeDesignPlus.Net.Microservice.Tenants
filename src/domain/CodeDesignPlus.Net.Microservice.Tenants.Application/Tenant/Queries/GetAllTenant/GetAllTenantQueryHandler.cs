namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetAllTenant;

public class GetAllTenantQueryHandler(ITenantRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetAllTenantQuery, TenantDto>
{
    public Task<TenantDto> Handle(GetAllTenantQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult<TenantDto>(default!);
    }
}
