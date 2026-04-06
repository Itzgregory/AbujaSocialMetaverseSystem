namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class StripeOptions : SecurityOptions
{
    public override string SectionName => "Stripe";

    public override string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.stripe.com";

    /// <summary>
    /// Currency for Stripe transactions.
    /// Default: USD — Stripe does not support NGN natively.
    /// </summary>
    public string Currency { get; set; } = "usd";

    public override void Validate()
    {
        base.Validate();

        if (string.IsNullOrWhiteSpace(WebhookSecret))
            throw new InvalidOperationException(
                $"[{SectionName}] WebhookSecret is required. " +
                $"Check STRIPE_WEBHOOK_SECRET in your .env file.");
    }
}