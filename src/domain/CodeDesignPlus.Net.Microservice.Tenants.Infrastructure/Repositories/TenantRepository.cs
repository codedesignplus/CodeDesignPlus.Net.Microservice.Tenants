namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Repositories;

public class TenantRepository(IServiceProvider serviceProvider, IOptions<MongoOptions> mongoOptions, ILogger<TenantRepository> logger)
    : RepositoryBase(serviceProvider, mongoOptions, logger), ITenantRepository
{
    public async Task<bool> ExistsByDocumentAsync(string typeDocumentCode, string numberDocument, CancellationToken cancellationToken)
    {
        var collection = GetCollection<TenantAggregate>();

        var filter = Builders<TenantAggregate>.Filter.And(
            Builders<TenantAggregate>.Filter.Eq(x => x.TypeDocument.Code, typeDocumentCode),
            Builders<TenantAggregate>.Filter.Eq(x => x.NumberDocument, numberDocument)
        );

        var count = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        return count > 0;
    }
}