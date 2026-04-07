using AbujaSocialMetaverse.Modules.Core.Public.Models;
using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Public.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<Result> RevokeTokenAsync(Guid userId, string jti, CancellationToken cancellationToken = default);
    Task<Result> RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(Guid userId, string jti, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<Result<Guid>> GetUserIdFromTokenAsync(string accessToken, CancellationToken cancellationToken = default);
}