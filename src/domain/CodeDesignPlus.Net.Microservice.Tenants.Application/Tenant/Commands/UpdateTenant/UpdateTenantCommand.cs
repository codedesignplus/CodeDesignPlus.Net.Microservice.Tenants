namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.UpdateTenant;

[DtoGenerator]
public record UpdateTenantCommand(Guid Id) : IRequest;

public class Validator : AbstractValidator<UpdateTenantCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
    }
}
