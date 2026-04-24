namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class PasswordPolicyOptions : FeatureOptions
{
    public override string SectionName => "PasswordPolicy";

    // Password requirements
    public int MinLength { get; set; } = 8;
    public int MaxLength { get; set; } = 128;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialCharacter { get; set; } = true;

    // Lockout settings
    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int AccountLockoutMinutes { get; set; } = 15;
}