namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;

public class CreateTenantCommandHandler(ITenantRepository repository, IUserContext user, IPubSub pubsub) : IRequestHandler<CreateTenantCommand>
{
    public Task Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}