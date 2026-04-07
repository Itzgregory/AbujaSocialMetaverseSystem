namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class UserOptions : FeatureOptions
{
    public override string SectionName => "User";
    
    public int MinAgePreference { get; set; } = 18;
    public int MaxAgePreference { get; set; } = 100;
    public int DefaultTravelRadiusMeters { get; set; } = 5000;
    public int MaxTravelRadiusMeters { get; set; } = 50000;
    public int MinPasswordLength { get; set; } = 8;
    public int MaxPasswordLength { get; set; } = 128;
}