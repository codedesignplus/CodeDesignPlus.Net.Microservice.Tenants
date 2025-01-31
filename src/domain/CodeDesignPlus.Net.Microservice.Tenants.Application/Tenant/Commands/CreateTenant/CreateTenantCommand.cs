namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;

[DtoGenerator]
public record CreateTenantCommand(Guid Id) : IRequest;

public class Validator : AbstractValidator<CreateTenantCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
    }
}
