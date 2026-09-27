namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.RestoreTenant;

[DtoGenerator]
public record RestoreTenantCommand(Guid Id) : IRequest;

public class Validator : AbstractValidator<RestoreTenantCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
    }
}
