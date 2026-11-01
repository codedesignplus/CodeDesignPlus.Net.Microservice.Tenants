using CodeDesignPlus.Net.Microservice.Tenants.Domain.ServiceDomain;
using MongoDB.Bson;
namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Repositories;

public class TenantRepository(IServiceProvider serviceProvider, IOptions<MongoOptions> mongoOptions, ILogger<TenantRepository> logger)
    : RepositoryBase(serviceProvider, mongoOptions, logger), ITenantRepository
{
    private static readonly FilterDefinition<TenantAggregate> Deleted = Builders<TenantAggregate>.Filter.Eq(x => x.IsDeleted, true);

    public async Task<bool> ExistsByDocumentAsync(string typeDocumentCode, string numberDocument, CancellationToken cancellationToken)
    {
        var collection = GetCollection<TenantAggregate>();

        var filter = ByDocumentFilter(typeDocumentCode, numberDocument);

        var count = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        return count > 0;
    }

    /// <summary>
    /// The tenants with that document, written with or without separators: «901888777-3», «901.888.777-3» and «9018887773» are
    /// the same (pendings/733.155). The stored number is compared with an anchored regular expression, which DocumentDB supports
    /// without collation.
    /// </summary>
    /// <param name="typeDocumentCode">The document type.</param>
    /// <param name="numberDocument">The number as written.</param>
    /// <returns>The filter.</returns>
    public static FilterDefinition<TenantAggregate> ByDocumentFilter(string typeDocumentCode, string numberDocument)
    {
        var pattern = DocumentNumberMatch.Pattern(numberDocument);

        var number = pattern is null
            ? Builders<TenantAggregate>.Filter.Eq(x => x.NumberDocument, numberDocument)
            : Builders<TenantAggregate>.Filter.Regex(x => x.NumberDocument, new BsonRegularExpression(pattern));

        return Builders<TenantAggregate>.Filter.And(Builders<TenantAggregate>.Filter.Eq(x => x.TypeDocument.Code, typeDocumentCode), number);
    }

    public async Task<TenantAggregate?> FindDeletedAsync(Guid id, CancellationToken cancellationToken)
    {
        var filter = Builders<TenantAggregate>.Filter.And(Builders<TenantAggregate>.Filter.Eq(x => x.Id, id), Deleted);

        return await GetCollection<TenantAggregate>().Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<TenantAggregate>> GetDeletedAsync(CancellationToken cancellationToken)
    {
        return GetCollection<TenantAggregate>().Find(Deleted).SortBy(x => x.PurgeAfter).ToListAsync(cancellationToken);
    }

    public Task<List<TenantAggregate>> FindDueForPurgeAsync(Instant now, int limit, CancellationToken cancellationToken)
    {
        var filter = Builders<TenantAggregate>.Filter.And(Deleted, Builders<TenantAggregate>.Filter.Lte(x => x.PurgeAfter, now));

        return GetCollection<TenantAggregate>().Find(filter).SortBy(x => x.PurgeAfter).Limit(limit).ToListAsync(cancellationToken);
    }

    public async Task<bool> RestoreAsync(TenantAggregate tenant, CancellationToken cancellationToken)
    {
        // Condicionado a que siga eliminada: una purga que corrió entretanto no se deshace devolviendo el documento.
        var filter = Builders<TenantAggregate>.Filter.And(Builders<TenantAggregate>.Filter.Eq(x => x.Id, tenant.Id), Deleted);

        var result = await GetCollection<TenantAggregate>().ReplaceOneAsync(filter, tenant, cancellationToken: cancellationToken);

        return result.MatchedCount > 0;
    }

    public async Task<bool> PurgeAsync(Guid id, CancellationToken cancellationToken)
    {
        // Condicionado a que siga eliminada: una copropiedad restaurada entretanto se conserva.
        var filter = Builders<TenantAggregate>.Filter.And(Builders<TenantAggregate>.Filter.Eq(x => x.Id, id), Deleted);

        var result = await GetCollection<TenantAggregate>().DeleteOneAsync(filter, cancellationToken);

        return result.DeletedCount > 0;
    }

    /// <inheritdoc />
    public async Task<bool> CreateIfAbsentAsync(TenantAggregate tenant, CancellationToken cancellationToken)
    {
        try
        {
            await CreateAsync(tenant, cancellationToken);

            return true;
        }
        catch (MongoWriteException exception) when (exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }
}
