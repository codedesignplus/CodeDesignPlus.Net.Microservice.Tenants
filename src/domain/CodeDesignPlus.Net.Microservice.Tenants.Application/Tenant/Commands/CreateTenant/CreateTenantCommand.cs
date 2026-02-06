using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;

[DtoGenerator]
public record CreateTenantCommand(
    Guid Id,
    string Name,
    TypeDocument TypeDocument,
    string NumberDocument,
    Uri? Domain,
    string Phone,
    string Email,
    Location Location,
    License License,
    Guid IdUser,
    bool IsActive
) : IRequest;

public class Validator : AbstractValidator<CreateTenantCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
        RuleFor(x => x.Name).NotEmpty().NotNull().MaximumLength(128);
        RuleFor(x => x.License).NotNull();
        RuleFor(x => x.Location).NotNull();
        RuleFor(x => x.TypeDocument).NotNull();
        RuleFor(x => x.Email).NotEmpty().NotNull().EmailAddress().MaximumLength(256);
        RuleFor(x => x.NumberDocument).NotEmpty().NotNull().MaximumLength(64);
        RuleFor(x => x.Phone).NotEmpty().NotNull().MaximumLength(32).Matches(@"^\+?[1-9]\d{1,14}$");
    }
}
