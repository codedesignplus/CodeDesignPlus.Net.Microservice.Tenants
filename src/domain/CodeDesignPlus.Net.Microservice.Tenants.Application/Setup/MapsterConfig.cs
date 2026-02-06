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
            .MapWith((dto) => new CreateTenantCommand(dto.Id, dto.Name, dto.TypeDocument, dto.NumberDocument, dto.Domain, dto.Phone, dto.Email, dto.Location, dto.License, dto.IdUser, dto.IsActive));

        TypeAdapterConfig<UpdateTenantDto, UpdateTenantCommand>
            .NewConfig()
            .MapWith((dto) => new UpdateTenantCommand(dto.Id, dto.Name, dto.TypeDocument, dto.NumberDocument, dto.Domain, dto.Phone, dto.Email, dto.Location, dto.License, dto.IsActive));

        TypeAdapterConfig<TenantAggregate, TenantDto>
            .NewConfig()
            .MapWith((tenant) => new TenantDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                TypeDocument = tenant.TypeDocument,
                NumberDocument = tenant.NumberDocument,
                Domain = tenant.Domain,
                Email = tenant.Email,
                Phone = tenant.Phone,
                License = tenant.License,
                Location = tenant.Location,
                IsActive = tenant.IsActive
            });
    }
}
