namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ValueObjects;

/// <summary>
/// Un paquete complementario pagado de la copropiedad, tal como lo vendió ms-licenses (pendings/739): sus atributos con su
/// regla de combinación y su periodo.
/// </summary>
/// <param name="LineId">La línea de la orden que lo pagó.</param>
/// <param name="PackageId">El paquete del catálogo.</param>
/// <param name="Name">Su nombre.</param>
/// <param name="StartDate">El inicio de su periodo.</param>
/// <param name="EndDate">El fin de su periodo.</param>
/// <param name="ReplacesLineId">La línea que reemplaza desde <paramref name="StartDate"/> (una ampliación o una renovación).</param>
/// <param name="Attributes">Los atributos que aporta.</param>
public sealed record TenantPackage(Guid LineId, Guid PackageId, string Name, Instant StartDate, Instant EndDate, Guid? ReplacesLineId, List<TenantPackageAttribute> Attributes);

/// <summary>
/// Un atributo de un paquete.
/// </summary>
/// <param name="Key">La clave, la misma de la licencia.</param>
/// <param name="Value">El valor como texto.</param>
/// <param name="Combination">Cómo se combina: <c>Sum</c>, <c>Max</c> u <c>Or</c>, como texto (regla 26, §4).</param>
public sealed record TenantPackageAttribute(string Key, string Value, string Combination);
