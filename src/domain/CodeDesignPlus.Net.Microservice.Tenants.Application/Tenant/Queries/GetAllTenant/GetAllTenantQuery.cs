namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetAllTenant;

public record GetAllTenantQuery(Guid Id) : IRequest<TenantDto>;

