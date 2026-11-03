using System.Globalization;

namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ServiceDomain;

/// <summary>
/// Los atributos efectivos de una copropiedad: los de su licencia combinados con los de sus paquetes vigentes (pendings/739).
/// </summary>
/// <remarks>
/// <para>
/// Es la misma regla que <c>AttributeCombiner</c> y <c>PackageTerm</c> de ms-licenses, duplicada a propósito con este
/// comentario cruzado (como <see cref="LicenseName"/>): ms-tenants publica la foto y <c>TenantLicenseUpdated</c> sin
/// preguntarle a nadie, y una beta del SDK para dos funciones sería esperar sin necesidad.
/// </para>
/// <list type="bullet">
/// <item><c>Sum</c> suma enteros, <c>Max</c> toma el mayor, <c>Or</c> hace «o» de sí/no. Si la licencia no trae la clave,
/// la pone el paquete.</item>
/// <item>Un paquete cuenta mientras su periodo cubra el instante y no haya empezado la línea que lo reemplaza.</item>
/// <item>Ningún paquete cuenta si la licencia no está pagada en ese instante: se suspenden con ella y vuelven al pagarla,
/// sin alargar su periodo (precisión del usuario, 2026-11-03).</item>
/// <item>Una combinación desconocida (de una versión más nueva de ms-licenses) no se aplica: el atributo de la licencia
/// queda como estaba.</item>
/// </list>
/// </remarks>
public static class EffectiveAttributes
{
    /// <summary>
    /// Los paquetes que cuentan en un instante.
    /// </summary>
    /// <param name="packages">Los paquetes de la copropiedad.</param>
    /// <param name="licenseEnd">Hasta cuándo está pagada la licencia.</param>
    /// <param name="now">El instante.</param>
    /// <returns>Los que cuentan.</returns>
    public static List<TenantPackage> Counting(IReadOnlyCollection<TenantPackage> packages, Instant licenseEnd, Instant now)
    {
        if (now >= licenseEnd)
        {
            return [];
        }

        var replaced = packages.Where(x => x.ReplacesLineId is not null && x.StartDate <= now).Select(x => x.ReplacesLineId!.Value).ToHashSet();

        return [.. packages.Where(x => x.StartDate <= now && now < x.EndDate && !replaced.Contains(x.LineId))];
    }

    /// <summary>
    /// Combina los atributos de la licencia con los de los paquetes que cuentan.
    /// </summary>
    /// <param name="license">Los atributos de la licencia.</param>
    /// <param name="packages">Los paquetes de la copropiedad.</param>
    /// <param name="licenseEnd">Hasta cuándo está pagada la licencia.</param>
    /// <param name="now">El instante.</param>
    /// <returns>Los atributos efectivos.</returns>
    public static Dictionary<string, string> Combine(IReadOnlyDictionary<string, string> license, IReadOnlyCollection<TenantPackage> packages, Instant licenseEnd, Instant now)
    {
        var result = new Dictionary<string, string>(license);

        foreach (var attribute in Counting(packages, licenseEnd, now).SelectMany(x => x.Attributes))
        {
            if (!result.TryGetValue(attribute.Key, out var current))
            {
                if (attribute.Combination is "Sum" or "Max" or "Or")
                {
                    result[attribute.Key] = attribute.Value;
                }

                continue;
            }

            result[attribute.Key] = Merge(current, attribute.Value, attribute.Combination);
        }

        return result;
    }

    private static string Merge(string current, string added, string combination)
    {
        if (combination == "Or")
        {
            var a = bool.TryParse(current, out var left) && left;
            var b = bool.TryParse(added, out var right) && right;

            return a || b ? "true" : "false";
        }

        if (combination is not ("Sum" or "Max") || !long.TryParse(added, NumberStyles.Integer, CultureInfo.InvariantCulture, out var addedNumber))
        {
            return current;
        }

        if (!long.TryParse(current, NumberStyles.Integer, CultureInfo.InvariantCulture, out var currentNumber))
        {
            return added;
        }

        var value = combination == "Sum" ? currentNumber + addedNumber : Math.Max(currentNumber, addedNumber);

        return value.ToString(CultureInfo.InvariantCulture);
    }
}
