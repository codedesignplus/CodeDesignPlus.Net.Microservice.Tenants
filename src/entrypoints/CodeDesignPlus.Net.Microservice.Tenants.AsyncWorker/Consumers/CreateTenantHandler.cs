using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.DomainEvents;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.DomainEvents;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.Repositories;
using MediatR;

namespace CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Consumers;

[QueueName<TenantAggregate>("CreateTenantHandler")]
public class CreateTenantHandler(IMediator mediator, ITenantRepository tenantRepository, IPubSub pubsub, ILogger<CreateTenantHandler> logger) : IEventHandler<OrderPaidAndReadyForProvisioningDomainEvent>
{
    public async Task HandleAsync(OrderPaidAndReadyForProvisioningDomainEvent data, CancellationToken token)
    {
        var exists = await tenantRepository.ExistsAsync<TenantAggregate>(data.TenantDetail.Id, token);

        if (exists)
        {
            logger.LogInformation("Tenant {TenantId} already exists. Publishing success (idempotent).", data.TenantDetail.Id);

            var provisionedEvent = TenantProvisionedForOrderDomainEvent.Create(
                data.TenantDetail.Id,
                data.AggregateId
            );
            await pubsub.PublishAsync(provisionedEvent, token);
            return;
        }

        logger.LogInformation("Handling OrderPaidAndReadyForProvisioningDomainEvent for TenantId: {TenantId}", data.TenantDetail.Id);

        var modules = data.License.Modules?
            .Select(m => new Domain.ValueObjects.ModuleInfo(m.Id, m.Name))
            .ToList() ?? [];

        var command = new CreateTenantCommand(
            data.TenantDetail.Id,
            data.TenantDetail.Name,
            data.TenantDetail.TypeDocument,
            data.TenantDetail.NumberDocument,
            string.IsNullOrEmpty(data.TenantDetail.Web) ? null : new Uri(data.TenantDetail.Web),
            data.TenantDetail.Phone,
            data.TenantDetail.Email,
            data.TenantDetail.Location,
            Domain.ValueObjects.License.Create(data.License.Id, data.License.Name, data.License.StartDate, data.License.EndDate, modules, data.License.Metadata),
            data.BuyerId,
            true
        );

        try
        {
            await mediator.Send(command, token);
        }
        catch (CodeDesignPlusException ex)
        {
            logger.LogWarning(ex, "Tenant creation failed for Order {OrderId}: {Message}", data.AggregateId, ex.Message);

            var failedEvent = TenantProvisioningFailedForOrderDomainEvent.Create(
                data.TenantDetail.Id,
                data.AggregateId,
                ex.Message
            );
            await pubsub.PublishAsync(failedEvent, token);
            return;
        }

        var provisionedSuccessEvent = TenantProvisionedForOrderDomainEvent.Create(
            data.TenantDetail.Id,
            data.AggregateId
        );

        await pubsub.PublishAsync(provisionedSuccessEvent, token);
    }
}
