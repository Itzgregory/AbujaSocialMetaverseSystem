using Microsoft.Extensions.DependencyInjection;
using AbujaSocialMetaverse.Modules.Core.Internal;
using AbujaSocialMetaverse.Modules.Core.Public;

namespace AbujaSocialMetaverse.Modules.Core;

public static class ModuleRegistration
{
    public static IServiceCollection AddCoreModule(this IServiceCollection services)
    {
        // Services (direct DbContext usage — no repository layer)
        services.AddScoped<IUserQueryService, UserQueryService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IUserInterestService, UserInterestService>();
        services.AddScoped<IModeAvailabilityService, ModeAvailabilityService>();
        
        // AuthService will be added when implemented
        // services.AddScoped<IAuthService, AuthService>();
        
        return services;
    }
}