namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;

/// <summary>
/// El aprovisionamiento de la copropiedad de un pedido fallo.
/// </summary>
/// <remarks>
/// Lo publica <c>CreateTenantHandler</c> y lo consume ms-licenses para dejar el pedido en
/// <c>PartiallyFailed</c>: es el camino de vuelta de la cadena de compra.
/// <para>
/// <b>Vive en el dominio, no en el worker.</b> Estuvo en <c>AsyncWorker/DomainEvents</c>, que es la carpeta
/// de los <i>gemelos</i> —eventos de otros micros—, y el guardarrail lo leia como un gemelo sin publicador.
/// Por eso se dio por muerto y estuvo a punto de borrarse el consumidor que si lo escucha. Su hermano de
/// exito, <see cref="TenantProvisionedForOrderDomainEvent"/>, siempre estuvo aqui.
/// </para>
/// </remarks>
[EventKey<TenantAggregate>(1, "TenantProvisioningFailedForOrderDomainEvent")]
public class TenantProvisioningFailedForOrderDomainEvent(
    Guid aggregateId,
    Guid orderId,
    string reason,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Guid OrderId { get; } = orderId;
    public string Reason { get; } = reason;

    public static TenantProvisioningFailedForOrderDomainEvent Create(Guid tenantId, Guid orderId, string reason)
        => new(tenantId, orderId, reason);
}
