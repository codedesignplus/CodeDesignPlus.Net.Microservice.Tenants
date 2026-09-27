namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Test.Helpers;

/// <summary>
/// Comparte un unico contenedor de Mongo entre todas las pruebas de repositorio.
/// </summary>
[CollectionDefinition(MongoContainerFixture.Collection)]
public class MongoCollectionDefinition : ICollectionFixture<MongoContainerFixture>
{
}
