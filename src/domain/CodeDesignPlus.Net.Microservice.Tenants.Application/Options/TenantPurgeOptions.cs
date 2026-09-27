using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Options;

/// <summary>
/// How long a deleted tenant can be restored before every microservice purges its data.
/// </summary>
/// <remarks>
/// It changes per environment (30, 60, 90 days…), so it lives in the <c>appsettings</c>. The period is fixed when the
/// tenant is deleted, in <see cref="TenantAggregate.PurgeAfter"/>: changing it later does not move the tenants
/// already deleted.
/// </remarks>
public class TenantPurgeOptions
{
    public const string Section = "TenantPurge";

    /// <summary>
    /// Days a deleted tenant can be restored.
    /// </summary>
    public int RetentionDays { get; set; }

    /// <summary>
    /// The retention as a duration.
    /// </summary>
    public Duration Retention => Duration.FromDays(RetentionDays);
}

/// <summary>
/// Checks at startup that the environment defined the retention.
/// </summary>
/// <remarks>
/// Without it a deleted tenant would be purged on the next run of the job, with no chance to restore it.
/// </remarks>
public class TenantPurgeOptionsValidator : IValidateOptions<TenantPurgeOptions>
{
    public ValidateOptionsResult Validate(string? name, TenantPurgeOptions options)
    {
        if (options.RetentionDays > 0)
        {
            return ValidateOptionsResult.Success;
        }

        return ValidateOptionsResult.Fail($"The section '{TenantPurgeOptions.Section}' must define {nameof(TenantPurgeOptions.RetentionDays)} greater than zero.");
    }
}
