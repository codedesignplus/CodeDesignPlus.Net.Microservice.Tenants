namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.DeleteTenant;

[DtoGenerator]
public record DeleteTenantCommand(Guid Id) : IRequest;

public class Validator : AbstractValidator<DeleteTenantCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
    }
}
