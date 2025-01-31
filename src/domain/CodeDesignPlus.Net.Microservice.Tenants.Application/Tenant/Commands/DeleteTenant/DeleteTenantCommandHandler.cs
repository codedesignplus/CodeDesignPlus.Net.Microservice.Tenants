namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.DeleteTenant;

public class DeleteTenantCommandHandler(ITenantRepository repository, IUserContext user, IPubSub pubsub) : IRequestHandler<DeleteTenantCommand>
{
    public Task Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}