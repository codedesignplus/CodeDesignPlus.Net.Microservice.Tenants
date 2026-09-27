using CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Helpers;
using CodeDesignPlus.Net.Mongo.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Test.Repositories;

/// <summary>
/// The writes on a deleted tenant, against a real Mongo: the SDK leaves deleted documents out of its own methods,
/// so these go to the collection directly and must hold their conditions by themselves.
/// </summary>
[Collection(MongoContainerFixture.Collection)]
public class TenantRepositoryTest
{
    private static readonly Duration Retention = Duration.FromDays(30);

    private readonly ITenantRepository repository;

    public TenantRepositoryTest(MongoContainerFixture fixture)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Mongo:Enable"] = "true",
                ["Mongo:ConnectionString"] = fixture.ConnectionString,
                ["Mongo:Database"] = $"db-ms-tenants-{Guid.NewGuid():N}",
                ["Mongo:RegisterHealthCheck"] = "false",
            })
            .Build();

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddMongo<Startup>(configuration);

        this.repository = services.BuildServiceProvider().GetRequiredService<ITenantRepository>();
    }

    private static TenantAggregate NewTenant() => TenantAggregate.Create(Guid.NewGuid(), "Test Tenant", Utils.TypeDocument, Guid.NewGuid().ToString("N")[..10], null, "3107845123", "fake@fake.com", Utils.Location, Utils.License, true, Guid.NewGuid());

    private async Task<TenantAggregate> DeletedTenantAsync(Duration retention)
    {
        var tenant = NewTenant();

        await repository.CreateAsync(tenant, CancellationToken.None);

        tenant.Delete(Guid.NewGuid(), retention);

        await repository.UpdateAsync(tenant, CancellationToken.None);

        return tenant;
    }

    [Fact]
    public async Task UpdateAsync_DeletedTenant_KeepsTheDocumentOutOfTheSdkQueries()
    {
        // Arrange
        var tenant = await DeletedTenantAsync(Retention);

        // Act
        var found = await repository.FindAsync<TenantAggregate>(tenant.Id, CancellationToken.None);
        var deleted = await repository.FindDeletedAsync(tenant.Id, CancellationToken.None);

        // Assert
        Assert.Null(found);
        Assert.NotNull(deleted);
        Assert.Equal(tenant.PurgeAfter!.Value.ToUnixTimeMilliseconds(), deleted.PurgeAfter!.Value.ToUnixTimeMilliseconds());
    }

    [Fact]
    public async Task FindDueForPurgeAsync_OnlyTenantsWhoseRetentionEnded()
    {
        // Arrange
        var due = await DeletedTenantAsync(Duration.FromTicks(1));
        var notDue = await DeletedTenantAsync(Retention);
        var active = NewTenant();

        await repository.CreateAsync(active, CancellationToken.None);

        // Act
        var tenants = await repository.FindDueForPurgeAsync(SystemClock.Instance.GetCurrentInstant(), 100, CancellationToken.None);

        // Assert
        Assert.Contains(tenants, x => x.Id == due.Id);
        Assert.DoesNotContain(tenants, x => x.Id == notDue.Id);
        Assert.DoesNotContain(tenants, x => x.Id == active.Id);
    }

    [Fact]
    public async Task RestoreAsync_DeletedTenant_MakesItVisibleAgain()
    {
        // Arrange
        var tenant = await DeletedTenantAsync(Retention);

        tenant.Restore(Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant());

        // Act
        var restored = await repository.RestoreAsync(tenant, CancellationToken.None);

        // Assert
        Assert.True(restored);
        Assert.NotNull(await repository.FindAsync<TenantAggregate>(tenant.Id, CancellationToken.None));
    }

    [Fact]
    public async Task RestoreAsync_PurgedMeanwhile_DoesNotBringTheDocumentBack()
    {
        // Arrange
        var tenant = await DeletedTenantAsync(Retention);

        await repository.PurgeAsync(tenant.Id, CancellationToken.None);

        tenant.Restore(Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant());

        // Act
        var restored = await repository.RestoreAsync(tenant, CancellationToken.None);

        // Assert
        Assert.False(restored);
        Assert.Null(await repository.FindAsync<TenantAggregate>(tenant.Id, CancellationToken.None));
    }

    [Fact]
    public async Task PurgeAsync_TenantNotDeleted_KeepsIt()
    {
        // Arrange
        var tenant = NewTenant();

        await repository.CreateAsync(tenant, CancellationToken.None);

        // Act
        var purged = await repository.PurgeAsync(tenant.Id, CancellationToken.None);

        // Assert
        Assert.False(purged);
        Assert.NotNull(await repository.FindAsync<TenantAggregate>(tenant.Id, CancellationToken.None));
    }

    [Fact]
    public async Task PurgeAsync_DeletedTenant_DeletesTheDocument()
    {
        // Arrange
        var tenant = await DeletedTenantAsync(Retention);

        // Act
        var purged = await repository.PurgeAsync(tenant.Id, CancellationToken.None);

        // Assert
        Assert.True(purged);
        Assert.Null(await repository.FindDeletedAsync(tenant.Id, CancellationToken.None));
    }
}
