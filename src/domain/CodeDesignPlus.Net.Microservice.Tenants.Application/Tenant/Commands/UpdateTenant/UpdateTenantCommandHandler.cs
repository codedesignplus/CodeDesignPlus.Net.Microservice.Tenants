namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.UpdateTenant;

public class UpdateTenantCommandHandler(ITenantRepository repository, IUserContext user, IPubSub pubsub) : IRequestHandler<UpdateTenantCommand>
{
    public Task Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}