namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class RedisOptions : ConnectionOptions
{
    public override string SectionName => "Redis";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 6379;
    public string? Password { get; set; }
    public int DatabaseIndex { get; set; } = 0;

    public override string ConnectionString =>
        string.IsNullOrWhiteSpace(Password)
            ? $"{Host}:{Port},defaultDatabase={DatabaseIndex}"
            : $"{Host}:{Port},password={Password},defaultDatabase={DatabaseIndex}";

    /// <summary>
    /// Channel prefix for SignalR backplane.
    /// Prevents key collisions if Redis is shared across services.
    /// </summary>
    public string ChannelPrefix { get; set; } = "asm";

    /// <summary>
    /// Default TTL for cached entries with no explicit expiry.
    /// Default: 60 minutes.
    /// </summary>
    public int DefaultTtlMinutes { get; set; } = 60;

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host))
            throw new InvalidOperationException(
                $"[{SectionName}] Host is required. Check REDIS_HOST in your .env file.");
    }
}