using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Public.Interfaces;

public interface ITokenService
{
    Task<TokenResult> GenerateTokensAsync(
        Guid userId,
        string email,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> ValidateAccessTokenAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    Task<bool> IsTokenRevokedAsync(
        string jti,
        CancellationToken cancellationToken = default);

    Task RevokeTokenAsync(
        string jti,
        CancellationToken cancellationToken = default);
}

public record TokenResult(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt,
    string Jti
);