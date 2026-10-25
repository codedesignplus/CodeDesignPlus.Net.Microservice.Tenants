namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;

public class CreateTenantCommandHandler(ITenantRepository repository, IPubSub pubsub, ITenantSnapshotPublisher snapshotPublisher) : IRequestHandler<CreateTenantCommand>
{
    public async Task Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        // Ya guardado por un envío anterior del mismo formulario: el reintento termina bien sin repetir nada (pendings/589).
        if (await repository.ExistsAsync<TenantAggregate>(request.Id, cancellationToken))
            return;

        // Validar que no exista un tenant con el mismo tipo de documento y número de documento
        var existsByDocument = await repository.ExistsByDocumentAsync(request.TypeDocument.Code, request.NumberDocument, cancellationToken);

        ApplicationGuard.IsTrue(existsByDocument, Errors.DuplicateDocument);

        var tenant = TenantAggregate.Create(request.Id, request.Name, request.TypeDocument, request.NumberDocument, request.Domain, request.Phone, request.Email, request.Location, request.License, request.IsActive, request.IdUser);

        // Dos envíos que llegan a la vez pasan los dos la comprobación de arriba; el segundo choca contra el id y no publica.
        if (!await repository.CreateIfAbsentAsync(tenant, cancellationToken))
            return;

        await snapshotPublisher.PublishAsync(tenant, cancellationToken);

        await pubsub.PublishAsync(tenant.GetAndClearEvents(), cancellationToken);
    }
}