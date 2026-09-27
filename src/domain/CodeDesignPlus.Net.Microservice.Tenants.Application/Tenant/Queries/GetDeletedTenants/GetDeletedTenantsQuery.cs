namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetDeletedTenants;

public record GetDeletedTenantsQuery() : IRequest<List<TenantDto>>;
