namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.RestoreTenant;

/// <summary>
/// Brings back a deleted tenant while its retention has not ended.
/// </summary>
public class RestoreTenantCommandHandler(ITenantRepository repository, IUserContext user, IPubSub pubsub, ITenantSnapshotPublisher snapshotPublisher) : IRequestHandler<RestoreTenantCommand>
{
    public async Task Handle(RestoreTenantCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindDeletedAsync(request.Id, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.TenantNotFound);

        aggregate.Restore(user.IdUser, SystemClock.Instance.GetCurrentInstant());

        var restored = await repository.RestoreAsync(aggregate, cancellationToken);

        ApplicationGuard.IsFalse(restored, Errors.TenantNotFound);

        await snapshotPublisher.PublishAsync(aggregate, cancellationToken);

        await pubsub.PublishAsync(aggregate.GetAndClearEvents(), cancellationToken);
    }
}
