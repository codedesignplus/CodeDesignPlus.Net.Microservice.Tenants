using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetTenantById;
using CodeDesignPlus.Net.Microservice.Tenants.Infrastructure;
using Google.Protobuf.WellKnownTypes;

namespace CodeDesignPlus.Net.Microservice.Tenants.gRpc.Services;

public class TenantService(IMediator mediator, IMapper mapper, ILogger<TenantService> logger) : Tenant.TenantBase
{
    public override async Task<Empty> CreateTenant(CreateTenantRequest request, ServerCallContext context)
    {
        var command = mapper.Map<CreateTenantCommand>(request);

        await mediator.Send(command, context.CancellationToken);

        return new Empty();
    }

    public override async Task<GetTenantResponse> GetTenant(GetTenantRequest request, ServerCallContext context)
    {
        DomainGuard.IsFalse(Guid.TryParse(request.Id, out var idTenant), Errors.TenantIdIsInvalid);

        var queryCommand = new GetTenantByIdQuery(idTenant);

        try
        {

            var tenant = await mediator.Send(queryCommand, context.CancellationToken);

            var response = mapper.Map<GetTenantResponse>(tenant);

            return response;

        }
        catch (CodeDesignPlusException ex)
        {
            logger.LogInformation("Error retrieving tenant with ID {Id}: {Message}", request.Id, ex.Message);
        }

        return null!;
    }
}