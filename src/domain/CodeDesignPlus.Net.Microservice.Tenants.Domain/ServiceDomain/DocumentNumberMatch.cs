namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.ServiceDomain;

/// <summary>
/// Compares document numbers ignoring the separators people write them with: hyphen, dot and spaces (pendings/733.155).
/// </summary>
/// <remarks>
/// <para>
/// «901888777-3», «901.888.777-3» and «9018887773» are the same document, and the duplicate check did not see it because it
/// compared the stored text exactly. The number is still stored as it arrives; only the comparison ignores the separators.
/// </para>
/// <para>
/// No country rule here: nothing knows what a check digit is, so a number written without it («901888777») is another number.
/// The comparison runs in Mongo with an anchored regular expression that allows the separators between digits, because the
/// production database (Azure DocumentDB) has no collation and a derived field would need data for the existing tenants.
/// </para>
/// </remarks>
public static class DocumentNumberMatch
{
    private const string Separators = @"[\s.\-]*";

    /// <summary>
    /// The number without separators.
    /// </summary>
    /// <param name="number">The number as written.</param>
    /// <returns>The number without hyphens, dots or spaces.</returns>
    public static string Normalize(string? number) =>
        new([.. (number ?? string.Empty).Where(c => c != '-' && c != '.' && !char.IsWhiteSpace(c))]);

    /// <summary>
    /// The regular expression that matches any way of writing the same number with separators.
    /// </summary>
    /// <param name="number">The number as written.</param>
    /// <returns>The anchored pattern, or <c>null</c> if the number has nothing but separators.</returns>
    public static string? Pattern(string? number)
    {
        var normalized = Normalize(number);

        if (normalized.Length == 0)
        {
            return null;
        }

        return "^" + Separators + string.Join(Separators, normalized.Select(c => Regex.Escape(c.ToString()))) + Separators + "$";
    }
}
