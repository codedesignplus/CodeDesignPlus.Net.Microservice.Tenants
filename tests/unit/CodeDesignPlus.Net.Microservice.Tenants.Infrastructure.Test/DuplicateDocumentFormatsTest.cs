using System.Text.RegularExpressions;
using CodeDesignPlus.Net.Microservice.Tenants.Domain;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ServiceDomain;
using CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Test;

/// <summary>
/// The duplicate check recognizes the same document written with or without separators (pendings/733.155): before, it compared
/// the stored text exactly and «901888777-3», «901.888.777-3» and «9018887773» were three different tenants.
/// </summary>
public class DuplicateDocumentFormatsTest
{
    [Theory]
    [InlineData("901888777-3", true)]
    [InlineData("901.888.777-3", true)]
    [InlineData("9018887773", true)]
    [InlineData(" 901 888 777 - 3 ", true)]
    [InlineData("901888777", false)]
    [InlineData("9018887774", false)]
    [InlineData("19018887773", false)]
    public void ByDocumentFilter_StoredInAnotherFormat_IsTheSameDocument(string stored, bool same)
    {
        // Arrange
        var filter = Render(TenantRepository.ByDocumentFilter("NIT", "901888777-3"));
        var pattern = filter["NumberDocument"].AsBsonRegularExpression.Pattern;

        // Act
        var matches = Regex.IsMatch(stored, pattern);

        // Assert
        Assert.Equal((same, "NIT"), (matches, filter["TypeDocument.Code"].AsString));
    }

    [Fact]
    public void Normalize_RemovesHyphenDotsAndSpaces_AndKeepsTheRest()
    {
        // Act
        var normalized = DocumentNumberMatch.Normalize(" 901.888.777-3 ");

        // Assert
        Assert.Equal("9018887773", normalized);
    }

    private static BsonDocument Render(FilterDefinition<TenantAggregate> filter) =>
        filter.Render(new RenderArgs<TenantAggregate>(BsonSerializer.SerializerRegistry.GetSerializer<TenantAggregate>(), BsonSerializer.SerializerRegistry));
}
