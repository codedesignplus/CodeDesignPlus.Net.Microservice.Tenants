using CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.Test.ValueObjects;

public class StateTest
{
    [Fact]
    public void Create_ValidParameters_ShouldReturnState()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test State";
        var code = "TS";

        // Act
        var state = State.Create(id, name, code);

        // Assert
        Assert.NotNull(state);
        Assert.Equal(id, state.Id);
        Assert.Equal(name, state.Name);
        Assert.Equal(code, state.Code);
    }

    [Fact]
    public void Create_EmptyName_ShouldThrowException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = string.Empty;
        var code = "TS";

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => State.Create(id, name, code));

        Assert.Equal(Errors.StateNameIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.StateNameIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }

    [Fact]
    public void Create_EmptyCode_ShouldThrowException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test State";
        var code = string.Empty;

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => State.Create(id, name, code));

        Assert.Equal(Errors.StateCodeIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.StateCodeIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }

    [Fact]
    public void Create_EmptyId_ShouldThrowException()
    {
        // Arrange
        var id = Guid.Empty;
        var name = "Test State";
        var code = "TS";

        // Act & Assert
        var exception = Assert.Throws<CodeDesignPlusException>(() => State.Create(id, name, code));

        Assert.Equal(Errors.StateIdIsEmpty.GetMessage(), exception.Message);
        Assert.Equal(Errors.StateIdIsEmpty.GetCode(), exception.Code);
        Assert.Equal(Layer.Domain, exception.Layer);
    }
}
