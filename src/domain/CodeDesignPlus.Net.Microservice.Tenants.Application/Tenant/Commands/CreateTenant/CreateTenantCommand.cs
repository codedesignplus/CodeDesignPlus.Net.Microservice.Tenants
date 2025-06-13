using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;

[DtoGenerator]
public record CreateTenantCommand(Guid Id, string Name, Uri? Domain, License License, Location Location) : IRequest;

public class Validator : AbstractValidator<CreateTenantCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
        RuleFor(x => x.Name).NotEmpty().NotNull().MaximumLength(128);
        RuleFor(x => x.License).NotNull();
        RuleFor(x => x.Location).NotNull();
    }
}
