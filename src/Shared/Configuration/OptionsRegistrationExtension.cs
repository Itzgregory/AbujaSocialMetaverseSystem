using AbujaSocialMetaverse.Shared.Configuration.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AbujaSocialMetaverse.Shared.Configuration;

public static class OptionsRegistrationExtension
{
    /// <summary>
    /// Registers and validates all application options.
    /// Adding a new options class requires no changes here —
    /// just add it to the AllOptions collection below.
    /// </summary>
    public static IServiceCollection AddApplicationOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allOptions = new List<BaseOptions>
        {
            new DatabaseOptions(),
            new RedisOptions(),
            new JwtOptions(),
            new MapboxOptions(),
            new StripeOptions(),
            new PaystackOptions(),
            new HangfireOptions(),
            new RealTimeOptions(),
            new PrivacyOptions(),
            new RecommendationOptions(),
            new RateLimitOptions(),
            new CorsOptions(),
            new LoggingOptions(),
            new UserOptions(),
            new LockoutOptions(),
            new PasswordPolicyOptions(),
            new EmailOptions()
        };

        foreach (var option in allOptions)
        {
            // Bind from IConfiguration
            configuration.GetSection(option.SectionName).Bind(option);

            // Validate at startup — fail fast on misconfiguration
            option.Validate();

            // Register as the concrete type and as BaseOptions
            var optionType = option.GetType();
            services.AddSingleton(optionType, option);
        }

        return services;
    }
}