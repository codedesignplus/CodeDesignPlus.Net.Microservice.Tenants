namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;

public class CreateTenantCommandHandler(ITenantRepository repository, IUserContext user, IPubSub pubsub) : IRequestHandler<CreateTenantCommand>
{
    public async Task Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);
        
        var exist = await repository.ExistsAsync<TenantAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsTrue(exist, Errors.TenantAlreadyExists);

        var tenant = TenantAggregate.Create(request.Id, request.Name, request.Domain, request.License, request.Location, user.IdUser);

        await repository.CreateAsync(tenant, cancellationToken);

        await pubsub.PublishAsync(tenant.GetAndClearEvents(), cancellationToken);
    }
}