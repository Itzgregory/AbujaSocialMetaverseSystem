using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;
using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Helpers;
using AbujaSocialMetaverse.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AbujaSocialMetaverse.Modules.Core.Internal.Services;

/// <summary>
/// Core rules for using BaseService:
/// 
/// 1. INHERIT FROM BaseService IF:
///    - Service needs standardized error handling
///    - Service uses IUnitOfWork for data access
/// 
/// 2. DO NOT:
///    - Call BeginTransactionAsync inside ExecuteAsync (use separate pattern)
///    - Add module-specific helpers to BaseService (keep it universal)
///    - Inject other services into BaseService (causes circular deps)
/// 
/// 3. USE ExecuteAsync FOR:
///    - Methods that need try-catch-error logging
///    - Methods that return Result<T> or Result
/// 
/// 4. HANDLE SPECIFIC ERRORS OUTSIDE ExecuteAsync:
///    - Catch DbUpdateException in the calling method for duplicate keys, etc.
///    - Example: try { return await ExecuteAsync(...); } catch (DbUpdateException ex) { ... }
/// 
/// 5. DO NOT USE ExecuteAsync FOR:
///    - Simple validation (no I/O, low risk)
///    - Methods that need custom error handling
/// </summary>
public abstract class BaseService
{
    private const string ArgValidationErrorMsg = "{Operation} failed with argument validation error";
    private const string InvalidStateErrorMsg = "{Operation} failed with invalid state";
    private const string UnexpectedErrorMsg = "{Operation} failed with unexpected error";
    private const string GenericErrorMessage = "An unexpected error occurred. Please try again.";
    private const string DatabaseErrorMessage = "A database error occurred. Please try again.";
    private const string SaveChangesFailedMsg = "Failed to save changes to database";
    private const string UnitOfWorkNotAvailableMsg = "Unit of work not available.";

    protected readonly ILogger _logger;
    protected readonly IUnitOfWork? _unitOfWork;

    protected BaseService(ILogger logger, IUnitOfWork? unitOfWork = null)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Executes an async operation that returns a Result<T> with standardized error handling.
    /// NOTE: DbUpdateException is NOT caught here intentionally. Services should handle
    /// specific database errors (like duplicate keys) in their own try-catch blocks.
    /// </summary>
    protected async Task<Result<T>> ExecuteAsync<T>(
        string operationName,
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await action(cancellationToken);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, ArgValidationErrorMsg, operationName);
            return Result<T>.ValidationError(ErrorCodes.Validation.InvalidInput, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, InvalidStateErrorMsg, operationName);
            return Result<T>.ValidationError(ErrorCodes.Validation.InvalidState, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UnexpectedErrorMsg, operationName);
            return Result<T>.Failure(ErrorCodes.Validation.InternalError, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Executes an async operation that returns a Result (void) with standardized error handling.
    /// NOTE: DbUpdateException is NOT caught here intentionally. Services should handle
    /// specific database errors (like duplicate keys) in their own try-catch blocks.
    /// </summary>
    protected async Task<Result> ExecuteAsync(
        string operationName,
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await action(cancellationToken);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, ArgValidationErrorMsg, operationName);
            return Result.ValidationError(ErrorCodes.Validation.InvalidInput, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, InvalidStateErrorMsg, operationName);
            return Result.ValidationError(ErrorCodes.Validation.InvalidState, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UnexpectedErrorMsg, operationName);
            return Result.Failure(ErrorCodes.Validation.InternalError, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Executes a sync operation that returns a Result<T> with standardized error handling.
    /// Use this for CPU-bound operations like password hashing.
    /// </summary>
    protected Result<T> ExecuteSync<T>(
        string operationName,
        Func<Result<T>> action)
    {
        try
        {
            return action();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, ArgValidationErrorMsg, operationName);
            return Result<T>.ValidationError(ErrorCodes.Validation.InvalidInput, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, InvalidStateErrorMsg, operationName);
            return Result<T>.ValidationError(ErrorCodes.Validation.InvalidState, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UnexpectedErrorMsg, operationName);
            return Result<T>.Failure(ErrorCodes.Validation.InternalError, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Executes a sync operation that returns a Result (void) with standardized error handling.
    /// Use this for CPU-bound operations.
    /// </summary>
    protected Result ExecuteSync(
        string operationName,
        Func<Result> action)
    {
        try
        {
            return action();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, ArgValidationErrorMsg, operationName);
            return Result.ValidationError(ErrorCodes.Validation.InvalidInput, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, InvalidStateErrorMsg, operationName);
            return Result.ValidationError(ErrorCodes.Validation.InvalidState, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UnexpectedErrorMsg, operationName);
            return Result.Failure(ErrorCodes.Validation.InternalError, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Saves changes to the database with standardized error handling.
    /// NOTE: For operations that need specific DbUpdateException handling,
    /// call _unitOfWork.SaveChangesAsync() directly instead of using this helper.
    /// </summary>
    protected async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_unitOfWork is null)
        {
            return Result.Failure(ErrorCodes.Validation.InternalError, UnitOfWorkNotAvailableMsg);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, SaveChangesFailedMsg);
            return Result.Failure(ErrorCodes.Validation.DatabaseError, DatabaseErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, SaveChangesFailedMsg);
            return Result.Failure(ErrorCodes.Validation.InternalError, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Retrieves a user by ID with existence check.
    /// </summary>
    protected async Task<Result<User>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (_unitOfWork is null)
        {
            return Result<User>.Failure(ErrorCodes.Validation.InternalError, UnitOfWorkNotAvailableMsg);
        }

        var user = await _unitOfWork.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            return Result<User>.NotFound(ErrorCodes.User.NotFound, $"User with ID '{userId}' was not found.");
        }

        return Result<User>.Success(user);
    }

    /// <summary>
    /// Retrieves a user with their settings and interests.
    /// </summary>
    protected async Task<Result<User>> GetUserWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (_unitOfWork is null)
        {
            return Result<User>.Failure(ErrorCodes.Validation.InternalError, UnitOfWorkNotAvailableMsg);
        }

        var user = await _unitOfWork.Set<User>()
            .Include(u => u.Settings)
            .Include(u => u.Interests)
                .ThenInclude(ui => ui.Interest)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            return Result<User>.NotFound(ErrorCodes.User.NotFound, $"User with ID '{userId}' was not found.");
        }

        return Result<User>.Success(user);
    }
}