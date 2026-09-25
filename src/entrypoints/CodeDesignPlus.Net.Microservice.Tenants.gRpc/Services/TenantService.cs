using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.DeleteTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.UpdateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.ExistTenantById;
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

        TenantDto tenant;

        try
        {
            tenant = await mediator.Send(queryCommand, context.CancellationToken);
        }
        catch (CodeDesignPlusException exception) when (exception.Code == Application.Errors.TenantNotFound.GetCode())
        {
            // NotFound, no FailedPrecondition: es la unica forma de que quien pregunta sepa que el tenant no existe
            // y no confundirlo con una caida. El directorio de tenants del SDK responde entonces 400 en vez de 503
            // (pendings/028). El ErrorInterceptor deja pasar la RpcException tal cual.
            throw new RpcException(new Status(StatusCode.NotFound, exception.Message));
        }

        var response = mapper.Map<GetTenantResponse>(tenant);

        return response;

    }

    public override async Task<BoolValue> ExistTenant(ExistTenantRequest request, ServerCallContext context)
    {
        DomainGuard.IsFalse(Guid.TryParse(request.Id, out var idTenant), Errors.TenantIdIsInvalid);

        var queryCommand = new ExistTenantByIdQuery(idTenant);

        var exists = await mediator.Send(queryCommand, context.CancellationToken);

        return new BoolValue { Value = exists };
    }
}