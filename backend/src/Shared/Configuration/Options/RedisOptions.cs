namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class RedisOptions : ConnectionOptions
{
    public override string SectionName => "Redis";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 6379;
    public string? Password { get; set; }
    public int DatabaseIndex { get; set; } = 0;

    /// <summary>
    /// When false (default), adds <c>abortConnect=false</c> so the process can start if Redis is down.
    /// Set true to fail fast when Redis must be available before serving traffic.
    /// </summary>
    public bool AbortOnConnectFail { get; set; }

    public override string ConnectionString
    {
        get
        {
            var core = string.IsNullOrWhiteSpace(Password)
                ? $"{Host}:{Port},defaultDatabase={DatabaseIndex}"
                : $"{Host}:{Port},password={Password},defaultDatabase={DatabaseIndex}";
            return AbortOnConnectFail ? core : $"{core},abortConnect=false";
        }
    }

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