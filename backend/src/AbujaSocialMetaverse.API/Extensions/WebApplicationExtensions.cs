using AbujaSocialMetaverse.API.Middleware;
using AbujaSocialMetaverse.Shared.Configuration.Options;
using Hangfire;
using Serilog;

namespace AbujaSocialMetaverse.API.Extensions;

/// <summary>
/// Extension methods for configuring the middleware pipeline on <see cref="WebApplication"/>.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the full middleware pipeline: exception handling, Serilog request logging,
    /// HTTPS redirection, CORS, rate limiting, auth, Hangfire dashboard, controllers, and health checks.
    /// </summary>
    public static WebApplication UseMiddlewarePipeline(
        this WebApplication app,
        CorsOptions corsOptions,
        HangfireOptions hangfireOptions)
    {
        // Global exception handling (outermost middleware)
        app.UseMiddleware<GlobalExceptionMiddleware>();

        if (app.Environment.IsDevelopment())
            app.MapOpenApi();

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
        });

        app.UseHttpsRedirection();
        app.UseCors(corsOptions.PolicyName);
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        // Hangfire Dashboard (dev only)
        if (app.Environment.IsDevelopment())
        {
            app.UseHangfireDashboard(hangfireOptions.DashboardPath,
                new DashboardOptions { Authorization = [] });
        }

        app.MapControllers();
        app.MapHealthChecks("/health");

        // SignalR Hubs (uncomment when hubs are created)
        // app.MapHub<AvatarHub>("/hubs/avatar");
        // app.MapHub<ChatHub>("/hubs/chat");

        return app;
    }
}
