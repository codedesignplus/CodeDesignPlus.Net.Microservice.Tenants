using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Options;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.DeleteTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.RestoreTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.UpdateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Helpers;
using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Tenant.Commands;

/// <summary>
/// Ningún comando que escribe el documento de una copropiedad acepta un tipo con dígito de verificación sin guion o con
/// otro dígito, y la configuración del despliegue es la que dice qué tipos y con qué pesos (pendings/744). Eliminar y
/// restaurar no lo escriben: funcionan con una copropiedad guardada antes de la regla.
/// </summary>
public class TenantDocumentCheckDigitTest
{
    private static readonly IOptions<TenantDocumentOptions> Configured = Microsoft.Extensions.Options.Options.Create(new TenantDocumentOptions
    {
        CheckDigitDocumentTypes = ["31"],
        CheckDigitWeights = [3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71]
    });

    private static readonly TypeDocument TaxId = TypeDocument.Create("31", "Tax identification number");

    private readonly Mock<ITenantRepository> repository = new();
    private readonly Mock<IPubSub> pubSub = new();
    private readonly Mock<IUserContext> userContext = new();
    private readonly Mock<ITenantSnapshotPublisher> snapshots = new();

    public TenantDocumentCheckDigitTest()
    {
        userContext.SetupGet(x => x.IdUser).Returns(Guid.NewGuid());
        repository.Setup(x => x.CreateIfAbsentAsync(It.IsAny<TenantAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    private static CreateTenantCommand CreateCommand(string numberDocument)
    {
        return new CreateTenantCommand(Guid.NewGuid(), "Conjunto", TaxId, numberDocument, null, "3107845123", "admin@example.com", Utils.Location, Utils.License, Guid.NewGuid(), true);
    }

    /// <summary>
    /// Una copropiedad como las guardadas antes de la regla: con el documento sin guion ni dígito.
    /// </summary>
    private TenantAggregate StoredBeforeTheRule()
    {
        var tenant = TenantAggregate.Create(Guid.NewGuid(), "Conjunto", TaxId, "900765432", null, "3107845123", "admin@example.com", Utils.Location, Utils.License, true, Guid.NewGuid(), DocumentCheckDigitRules.None);

        tenant.GetAndClearEvents();

        repository.Setup(x => x.FindAsync<TenantAggregate>(tenant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        return tenant;
    }

    [Theory]
    [InlineData("900765432", "151")]
    [InlineData("9018887773", "155")]
    [InlineData("901888777-5", "152")]
    [InlineData("901.888.777-3", "153")]
    public async Task CreateTenant_TaxIdNotWrittenWithItsCheckDigit_ThrowsAndDoesNotSave(string numberDocument, string code)
    {
        // Arrange
        var handler = new CreateTenantCommandHandler(repository.Object, pubSub.Object, snapshots.Object, Configured);

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => handler.Handle(CreateCommand(numberDocument), CancellationToken.None));

        // Assert
        Assert.Equal(code, exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
        repository.Verify(x => x.CreateIfAbsentAsync(It.IsAny<TenantAggregate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("901777333-0")]
    [InlineData("79456123-2")]
    public async Task CreateTenant_TaxIdWithItsCheckDigit_Saves(string numberDocument)
    {
        // Arrange
        var handler = new CreateTenantCommandHandler(repository.Object, pubSub.Object, snapshots.Object, Configured);

        // Act
        await handler.Handle(CreateCommand(numberDocument), CancellationToken.None);

        // Assert
        repository.Verify(x => x.CreateIfAbsentAsync(It.Is<TenantAggregate>(tenant => tenant.NumberDocument == numberDocument), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTenant_TaxIdWithWrongCheckDigit_ThrowsAndDoesNotSave()
    {
        // Arrange
        var tenant = this.StoredBeforeTheRule();
        var command = new UpdateTenantCommand(tenant.Id, "Conjunto", TaxId, "900765432-5", null, "3107845123", "admin@example.com", Utils.Location, Utils.License, true);
        var handler = new UpdateTenantCommandHandler(repository.Object, userContext.Object, pubSub.Object, snapshots.Object, Configured);

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal(Domain.Errors.DocumentCheckDigitDoesNotMatch.GetCode(), exception.Code);
        repository.Verify(x => x.UpdateAsync(It.IsAny<TenantAggregate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTenant_TaxIdWithItsCheckDigit_Saves()
    {
        // Arrange
        var tenant = this.StoredBeforeTheRule();
        var command = new UpdateTenantCommand(tenant.Id, "Conjunto", TaxId, "900765432-6", null, "3107845123", "admin@example.com", Utils.Location, Utils.License, true);
        var handler = new UpdateTenantCommandHandler(repository.Object, userContext.Object, pubSub.Object, snapshots.Object, Configured);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repository.Verify(x => x.UpdateAsync(It.Is<TenantAggregate>(saved => saved.NumberDocument == "900765432-6"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTenant_StoredBeforeTheRule_DeletesIt()
    {
        // Arrange
        var tenant = this.StoredBeforeTheRule();
        var purge = Microsoft.Extensions.Options.Options.Create(new TenantPurgeOptions { RetentionDays = 30 });
        var handler = new DeleteTenantCommandHandler(repository.Object, userContext.Object, pubSub.Object, snapshots.Object, purge);

        // Act
        await handler.Handle(new DeleteTenantCommand(tenant.Id), CancellationToken.None);

        // Assert
        Assert.True(tenant.IsDeleted);
    }

    [Fact]
    public async Task RestoreTenant_DeletedBeforeTheRule_RestoresIt()
    {
        // Arrange
        var tenant = this.StoredBeforeTheRule();

        tenant.Delete(Guid.NewGuid(), Duration.FromDays(30));
        repository.Setup(x => x.FindDeletedAsync(tenant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        repository.Setup(x => x.RestoreAsync(tenant, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new RestoreTenantCommandHandler(repository.Object, userContext.Object, pubSub.Object, snapshots.Object);

        // Act
        await handler.Handle(new RestoreTenantCommand(tenant.Id), CancellationToken.None);

        // Assert
        Assert.False(tenant.IsDeleted);
    }

    [Fact]
    public void Validate_TypesWithoutWeights_Fails()
    {
        // Arrange
        var options = new TenantDocumentOptions { CheckDigitDocumentTypes = ["31"] };

        // Act
        var result = new TenantDocumentOptionsValidator().Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }

    [Fact]
    public void Validate_WeightThatIsNotPositive_Fails()
    {
        // Arrange
        var options = new TenantDocumentOptions { CheckDigitDocumentTypes = ["31"], CheckDigitWeights = [3, 0, 13] };

        // Act
        var result = new TenantDocumentOptionsValidator().Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }

    [Fact]
    public void Validate_EmptyDocumentTypeCode_Fails()
    {
        // Arrange
        var options = new TenantDocumentOptions { CheckDigitDocumentTypes = [" "], CheckDigitWeights = [3, 7] };

        // Act
        var result = new TenantDocumentOptionsValidator().Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }

    [Fact]
    public void Validate_NoSection_Succeeds()
    {
        // Act
        var result = new TenantDocumentOptionsValidator().Validate(null, new TenantDocumentOptions());

        // Assert
        Assert.True(result.Succeeded);
    }

    /// <summary>
    /// El valor desplegado. ms-licenses tiene esta misma prueba con el mismo valor esperado: si una de las dos
    /// configuraciones cambia sin la otra, su prueba falla. La compra que ms-licenses acepta la tiene que poder crear este
    /// micro.
    /// </summary>
    [Theory]
    [InlineData("CodeDesignPlus.Net.Microservice.Tenants.Rest")]
    [InlineData("CodeDesignPlus.Net.Microservice.Tenants.gRpc")]
    [InlineData("CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker")]
    public void Appsettings_TenantDocument_IsTheSharedDeployedValue(string entrypoint)
    {
        // Arrange
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CodeDesignPlus.Net.Microservice.Tenants.sln")))
        {
            directory = directory.Parent;
        }

        var configuration = new ConfigurationBuilder().AddJsonFile(Path.Combine(directory!.FullName, "src", "entrypoints", entrypoint, "appsettings.json")).Build();

        // Act
        var options = configuration.GetSection(TenantDocumentOptions.Section).Get<TenantDocumentOptions>();

        // Assert
        Assert.NotNull(options);
        Assert.True(new TenantDocumentOptionsValidator().Validate(null, options).Succeeded);
        Assert.Equal(["31"], options.CheckDigitDocumentTypes);
        Assert.Equal([3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71], options.CheckDigitWeights);
    }
}
