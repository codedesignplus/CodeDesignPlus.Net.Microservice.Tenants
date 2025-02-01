namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.UpdateTenant;

public class UpdateTenantCommandHandler(ITenantRepository repository, IUserContext user, IPubSub pubsub) : IRequestHandler<UpdateTenantCommand>
{    
    public async Task Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var tenant = await repository.FindAsync<TenantAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsNull(tenant, Errors.TenantNotFound);

        tenant.Update(request.Name, request.Domain, request.IsActive, user.IdUser);
        tenant.UpdateLocation(request.Location, user.IdUser);
        tenant.UpdateLicense(request.License, user.IdUser);

        await repository.UpdateAsync(tenant, cancellationToken);

        await pubsub.PublishAsync(tenant.GetAndClearEvents(), cancellationToken);
    }
}