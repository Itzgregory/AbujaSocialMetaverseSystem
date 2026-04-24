namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class LockoutOptions : FeatureOptions
{
    public override string SectionName => "Lockout";

    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int AccountLockoutMinutes { get; set; } = 15;
}