using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetAllTenant;

public record GetAllTenantQuery(C.Criteria Criteria) : IRequest<Pagination<TenantDto>>;
