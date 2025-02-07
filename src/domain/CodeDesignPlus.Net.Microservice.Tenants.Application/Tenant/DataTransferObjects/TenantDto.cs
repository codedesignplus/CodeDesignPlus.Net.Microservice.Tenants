using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.DataTransferObjects;

public class TenantDto: IDtoBase
{
    public required Guid Id { get; set; }    
    public string Name { get; set; } = null!;
    public Uri? Domain { get; set; }
    public License License { get; set; } = null!;
    public Location Location { get; set; } = null!;
}