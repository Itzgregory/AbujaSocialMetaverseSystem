namespace AbujaSocialMetaverse.Infrastructure.Caching;

/// <summary>
/// Maintenance-only cache operations.
/// Never called in the request path.
/// Used by background jobs and admin tooling only.
/// </summary>
public interface ICacheAdminService
{
    /// <summary>
    /// Scans and deletes keys matching a pattern in batches.
    /// Uses Redis SCAN — non-blocking, safe for production.
    /// </summary>
    Task DeleteByPatternAsync(
        string pattern,
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    Task<long> CountByPatternAsync(
        string pattern,
        CancellationToken cancellationToken = default);

    Task FlushUserDataAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}