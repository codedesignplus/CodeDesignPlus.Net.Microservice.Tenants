namespace CodeDesignPlus.Net.Microservice.Tenants.Domain.Repositories;

/// <summary>
/// El almacén de copropiedades.
/// </summary>
/// <remarks>
/// El SDK deja fuera todo documento con <c>IsDeleted</c> en <c>FindAsync</c>, <c>MatchingAsync</c> y
/// <c>UpdateAsync</c>. Una copropiedad eliminada sigue ahí hasta que se purga, así que los métodos que trabajan con
/// eliminadas van directamente a la colección.
/// </remarks>
public interface ITenantRepository : IRepositoryBase
{
    Task<bool> ExistsByDocumentAsync(string typeDocumentCode, string numberDocument, CancellationToken cancellationToken);

    /// <summary>
    /// Busca una copropiedad eliminada que todavía no se ha purgado.
    /// </summary>
    /// <param name="id">El identificador de la copropiedad.</param>
    /// <param name="cancellationToken">El token de cancelación.</param>
    /// <returns>La copropiedad eliminada, o <c>null</c> si no existe o no está eliminada.</returns>
    Task<TenantAggregate?> FindDeletedAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Todas las copropiedades eliminadas que aún no se han purgado, primero las que antes se purgan.
    /// </summary>
    /// <param name="cancellationToken">El token de cancelación.</param>
    /// <returns>Las copropiedades eliminadas.</returns>
    Task<List<TenantAggregate>> GetDeletedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Las copropiedades eliminadas cuyo plazo venció.
    /// </summary>
    /// <param name="now">El instante actual.</param>
    /// <param name="limit">Cuántas devolver como máximo.</param>
    /// <param name="cancellationToken">El token de cancelación.</param>
    /// <returns>Las copropiedades por purgar, primero las más antiguas.</returns>
    Task<List<TenantAggregate>> FindDueForPurgeAsync(Instant now, int limit, CancellationToken cancellationToken);

    /// <summary>
    /// Guarda una copropiedad restaurada, solo si sigue eliminada.
    /// </summary>
    /// <param name="tenant">La copropiedad restaurada.</param>
    /// <param name="cancellationToken">El token de cancelación.</param>
    /// <returns><c>true</c> si se guardó; <c>false</c> si entretanto se purgó o se restauró.</returns>
    Task<bool> RestoreAsync(TenantAggregate tenant, CancellationToken cancellationToken);

    /// <summary>
    /// Borra el documento de una copropiedad eliminada, una vez avisado cada micro de que purgue sus datos.
    /// </summary>
    /// <param name="id">El identificador de la copropiedad.</param>
    /// <param name="cancellationToken">El token de cancelación.</param>
    /// <returns><c>true</c> si se borró; <c>false</c> si entretanto se restauró o se purgó.</returns>
    Task<bool> PurgeAsync(Guid id, CancellationToken cancellationToken);
}
