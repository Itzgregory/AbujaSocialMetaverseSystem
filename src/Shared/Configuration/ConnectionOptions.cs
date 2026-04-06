namespace AbujaSocialMetaverse.Shared.Configuration;

/// <summary>
/// Base for all options that represent a connection to an external service.
/// </summary>
public abstract class ConnectionOptions : BaseOptions
{
    /// <summary>
    /// The primary connection string or base URL for the service.
    /// </summary>
    public abstract string ConnectionString { get; }

    /// <summary>
    /// Timeout in seconds for connection attempts.
    /// Default: 30 seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of retry attempts on transient failures.
    /// Default: 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new InvalidOperationException(
                $"[{SectionName}] ConnectionString is required but was not provided. " +
                $"Check your .env file.");
    }
}