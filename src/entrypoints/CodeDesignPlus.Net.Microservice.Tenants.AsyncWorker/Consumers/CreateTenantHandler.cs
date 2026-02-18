using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.DomainEvents;
using MediatR;

namespace CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Consumers;

[QueueName<TenantAggregate>("CreateTenantHandler")]
public class CreateTenantHandler(IMediator mediator, ILogger<CreateTenantHandler> logger) : IEventHandler<OrderPaidAndReadyForProvisioningDomainEvent>
{
    public Task HandleAsync(OrderPaidAndReadyForProvisioningDomainEvent data, CancellationToken token)
    {
        logger.LogInformation("Handling OrderPaidAndReadyForProvisioningDomainEvent for TenantId: {TenantId} - {@TenantDetail}", data.TenantDetail.Id, data.TenantDetail);
        var command = new CreateTenantCommand(
            data.TenantDetail.Id,
            data.TenantDetail.Name,
            data.TenantDetail.TypeDocument,
            data.TenantDetail.NumberDocument,
            string.IsNullOrEmpty(data.TenantDetail.Web) ? null : new Uri(data.TenantDetail.Web),
            data.TenantDetail.Phone,
            data.TenantDetail.Email,
            data.TenantDetail.Location,
            Domain.ValueObjects.License.Create(data.License.Id, data.License.Name, data.License.StartDate, data.License.EndDate, data.License.Metadata),
            data.BuyerId,
            true
        );

        return mediator.Send(command, token);
    }
}
