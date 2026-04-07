using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Exceptions;

namespace AbujaSocialMetaverse.Shared.Models;

/// <summary>
/// Represents the error detail on a failed Result.
/// </summary>
public record ResultError(
    string Code,
    string Message,
    ErrorType Type = ErrorType.ServerError,
    IReadOnlyDictionary<string, object>? Metadata = null);

/// <summary>
/// Represents the outcome of an operation that returns no value.
/// Use for void operations that can fail in expected ways.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public ResultError? Error { get; }

    protected Result(bool isSuccess, ResultError? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException(
                "A successful result cannot carry an error.");

        if (!isSuccess && error is null)
            throw new InvalidOperationException(
                "A failed result must carry an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(ResultError error) => new(false, error);

    public static Result Failure(string code, string message,
        ErrorType type = ErrorType.ServerError)
        => new(false, new ResultError(code, message, type));

    // Shorthand factory methods 

    public static Result NotFound(string code, string message)
        => Failure(code, message, ErrorType.NotFound);

    public static Result Conflict(string code, string message)
        => Failure(code, message, ErrorType.Conflict);

    public static Result Unauthorized(string code, string message)
        => Failure(code, message, ErrorType.Unauthorized);

    public static Result Forbidden(string code, string message)
        => Failure(code, message, ErrorType.Forbidden);

    public static Result ValidationError(string code, string message)
        => Failure(code, message, ErrorType.Validation);
}

/// <summary>
/// Represents the outcome of an operation that returns a value on success.
/// Use instead of throwing exceptions for expected failure scenarios.
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, ResultError? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null);

    public new static Result<T> Failure(ResultError error) => new(false, default, error);

    public new static Result<T> Failure(string code, string message,
        ErrorType type = ErrorType.ServerError)
        => new(false, default, new ResultError(code, message, type));

    // Shorthand factory methods 

    public new static Result<T> NotFound(string code, string message)
        => Failure(code, message, ErrorType.NotFound);

    public new static Result<T> Conflict(string code, string message)
        => Failure(code, message, ErrorType.Conflict);

    public new static Result<T> Unauthorized(string code, string message)
        => Failure(code, message, ErrorType.Unauthorized);

    public new static Result<T> Forbidden(string code, string message)
        => Failure(code, message, ErrorType.Forbidden);

    public new static Result<T> ValidationError(string code, string message)
        => Failure(code, message, ErrorType.Validation);

    /// <summary>
    /// Transforms the value if successful, propagates failure otherwise.
    /// </summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        if (IsFailure)
            return Result<TOut>.Failure(Error!);

        return Result<TOut>.Success(mapper(Value!));
    }

    /// <summary>
    /// Chains async operations — only executes next if current is successful.
    /// </summary>
    public async Task<Result<TOut>> BindAsync<TOut>(
        Func<T, Task<Result<TOut>>> next)
    {
        if (IsFailure)
            return Result<TOut>.Failure(Error!);

        return await next(Value!);
    }
}