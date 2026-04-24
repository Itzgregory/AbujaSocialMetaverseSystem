using AbujaSocialMetaverse.Modules.Core.Public.Interfaces;
using AbujaSocialMetaverse.Modules.Core.Public.Models;
using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Helpers;
using AbujaSocialMetaverse.Shared.Models;
using AbujaSocialMetaverse.Shared.Validators;
using Microsoft.Extensions.Logging;
using AbujaSocialMetaverse.Modules.Core.Internal.Mappers;
using AbujaSocialMetaverse.Shared.Exceptions;

namespace AbujaSocialMetaverse.Modules.Core.Internal.Services;

public class AuthService : BaseService, IAuthService
{
    private readonly IUserCreationService _userCreationService;
    private readonly IUserQueryService _userQueryService;
    private readonly IPasswordService _passwordService;
    private readonly ILockoutService _lockoutService;
    private readonly ISessionService _sessionService;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUnitOfWork unitOfWork,
        IUserCreationService userCreationService,
        IUserQueryService userQueryService,
        IPasswordService passwordService,
        ILockoutService lockoutService,
        ISessionService sessionService,
        ITokenService tokenService,
        ILogger<AuthService> logger)
        : base(logger, unitOfWork)
    {
        _userCreationService = userCreationService;
        _userQueryService = userQueryService;
        _passwordService = passwordService;
        _lockoutService = lockoutService;
        _sessionService = sessionService;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(RegisterAsync), async (ct) =>
        {
            Guard.Against.Null(request, nameof(request));
            Guard.Against.NullOrWhiteSpace(request.Email, nameof(request.Email));
            Guard.Against.NullOrWhiteSpace(request.Password, nameof(request.Password));
            Guard.Against.NullOrWhiteSpace(request.DisplayName, nameof(request.DisplayName));

            if (!CommonValidators.IsValidEmail(request.Email))
            {
                return Result<AuthResponse>.ValidationError(
                    ErrorCodes.User.InvalidEmail,
                    "Invalid email format.");
            }

            var passwordStrengthResult = _passwordService.ValidateStrength(request.Password);
            if (!passwordStrengthResult.IsSuccess)
            {
                return Result<AuthResponse>.ValidationError(
                    ErrorCodes.User.PasswordTooWeak,
                    "Password does not meet security requirements.");
            }

            var hashResult = _passwordService.HashPassword(request.Password);
            if (!hashResult.IsSuccess || string.IsNullOrWhiteSpace(hashResult.Value))
            {
                return Result<AuthResponse>.Failure(
                    ErrorCodes.Validation.InternalError,
                    "Failed to generate password hash.");
            }

            var userResult = await _userCreationService.CreateUserAsync(
                request.Email,
                hashResult.Value!,
                request.DisplayName,
                ct);

            if (!userResult.IsSuccess || userResult.Value is null)
            {
                return Result<AuthResponse>.Failure(
                    userResult.Error ?? new ResultError(ErrorCodes.Validation.InternalError, "Failed to create user.", ErrorType.ServerError));
            }

            _logger.LogInformation("User registered: {Email} with ID {UserId}", request.Email, userResult.Value.Id);

            // Return empty response for registration (no auto-login)
            return Result<AuthResponse>.Success(new AuthResponse(
                string.Empty,
                string.Empty,
                DateTimeOffset.UtcNow,
                new UserDto(Guid.Empty, string.Empty, string.Empty, null, null, SocialMode.Leisure, DateTimeOffset.UtcNow, false, false)));
        }, cancellationToken);
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(LoginAsync), async (ct) =>
        {
            Guard.Against.Null(request, nameof(request));
            Guard.Against.NullOrWhiteSpace(request.Email, nameof(request.Email));
            Guard.Against.NullOrWhiteSpace(request.Password, nameof(request.Password));

            var userResult = await GetAndValidateUserAsync(request.Email, ct);
            if (!userResult.IsSuccess || userResult.Value is null)
            {
                return Result<AuthResponse>.Failure(
                    new ResultError(ErrorCodes.Auth.InvalidCredentials, "Invalid email or password.", ErrorType.Validation));
            }

            var user = userResult.Value;

            var lockoutCheck = await CheckLockoutStatusAsync(user.Id, ct);
            if (!lockoutCheck.IsSuccess)
            {
                return Result<AuthResponse>.Failure(lockoutCheck.Error!);
            }

            var passwordCheck = await VerifyPasswordAndHandleLockoutAsync(user.Id, request.Password, user.PasswordHash, ct);
            if (!passwordCheck.IsSuccess)
            {
                return Result<AuthResponse>.Failure(passwordCheck.Error!);
            }

            var accountStatusCheck = await CheckAccountStatusAsync(user);
            if (!accountStatusCheck.IsSuccess)
            {
                return Result<AuthResponse>.Failure(accountStatusCheck.Error!);
            }

            var sessionResult = await CreateUserSessionAsync(user, userAgent, ipAddress, ct);
            if (!sessionResult.IsSuccess || sessionResult.Value is null)
            {
                return Result<AuthResponse>.Failure(
                    new ResultError(ErrorCodes.Validation.InternalError, "Failed to create session.", ErrorType.ServerError));
            }

            var tokens = sessionResult.Value;
            var userDto = UserMapper.ToDto(user);

            _logger.LogInformation("User logged in: {Email} from {IpAddress}", request.Email, ipAddress);

            return Result<AuthResponse>.Success(new AuthResponse(
                tokens.AccessToken,
                tokens.RefreshToken,
                tokens.AccessTokenExpiresAt,
                userDto));
        }, cancellationToken);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(RefreshTokenAsync), async (ct) =>
        {
            Guard.Against.Null(request, nameof(request));
            Guard.Against.NullOrWhiteSpace(request.RefreshToken, nameof(request.RefreshToken));

            var refreshResult = await _sessionService.RefreshSessionAsync(request.RefreshToken, ct);
            if (!refreshResult.IsSuccess)
            {
                return Result<AuthResponse>.Failure(refreshResult.Error!);
            }

            var (userId, newTokens) = refreshResult.Value;

            var userResult = await _userQueryService.GetByIdAsync(userId, ct);
            if (!userResult.IsSuccess || userResult.Value is null)
            {
                return Result<AuthResponse>.Failure(
                    new ResultError(ErrorCodes.User.NotFound, "User not found.", ErrorType.NotFound));
            }

            return Result<AuthResponse>.Success(new AuthResponse(
                newTokens.AccessToken,
                newTokens.RefreshToken,
                newTokens.AccessTokenExpiresAt,
                userResult.Value));
        }, cancellationToken);
    }

    public async Task<Result> RevokeTokenAsync(
        Guid userId,
        string jti,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(RevokeTokenAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            Guard.Against.NullOrWhiteSpace(jti, nameof(jti));

            return await _sessionService.RevokeSessionAsync(userId, jti, ct);
        }, cancellationToken);
    }

    public async Task<Result> RevokeAllUserTokensAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(RevokeAllUserTokensAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));

            return await _sessionService.RevokeAllUserSessionsAsync(userId, ct);
        }, cancellationToken);
    }

    public async Task<Result> LogoutAsync(
        Guid userId,
        string jti,
        CancellationToken cancellationToken = default)
    {
        return await RevokeTokenAsync(userId, jti, cancellationToken);
    }

    public async Task<bool> ValidateTokenAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var result = await _tokenService.ValidateAccessTokenAsync(accessToken, cancellationToken);
        return result.IsSuccess;
    }

    public async Task<Result<Guid>> GetUserIdFromTokenAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        return await _tokenService.ValidateAccessTokenAsync(accessToken, cancellationToken);
    }

    // Private helper methods for LoginAsync

    private async Task<Result<User>> GetAndValidateUserAsync(string email, CancellationToken ct)
    {
        if (_unitOfWork is null)
        {
            return Result<User>.Failure(
                ErrorCodes.Validation.InternalError,
                "Unit of work not available.");
        }

        var user = await _unitOfWork.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);

        if (user is null)
        {
            _logger.LogWarning("Login failed: user not found for email {Email}", email);
            return Result<User>.ValidationError(
                ErrorCodes.Auth.InvalidCredentials,
                "Invalid email or password.");
        }

        return Result<User>.Success(user);
    }

    private async Task<Result> CheckLockoutStatusAsync(Guid userId, CancellationToken ct)
    {
        var isLockedOutResult = await _lockoutService.IsLockedOutAsync(userId, ct);
        if (isLockedOutResult.IsSuccess && isLockedOutResult.Value)
        {
            _logger.LogWarning("Login failed: account locked for user {UserId}", userId);
            return Result.ValidationError(
                ErrorCodes.Auth.AccountLocked,
                "Account is locked. Please try again later.");
        }

        return Result.Success();
    }

    private async Task<Result> VerifyPasswordAndHandleLockoutAsync(
        Guid userId,
        string password,
        string passwordHash,
        CancellationToken ct)
    {
        var verifyResult = _passwordService.VerifyPassword(password, passwordHash);
        if (!verifyResult.IsSuccess || !verifyResult.Value)
        {
            await _lockoutService.RecordFailedAttemptAsync(userId, ct);
            _logger.LogWarning("Login failed: invalid password for user {UserId}", userId);
            return Result.ValidationError(
                ErrorCodes.Auth.InvalidCredentials,
                "Invalid email or password.");
        }

        await _lockoutService.ResetLockoutAsync(userId, ct);
        return Result.Success();
    }

    private async Task<Result> CheckAccountStatusAsync(User user)
    {
        if (!user.EmailVerified)
        {
            _logger.LogWarning("Login failed: email not verified for user {UserId}", user.Id);
            return Result.ValidationError(
                ErrorCodes.Auth.EmailNotVerified,
                "Please verify your email before logging in.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: inactive account for user {UserId}", user.Id);
            return Result.ValidationError(
                ErrorCodes.Auth.AccountLocked,
                "Account is deactivated. Contact support.");
        }

        return Result.Success();
    }

    private async Task<Result<TokenResult>> CreateUserSessionAsync(
        User user,
        string userAgent,
        string ipAddress,
        CancellationToken ct)
    {
        var sessionResult = await _sessionService.CreateSessionAsync(
            user.Id,
            user.Email,
            userAgent,
            ipAddress,
            ct);

        if (!sessionResult.IsSuccess || sessionResult.Value is null)
        {
            return Result<TokenResult>.Failure(
                new ResultError(ErrorCodes.Validation.InternalError, "Failed to create user session.", ErrorType.ServerError));
        }

        return Result<TokenResult>.Success(sessionResult.Value);
    }
}