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
            logger.LogInformation("Tenant {TenantId} already exists. Skipping.", data.TenantDetail.Id);
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

        await mediator.Send(command, token);

        var provisionedEvent = TenantProvisionedForOrderDomainEvent.Create(
            data.TenantDetail.Id,
            data.AggregateId
        );

        await pubsub.PublishAsync(provisionedEvent, token);
    }
}
