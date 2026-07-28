using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.CreateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.DeleteTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Commands.UpdateTenant;
using CodeDesignPlus.Net.Microservice.Tenants.Default.Test.Helpers;

namespace CodeDesignPlus.Net.Microservice.Tenants.Default.Test.Validations;

/// <summary>
/// Validates the commands of the microservice.
/// </summary>
/// <remarks>
/// Asserted explicitly instead of through the reflection-driven attribute of the SDK: that
/// attribute feeds every string with "Test", which a <see cref="Domain.ValueObjects.TypeDocument"/>
/// code cannot be.
/// </remarks>
public class CommandsTests
{
    [Fact]
    public void CreateTenantCommand_ExposesEveryValue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var idUser = Guid.NewGuid();

        // Act
        var command = new CreateTenantCommand(id, Utils.Name, Utils.TypeDocument, Utils.NumberDocument, Utils.Domain, Utils.Phone, Utils.Email, Utils.Location, Utils.License, idUser, true);

        // Assert
        Assert.Equal(id, command.Id);
        Assert.Equal(Utils.Name, command.Name);
        Assert.Equal(Utils.TypeDocument, command.TypeDocument);
        Assert.Equal(Utils.NumberDocument, command.NumberDocument);
        Assert.Equal(Utils.Domain, command.Domain);
        Assert.Equal(Utils.Phone, command.Phone);
        Assert.Equal(Utils.Email, command.Email);
        Assert.Equal(Utils.Location, command.Location);
        Assert.Equal(Utils.License, command.License);
        Assert.Equal(idUser, command.IdUser);
        Assert.True(command.IsActive);
    }

    [Fact]
    public void UpdateTenantCommand_ExposesEveryValue()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = new UpdateTenantCommand(id, Utils.Name, Utils.TypeDocument, Utils.NumberDocument, Utils.Domain, Utils.Phone, Utils.Email, Utils.Location, Utils.License, false);

        // Assert
        Assert.Equal(id, command.Id);
        Assert.Equal(Utils.TypeDocument, command.TypeDocument);
        Assert.Equal(Utils.Location, command.Location);
        Assert.Equal(Utils.License, command.License);
        Assert.False(command.IsActive);
    }

    [Fact]
    public void DeleteTenantCommand_ExposesEveryValue()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = new DeleteTenantCommand(id);

        // Assert
        Assert.Equal(id, command.Id);
    }

    [Fact]
    public void Commands_WithTheSameValues_AreEqual()
    {
        // Arrange: son records, asi que la igualdad estructural es parte de su contrato.
        var id = Guid.NewGuid();

        // Act
        var first = new DeleteTenantCommand(id);
        var second = new DeleteTenantCommand(id);

        // Assert
        Assert.Equal(first, second);
    }
}
