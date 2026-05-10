using AbujaSocialMetaverse.Modules.Core.Public.Models;
using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Public.Interfaces;

/// <summary>
/// Authentication service interface for user registration, login, and session management.
/// Email verification and password reset are handled by <see cref="IAccountVerificationService"/>.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user and creates a new session.
    /// </summary>
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, string ipAddress, string userAgent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a specific session by its JTI.
    /// </summary>
    Task<Result> RevokeTokenAsync(Guid userId, string jti, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all active sessions for a user.
    /// </summary>
    Task<Result> RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs the user out by revoking their current session.
    /// </summary>
    Task<Result> LogoutAsync(Guid userId, string jti, CancellationToken cancellationToken = default);
}