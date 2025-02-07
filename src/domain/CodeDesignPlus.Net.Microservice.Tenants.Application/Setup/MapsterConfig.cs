using CodeDesignPlus.Microservice.Api.Dtos;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.UpdateTenant;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Setup;

public static class MapsterConfigTenant
{
    public static void Configure()
    {
        //Tenant
        TypeAdapterConfig<CreateTenantDto, CreateTenantCommand>
            .NewConfig()
            .MapWith((dto) => new CreateTenantCommand(dto.Id, dto.Name, dto.Domain, dto.License, dto.Location));

        TypeAdapterConfig<UpdateTenantDto, UpdateTenantCommand>
            .NewConfig()
            .MapWith((dto) => new UpdateTenantCommand(dto.Id, dto.Name, dto.Domain, dto.License, dto.Location, dto.IsActive));

        TypeAdapterConfig<TenantAggregate, TenantDto>
            .NewConfig()
            .MapWith((tenant) => new TenantDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Domain = tenant.Domain,
                License = tenant.License,
                Location = tenant.Location
            });
    }
}
