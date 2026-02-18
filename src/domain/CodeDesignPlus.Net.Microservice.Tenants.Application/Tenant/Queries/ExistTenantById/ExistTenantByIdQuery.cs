namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.ExistTenantById;

public record ExistTenantByIdQuery(Guid Id) : IRequest<bool>;


public class Validator : AbstractValidator<ExistTenantByIdQuery>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}