using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;
using AbujaSocialMetaverse.Modules.Core.Public.Interfaces;
using AbujaSocialMetaverse.Modules.Core.Public.Models;
using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Helpers;
using AbujaSocialMetaverse.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AbujaSocialMetaverse.Modules.Core.Internal.Services;

public class SessionService : BaseService, ISessionService
{
    private readonly ITokenService _tokenService;
    public SessionService(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        ILogger<SessionService> logger)
        : base(logger, unitOfWork)
    {
        _tokenService = tokenService;
    }
    
 

    public async Task<Result<TokenResult>> CreateSessionAsync(
        Guid userId,
        string email,
        string userAgent,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(CreateSessionAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            Guard.Against.NullOrWhiteSpace(email, nameof(email));

            var tokens = await _tokenService.GenerateTokensAsync(userId, email, ct);
            if (tokens is null)
            {
                return Result<TokenResult>.Failure(
                    ErrorCodes.Auth.TokenInvalid,
                    "Failed to generate tokens.");
            }

            // Update the session with metadata
            var session = await _unitOfWork!.Set<Session>()
                .FirstOrDefaultAsync(s => s.Jti == tokens.Jti, ct);

            if (session is not null)
            {
                session.UserAgent = userAgent;
                session.IpAddress = ipAddress;
                session.UpdatedAt = DateTimeOffset.UtcNow;
                await _unitOfWork.SaveChangesAsync(ct);
            }

            _logger.LogInformation("Session created for user {UserId} with JTI {Jti}", userId, tokens.Jti);
            return Result<TokenResult>.Success(tokens);
        }, cancellationToken);
    }

    public async Task<Result<(Guid UserId, TokenResult NewTokens)>> RefreshSessionAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(RefreshSessionAsync), async (ct) =>
        {
            Guard.Against.NullOrWhiteSpace(refreshToken, nameof(refreshToken));

            // Find session with this refresh token
            var session = await _unitOfWork!.Set<Session>()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken && !s.IsDeleted, ct);

            if (session is null)
            {
                _logger.LogWarning("Refresh failed: invalid refresh token");
                return Result<(Guid UserId, TokenResult NewTokens)>.ValidationError(
                    ErrorCodes.Auth.RefreshTokenInvalid,
                    "Invalid refresh token.");
            }

            // Check if refresh token is expired
            if (session.ExpiresAt < DateTimeOffset.UtcNow)
            {
                _logger.LogWarning("Refresh failed: expired refresh token for user {UserId}", session.UserId);
                return Result<(Guid UserId, TokenResult NewTokens)>.ValidationError(
                    ErrorCodes.Auth.RefreshTokenExpired,
                    "Refresh token has expired. Please login again.");
            }

            // Check if session is revoked
            if (session.RevokedAt is not null)
            {
                _logger.LogWarning("Refresh failed: revoked session for user {UserId}", session.UserId);
                return Result<(Guid UserId, TokenResult NewTokens)>.ValidationError(
                    ErrorCodes.Auth.TokenRevoked,
                    "Session has been revoked.");
            }

            var user = session.User;
            if (user is null || !user.IsActive)
            {
                _logger.LogWarning("Refresh failed: user not found or inactive");
                return Result<(Guid UserId, TokenResult NewTokens)>.ValidationError(
                    ErrorCodes.User.NotFound,
                    "User not found or inactive.");
            }

            // Generate new tokens
            var newTokens = await _tokenService.GenerateTokensAsync(user.Id, user.Email, ct);

            // Revoke old session
            session.RevokedAt = DateTimeOffset.UtcNow;
            session.RevokedReason = "Refreshed";
            session.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Session refreshed for user {UserId}", user.Id);
            return Result<(Guid UserId, TokenResult NewTokens)>.Success((user.Id, newTokens));
        }, cancellationToken);
    }

    public async Task<Result> RevokeSessionAsync(
        Guid userId,
        string jti,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(RevokeSessionAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            Guard.Against.NullOrWhiteSpace(jti, nameof(jti));

            var session = await _unitOfWork!.Set<Session>()
                .FirstOrDefaultAsync(s => s.Jti == jti && s.UserId == userId && !s.IsDeleted, ct);

            if (session is null)
            {
                _logger.LogWarning("Revoke failed: session not found for user {UserId}, jti {Jti}", userId, jti);
                return Result.ValidationError(
                    ErrorCodes.Auth.TokenInvalid,
                    "Session not found.");
            }

            session.RevokedAt = DateTimeOffset.UtcNow;
            session.RevokedReason = "Revoked by user";
            session.UpdatedAt = DateTimeOffset.UtcNow;

            await _tokenService.RevokeTokenAsync(jti, ct);

            var saveResult = await SaveChangesAsync(ct);
            if (!saveResult.IsSuccess)
            {
                return saveResult;
            }

            _logger.LogInformation("Session {Jti} revoked for user {UserId}", jti, userId);
            return Result.Success();
        }, cancellationToken);
    }

    public async Task<Result> RevokeAllUserSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(RevokeAllUserSessionsAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));

            var sessions = await _unitOfWork!.Set<Session>()
                .Where(s => s.UserId == userId && s.RevokedAt == null && !s.IsDeleted)
                .ToListAsync(ct);

            foreach (var session in sessions)
            {
                session.RevokedAt = DateTimeOffset.UtcNow;
                session.RevokedReason = "All sessions revoked";
                session.UpdatedAt = DateTimeOffset.UtcNow;
                await _tokenService.RevokeTokenAsync(session.Jti, ct);
            }

            var saveResult = await SaveChangesAsync(ct);
            if (!saveResult.IsSuccess)
            {
                return saveResult;
            }

            _logger.LogInformation("All sessions revoked for user {UserId}", userId);
            return Result.Success();
        }, cancellationToken);
    }

    public async Task<Result> UpdateSessionMetadataAsync(
        string jti,
        string userAgent,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(UpdateSessionMetadataAsync), async (ct) =>
        {
            Guard.Against.NullOrWhiteSpace(jti, nameof(jti));

            var session = await _unitOfWork!.Set<Session>()
                .FirstOrDefaultAsync(s => s.Jti == jti && !s.IsDeleted, ct);

            if (session is null)
            {
                return Result.NotFound(ErrorCodes.Auth.TokenInvalid, $"Session with JTI '{jti}' not found.");
            }

            session.UserAgent = userAgent;
            session.IpAddress = ipAddress;
            session.UpdatedAt = DateTimeOffset.UtcNow;

            return await SaveChangesAsync(ct);
        }, cancellationToken);
    }
}