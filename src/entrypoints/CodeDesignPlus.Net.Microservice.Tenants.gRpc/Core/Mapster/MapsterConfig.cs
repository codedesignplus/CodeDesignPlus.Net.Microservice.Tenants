using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.UpdateTenant;
using NodaTime;
using NodaTime.Serialization.Protobuf;
using NodaTime.Text;

namespace CodeDesignPlus.Net.Microservice.Tenants.gRpc.Core.Mapster;

public static class MapsterConfig
{
    public static void Configure()
    {
        TypeAdapterConfig.GlobalSettings
            .NewConfig<CreateTenantRequest, CreateTenantCommand>()
            .ConstructUsing(src => new CreateTenantCommand(
                Guid.Parse(src.Id),
                src.Name,
                Domain.ValueObjects.TypeDocument.Create(src.TypeDocument.Code, src.TypeDocument.Name),
                src.NumbreDocument,
                string.IsNullOrEmpty(src.Domain) ? null : new Uri(src.Domain),
                src.Phone,
                src.Email,
                Domain.ValueObjects.Location.Create(
                    Domain.ValueObjects.Country.Create(Guid.Parse(src.Location.Country.Id), src.Location.Country.Name, (ushort)src.Location.Country.Code, src.Location.Country.Timezone, Domain.ValueObjects.Currency.Create(Guid.Parse(src.Location.Country.Currency.Id), src.Location.Country.Currency.Name, src.Location.Country.Currency.Code, src.Location.Country.Currency.Symbol)),
                    Domain.ValueObjects.State.Create(Guid.Parse(src.Location.State.Id), src.Location.State.Name, src.Location.State.Code),
                    Domain.ValueObjects.City.Create(Guid.Parse(src.Location.City.Id), src.Location.City.Name, src.Location.City.Timezone),
                    Domain.ValueObjects.Locality.Create(Guid.Parse(src.Location.Locality.Id), src.Location.Locality.Name),
                    Domain.ValueObjects.Neighborhood.Create(Guid.Parse(src.Location.Neighborhood.Id), src.Location.Neighborhood.Name),
                    src.Location.Address,
                    src.Location.PostalCode
                ),
                Domain.ValueObjects.License.Create(
                    Guid.Parse(src.License.Id),
                    src.License.Name,
                    InstantPattern.General.Parse(src.License.StartDate).Value,
                    InstantPattern.General.Parse(src.License.EndDate).Value,
                    src.License.Metadata.ToDictionary()
                ),
                Guid.Empty,
                src.IsActive
            ));

        TypeAdapterConfig.GlobalSettings
            .NewConfig<UpdateTenantRequest, UpdateTenantCommand>()
            .ConstructUsing(src => new UpdateTenantCommand(
                Guid.Parse(src.Id),
                src.Name,
                Domain.ValueObjects.TypeDocument.Create(src.TypeDocument.Code, src.TypeDocument.Name),
                src.NumbreDocument,
                string.IsNullOrEmpty(src.Domain) ? null : new Uri(src.Domain),
                src.Phone,
                src.Email,
                Domain.ValueObjects.Location.Create(
                    Domain.ValueObjects.Country.Create(Guid.Parse(src.Location.Country.Id), src.Location.Country.Name, (ushort)src.Location.Country.Code, src.Location.Country.Timezone, Domain.ValueObjects.Currency.Create(Guid.Parse(src.Location.Country.Currency.Id), src.Location.Country.Currency.Name, src.Location.Country.Currency.Code, src.Location.Country.Currency.Symbol)),
                    Domain.ValueObjects.State.Create(Guid.Parse(src.Location.State.Id), src.Location.State.Name, src.Location.State.Code),
                    Domain.ValueObjects.City.Create(Guid.Parse(src.Location.City.Id), src.Location.City.Name, src.Location.City.Timezone),
                    Domain.ValueObjects.Locality.Create(Guid.Parse(src.Location.Locality.Id), src.Location.Locality.Name),
                    Domain.ValueObjects.Neighborhood.Create(Guid.Parse(src.Location.Neighborhood.Id), src.Location.Neighborhood.Name),
                    src.Location.Address,
                    src.Location.PostalCode
                ),
                Domain.ValueObjects.License.Create(
                    Guid.Parse(src.License.Id),
                    src.License.Name,
                    InstantPattern.General.Parse(src.License.StartDate).Value,
                    InstantPattern.General.Parse(src.License.EndDate).Value,
                    src.License.Metadata.ToDictionary()
                ),
                src.IsActive
            ));

        TypeAdapterConfig.GlobalSettings
            .NewConfig<TenantDto, GetTenantResponse>();

    }
}