using AbujaSocialMetaverse.Modules.Core.Public.Models;
using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Public.Interfaces;

public interface ISessionService
{
    Task<Result<TokenResult>> CreateSessionAsync(
        Guid userId,
        string email,
        string userAgent,
        string ipAddress,
        CancellationToken cancellationToken = default);

    Task<Result<(Guid UserId, TokenResult NewTokens)>> RefreshSessionAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<Result> RevokeSessionAsync(
        Guid userId,
        string jti,
        CancellationToken cancellationToken = default);

    Task<Result> RevokeAllUserSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateSessionMetadataAsync(
        string jti,
        string userAgent,
        string ipAddress,
        CancellationToken cancellationToken = default);
}