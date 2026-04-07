using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Shared.Contracts;

/// <summary>
/// Implemented internally by each module.
/// AdminModule receives all implementations via DI as IEnumerable<IAdminProjection>
/// and aggregates their snapshots.
/// AdminModule never imports module services directly — only this contract.
/// </summary>
public interface IAdminProjection
{
    string ModuleName { get; }
    Task<AdminMetricSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}