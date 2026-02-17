using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.DeleteTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.UpdateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetTenantById;
using CodeDesignPlus.Net.Microservice.Tenants.Infrastructure;
using CodeDesignPlus.Net.Security.Abstractions;
using Google.Protobuf.WellKnownTypes;

namespace CodeDesignPlus.Net.Microservice.Tenants.gRpc.Services;

public class TenantService(IMediator mediator, IMapper mapper, IUserContext user) : Tenant.TenantBase
{
    public override async Task<Empty> CreateTenant(CreateTenantRequest request, ServerCallContext context)
    {
        var command = mapper.Map<CreateTenantCommand>(request) with { IdUser = user.IdUser };

        await mediator.Send(command, context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> UpdateTenant(UpdateTenantRequest request, ServerCallContext context)
    {
        var command = mapper.Map<UpdateTenantCommand>(request);

        await mediator.Send(command, context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> DeleteTenant(DeleteTenantRequest request, ServerCallContext context)
    {
        DomainGuard.IsFalse(Guid.TryParse(request.Id, out var id), Errors.TenantIdIsInvalid);

        var command = new DeleteTenantCommand(id);

        await mediator.Send(command, context.CancellationToken);

        return new Empty();
    }

    public override async Task<GetTenantResponse> GetTenant(GetTenantRequest request, ServerCallContext context)
    {
        DomainGuard.IsFalse(Guid.TryParse(request.Id, out var idTenant), Errors.TenantIdIsInvalid);

        var queryCommand = new GetTenantByIdQuery(idTenant);

        var tenant = await mediator.Send(queryCommand, context.CancellationToken);

        var response = mapper.Map<GetTenantResponse>(tenant);

        return response;

    }

    public override async Task<BoolValue> ExistTenant(ExistTenantRequest request, ServerCallContext context)
    {
        DomainGuard.IsFalse(Guid.TryParse(request.Id, out var idTenant), Errors.TenantIdIsInvalid);

        var queryCommand = new GetTenantByIdQuery(idTenant);

        var tenant = await mediator.Send(queryCommand, context.CancellationToken);

        return new BoolValue { Value = tenant != null };
    }
}