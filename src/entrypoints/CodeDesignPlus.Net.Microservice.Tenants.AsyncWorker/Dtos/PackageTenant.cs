namespace CodeDesignPlus.Net.Microservice.Tenants.AsyncWorker.Dtos;

/// <summary>
/// Gemelo de <c>PackageTenant</c> de ms-licenses: un paquete pagado con su periodo (pendings/739).
/// </summary>
public class PackageTenant
{
    /// <summary>La línea de la orden.</summary>
    public Guid LineId { get; set; }

    /// <summary>El paquete del catálogo.</summary>
    public Guid PackageId { get; set; }

    /// <summary>Su nombre.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>El inicio de su periodo.</summary>
    public Instant StartDate { get; set; }

    /// <summary>El fin de su periodo.</summary>
    public Instant EndDate { get; set; }

    /// <summary>La línea que reemplaza desde su inicio.</summary>
    public Guid? ReplacesLineId { get; set; }

    /// <summary>Los atributos que aporta.</summary>
    public List<PackageTenantAttribute> Attributes { get; set; } = [];

    /// <summary>
    /// El paquete de la copropiedad.
    /// </summary>
    /// <returns>El valor del dominio.</returns>
    public Domain.ValueObjects.TenantPackage ToPackage() => new(LineId, PackageId, Name, StartDate, EndDate, ReplacesLineId, [.. (Attributes ?? []).Select(x => new Domain.ValueObjects.TenantPackageAttribute(x.Key, x.Value, x.Combination))]);
}

/// <summary>
/// Un atributo de un paquete.
/// </summary>
public class PackageTenantAttribute
{
    /// <summary>La clave.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>El valor.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>La combinación como texto.</summary>
    public string Combination { get; set; } = string.Empty;
}
