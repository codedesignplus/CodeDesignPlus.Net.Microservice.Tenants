namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetAllTenant;

public record GetAllTenantQuery(C.Criteria Criteria) : IRequest<List<TenantDto>>;

