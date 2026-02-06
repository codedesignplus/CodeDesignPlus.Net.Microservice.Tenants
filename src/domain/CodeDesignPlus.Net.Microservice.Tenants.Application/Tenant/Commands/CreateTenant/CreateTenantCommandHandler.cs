namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;

public class CreateTenantCommandHandler(ITenantRepository repository, IPubSub pubsub) : IRequestHandler<CreateTenantCommand>
{
    public async Task Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);
        
        var exist = await repository.ExistsAsync<TenantAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsTrue(exist, Errors.TenantAlreadyExists);

        var tenant = TenantAggregate.Create(request.Id, request.Name, request.TypeDocument, request.NumberDocument, request.Domain, request.Phone, request.Email, request.Location, request.License, request.IsActive, request.IdUser);

        await repository.CreateAsync(tenant, cancellationToken);

        await pubsub.PublishAsync(tenant.GetAndClearEvents(), cancellationToken);
    }
}