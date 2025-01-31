namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetTenantById;

public class GetTenantByIdQueryHandler(ITenantRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetTenantByIdQuery, TenantDto>
{
    public Task<TenantDto> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult<TenantDto>(default!);
    }
}
