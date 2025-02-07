namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetTenantById;

public record GetTenantByIdQuery(Guid Id) : IRequest<TenantDto>;


public class Validator : AbstractValidator<GetTenantByIdQuery>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}