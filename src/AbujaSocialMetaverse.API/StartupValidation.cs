namespace AbujaSocialMetaverse.API;

public static class StartupValidation
{
    private static readonly string[] RequiredVars =
    [
        "DB_HOST", "DB_PORT", "DB_NAME", "DB_USER", "DB_PASSWORD",
        "REDIS_CONNECTION",
        "JWT_KEY", "JWT_ISSUER", "JWT_AUDIENCE",
        "JWT_EXPIRY_MINUTES", "JWT_REFRESH_EXPIRY_DAYS",
        "CORS_ALLOWED_ORIGINS"
    ];

    public static void Validate()
    {
        var missing = RequiredVars
            .Where(key => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
            .ToList();

        if (missing.Count == 0) return;

        var formatted = string.Join(Environment.NewLine, missing.Select(k => $"  - {k}"));
        throw new InvalidOperationException(
            $"The following required environment variables are missing or empty:{Environment.NewLine}{formatted}{Environment.NewLine}Check your .env file.");
    }

    public static void ValidateJwtKey()
    {
        var key = Environment.GetEnvironmentVariable("JWT_KEY") ?? string.Empty;
        if (key.Length < 32)
            throw new InvalidOperationException(
                "JWT_KEY must be at least 32 characters long. Generate a cryptographically strong key.");
    }
}