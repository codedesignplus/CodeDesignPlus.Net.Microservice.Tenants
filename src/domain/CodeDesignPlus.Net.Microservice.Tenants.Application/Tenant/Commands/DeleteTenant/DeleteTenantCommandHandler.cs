using CodeDesignPlus.Net.Microservice.Tenants.Application.Options;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.DeleteTenant;

/// <summary>
/// Deletes a tenant: it stops being usable at once and its data is purged when the retention ends.
/// </summary>
public class DeleteTenantCommandHandler(ITenantRepository repository, IUserContext user, IPubSub pubsub, ITenantSnapshotPublisher snapshotPublisher, IOptions<TenantPurgeOptions> options) : IRequestHandler<DeleteTenantCommand>
{
    public async Task Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<TenantAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.TenantNotFound);

        aggregate.Delete(user.IdUser, options.Value.Retention);

        // The document stays, marked as deleted, so the tenant can be restored; TenantPurgeJob deletes it later.
        await repository.UpdateAsync(aggregate, cancellationToken);

        await snapshotPublisher.RemoveAsync(aggregate.Id, cancellationToken);

        await pubsub.PublishAsync(aggregate.GetAndClearEvents(), cancellationToken);
    }
}
