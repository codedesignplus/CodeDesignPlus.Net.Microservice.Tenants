namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.DeleteTenant;

public class DeleteTenantCommandHandler(ITenantRepository repository, IUserContext user, IPubSub pubsub) : IRequestHandler<DeleteTenantCommand>
{    
    public async Task Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<TenantAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.TenantNotFound);

        aggregate.Delete(user.IdUser);

        await repository.DeleteAsync<TenantAggregate>(aggregate.Id, cancellationToken);

        await pubsub.PublishAsync(aggregate.GetAndClearEvents(), cancellationToken);
    }
}