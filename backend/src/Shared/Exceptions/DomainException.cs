namespace AbujaSocialMetaverse.Shared.Exceptions;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    ServerError
}

/// <summary>
/// Base exception for all domain-level failures.
/// Caught by GlobalExceptionMiddleware and mapped to HTTP responses.
/// Use Result<T> for expected failures — throw DomainException only
/// when the failure is truly exceptional within the domain flow.
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }
    public ErrorType Type { get; }
    public IReadOnlyDictionary<string, object>? Metadata { get; }

    public DomainException(
        string code,
        string message,
        ErrorType type = ErrorType.ServerError,
        IReadOnlyDictionary<string, object>? metadata = null)
        : base(message)
    {
        Code = code;
        Type = type;
        Metadata = metadata;
    }

    // ─── Factory methods — prefer these over constructors ─────────────────────

    public static DomainException NotFound(string code, string message)
        => new(code, message, ErrorType.NotFound);

    public static DomainException Conflict(string code, string message)
        => new(code, message, ErrorType.Conflict);

    public static DomainException Unauthorized(string code, string message)
        => new(code, message, ErrorType.Unauthorized);

    public static DomainException Forbidden(string code, string message)
        => new(code, message, ErrorType.Forbidden);

    public static DomainException Validation(string code, string message)
        => new(code, message, ErrorType.Validation);

    public static DomainException Server(string code, string message)
        => new(code, message, ErrorType.ServerError);
}