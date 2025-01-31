namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetTenantById;

public record GetTenantByIdQuery(Guid Id) : IRequest<TenantDto>;

