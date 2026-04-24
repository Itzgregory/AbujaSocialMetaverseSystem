using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AbujaSocialMetaverse.Infrastructure.Caching;
using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;
using AbujaSocialMetaverse.Modules.Core.Public.Interfaces;
using AbujaSocialMetaverse.Shared.Configuration.Options;
using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Helpers;
using AbujaSocialMetaverse.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AbujaSocialMetaverse.Modules.Core.Internal.Services;

public class TokenService : ITokenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtOptions _jwtOptions;
    private readonly ICacheService _cache;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IUnitOfWork unitOfWork,
        IOptions<JwtOptions> jwtOptions,
        ICacheService cache,
        ILogger<TokenService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtOptions = jwtOptions.Value;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TokenResult> GenerateTokensAsync(
        Guid userId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var jti = Guid.NewGuid().ToString();
        var refreshToken = GenerateRefreshToken();

        var accessToken = GenerateAccessToken(userId, email, jti);
        var accessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes);
        var refreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshExpiryDays);

        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Jti = jti,
            RefreshToken = refreshToken,
            ExpiresAt = refreshTokenExpiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _unitOfWork.Set<Session>().AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tokens generated for user {UserId}", userId);

        return new TokenResult(
            accessToken,
            refreshToken,
            accessTokenExpiresAt,
            refreshTokenExpiresAt,
            jti
        );
    }

    public async Task<Result<Guid>> ValidateAccessTokenAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(accessToken, validationParameters, out _);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Result<Guid>.ValidationError(
                    ErrorCodes.Auth.TokenInvalid,
                    "Token does not contain a valid user ID.");
            }

            var jtiClaim = principal.FindFirst(JwtRegisteredClaimNames.Jti);
            if (jtiClaim is null)
            {
                return Result<Guid>.ValidationError(
                    ErrorCodes.Auth.TokenInvalid,
                    "Token does not contain a JTI.");
            }

            var isRevoked = await IsTokenRevokedAsync(jtiClaim.Value, cancellationToken);
            if (isRevoked)
            {
                return Result<Guid>.ValidationError(
                    ErrorCodes.Auth.TokenRevoked,
                    "Token has been revoked.");
            }

            return Result<Guid>.Success(userId);
        }
        catch (SecurityTokenExpiredException)
        {
            return Result<Guid>.ValidationError(
                ErrorCodes.Auth.TokenExpired,
                "Token has expired.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
            return Result<Guid>.ValidationError(
                ErrorCodes.Auth.TokenInvalid,
                "Invalid token.");
        }
    }

    public async Task<bool> IsTokenRevokedAsync(
        string jti,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"token:revoked:{jti}";
        return await _cache.ExistsAsync(cacheKey, cancellationToken);
    }

    public async Task RevokeTokenAsync(
        string jti,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"token:revoked:{jti}";
        var expiry = TimeSpan.FromMinutes(_jwtOptions.ExpiryMinutes);
        await _cache.SetAsync(cacheKey, true, expiry, cancellationToken);

        _logger.LogInformation("Token {Jti} revoked", jti);
    }

    private string GenerateAccessToken(Guid userId, string email, string jti)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}