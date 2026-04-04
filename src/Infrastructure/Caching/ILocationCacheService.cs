namespace AbujaSocialMetaverse.Infrastructure.Caching;

/// <summary>
/// Redis Hash and Set operations for avatar positions and region membership.
/// Used exclusively by the Social module.
/// </summary>
public interface ILocationCacheService
{
    // Hash operations — avatar position state
    Task HashSetAsync<T>(
        string key,
        string field,
        T value,
        CancellationToken cancellationToken = default);

    Task<T?> HashGetAsync<T>(
        string key,
        string field,
        CancellationToken cancellationToken = default);

    Task<bool> HashDeleteAsync(
        string key,
        string field,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> HashGetAllAsync<T>(
        string key,
        CancellationToken cancellationToken = default);

    // Set operations — region group membership
    Task SetAddAsync(
        string key,
        string value,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);

    Task SetRemoveAsync(
        string key,
        string value,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> SetMembersAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task<bool> SetContainsAsync(
        string key,
        string value,
        CancellationToken cancellationToken = default);
}