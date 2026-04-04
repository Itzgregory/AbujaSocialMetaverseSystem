namespace AbujaSocialMetaverse.Infrastructure.Caching;

/// <summary>
/// Core cache operations. Used by all modules.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> SetIfNotExistsAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);

    Task<long> IncrementAsync(string key, CancellationToken cancellationToken = default);

    Task SetExpiryAsync(
        string key,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Read-through pattern. Returns cached value if exists,
    /// otherwise executes factory, caches the result, and returns it.
    /// </summary>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);
}