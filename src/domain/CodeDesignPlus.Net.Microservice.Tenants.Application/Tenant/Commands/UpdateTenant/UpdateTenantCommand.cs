using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.UpdateTenant;

[DtoGenerator]
public record UpdateTenantCommand(Guid Id, string Name, Uri? Domain, License License, Location Location, bool IsActive) : IRequest;

public class Validator : AbstractValidator<UpdateTenantCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
        RuleFor(x => x.Name).NotEmpty().NotNull().MaximumLength(100);
        RuleFor(x => x.Domain).NotEmpty().NotNull();
        RuleFor(x => x.License).NotNull();
        RuleFor(x => x.Location).NotNull();
    }
}
