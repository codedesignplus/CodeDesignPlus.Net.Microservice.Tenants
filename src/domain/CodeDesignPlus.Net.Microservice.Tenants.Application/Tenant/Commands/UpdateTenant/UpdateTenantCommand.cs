using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.UpdateTenant;

[DtoGenerator]
public record UpdateTenantCommand(Guid Id, string Name, TypeDocument TypeDocument, string NumberDocument, Uri? Domain, string Phone, Location Location, License License, bool IsActive) : IRequest;

public class Validator : AbstractValidator<UpdateTenantCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
        RuleFor(x => x.Name).NotEmpty().NotNull().MaximumLength(128);
        RuleFor(x => x.Domain).NotEmpty().NotNull();
        RuleFor(x => x.License).NotNull();
        RuleFor(x => x.Location).NotNull();
        RuleFor(x => x.TypeDocument).NotNull();
        RuleFor(x => x.NumberDocument).NotEmpty().NotNull().MaximumLength(64);
        RuleFor(x => x.Phone).NotEmpty().NotNull().MaximumLength(32).Matches(@"^\+?[1-9]\d{1,14}$");
    }
}
