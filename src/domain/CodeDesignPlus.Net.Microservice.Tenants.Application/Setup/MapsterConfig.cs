using CodeDesignPlus.Microservice.Api.Dtos;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.UpdateTenant;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Setup;

public static class MapsterConfigTenant
{
    public static void Configure()
    {
        //Tenant
        TypeAdapterConfig<CreateTenantDto, CreateTenantCommand>.NewConfig();
        TypeAdapterConfig<UpdateTenantDto, UpdateTenantCommand>.NewConfig();
        TypeAdapterConfig<TenantAggregate, TenantDto>.NewConfig();
    }
}
