using Microsoft.Extensions.DependencyInjection;
using AbujaSocialMetaverse.Modules.Core.Internal.Services;
using AbujaSocialMetaverse.Modules.Core.Public.Interfaces;

namespace AbujaSocialMetaverse.Modules.Core;

public static class ModuleRegistration
{
    public static IServiceCollection AddCoreModule(this IServiceCollection services)
    {
        // Query Services
        services.AddScoped<IUserQueryService, UserQueryService>();
        services.AddScoped<IUserCreationService, UserCreationService>();
        
        // Profile Services
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IUserInterestService, UserInterestService>();
        services.AddScoped<IModeAvailabilityService, ModeAvailabilityService>();
        
        // Security Services
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ILockoutService, LockoutService>();
        services.AddScoped<ITokenService, TokenService>();
        
        // Session & Auth Services
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IAuthService, AuthService>();
        
        return services;
    }
}