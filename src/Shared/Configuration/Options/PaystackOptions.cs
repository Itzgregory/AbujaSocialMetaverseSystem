namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class PaystackOptions : SecurityOptions
{
    public override string SectionName => "Paystack";

    public override string SecretKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.paystack.co";

    /// <summary>
    /// Currency for Paystack transactions.
    /// Default: NGN — Paystack's primary supported currency.
    /// </summary>
    public string Currency { get; set; } = "NGN";

    /// <summary>
    /// Paystack charges in kobo (smallest NGN unit).
    /// This multiplier converts naira amounts to kobo for API calls.
    /// </summary>
    public int KoboMultiplier { get; set; } = 100;

    public override void Validate()
    {
        base.Validate();

        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new InvalidOperationException(
                $"[{SectionName}] BaseUrl is required. " +
                $"Check PAYSTACK_BASE_URL in your .env file.");
    }
}