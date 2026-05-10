namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class DatabaseOptions : ConnectionOptions
{
    public override string SectionName => "Database";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 5432;
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Builds the Npgsql connection string from individual components.
    /// </summary>
    public override string ConnectionString =>
        $"Host={Host};Port={Port};Database={Name};Username={Username};Password={Password}";

    /// <summary>
    /// Maximum number of connections in the pool.
    /// Default: 20 — suitable for a modular monolith under moderate load.
    /// </summary>
    public int MaxPoolSize { get; set; } = 20;

    /// <summary>
    /// Minimum number of connections maintained in the pool.
    /// </summary>
    public int MinPoolSize { get; set; } = 2;

    /// <summary>
    /// Whether to enable detailed EF Core errors.
    /// Should only be true in Development.
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Whether to log sensitive data (query parameters).
    /// Must never be true in Production.
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    public override void Validate()
    {
        base.Validate();

        if (string.IsNullOrWhiteSpace(Host))
            throw new InvalidOperationException(
                $"[{SectionName}] Host is required. Check DB_HOST in your .env file.");

        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException(
                $"[{SectionName}] Database name is required. Check DB_NAME in your .env file.");

        if (string.IsNullOrWhiteSpace(Username))
            throw new InvalidOperationException(
                $"[{SectionName}] Username is required. Check DB_USER in your .env file.");

        if (string.IsNullOrWhiteSpace(Password))
            throw new InvalidOperationException(
                $"[{SectionName}] Password is required. Check DB_PASSWORD in your .env file.");

        if (Port <= 0 || Port > 65535)
            throw new InvalidOperationException(
                $"[{SectionName}] Port must be between 1 and 65535. Check DB_PORT in your .env file.");
    }
}