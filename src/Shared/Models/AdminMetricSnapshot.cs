namespace AbujaSocialMetaverse.Shared.Models;

/// <summary>
/// Read-only snapshot published by each module's IAdminProjection implementation.
/// AdminModule aggregates these — never calls module services directly.
/// </summary>
public record AdminMetricSnapshot
{
    public string ModuleName { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object> Metrics { get; init; }
        = new Dictionary<string, object>();
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;
    public bool IsHealthy { get; init; } = true;
    public string? HealthMessage { get; init; }

    public static AdminMetricSnapshot Create(
        string moduleName,
        Dictionary<string, object> metrics,
        bool isHealthy = true,
        string? healthMessage = null)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
            throw new ArgumentException(
                "Module name cannot be empty.", nameof(moduleName));

        return new AdminMetricSnapshot
        {
            ModuleName = moduleName,
            Metrics = metrics.AsReadOnly(),
            GeneratedAt = DateTimeOffset.UtcNow,
            IsHealthy = isHealthy,
            HealthMessage = healthMessage
        };
    }
}