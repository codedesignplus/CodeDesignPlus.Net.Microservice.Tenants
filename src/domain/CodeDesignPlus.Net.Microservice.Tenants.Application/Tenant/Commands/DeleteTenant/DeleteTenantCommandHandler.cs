using CodeDesignPlus.Net.Microservice.Tenants.Application.Options;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.DeleteTenant;

/// <summary>
/// Elimina una copropiedad: deja de poder usarse en el acto y sus datos se purgan al vencer el plazo.
/// </summary>
public class DeleteTenantCommandHandler(ITenantRepository repository, IUserContext user, IPubSub pubsub, ITenantSnapshotPublisher snapshotPublisher, IOptions<TenantPurgeOptions> options) : IRequestHandler<DeleteTenantCommand>
{
    public async Task Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<TenantAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.TenantNotFound);

        aggregate.Delete(user.IdUser, options.Value.Retention);

        // El documento se queda, marcado como eliminado, para poder restaurarla; lo borra después TenantPurgeJob.
        await repository.UpdateAsync(aggregate, cancellationToken);

        await snapshotPublisher.RemoveAsync(aggregate.Id, cancellationToken);

        await pubsub.PublishAsync(aggregate.GetAndClearEvents(), cancellationToken);
    }
}
