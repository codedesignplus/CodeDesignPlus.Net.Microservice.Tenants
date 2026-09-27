using CodeDesignPlus.Net.Microservice.Tenants.Application.Options;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Options;

public class TenantPurgeOptionsTest
{
    private readonly TenantPurgeOptionsValidator validator = new();

    [Theory]
    [InlineData(30)]
    [InlineData(90)]
    public void Validate_PositiveRetention_Succeeds(int days)
    {
        // Act
        var result = validator.Validate(null, new TenantPurgeOptions { RetentionDays = days });

        // Assert
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_MissingOrNegativeRetention_Fails(int days)
    {
        // Act
        var result = validator.Validate(null, new TenantPurgeOptions { RetentionDays = days });

        // Assert
        Assert.True(result.Failed);
    }

    [Fact]
    public void Retention_Days_IsTheSameDuration()
    {
        // Act
        var options = new TenantPurgeOptions { RetentionDays = 60 };

        // Assert
        Assert.Equal(Duration.FromDays(60), options.Retention);
    }
}
