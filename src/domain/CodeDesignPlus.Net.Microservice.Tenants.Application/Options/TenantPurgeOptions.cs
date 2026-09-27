using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Options;

/// <summary>
/// Cuánto tiempo se puede restaurar una copropiedad eliminada antes de que cada micro purgue sus datos.
/// </summary>
/// <remarks>
/// Cambia por entorno (30, 60, 90 días…), por eso vive en el <c>appsettings</c>. El plazo queda fijado al eliminar
/// la copropiedad, en <see cref="TenantAggregate.PurgeAfter"/>: cambiarlo después no mueve las que ya estaban
/// eliminadas.
/// </remarks>
public class TenantPurgeOptions
{
    public const string Section = "TenantPurge";

    /// <summary>
    /// Días durante los que se puede restaurar una copropiedad eliminada.
    /// </summary>
    public int RetentionDays { get; set; }

    /// <summary>
    /// El plazo como duración.
    /// </summary>
    public Duration Retention => Duration.FromDays(RetentionDays);
}

/// <summary>
/// Comprueba al arrancar que el entorno definió el plazo.
/// </summary>
/// <remarks>
/// Sin él, una copropiedad eliminada se purgaría en la siguiente pasada del job, sin opción de restaurarla. Un micro
/// que no arranca se ve en el primer despliegue.
/// </remarks>
public class TenantPurgeOptionsValidator : IValidateOptions<TenantPurgeOptions>
{
    public ValidateOptionsResult Validate(string? name, TenantPurgeOptions options)
    {
        if (options.RetentionDays > 0)
        {
            return ValidateOptionsResult.Success;
        }

        return ValidateOptionsResult.Fail($"La sección '{TenantPurgeOptions.Section}' debe definir {nameof(TenantPurgeOptions.RetentionDays)} mayor que cero.");
    }
}
