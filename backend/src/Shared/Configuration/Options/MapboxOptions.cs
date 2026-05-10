namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class MapboxOptions : ConnectionOptions
{
    public override string SectionName => "Mapbox";

    public string AccessToken { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.mapbox.com";

    public override string ConnectionString => BaseUrl;

    /// <summary>
    /// Default map style for Abuja city rendering.
    /// </summary>
    public string DefaultStyle { get; set; } = "mapbox://styles/mapbox/streets-v12";

    /// <summary>
    /// Tile cache TTL in minutes.
    /// Default: 60 minutes — tiles change infrequently.
    /// </summary>
    public int TileCacheTtlMinutes { get; set; } = 60;

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(AccessToken))
            throw new InvalidOperationException(
                $"[{SectionName}] AccessToken is required. " +
                $"Check MAPBOX_ACCESS_TOKEN in your .env file.");

        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new InvalidOperationException(
                $"[{SectionName}] BaseUrl is required. " +
                $"Check MAPBOX_BASE_URL in your .env file.");
    }
}