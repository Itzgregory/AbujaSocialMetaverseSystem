namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class LoggingOptions : FeatureOptions
{
    public override string SectionName => "Logging";

    /// <summary>
    /// Minimum log level. Maps to Serilog.Events.LogEventLevel.
    /// Valid values: Verbose, Debug, Information, Warning, Error, Fatal.
    /// Sourced from LOG_LEVEL in .env.
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// Path and filename pattern for log files.
    /// Sourced from LOG_FILE_PATH in .env.
    /// </summary>
    public string FilePath { get; set; } = "logs/abuja-metaverse-.log";

    /// <summary>
    /// Number of days to retain log files before deletion.
    /// </summary>
    public int RetainedFileCount { get; set; } = 30;

    /// <summary>
    /// Whether to write logs to console.
    /// Always true in Development. Can be disabled in Production
    /// if a log aggregator is used instead.
    /// </summary>
    public bool WriteToConsole { get; set; } = true;

    /// <summary>
    /// Whether to write logs to file.
    /// </summary>
    public bool WriteToFile { get; set; } = true;

    public override void Validate()
    {
        var validLevels = new[]
        {
            "Verbose", "Debug", "Information", "Warning", "Error", "Fatal"
        };

        if (!validLevels.Contains(MinimumLevel, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"[{SectionName}] MinimumLevel '{MinimumLevel}' is not valid. " +
                $"Valid values: {string.Join(", ", validLevels)}.");

        if (RetainedFileCount <= 0)
            throw new InvalidOperationException(
                $"[{SectionName}] RetainedFileCount must be greater than 0.");

        if (!WriteToConsole && !WriteToFile)
            throw new InvalidOperationException(
                $"[{SectionName}] At least one log sink (Console or File) must be enabled.");
    }
}