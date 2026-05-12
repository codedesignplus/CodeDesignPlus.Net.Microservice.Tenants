namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.Repositories;

public interface ITenantRepository : IRepositoryBase
{
    Task<bool> ExistsByDocumentAsync(string typeDocumentCode, string numberDocument, CancellationToken cancellationToken);
}