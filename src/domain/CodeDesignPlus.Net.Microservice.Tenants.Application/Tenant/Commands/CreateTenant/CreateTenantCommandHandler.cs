namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;

public class CreateTenantCommandHandler(ITenantRepository repository, IUserContext user, IPubSub pubsub) : IRequestHandler<CreateTenantCommand>
{
    public async Task Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);
        
        var exist = await repository.ExistsAsync<TenantAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsTrue(exist, Errors.TenantAlreadyExists);

        var tenant = TenantAggregate.Create(request.Id, request.Name, request.TypeDocument, request.NumberDocument, request.Domain, request.Phone, request.Location, request.License, request.IsActive, user.IdUser);

        await repository.CreateAsync(tenant, cancellationToken);

        await pubsub.PublishAsync(tenant.GetAndClearEvents(), cancellationToken);
    }
}